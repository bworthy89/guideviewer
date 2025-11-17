using FluentAssertions;
using GuideViewer.Core.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace GuideViewer.Tests.Services;

/// <summary>
/// Unit tests for TimerService.
/// </summary>
public class TimerServiceTests : IDisposable
{
    private readonly TimerService _timerService;

    public TimerServiceTests()
    {
        _timerService = new TimerService();
    }

    [Fact]
    public void Constructor_InitializesWithZeroElapsed()
    {
        // Assert
        _timerService.Elapsed.Should().Be(TimeSpan.Zero);
        _timerService.IsRunning.Should().BeFalse();
    }

    [Fact]
    public void Start_SetsIsRunningToTrue()
    {
        // Act
        _timerService.Start();

        // Assert
        _timerService.IsRunning.Should().BeTrue();
    }

    [Fact]
    public void Start_WhenAlreadyRunning_DoesNothing()
    {
        // Arrange
        _timerService.Start();
        var firstElapsed = _timerService.Elapsed;

        // Act - Start again
        _timerService.Start();

        // Assert - Should not reset elapsed time
        _timerService.IsRunning.Should().BeTrue();
        _timerService.Elapsed.Should().BeGreaterThanOrEqualTo(firstElapsed);
    }

    [Fact]
    public void Stop_SetsIsRunningToFalse()
    {
        // Arrange
        _timerService.Start();

        // Act
        _timerService.Stop();

        // Assert
        _timerService.IsRunning.Should().BeFalse();
    }

    [Fact]
    public void Stop_PreservesElapsedTime()
    {
        // Arrange
        _timerService.Start();
        Thread.Sleep(100);
        _timerService.Stop();
        var elapsedAfterStop = _timerService.Elapsed;

        // Act - Wait more time
        Thread.Sleep(100);

        // Assert - Elapsed should not increase
        _timerService.Elapsed.Should().Be(elapsedAfterStop);
    }

    [Fact]
    public void Stop_WhenNotRunning_DoesNothing()
    {
        // Act
        _timerService.Stop();

        // Assert
        _timerService.IsRunning.Should().BeFalse();
        _timerService.Elapsed.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void Reset_StopsTimerAndClearsElapsed()
    {
        // Arrange
        _timerService.Start();
        Thread.Sleep(100);

        // Act
        _timerService.Reset();

        // Assert
        _timerService.IsRunning.Should().BeFalse();
        _timerService.Elapsed.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void Elapsed_IncreasesWhileRunning()
    {
        // Arrange
        _timerService.Start();
        var initialElapsed = _timerService.Elapsed;

        // Act - Wait at least 100ms
        Thread.Sleep(150);

        // Assert - Should have increased by at least 100ms
        _timerService.Elapsed.Should().BeGreaterThan(initialElapsed);
        _timerService.Elapsed.TotalMilliseconds.Should().BeGreaterThanOrEqualTo(100);
    }

    [Fact]
    public async Task TickEvent_FiresEverySecondWhenRunning()
    {
        // Arrange
        var tickCount = 0;
        TimeSpan? lastElapsed = null;

        _timerService.Tick += (sender, elapsed) =>
        {
            tickCount++;
            lastElapsed = elapsed;
        };

        // Act - Run for ~2.5 seconds
        _timerService.Start();
        await Task.Delay(2500);
        _timerService.Stop();

        // Assert - Should have fired at least 2 times (allowing for timer variance)
        tickCount.Should().BeGreaterThanOrEqualTo(2);
        lastElapsed.Should().NotBeNull();
        lastElapsed!.Value.TotalSeconds.Should().BeGreaterThan(1);
    }

    [Fact]
    public async Task TickEvent_DoesNotFireWhenStopped()
    {
        // Arrange
        var tickCount = 0;

        _timerService.Tick += (sender, elapsed) =>
        {
            tickCount++;
        };

        _timerService.Start();
        await Task.Delay(1100); // Let it tick once
        _timerService.Stop();
        var tickCountAfterStop = tickCount;

        // Act - Wait more time after stopping
        await Task.Delay(1100);

        // Assert - Should not have increased
        tickCount.Should().Be(tickCountAfterStop);
    }

    [Fact]
    public async Task TickEvent_ProvidesCurrentElapsedTime()
    {
        // Arrange
        TimeSpan? receivedElapsed = null;

        _timerService.Tick += (sender, elapsed) =>
        {
            receivedElapsed = elapsed;
        };

        // Act
        _timerService.Start();
        await Task.Delay(1100);
        _timerService.Stop();

        // Assert
        receivedElapsed.Should().NotBeNull();
        receivedElapsed!.Value.TotalSeconds.Should().BeGreaterThanOrEqualTo(1);
        // Use larger tolerance (300ms) to account for timer thread scheduling variability
        receivedElapsed.Value.Should().BeCloseTo(_timerService.Elapsed, TimeSpan.FromMilliseconds(300));
    }

    [Fact]
    public void StartStopResume_WorksCorrectly()
    {
        // Act 1: Start and run for a bit
        _timerService.Start();
        Thread.Sleep(100);
        var elapsedAfterFirstRun = _timerService.Elapsed;

        // Act 2: Stop and verify time is preserved (within tolerance)
        _timerService.Stop();
        Thread.Sleep(100);
        _timerService.Elapsed.Should().BeCloseTo(elapsedAfterFirstRun, TimeSpan.FromMilliseconds(10));

        // Act 3: Resume and verify time continues
        _timerService.Start();
        Thread.Sleep(100);

        // Assert
        _timerService.Elapsed.Should().BeGreaterThan(elapsedAfterFirstRun);
        _timerService.IsRunning.Should().BeTrue();
    }

    [Fact]
    public void Dispose_StopsTimer()
    {
        // Arrange
        _timerService.Start();

        // Act
        _timerService.Dispose();

        // Assert
        _timerService.IsRunning.Should().BeFalse();
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        // Act & Assert - Should not throw
        _timerService.Dispose();
        _timerService.Dispose();
    }

    [Fact]
    public async Task MultipleSubscribers_AllReceiveTickEvents()
    {
        // Arrange
        var tickCount1 = 0;
        var tickCount2 = 0;

        _timerService.Tick += (sender, elapsed) => tickCount1++;
        _timerService.Tick += (sender, elapsed) => tickCount2++;

        // Act
        _timerService.Start();
        await Task.Delay(1100);
        _timerService.Stop();

        // Assert
        tickCount1.Should().BeGreaterThanOrEqualTo(1);
        tickCount2.Should().BeGreaterThanOrEqualTo(1);
        tickCount1.Should().Be(tickCount2);
    }

    public void Dispose()
    {
        _timerService?.Dispose();
    }
}

using FluentAssertions;
using GuideViewer.Core.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace GuideViewer.Tests.Services;

/// <summary>
/// Unit tests for AutoSaveService.
/// </summary>
public class AutoSaveServiceTests : IDisposable
{
    private readonly AutoSaveService _autoSaveService;

    public AutoSaveServiceTests()
    {
        _autoSaveService = new AutoSaveService();
    }

    [Fact]
    public void StartAutoSave_WithValidCallback_ShouldActivateService()
    {
        // Arrange
        var saveCallbackInvoked = false;
        Task SaveCallback()
        {
            saveCallbackInvoked = true;
            return Task.CompletedTask;
        }

        // Act
        _autoSaveService.StartAutoSave(SaveCallback, intervalSeconds: 1);

        // Assert
        _autoSaveService.IsActive.Should().BeTrue();
        _autoSaveService.IntervalSeconds.Should().Be(1);
    }

    [Fact]
    public void StartAutoSave_WithNullCallback_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            _autoSaveService.StartAutoSave(null!, intervalSeconds: 30));
    }

    [Fact]
    public void StartAutoSave_WithNegativeInterval_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            _autoSaveService.StartAutoSave(() => Task.CompletedTask, intervalSeconds: -1));
    }

    [Fact]
    public void StartAutoSave_WithZeroInterval_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            _autoSaveService.StartAutoSave(() => Task.CompletedTask, intervalSeconds: 0));
    }

    [Fact]
    public void StopAutoSave_ShouldDeactivateService()
    {
        // Arrange
        _autoSaveService.StartAutoSave(() => Task.CompletedTask, intervalSeconds: 30);

        // Act
        _autoSaveService.StopAutoSave();

        // Assert
        _autoSaveService.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task AutoSave_WhenDirty_ShouldInvokeSaveCallback()
    {
        // Arrange
        var saveCallbackInvoked = false;
        var saveCount = 0;
        Task SaveCallback()
        {
            saveCallbackInvoked = true;
            saveCount++;
            return Task.CompletedTask;
        }

        _autoSaveService.StartAutoSave(SaveCallback, intervalSeconds: 1);
        _autoSaveService.IsDirty = true;

        // Act - wait for auto-save to trigger (1 second interval + buffer)
        await Task.Delay(1500);

        // Assert
        saveCallbackInvoked.Should().BeTrue();
        saveCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task AutoSave_WhenNotDirty_ShouldNotInvokeSaveCallback()
    {
        // Arrange
        var saveCallbackInvoked = false;
        Task SaveCallback()
        {
            saveCallbackInvoked = true;
            return Task.CompletedTask;
        }

        _autoSaveService.StartAutoSave(SaveCallback, intervalSeconds: 1);
        _autoSaveService.IsDirty = false;

        // Act - wait for auto-save interval
        await Task.Delay(1500);

        // Assert
        saveCallbackInvoked.Should().BeFalse();
    }

    [Fact]
    public async Task AutoSave_AfterSave_ShouldSetIsDirtyToFalse()
    {
        // Arrange
        Task SaveCallback() => Task.CompletedTask;

        _autoSaveService.StartAutoSave(SaveCallback, intervalSeconds: 1);
        _autoSaveService.IsDirty = true;

        // Act - wait for auto-save to trigger
        await Task.Delay(1500);

        // Assert
        _autoSaveService.IsDirty.Should().BeFalse();
    }

    [Fact]
    public async Task AutoSave_AfterSave_ShouldUpdateLastSavedAt()
    {
        // Arrange
        Task SaveCallback() => Task.CompletedTask;

        _autoSaveService.StartAutoSave(SaveCallback, intervalSeconds: 1);
        _autoSaveService.IsDirty = true;
        var beforeSave = DateTime.UtcNow;

        // Act - wait for auto-save to trigger
        await Task.Delay(1500);

        // Assert
        _autoSaveService.LastSavedAt.Should().NotBeNull();
        _autoSaveService.LastSavedAt.Should().BeOnOrAfter(beforeSave);
        _autoSaveService.LastSavedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task ManualSaveAsync_WhenDirty_ShouldInvokeSaveCallback()
    {
        // Arrange
        var saveCallbackInvoked = false;
        Task SaveCallback()
        {
            saveCallbackInvoked = true;
            return Task.CompletedTask;
        }

        _autoSaveService.StartAutoSave(SaveCallback, intervalSeconds: 30);
        _autoSaveService.IsDirty = true;

        // Act
        var result = await _autoSaveService.ManualSaveAsync();

        // Assert
        result.Should().BeTrue();
        saveCallbackInvoked.Should().BeTrue();
        _autoSaveService.IsDirty.Should().BeFalse();
    }

    [Fact]
    public async Task ManualSaveAsync_WhenNotDirty_ShouldReturnFalse()
    {
        // Arrange
        var saveCallbackInvoked = false;
        Task SaveCallback()
        {
            saveCallbackInvoked = true;
            return Task.CompletedTask;
        }

        _autoSaveService.StartAutoSave(SaveCallback, intervalSeconds: 30);
        _autoSaveService.IsDirty = false;

        // Act
        var result = await _autoSaveService.ManualSaveAsync();

        // Assert
        result.Should().BeFalse();
        saveCallbackInvoked.Should().BeFalse();
    }

    [Fact]
    public async Task ManualSaveAsync_WithoutStartingAutoSave_ShouldReturnFalse()
    {
        // Act
        var result = await _autoSaveService.ManualSaveAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsDirty_WhenSet_ShouldRetainValue()
    {
        // Act
        _autoSaveService.IsDirty = true;

        // Assert
        _autoSaveService.IsDirty.Should().BeTrue();

        // Act
        _autoSaveService.IsDirty = false;

        // Assert
        _autoSaveService.IsDirty.Should().BeFalse();
    }

    [Fact]
    public async Task AutoSave_WithFailingCallback_ShouldKeepIsDirtyTrue()
    {
        // Arrange
        Task SaveCallback() => throw new InvalidOperationException("Save failed");

        _autoSaveService.StartAutoSave(SaveCallback, intervalSeconds: 1);
        _autoSaveService.IsDirty = true;

        // Act - wait for auto-save to trigger
        await Task.Delay(1500);

        // Assert - should remain dirty to retry later
        _autoSaveService.IsDirty.Should().BeTrue();
    }

    [Fact]
    public async Task ResetTimer_ShouldDelayNextAutoSave()
    {
        // Arrange
        var saveCount = 0;
        Task SaveCallback()
        {
            saveCount++;
            return Task.CompletedTask;
        }

        _autoSaveService.StartAutoSave(SaveCallback, intervalSeconds: 2);
        _autoSaveService.IsDirty = true;

        // Act - wait 1 second, then reset timer
        await Task.Delay(1000);
        _autoSaveService.ResetTimer();
        _autoSaveService.IsDirty = true; // Keep dirty after potential save

        // Wait another 1.5 seconds (should not trigger yet since timer was reset)
        await Task.Delay(1500);

        // Assert - save should not have happened yet
        saveCount.Should().Be(0);

        // Wait another second (total 2.5 seconds after reset)
        await Task.Delay(1000);

        // Now save should have happened
        saveCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public void LastSavedAt_InitiallyNull()
    {
        // Assert
        _autoSaveService.LastSavedAt.Should().BeNull();
    }

    [Fact]
    public void IsActive_InitiallyFalse()
    {
        // Assert
        _autoSaveService.IsActive.Should().BeFalse();
    }

    [Fact]
    public void IsDirty_InitiallyFalse()
    {
        // Assert
        _autoSaveService.IsDirty.Should().BeFalse();
    }

    [Fact]
    public void IntervalSeconds_BeforeStart_ShouldBeZero()
    {
        // Assert
        _autoSaveService.IntervalSeconds.Should().Be(0);
    }

    [Fact]
    public async Task MultipleStartCalls_ShouldReplaceExistingTimer()
    {
        // Arrange
        var saveCount = 0;
        Task SaveCallback()
        {
            saveCount++;
            return Task.CompletedTask;
        }

        // Act - start with 2 second interval
        _autoSaveService.StartAutoSave(SaveCallback, intervalSeconds: 2);
        _autoSaveService.IsDirty = true;

        // Immediately start again with 1 second interval
        _autoSaveService.StartAutoSave(SaveCallback, intervalSeconds: 1);
        _autoSaveService.IsDirty = true;

        // Wait 1.5 seconds (should trigger with 1 second interval)
        await Task.Delay(1500);

        // Assert
        saveCount.Should().BeGreaterThan(0);
        _autoSaveService.IntervalSeconds.Should().Be(1);
    }

    public void Dispose()
    {
        _autoSaveService?.Dispose();
    }
}

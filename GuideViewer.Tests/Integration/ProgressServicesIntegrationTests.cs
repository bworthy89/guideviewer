using FluentAssertions;
using GuideViewer.Core.Services;
using GuideViewer.Data.Entities;
using GuideViewer.Data.Repositories;
using GuideViewer.Data.Services;
using LiteDB;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace GuideViewer.Tests.Integration;

/// <summary>
/// Integration tests for Progress services layer (ProgressTrackingService + TimerService).
/// Tests end-to-end workflows with real database interactions.
/// </summary>
public class ProgressServicesIntegrationTests : IDisposable
{
    private readonly string _testDatabasePath;
    private readonly DatabaseService _databaseService;
    private readonly ProgressRepository _progressRepository;
    private readonly GuideRepository _guideRepository;
    private readonly UserRepository _userRepository;
    private readonly ProgressTrackingService _progressTrackingService;

    public ProgressServicesIntegrationTests()
    {
        _testDatabasePath = Path.Combine(Path.GetTempPath(), $"test_progress_services_{Guid.NewGuid()}.db");
        _databaseService = new DatabaseService(_testDatabasePath);
        _progressRepository = new ProgressRepository(_databaseService);
        _guideRepository = new GuideRepository(_databaseService);
        _userRepository = new UserRepository(_databaseService);
        _progressTrackingService = new ProgressTrackingService(_progressRepository, _guideRepository);
    }

    [Fact]
    public async Task CompleteGuideWorkflow_StartTrackCompleteSteps_ShouldWorkEndToEnd()
    {
        // Arrange
        var user = CreateTestUser();
        var guide = CreateTestGuide(stepCount: 5, estimatedMinutes: 50);

        // Act 1: Start guide
        var progress = await _progressTrackingService.StartGuideAsync(guide.Id, user.Id);

        // Assert 1: Progress created
        progress.Should().NotBeNull();
        progress.CurrentStepOrder.Should().Be(1);
        progress.CompletedStepOrders.Should().BeEmpty();

        // Act 2: Complete steps 1, 2, 3
        await _progressTrackingService.CompleteStepAsync(progress.Id, 1, true);
        await _progressTrackingService.CompleteStepAsync(progress.Id, 2, true);
        await _progressTrackingService.CompleteStepAsync(progress.Id, 3, true);

        // Assert 2: Steps marked complete
        var updatedProgress = await _progressTrackingService.GetProgressAsync(guide.Id, user.Id);
        updatedProgress!.CompletedStepOrders.Should().HaveCount(3);
        updatedProgress.CompletedStepOrders.Should().Contain(new[] { 1, 2, 3 });

        // Act 3: Update current step to 4
        await _progressTrackingService.UpdateCurrentStepAsync(progress.Id, 4);

        // Assert 3: Current step updated
        updatedProgress = await _progressTrackingService.GetProgressAsync(guide.Id, user.Id);
        updatedProgress!.CurrentStepOrder.Should().Be(4);

        // Act 4: Complete remaining steps and finish
        await _progressTrackingService.CompleteStepAsync(progress.Id, 4, true);
        await _progressTrackingService.CompleteStepAsync(progress.Id, 5, true);
        await _progressTrackingService.MarkGuideCompleteAsync(progress.Id);

        // Assert 4: Guide marked complete
        var completedProgress = await _progressTrackingService.GetProgressAsync(guide.Id, user.Id);
        completedProgress!.CompletedAt.Should().NotBeNull();
        completedProgress.CompletedStepOrders.Should().HaveCount(5);
    }

    [Fact]
    public async Task NonLinearStepCompletion_SkipAndBacktrack_ShouldWork()
    {
        // Arrange
        var user = CreateTestUser();
        var guide = CreateTestGuide(stepCount: 5);
        var progress = await _progressTrackingService.StartGuideAsync(guide.Id, user.Id);

        // Act - Complete steps in non-linear order: 1, 3, 5, 2, 4
        await _progressTrackingService.CompleteStepAsync(progress.Id, 1, true);
        await _progressTrackingService.CompleteStepAsync(progress.Id, 3, true);
        await _progressTrackingService.CompleteStepAsync(progress.Id, 5, true);
        await _progressTrackingService.CompleteStepAsync(progress.Id, 2, true);
        await _progressTrackingService.CompleteStepAsync(progress.Id, 4, true);

        // Assert - All steps completed regardless of order
        var updatedProgress = await _progressTrackingService.GetProgressAsync(guide.Id, user.Id);
        updatedProgress!.CompletedStepOrders.Should().HaveCount(5);
        updatedProgress.CompletedStepOrders.Should().Contain(new[] { 1, 2, 3, 4, 5 });
    }

    [Fact]
    public async Task EstimatedTimeRemaining_WithPartialCompletion_CalculatesCorrectly()
    {
        // Arrange
        var user = CreateTestUser();
        var guide = CreateTestGuide(stepCount: 10, estimatedMinutes: 100);
        var progress = await _progressTrackingService.StartGuideAsync(guide.Id, user.Id);

        // Act 1: No steps completed
        var estimatedTime1 = _progressTrackingService.CalculateEstimatedTimeRemaining(progress, guide);

        // Assert 1: Full time remaining
        estimatedTime1.Should().Be(100);

        // Act 2: Complete 3 out of 10 steps (30%)
        await _progressTrackingService.CompleteStepAsync(progress.Id, 1, true);
        await _progressTrackingService.CompleteStepAsync(progress.Id, 2, true);
        await _progressTrackingService.CompleteStepAsync(progress.Id, 3, true);

        var updatedProgress = await _progressTrackingService.GetProgressAsync(guide.Id, user.Id);
        var estimatedTime2 = _progressTrackingService.CalculateEstimatedTimeRemaining(updatedProgress!, guide);

        // Assert 2: 70% time remaining (70 minutes)
        estimatedTime2.Should().Be(70);

        // Act 3: Complete all 10 steps
        for (int i = 4; i <= 10; i++)
        {
            await _progressTrackingService.CompleteStepAsync(progress.Id, i, true);
        }

        updatedProgress = await _progressTrackingService.GetProgressAsync(guide.Id, user.Id);
        var estimatedTime3 = _progressTrackingService.CalculateEstimatedTimeRemaining(updatedProgress!, guide);

        // Assert 3: No time remaining
        estimatedTime3.Should().Be(0);
    }

    [Fact]
    public async Task ActiveTimeTracking_WithTimerService_TracksAccurately()
    {
        // Arrange
        var user = CreateTestUser();
        var guide = CreateTestGuide();
        var progress = await _progressTrackingService.StartGuideAsync(guide.Id, user.Id);
        using var timer = new TimerService();

        // Act 1: Simulate user working for ~2 seconds
        timer.Start();
        await Task.Delay(2100);
        timer.Stop();

        var elapsedSeconds = (int)timer.Elapsed.TotalSeconds;

        // Act 2: Update active time
        await _progressTrackingService.UpdateActiveTimeAsync(progress.Id, elapsedSeconds);

        // Assert: Active time recorded
        var updatedProgress = await _progressTrackingService.GetProgressAsync(guide.Id, user.Id);
        updatedProgress!.TotalActiveTimeSeconds.Should().BeGreaterThanOrEqualTo(2);
        updatedProgress.TotalActiveTimeSeconds.Should().BeLessThan(4); // Allow for variance
    }

    [Fact]
    public async Task MultipleActiveGuides_WithStatistics_TracksCorrectly()
    {
        // Arrange
        var user = CreateTestUser();
        var guide1 = CreateTestGuide("Guide 1");
        var guide2 = CreateTestGuide("Guide 2");
        var guide3 = CreateTestGuide("Guide 3");

        // Act - Start 3 guides, complete 1
        var progress1 = await _progressTrackingService.StartGuideAsync(guide1.Id, user.Id);
        var progress2 = await _progressTrackingService.StartGuideAsync(guide2.Id, user.Id);
        var progress3 = await _progressTrackingService.StartGuideAsync(guide3.Id, user.Id);

        // Complete guide1
        for (int i = 1; i <= guide1.Steps.Count; i++)
        {
            await _progressTrackingService.CompleteStepAsync(progress1.Id, i, true);
        }
        await _progressTrackingService.MarkGuideCompleteAsync(progress1.Id);

        // Get statistics
        var statistics = await _progressTrackingService.GetStatisticsAsync(user.Id);

        // Assert
        statistics.TotalStarted.Should().Be(3);
        statistics.TotalCompleted.Should().Be(1);
        statistics.CurrentlyInProgress.Should().Be(2);
        statistics.CompletionRate.Should().BeApproximately(33.33, 0.1);
    }

    [Fact]
    public async Task StepCompletionWithNotes_StoresAndRetrieves()
    {
        // Arrange
        var user = CreateTestUser();
        var guide = CreateTestGuide(stepCount: 3);
        var progress = await _progressTrackingService.StartGuideAsync(guide.Id, user.Id);
        var notes = "Encountered issue with Step 1. Resolved by restarting service.";

        // Act
        await _progressTrackingService.CompleteStepAsync(progress.Id, 1, true, notes);

        // Assert
        var updatedProgress = await _progressTrackingService.GetProgressAsync(guide.Id, user.Id);
        updatedProgress!.Notes.Should().Be(notes);
    }

    [Fact]
    public async Task DuplicateProgressPrevention_ThrowsException()
    {
        // Arrange
        var user = CreateTestUser();
        var guide = CreateTestGuide();

        // Act 1: Start guide (should succeed)
        var progress1 = await _progressTrackingService.StartGuideAsync(guide.Id, user.Id);
        progress1.Should().NotBeNull();

        // Act 2: Try to start same guide again (should fail)
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _progressTrackingService.StartGuideAsync(guide.Id, user.Id));

        // Assert
        exception.Message.Should().Contain("already exists");
    }

    [Fact]
    public async Task TimerService_MultipleStartStopCycles_AccumulatesTime()
    {
        // Arrange
        using var timer = new TimerService();

        // Act - Multiple work sessions
        timer.Start();
        await Task.Delay(500);
        timer.Stop();
        var elapsed1 = timer.Elapsed;

        timer.Start();
        await Task.Delay(500);
        timer.Stop();
        var elapsed2 = timer.Elapsed;

        timer.Start();
        await Task.Delay(500);
        timer.Stop();
        var totalElapsed = timer.Elapsed;

        // Assert - Time should accumulate across sessions
        elapsed2.Should().BeGreaterThan(elapsed1);
        totalElapsed.Should().BeGreaterThan(elapsed2);
        totalElapsed.TotalSeconds.Should().BeGreaterThanOrEqualTo(1.4); // ~1.5 seconds total
    }

    // Helper methods

    private User CreateTestUser(string email = "test@example.com")
    {
        var user = new User
        {
            ProductKey = $"TEST-{Guid.NewGuid().ToString().Substring(0, 4)}",
            Role = "TECHNICIAN",
            ActivatedAt = DateTime.UtcNow,
            LastLogin = DateTime.UtcNow
        };
        user.Id = _userRepository.Insert(user);
        return user;
    }

    private Guide CreateTestGuide(string title = "Test Guide", int stepCount = 3, int estimatedMinutes = 30)
    {
        var guide = new Guide
        {
            Title = title,
            Description = "Test Description",
            Category = "Test Category",
            EstimatedMinutes = estimatedMinutes,
            CreatedBy = "Test User",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        for (int i = 1; i <= stepCount; i++)
        {
            guide.Steps.Add(new Step
            {
                Order = i,
                Title = $"Step {i}",
                Content = $"{{\\rtf1 Step {i} content}}",
                CreatedAt = DateTime.UtcNow
            });
        }

        guide.Id = _guideRepository.Insert(guide);
        return guide;
    }

    public void Dispose()
    {
        _databaseService?.Dispose();
        if (File.Exists(_testDatabasePath))
        {
            File.Delete(_testDatabasePath);
        }
    }
}

using FluentAssertions;
using GuideViewer.Core.Services;
using GuideViewer.Data.Entities;
using GuideViewer.Data.Repositories;
using GuideViewer.Data.Services;
using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace GuideViewer.Tests.Integration;

/// <summary>
/// Integration tests for complete progress tracking workflows.
/// Tests end-to-end scenarios from starting a guide to completion.
/// </summary>
public class ProgressTrackingWorkflowIntegrationTests : IDisposable
{
    private readonly string _testDatabasePath;
    private readonly DatabaseService _databaseService;
    private readonly GuideRepository _guideRepository;
    private readonly UserRepository _userRepository;
    private readonly ProgressRepository _progressRepository;
    private readonly IProgressTrackingService _progressTrackingService;

    public ProgressTrackingWorkflowIntegrationTests()
    {
        _testDatabasePath = Path.Combine(Path.GetTempPath(), $"test_progress_workflow_{Guid.NewGuid()}.db");
        _databaseService = new DatabaseService(_testDatabasePath);
        _guideRepository = new GuideRepository(_databaseService);
        _userRepository = new UserRepository(_databaseService);
        _progressRepository = new ProgressRepository(_databaseService);
        _progressTrackingService = new ProgressTrackingService(_progressRepository, _guideRepository);
    }

    [Fact]
    public async Task CompleteProgressWorkflow_StartToFinish_Success()
    {
        // Arrange - Create user and guide
        var user = new User { ProductKey = "TEST-KEY", Role = "Technician" };
        var userId = _userRepository.Insert(user);

        var guide = CreateTestGuide("Complete Workflow Guide", 5);
        var guideId = _guideRepository.Insert(guide);

        // Act & Assert - Start guide
        var progress = await _progressTrackingService.StartGuideAsync(guideId, userId);
        progress.Should().NotBeNull();
        progress.GuideId.Should().Be(guideId);
        progress.UserId.Should().Be(userId);
        progress.CurrentStepOrder.Should().Be(1);
        progress.CompletedStepOrders.Should().BeEmpty();
        progress.CompletedAt.Should().BeNull();

        // Act & Assert - Complete each step
        for (int stepOrder = 1; stepOrder <= 5; stepOrder++)
        {
            var completed = await _progressTrackingService.CompleteStepAsync(progress.Id, stepOrder, true, $"Notes for step {stepOrder}");
            completed.Should().BeTrue();

            // Update current step
            if (stepOrder < 5)
            {
                await _progressTrackingService.UpdateCurrentStepAsync(progress.Id, stepOrder + 1);
            }
        }

        // Verify all steps completed
        var updatedProgress = await _progressTrackingService.GetProgressAsync(guideId, userId);
        updatedProgress.Should().NotBeNull();
        updatedProgress!.CompletedStepOrders.Should().HaveCount(5);
        updatedProgress.CompletedStepOrders.Should().Contain(new[] { 1, 2, 3, 4, 5 });

        // Act & Assert - Finish guide
        var finished = await _progressTrackingService.MarkGuideCompleteAsync(progress.Id);
        finished.Should().BeTrue();

        var finalProgress = await _progressTrackingService.GetProgressAsync(guideId, userId);
        finalProgress.Should().NotBeNull();
        finalProgress!.CompletedAt.Should().NotBeNull();
        finalProgress.CompletedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task ResumeProgressWorkflow_PauseAndResume_Success()
    {
        // Arrange
        var user = new User { ProductKey = "TEST-KEY", Role = "Technician" };
        var userId = _userRepository.Insert(user);

        var guide = CreateTestGuide("Resume Test Guide", 4);
        var guideId = _guideRepository.Insert(guide);

        // Act - Start and complete 2 steps
        var progress = await _progressTrackingService.StartGuideAsync(guideId, userId);
        await _progressTrackingService.CompleteStepAsync(progress.Id, 1, true);
        await _progressTrackingService.CompleteStepAsync(progress.Id, 2, true);
        await _progressTrackingService.UpdateCurrentStepAsync(progress.Id, 3);

        // Simulate pause (just leave it)
        var lastAccessedBefore = progress.LastAccessedAt;
        await Task.Delay(100); // Small delay to ensure timestamp difference

        // Act - Resume (get existing progress)
        var resumedProgress = await _progressTrackingService.GetProgressAsync(guideId, userId);

        // Assert
        resumedProgress.Should().NotBeNull();
        resumedProgress!.Id.Should().Be(progress.Id);
        resumedProgress.CompletedStepOrders.Should().HaveCount(2);
        resumedProgress.CurrentStepOrder.Should().Be(3);
        resumedProgress.CompletedAt.Should().BeNull();

        // Continue and finish
        await _progressTrackingService.CompleteStepAsync(resumedProgress.Id, 3, true);
        await _progressTrackingService.CompleteStepAsync(resumedProgress.Id, 4, true);
        await _progressTrackingService.MarkGuideCompleteAsync(resumedProgress.Id);

        var finalProgress = await _progressTrackingService.GetProgressAsync(guideId, userId);
        finalProgress!.CompletedAt.Should().NotBeNull();
        finalProgress.CompletedStepOrders.Should().HaveCount(4);
    }

    [Fact]
    public async Task MultipleUsers_SameGuide_IndependentProgress()
    {
        // Arrange - Create two users
        var user1 = new User { ProductKey = "USER1-KEY", Role = "Technician" };
        var user2 = new User { ProductKey = "USER2-KEY", Role = "Technician" };
        var userId1 = _userRepository.Insert(user1);
        var userId2 = _userRepository.Insert(user2);

        var guide = CreateTestGuide("Multi-User Guide", 3);
        var guideId = _guideRepository.Insert(guide);

        // Act - User 1 starts and completes step 1
        var progress1 = await _progressTrackingService.StartGuideAsync(guideId, userId1);
        await _progressTrackingService.CompleteStepAsync(progress1.Id, 1, true);

        // Act - User 2 starts and completes steps 1 and 2
        var progress2 = await _progressTrackingService.StartGuideAsync(guideId, userId2);
        await _progressTrackingService.CompleteStepAsync(progress2.Id, 1, true);
        await _progressTrackingService.CompleteStepAsync(progress2.Id, 2, true);

        // Assert - Independent progress
        var user1Progress = await _progressTrackingService.GetProgressAsync(guideId, userId1);
        var user2Progress = await _progressTrackingService.GetProgressAsync(guideId, userId2);

        user1Progress.Should().NotBeNull();
        user2Progress.Should().NotBeNull();

        user1Progress!.Id.Should().NotBe(user2Progress!.Id);
        user1Progress.CompletedStepOrders.Should().HaveCount(1);
        user2Progress.CompletedStepOrders.Should().HaveCount(2);
    }

    [Fact]
    public async Task StepCompletion_ToggleCompleteAndUncomplete_Success()
    {
        // Arrange
        var user = new User { ProductKey = "TEST-KEY", Role = "Technician" };
        var userId = _userRepository.Insert(user);

        var guide = CreateTestGuide("Toggle Test Guide", 3);
        var guideId = _guideRepository.Insert(guide);

        var progress = await _progressTrackingService.StartGuideAsync(guideId, userId);

        // Act - Complete step 1
        await _progressTrackingService.CompleteStepAsync(progress.Id, 1, true);
        var afterComplete = await _progressTrackingService.GetProgressAsync(guideId, userId);
        afterComplete!.CompletedStepOrders.Should().Contain(1);

        // Act - Uncomplete step 1
        await _progressTrackingService.CompleteStepAsync(progress.Id, 1, false);
        var afterUncomplete = await _progressTrackingService.GetProgressAsync(guideId, userId);
        afterUncomplete!.CompletedStepOrders.Should().NotContain(1);

        // Act - Complete again
        await _progressTrackingService.CompleteStepAsync(progress.Id, 1, true);
        var afterReComplete = await _progressTrackingService.GetProgressAsync(guideId, userId);
        afterReComplete!.CompletedStepOrders.Should().Contain(1);
    }

    [Fact]
    public async Task Statistics_CalculateAccurately_Success()
    {
        // Arrange - Create user
        var user = new User { ProductKey = "TEST-KEY", Role = "Technician" };
        var userId = _userRepository.Insert(user);

        // Create 3 guides
        var guide1 = CreateTestGuide("Guide 1", 2);
        var guide2 = CreateTestGuide("Guide 2", 2);
        var guide3 = CreateTestGuide("Guide 3", 2);

        var guideId1 = _guideRepository.Insert(guide1);
        var guideId2 = _guideRepository.Insert(guide2);
        var guideId3 = _guideRepository.Insert(guide3);

        // Start all 3 guides
        var progress1 = await _progressTrackingService.StartGuideAsync(guideId1, userId);
        var progress2 = await _progressTrackingService.StartGuideAsync(guideId2, userId);
        var progress3 = await _progressTrackingService.StartGuideAsync(guideId3, userId);

        // Complete guide 1
        await _progressTrackingService.CompleteStepAsync(progress1.Id, 1, true);
        await _progressTrackingService.CompleteStepAsync(progress1.Id, 2, true);
        await _progressTrackingService.MarkGuideCompleteAsync(progress1.Id);

        // Partially complete guide 2
        await _progressTrackingService.CompleteStepAsync(progress2.Id, 1, true);

        // Leave guide 3 with no steps completed

        // Act - Get statistics
        var stats = await _progressTrackingService.GetStatisticsAsync(userId);

        // Assert
        stats.Should().NotBeNull();
        stats.TotalStarted.Should().Be(3);
        stats.TotalCompleted.Should().Be(1);
        stats.CurrentlyInProgress.Should().Be(2);
        stats.CompletionRate.Should().BeApproximately(33.33, 0.1); // 1/3 = 33.33%
    }

    [Fact]
    public async Task EdgeCase_InvalidStepOrder_ThrowsException()
    {
        // Arrange
        var user = new User { ProductKey = "TEST-KEY", Role = "Technician" };
        var userId = _userRepository.Insert(user);

        var guide = CreateTestGuide("Edge Case Guide", 3);
        var guideId = _guideRepository.Insert(guide);

        var progress = await _progressTrackingService.StartGuideAsync(guideId, userId);

        // Act & Assert - Invalid step order (too high)
        var act = async () => await _progressTrackingService.CompleteStepAsync(progress.Id, 999, true);
        await act.Should().ThrowAsync<ArgumentException>();

        // Act & Assert - Invalid step order (zero)
        var act2 = async () => await _progressTrackingService.CompleteStepAsync(progress.Id, 0, true);
        await act2.Should().ThrowAsync<ArgumentException>();

        // Act & Assert - Invalid step order (negative)
        var act3 = async () => await _progressTrackingService.CompleteStepAsync(progress.Id, -1, true);
        await act3.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task EdgeCase_DuplicateStart_ThrowsException()
    {
        // Arrange
        var user = new User { ProductKey = "TEST-KEY", Role = "Technician" };
        var userId = _userRepository.Insert(user);

        var guide = CreateTestGuide("Duplicate Test Guide", 2);
        var guideId = _guideRepository.Insert(guide);

        // Act - Start guide first time
        await _progressTrackingService.StartGuideAsync(guideId, userId);

        // Act & Assert - Try to start again
        var act = async () => await _progressTrackingService.StartGuideAsync(guideId, userId);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task EdgeCase_CompleteAlreadyFinishedGuide_Fails()
    {
        // Arrange
        var user = new User { ProductKey = "TEST-KEY", Role = "Technician" };
        var userId = _userRepository.Insert(user);

        var guide = CreateTestGuide("Already Complete Guide", 2);
        var guideId = _guideRepository.Insert(guide);

        var progress = await _progressTrackingService.StartGuideAsync(guideId, userId);
        await _progressTrackingService.CompleteStepAsync(progress.Id, 1, true);
        await _progressTrackingService.CompleteStepAsync(progress.Id, 2, true);
        await _progressTrackingService.MarkGuideCompleteAsync(progress.Id);

        // Act & Assert - Try to complete step on finished guide (should still work for corrections)
        var result = await _progressTrackingService.CompleteStepAsync(progress.Id, 1, false);
        result.Should().BeTrue(); // Should allow uncompleting even after guide is marked complete
    }

    [Fact]
    public async Task ActiveProgress_ReturnsOnlyInProgressGuides()
    {
        // Arrange
        var user = new User { ProductKey = "TEST-KEY", Role = "Technician" };
        var userId = _userRepository.Insert(user);

        var guide1 = CreateTestGuide("Active Guide", 2);
        var guide2 = CreateTestGuide("Completed Guide", 2);

        var guideId1 = _guideRepository.Insert(guide1);
        var guideId2 = _guideRepository.Insert(guide2);

        // Start both
        var progress1 = await _progressTrackingService.StartGuideAsync(guideId1, userId);
        var progress2 = await _progressTrackingService.StartGuideAsync(guideId2, userId);

        // Complete guide 2
        await _progressTrackingService.CompleteStepAsync(progress2.Id, 1, true);
        await _progressTrackingService.CompleteStepAsync(progress2.Id, 2, true);
        await _progressTrackingService.MarkGuideCompleteAsync(progress2.Id);

        // Act
        var activeProgress = await _progressTrackingService.GetActiveProgressAsync(userId);
        var completedProgress = await _progressTrackingService.GetCompletedProgressAsync(userId);

        // Assert
        activeProgress.Should().ContainSingle();
        activeProgress.First().Id.Should().Be(progress1.Id);

        completedProgress.Should().ContainSingle();
        completedProgress.First().Id.Should().Be(progress2.Id);
    }

    [Fact]
    public async Task NonLinearCompletion_SkipAndBacktrack_Success()
    {
        // Arrange
        var user = new User { ProductKey = "TEST-KEY", Role = "Technician" };
        var userId = _userRepository.Insert(user);

        var guide = CreateTestGuide("Non-Linear Guide", 5);
        var guideId = _guideRepository.Insert(guide);

        var progress = await _progressTrackingService.StartGuideAsync(guideId, userId);

        // Act - Complete steps in non-linear order: 1, 3, 5, 2, 4
        await _progressTrackingService.CompleteStepAsync(progress.Id, 1, true);
        await _progressTrackingService.CompleteStepAsync(progress.Id, 3, true);
        await _progressTrackingService.CompleteStepAsync(progress.Id, 5, true);
        await _progressTrackingService.CompleteStepAsync(progress.Id, 2, true);
        await _progressTrackingService.CompleteStepAsync(progress.Id, 4, true);

        // Assert
        var updatedProgress = await _progressTrackingService.GetProgressAsync(guideId, userId);
        updatedProgress!.CompletedStepOrders.Should().HaveCount(5);
        updatedProgress.CompletedStepOrders.Should().Contain(new[] { 1, 2, 3, 4, 5 });
    }

    private Guide CreateTestGuide(string title, int stepCount)
    {
        var guide = new Guide
        {
            Title = title,
            Description = $"Test guide with {stepCount} steps",
            Category = "Test Category",
            EstimatedMinutes = stepCount * 10
        };

        for (int i = 1; i <= stepCount; i++)
        {
            guide.Steps.Add(new Step
            {
                Order = i,
                Title = $"Step {i}",
                Content = $"Instructions for step {i}"
            });
        }

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

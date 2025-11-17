using FluentAssertions;
using GuideViewer.Data.Entities;
using GuideViewer.Data.Repositories;
using GuideViewer.Data.Services;
using LiteDB;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace GuideViewer.Tests.Integration;

/// <summary>
/// Integration tests for Progress data layer including cross-repository interactions.
/// </summary>
public class ProgressDataLayerIntegrationTests : IDisposable
{
    private readonly string _testDatabasePath;
    private readonly DatabaseService _databaseService;
    private readonly ProgressRepository _progressRepository;
    private readonly GuideRepository _guideRepository;
    private readonly UserRepository _userRepository;

    public ProgressDataLayerIntegrationTests()
    {
        _testDatabasePath = Path.Combine(Path.GetTempPath(), $"test_progress_integration_{Guid.NewGuid()}.db");
        _databaseService = new DatabaseService(_testDatabasePath);
        _progressRepository = new ProgressRepository(_databaseService);
        _guideRepository = new GuideRepository(_databaseService);
        _userRepository = new UserRepository(_databaseService);
    }

    [Fact]
    public void CompleteProgressWorkflow_CreateCompleteStepsFinish_ShouldWorkEndToEnd()
    {
        // Arrange - Create real user and guide
        var user = CreateTestUser();
        var guide = CreateTestGuide(stepCount: 5);

        // Act 1: Start progress
        var progress = new Progress
        {
            UserId = user.Id,
            GuideId = guide.Id,
            CurrentStepOrder = 1,
            StartedAt = DateTime.UtcNow,
            LastAccessedAt = DateTime.UtcNow
        };
        var progressId = _progressRepository.Insert(progress);
        progressId.Should().NotBe(ObjectId.Empty);

        // Act 2: Complete steps 1, 2, and 3
        _progressRepository.UpdateStepCompletion(progressId, 1, true);
        _progressRepository.UpdateStepCompletion(progressId, 2, true);
        _progressRepository.UpdateStepCompletion(progressId, 3, true);
        _progressRepository.UpdateCurrentStep(progressId, 4);

        // Assert 2: Verify progress state
        var midProgress = _progressRepository.GetById(progressId);
        midProgress!.CompletedStepOrders.Should().HaveCount(3);
        midProgress.CompletedStepOrders.Should().Contain(new[] { 1, 2, 3 });
        midProgress.CurrentStepOrder.Should().Be(4);

        // Act 3: Complete remaining steps and finish
        _progressRepository.UpdateStepCompletion(progressId, 4, true);
        _progressRepository.UpdateStepCompletion(progressId, 5, true);
        _progressRepository.MarkGuideComplete(progressId);

        // Assert 3: Verify completion
        var completedProgress = _progressRepository.GetById(progressId);
        completedProgress!.CompletedStepOrders.Should().HaveCount(5);
        completedProgress.CompletedAt.Should().NotBeNull();
        completedProgress.CompletedAt.Should().BeOnOrAfter(completedProgress.StartedAt);
    }

    [Fact]
    public void MultipleUsers_TrackingSameGuide_ShouldHaveSeparateProgress()
    {
        // Arrange
        var user1 = CreateTestUser("user1@test.com");
        var user2 = CreateTestUser("user2@test.com");
        var guide = CreateTestGuide();

        // Act - Both users start the same guide
        var progress1 = CreateTestProgress(user1.Id, guide.Id);
        var progress2 = CreateTestProgress(user2.Id, guide.Id);

        // Mark different steps complete for each user
        _progressRepository.UpdateStepCompletion(progress1.Id, 1, true);
        _progressRepository.UpdateStepCompletion(progress1.Id, 2, true);

        _progressRepository.UpdateStepCompletion(progress2.Id, 1, true);

        // Assert - Each user has independent progress
        var user1Progress = _progressRepository.GetByUserAndGuide(user1.Id, guide.Id);
        var user2Progress = _progressRepository.GetByUserAndGuide(user2.Id, guide.Id);

        user1Progress!.CompletedStepOrders.Should().HaveCount(2);
        user2Progress!.CompletedStepOrders.Should().ContainSingle();

        // Assert - Admin can see all progress for the guide
        var allProgress = _progressRepository.GetAllProgressForGuide(guide.Id).ToList();
        allProgress.Should().HaveCount(2);
    }

    [Fact]
    public void DuplicateProgress_SameUserAndGuide_ShouldFail()
    {
        // Arrange
        var user = CreateTestUser();
        var guide = CreateTestGuide();

        // Act 1: Create first progress
        CreateTestProgress(user.Id, guide.Id);

        // Act 2: Try to create duplicate
        var duplicateProgress = new Progress
        {
            UserId = user.Id,
            GuideId = guide.Id,
            CurrentStepOrder = 1,
            StartedAt = DateTime.UtcNow,
            LastAccessedAt = DateTime.UtcNow
        };

        // Assert - Should fail with unique constraint violation
        var exception = Assert.Throws<LiteException>(() => _progressRepository.Insert(duplicateProgress));
        exception.Message.Should().Contain("duplicate");
    }

    [Fact]
    public void ProgressWithLongNotes_ShouldStoreAndRetrieve()
    {
        // Arrange
        var user = CreateTestUser();
        var guide = CreateTestGuide();
        var progress = CreateTestProgress(user.Id, guide.Id);

        // Act - Add notes up to 5000 characters
        var longNotes = new string('A', 5000);
        progress.Notes = longNotes;
        _progressRepository.Update(progress);

        // Assert
        var retrieved = _progressRepository.GetById(progress.Id);
        retrieved!.Notes.Should().HaveLength(5000);
        retrieved.Notes.Should().Be(longNotes);
    }

    [Fact]
    public void QueryPerformance_With100ProgressRecords_ShouldBeFast()
    {
        // Arrange - Create 100 progress records for same user
        var user = CreateTestUser();
        var guides = Enumerable.Range(1, 100)
            .Select(i => CreateTestGuide($"Guide {i}"))
            .ToList();

        foreach (var guide in guides)
        {
            CreateTestProgress(user.Id, guide.Id);
        }

        // Act & Assert - GetActiveByUser should be fast
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var activeProgress = _progressRepository.GetActiveByUser(user.Id).ToList();
        stopwatch.Stop();

        activeProgress.Should().HaveCount(100);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100, "Query should complete in under 100ms");
    }

    [Fact]
    public void Statistics_WithMultipleProgressStates_ShouldCalculateCorrectly()
    {
        // Arrange
        var user = CreateTestUser();
        var guide1 = CreateTestGuide("Guide 1");
        var guide2 = CreateTestGuide("Guide 2");
        var guide3 = CreateTestGuide("Guide 3");
        var guide4 = CreateTestGuide("Guide 4");

        // Create progress in different states
        var progress1 = CreateTestProgress(user.Id, guide1.Id);
        progress1.StartedAt = DateTime.UtcNow.AddHours(-2);
        progress1.CompletedAt = DateTime.UtcNow.AddHours(-1); // Took 1 hour
        _progressRepository.Update(progress1);

        var progress2 = CreateTestProgress(user.Id, guide2.Id);
        progress2.StartedAt = DateTime.UtcNow.AddHours(-4);
        progress2.CompletedAt = DateTime.UtcNow.AddHours(-1); // Took 3 hours
        _progressRepository.Update(progress2);

        var progress3 = CreateTestProgress(user.Id, guide3.Id); // In progress
        var progress4 = CreateTestProgress(user.Id, guide4.Id); // In progress

        // Act
        var statistics = _progressRepository.GetStatistics(user.Id);

        // Assert
        statistics.TotalStarted.Should().Be(4);
        statistics.TotalCompleted.Should().Be(2);
        statistics.CurrentlyInProgress.Should().Be(2);
        statistics.AverageCompletionTimeMinutes.Should().BeApproximately(120, 1); // Average of 60 and 180 minutes
        statistics.CompletionRate.Should().BeApproximately(50, 0.1); // 2 out of 4 = 50%
    }

    [Fact]
    public void ProgressPersistence_AfterDatabaseRestart_ShouldRetainData()
    {
        // Arrange
        var user = CreateTestUser();
        var guide = CreateTestGuide();
        var progress = CreateTestProgress(user.Id, guide.Id);
        _progressRepository.UpdateStepCompletion(progress.Id, 1, true);
        _progressRepository.UpdateStepCompletion(progress.Id, 2, true);

        var originalProgressId = progress.Id;

        // Act - Simulate database restart by disposing and recreating
        _databaseService.Dispose();
        var newDatabaseService = new DatabaseService(_testDatabasePath);
        var newProgressRepository = new ProgressRepository(newDatabaseService);

        // Assert - Progress should still exist with same data
        var retrievedProgress = newProgressRepository.GetById(originalProgressId);
        retrievedProgress.Should().NotBeNull();
        retrievedProgress!.CompletedStepOrders.Should().HaveCount(2);
        retrievedProgress.CompletedStepOrders.Should().Contain(new[] { 1, 2 });

        // Cleanup
        newDatabaseService.Dispose();
    }

    [Fact]
    public void IndexPerformance_CompoundIndex_ShouldEnforceUniqueness()
    {
        // Arrange
        var user = CreateTestUser();
        var guide1 = CreateTestGuide("Guide 1");
        var guide2 = CreateTestGuide("Guide 2");

        // Act - Create progress for same user with different guides (should work)
        var progress1 = CreateTestProgress(user.Id, guide1.Id);
        var progress2 = CreateTestProgress(user.Id, guide2.Id);

        progress1.Should().NotBeNull();
        progress2.Should().NotBeNull();

        // Assert - Trying to create duplicate for same user+guide should fail
        var duplicateProgress = new Progress
        {
            UserId = user.Id,
            GuideId = guide1.Id,
            CurrentStepOrder = 1,
            StartedAt = DateTime.UtcNow,
            LastAccessedAt = DateTime.UtcNow
        };

        var exception = Assert.Throws<LiteException>(() => _progressRepository.Insert(duplicateProgress));
        exception.Message.Should().Contain("duplicate");
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

    private Guide CreateTestGuide(string title = "Test Guide", int stepCount = 3)
    {
        var guide = new Guide
        {
            Title = title,
            Description = "Test Description",
            Category = "Test Category",
            EstimatedMinutes = 30,
            CreatedBy = "Test User",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Add some steps
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

    private Progress CreateTestProgress(ObjectId userId, ObjectId guideId)
    {
        var progress = new Progress
        {
            UserId = userId,
            GuideId = guideId,
            CurrentStepOrder = 1,
            StartedAt = DateTime.UtcNow,
            LastAccessedAt = DateTime.UtcNow
        };
        progress.Id = _progressRepository.Insert(progress);
        return progress;
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

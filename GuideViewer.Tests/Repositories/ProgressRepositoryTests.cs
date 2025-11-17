using FluentAssertions;
using GuideViewer.Data.Entities;
using GuideViewer.Data.Repositories;
using GuideViewer.Data.Services;
using LiteDB;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace GuideViewer.Tests.Repositories;

/// <summary>
/// Unit tests for ProgressRepository.
/// </summary>
public class ProgressRepositoryTests : IDisposable
{
    private readonly string _testDatabasePath;
    private readonly DatabaseService _databaseService;
    private readonly ProgressRepository _progressRepository;
    private readonly UserRepository _userRepository;
    private readonly GuideRepository _guideRepository;

    public ProgressRepositoryTests()
    {
        _testDatabasePath = Path.Combine(Path.GetTempPath(), $"test_progress_{Guid.NewGuid()}.db");
        _databaseService = new DatabaseService(_testDatabasePath);
        _progressRepository = new ProgressRepository(_databaseService);
        _userRepository = new UserRepository(_databaseService);
        _guideRepository = new GuideRepository(_databaseService);
    }

    [Fact]
    public void Insert_ValidProgress_ReturnsId()
    {
        // Arrange
        var user = CreateTestUser();
        var guide = CreateTestGuide();
        var progress = new Progress
        {
            UserId = user.Id,
            GuideId = guide.Id,
            CurrentStepOrder = 1,
            StartedAt = DateTime.UtcNow,
            LastAccessedAt = DateTime.UtcNow
        };

        // Act
        var progressId = _progressRepository.Insert(progress);

        // Assert
        progressId.Should().NotBe(ObjectId.Empty);
        progress.Id.Should().Be(progressId);
    }

    [Fact]
    public void GetByUserAndGuide_WithValidIds_ReturnsProgress()
    {
        // Arrange
        var user = CreateTestUser();
        var guide = CreateTestGuide();
        var progress = CreateTestProgress(user.Id, guide.Id);

        // Act
        var result = _progressRepository.GetByUserAndGuide(user.Id, guide.Id);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(user.Id);
        result.GuideId.Should().Be(guide.Id);
    }

    [Fact]
    public void GetByUserAndGuide_WithInvalidIds_ReturnsNull()
    {
        // Arrange
        var invalidUserId = ObjectId.NewObjectId();
        var invalidGuideId = ObjectId.NewObjectId();

        // Act
        var result = _progressRepository.GetByUserAndGuide(invalidUserId, invalidGuideId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetByUserAndGuide_EnforcesUniqueConstraint()
    {
        // Arrange
        var user = CreateTestUser();
        var guide = CreateTestGuide();
        CreateTestProgress(user.Id, guide.Id);

        var duplicateProgress = new Progress
        {
            UserId = user.Id,
            GuideId = guide.Id,
            CurrentStepOrder = 1,
            StartedAt = DateTime.UtcNow,
            LastAccessedAt = DateTime.UtcNow
        };

        // Act & Assert
        var exception = Assert.Throws<LiteException>(() => _progressRepository.Insert(duplicateProgress));
        exception.Message.Should().Contain("duplicate");
    }

    [Fact]
    public void GetActiveByUser_WithMultipleProgress_ReturnsOnlyActive()
    {
        // Arrange
        var user = CreateTestUser();
        var guide1 = CreateTestGuide("Guide 1");
        var guide2 = CreateTestGuide("Guide 2");
        var guide3 = CreateTestGuide("Guide 3");

        var activeProgress1 = CreateTestProgress(user.Id, guide1.Id);
        var activeProgress2 = CreateTestProgress(user.Id, guide2.Id);
        var completedProgress = CreateTestProgress(user.Id, guide3.Id);

        // Mark one as completed
        completedProgress.CompletedAt = DateTime.UtcNow;
        _progressRepository.Update(completedProgress);

        // Act
        var result = _progressRepository.GetActiveByUser(user.Id).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.Id == activeProgress1.Id);
        result.Should().Contain(p => p.Id == activeProgress2.Id);
        result.Should().NotContain(p => p.Id == completedProgress.Id);
    }

    [Fact]
    public void GetActiveByUser_OrderedByLastAccessedAt_DescendingOrder()
    {
        // Arrange
        var user = CreateTestUser();
        var guide1 = CreateTestGuide("Guide 1");
        var guide2 = CreateTestGuide("Guide 2");
        var guide3 = CreateTestGuide("Guide 3");

        var progress1 = CreateTestProgress(user.Id, guide1.Id);
        progress1.LastAccessedAt = DateTime.UtcNow.AddHours(-3);
        _progressRepository.Update(progress1);

        var progress2 = CreateTestProgress(user.Id, guide2.Id);
        progress2.LastAccessedAt = DateTime.UtcNow.AddHours(-1);
        _progressRepository.Update(progress2);

        var progress3 = CreateTestProgress(user.Id, guide3.Id);
        progress3.LastAccessedAt = DateTime.UtcNow;
        _progressRepository.Update(progress3);

        // Act
        var result = _progressRepository.GetActiveByUser(user.Id).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].Id.Should().Be(progress3.Id); // Most recent
        result[1].Id.Should().Be(progress2.Id);
        result[2].Id.Should().Be(progress1.Id); // Oldest
    }

    [Fact]
    public void GetCompletedByUser_WithMultipleProgress_ReturnsOnlyCompleted()
    {
        // Arrange
        var user = CreateTestUser();
        var guide1 = CreateTestGuide("Guide 1");
        var guide2 = CreateTestGuide("Guide 2");
        var guide3 = CreateTestGuide("Guide 3");

        var activeProgress = CreateTestProgress(user.Id, guide1.Id);

        var completedProgress1 = CreateTestProgress(user.Id, guide2.Id);
        completedProgress1.CompletedAt = DateTime.UtcNow.AddHours(-1);
        _progressRepository.Update(completedProgress1);

        var completedProgress2 = CreateTestProgress(user.Id, guide3.Id);
        completedProgress2.CompletedAt = DateTime.UtcNow;
        _progressRepository.Update(completedProgress2);

        // Act
        var result = _progressRepository.GetCompletedByUser(user.Id).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.Id == completedProgress1.Id);
        result.Should().Contain(p => p.Id == completedProgress2.Id);
        result.Should().NotContain(p => p.Id == activeProgress.Id);
    }

    [Fact]
    public void GetAllProgressForGuide_WithMultipleUsers_ReturnsAllRecords()
    {
        // Arrange
        var user1 = CreateTestUser("user1@test.com");
        var user2 = CreateTestUser("user2@test.com");
        var guide = CreateTestGuide();

        var progress1 = CreateTestProgress(user1.Id, guide.Id);
        var progress2 = CreateTestProgress(user2.Id, guide.Id);

        // Act
        var result = _progressRepository.GetAllProgressForGuide(guide.Id).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.Id == progress1.Id);
        result.Should().Contain(p => p.Id == progress2.Id);
    }

    [Fact]
    public void GetStatistics_WithNoProgress_ReturnsZeroStats()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var statistics = _progressRepository.GetStatistics(user.Id);

        // Assert
        statistics.Should().NotBeNull();
        statistics.TotalStarted.Should().Be(0);
        statistics.TotalCompleted.Should().Be(0);
        statistics.CurrentlyInProgress.Should().Be(0);
        statistics.AverageCompletionTimeMinutes.Should().Be(0);
        statistics.CompletionRate.Should().Be(0);
    }

    [Fact]
    public void GetStatistics_WithMultipleProgress_ReturnsAccurateStats()
    {
        // Arrange
        var user = CreateTestUser();
        var guide1 = CreateTestGuide("Guide 1");
        var guide2 = CreateTestGuide("Guide 2");
        var guide3 = CreateTestGuide("Guide 3");

        // Create 2 completed and 1 active progress
        var completedProgress1 = CreateTestProgress(user.Id, guide1.Id);
        completedProgress1.StartedAt = DateTime.UtcNow.AddHours(-2);
        completedProgress1.CompletedAt = DateTime.UtcNow.AddHours(-1); // Took 1 hour (60 mins)
        _progressRepository.Update(completedProgress1);

        var completedProgress2 = CreateTestProgress(user.Id, guide2.Id);
        completedProgress2.StartedAt = DateTime.UtcNow.AddHours(-3);
        completedProgress2.CompletedAt = DateTime.UtcNow.AddHours(-1); // Took 2 hours (120 mins)
        _progressRepository.Update(completedProgress2);

        var activeProgress = CreateTestProgress(user.Id, guide3.Id);

        // Act
        var statistics = _progressRepository.GetStatistics(user.Id);

        // Assert
        statistics.TotalStarted.Should().Be(3);
        statistics.TotalCompleted.Should().Be(2);
        statistics.CurrentlyInProgress.Should().Be(1);
        statistics.AverageCompletionTimeMinutes.Should().BeApproximately(90, 1); // Average of 60 and 120
        statistics.CompletionRate.Should().BeApproximately(66.67, 0.1); // 2 out of 3 = 66.67%
    }

    [Fact]
    public void UpdateStepCompletion_MarkingComplete_AddsToCompletedList()
    {
        // Arrange
        var user = CreateTestUser();
        var guide = CreateTestGuide();
        var progress = CreateTestProgress(user.Id, guide.Id);
        var stepOrder = 2;

        // Act
        var result = _progressRepository.UpdateStepCompletion(progress.Id, stepOrder, true);

        // Assert
        result.Should().BeTrue();
        var updatedProgress = _progressRepository.GetById(progress.Id);
        updatedProgress!.CompletedStepOrders.Should().Contain(stepOrder);
    }

    [Fact]
    public void UpdateStepCompletion_MarkingComplete_DoesNotAddDuplicates()
    {
        // Arrange
        var user = CreateTestUser();
        var guide = CreateTestGuide();
        var progress = CreateTestProgress(user.Id, guide.Id);
        var stepOrder = 2;

        // Act - Mark complete twice
        _progressRepository.UpdateStepCompletion(progress.Id, stepOrder, true);
        _progressRepository.UpdateStepCompletion(progress.Id, stepOrder, true);

        // Assert
        var updatedProgress = _progressRepository.GetById(progress.Id);
        updatedProgress!.CompletedStepOrders.Count(s => s == stepOrder).Should().Be(1);
    }

    [Fact]
    public void UpdateStepCompletion_MarkingIncomplete_RemovesFromCompletedList()
    {
        // Arrange
        var user = CreateTestUser();
        var guide = CreateTestGuide();
        var progress = CreateTestProgress(user.Id, guide.Id);
        var stepOrder = 2;

        // Mark as complete first
        _progressRepository.UpdateStepCompletion(progress.Id, stepOrder, true);

        // Act - Mark as incomplete
        var result = _progressRepository.UpdateStepCompletion(progress.Id, stepOrder, false);

        // Assert
        result.Should().BeTrue();
        var updatedProgress = _progressRepository.GetById(progress.Id);
        updatedProgress!.CompletedStepOrders.Should().NotContain(stepOrder);
    }

    [Fact]
    public void UpdateStepCompletion_UpdatesLastAccessedAt()
    {
        // Arrange
        var user = CreateTestUser();
        var guide = CreateTestGuide();
        var progress = CreateTestProgress(user.Id, guide.Id);

        // Get original value from database to ensure timezone consistency
        var originalProgress = _progressRepository.GetById(progress.Id);
        var originalLastAccessedAt = originalProgress!.LastAccessedAt;

        // Wait a bit to ensure timestamp changes
        System.Threading.Thread.Sleep(100);

        // Act
        _progressRepository.UpdateStepCompletion(progress.Id, 1, true);

        // Assert
        var updatedProgress = _progressRepository.GetById(progress.Id);
        updatedProgress!.LastAccessedAt.Should().BeAfter(originalLastAccessedAt);
    }

    [Fact]
    public void UpdateStepCompletion_WithInvalidProgressId_ReturnsFalse()
    {
        // Arrange
        var invalidProgressId = ObjectId.NewObjectId();

        // Act
        var result = _progressRepository.UpdateStepCompletion(invalidProgressId, 1, true);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void UpdateCurrentStep_WithValidStep_UpdatesCurrentStepOrder()
    {
        // Arrange
        var user = CreateTestUser();
        var guide = CreateTestGuide();
        var progress = CreateTestProgress(user.Id, guide.Id);
        var newStepOrder = 5;

        // Act
        var result = _progressRepository.UpdateCurrentStep(progress.Id, newStepOrder);

        // Assert
        result.Should().BeTrue();
        var updatedProgress = _progressRepository.GetById(progress.Id);
        updatedProgress!.CurrentStepOrder.Should().Be(newStepOrder);
    }

    [Fact]
    public void UpdateCurrentStep_UpdatesLastAccessedAt()
    {
        // Arrange
        var user = CreateTestUser();
        var guide = CreateTestGuide();
        var progress = CreateTestProgress(user.Id, guide.Id);

        // Get original value from database to ensure timezone consistency
        var originalProgress = _progressRepository.GetById(progress.Id);
        var originalLastAccessedAt = originalProgress!.LastAccessedAt;

        // Wait a bit to ensure timestamp changes
        System.Threading.Thread.Sleep(100);

        // Act
        _progressRepository.UpdateCurrentStep(progress.Id, 3);

        // Assert
        var updatedProgress = _progressRepository.GetById(progress.Id);
        updatedProgress!.LastAccessedAt.Should().BeAfter(originalLastAccessedAt);
    }

    [Fact]
    public void UpdateCurrentStep_WithInvalidProgressId_ReturnsFalse()
    {
        // Arrange
        var invalidProgressId = ObjectId.NewObjectId();

        // Act
        var result = _progressRepository.UpdateCurrentStep(invalidProgressId, 3);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void MarkGuideComplete_SetsCompletedAtTimestamp()
    {
        // Arrange
        var user = CreateTestUser();
        var guide = CreateTestGuide();
        var progress = CreateTestProgress(user.Id, guide.Id);

        // Retrieve from database to check initial state
        var initialProgress = _progressRepository.GetById(progress.Id);
        initialProgress!.CompletedAt.Should().BeNull();

        // Act
        var result = _progressRepository.MarkGuideComplete(progress.Id);

        // Assert
        result.Should().BeTrue();
        var updatedProgress = _progressRepository.GetById(progress.Id);
        updatedProgress!.CompletedAt.Should().NotBeNull();

        // Verify CompletedAt is after StartedAt (basic sanity check)
        updatedProgress.CompletedAt!.Value.Should().BeOnOrAfter(updatedProgress.StartedAt);
    }

    [Fact]
    public void MarkGuideComplete_UpdatesLastAccessedAt()
    {
        // Arrange
        var user = CreateTestUser();
        var guide = CreateTestGuide();
        var progress = CreateTestProgress(user.Id, guide.Id);

        // Get original value from database to ensure timezone consistency
        var originalProgress = _progressRepository.GetById(progress.Id);
        var originalLastAccessedAt = originalProgress!.LastAccessedAt;

        // Wait a bit to ensure timestamp changes
        System.Threading.Thread.Sleep(100);

        // Act
        _progressRepository.MarkGuideComplete(progress.Id);

        // Assert
        var updatedProgress = _progressRepository.GetById(progress.Id);
        updatedProgress!.LastAccessedAt.Should().BeAfter(originalLastAccessedAt);
    }

    [Fact]
    public void MarkGuideComplete_WithInvalidProgressId_ReturnsFalse()
    {
        // Arrange
        var invalidProgressId = ObjectId.NewObjectId();

        // Act
        var result = _progressRepository.MarkGuideComplete(invalidProgressId);

        // Assert
        result.Should().BeFalse();
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

    private Guide CreateTestGuide(string title = "Test Guide")
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

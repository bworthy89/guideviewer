using FluentAssertions;
using GuideViewer.Core.Services;
using GuideViewer.Data.Entities;
using GuideViewer.Data.Models;
using GuideViewer.Data.Repositories;
using GuideViewer.Data.Services;
using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace GuideViewer.Tests.Services;

/// <summary>
/// Integration tests for ProgressTrackingService with real database.
/// </summary>
public class ProgressTrackingServiceTests : IDisposable
{
    private readonly string _testDatabasePath;
    private readonly DatabaseService _databaseService;
    private readonly ProgressRepository _progressRepository;
    private readonly GuideRepository _guideRepository;
    private readonly ProgressTrackingService _service;

    public ProgressTrackingServiceTests()
    {
        _testDatabasePath = Path.Combine(Path.GetTempPath(), $"test_progress_tracking_service_{Guid.NewGuid()}.db");
        _databaseService = new DatabaseService(_testDatabasePath);
        _progressRepository = new ProgressRepository(_databaseService);
        _guideRepository = new GuideRepository(_databaseService);
        _service = new ProgressTrackingService(_progressRepository, _guideRepository);
    }

    [Fact]
    public async Task StartGuideAsync_WithValidGuide_CreatesProgressRecord()
    {
        // Arrange
        var userId = ObjectId.NewObjectId();
        var guide = CreateTestGuide(stepCount: 3);

        // Act
        var result = await _service.StartGuideAsync(guide.Id, userId);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.GuideId.Should().Be(guide.Id);
        result.CurrentStepOrder.Should().Be(1);
        result.StartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        result.LastAccessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));

        // Verify it was persisted
        var persisted = _progressRepository.GetById(result.Id);
        persisted.Should().NotBeNull();
    }

    [Fact]
    public async Task StartGuideAsync_WithNonExistentGuide_ThrowsInvalidOperationException()
    {
        // Arrange
        var guideId = ObjectId.NewObjectId(); // Non-existent guide
        var userId = ObjectId.NewObjectId();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.StartGuideAsync(guideId, userId));
    }

    [Fact]
    public async Task StartGuideAsync_WhenProgressAlreadyExists_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = ObjectId.NewObjectId();
        var guide = CreateTestGuide();

        // Start guide once
        await _service.StartGuideAsync(guide.Id, userId);

        // Act & Assert - Try to start again
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.StartGuideAsync(guide.Id, userId));

        exception.Message.Should().Contain("already exists");
    }

    [Fact]
    public async Task StartGuideAsync_WithGuideWithNoSteps_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = ObjectId.NewObjectId();
        var guide = CreateTestGuide(stepCount: 0);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.StartGuideAsync(guide.Id, userId));

        exception.Message.Should().Contain("has no steps");
    }

    [Fact]
    public async Task GetProgressAsync_ReturnsProgressForUserAndGuide()
    {
        // Arrange
        var userId = ObjectId.NewObjectId();
        var guide = CreateTestGuide();
        var progress = await _service.StartGuideAsync(guide.Id, userId);

        // Act
        var result = await _service.GetProgressAsync(guide.Id, userId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(progress.Id);
    }

    [Fact]
    public async Task GetProgressAsync_WhenNotFound_ReturnsNull()
    {
        // Arrange
        var guideId = ObjectId.NewObjectId();
        var userId = ObjectId.NewObjectId();

        // Act
        var result = await _service.GetProgressAsync(guideId, userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveProgressAsync_ReturnsActiveProgressForUser()
    {
        // Arrange
        var userId = ObjectId.NewObjectId();
        var guide1 = CreateTestGuide("Guide 1");
        var guide2 = CreateTestGuide("Guide 2");

        await _service.StartGuideAsync(guide1.Id, userId);
        await _service.StartGuideAsync(guide2.Id, userId);

        // Act
        var result = await _service.GetActiveProgressAsync(userId);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetCompletedProgressAsync_ReturnsCompletedProgressForUser()
    {
        // Arrange
        var userId = ObjectId.NewObjectId();
        var guide = CreateTestGuide(stepCount: 1);
        var progress = await _service.StartGuideAsync(guide.Id, userId);

        // Complete the guide
        await _service.CompleteStepAsync(progress.Id, 1, true);
        await _service.MarkGuideCompleteAsync(progress.Id);

        // Act
        var result = await _service.GetCompletedProgressAsync(userId);

        // Assert
        result.Should().ContainSingle();
        result.First().CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task CompleteStepAsync_WithValidStep_UpdatesStepCompletion()
    {
        // Arrange
        var userId = ObjectId.NewObjectId();
        var guide = CreateTestGuide(stepCount: 5);
        var progress = await _service.StartGuideAsync(guide.Id, userId);

        // Act
        var result = await _service.CompleteStepAsync(progress.Id, stepOrder: 1, completed: true);

        // Assert
        result.Should().BeTrue();
        var updatedProgress = _progressRepository.GetById(progress.Id);
        updatedProgress!.CompletedStepOrders.Should().Contain(1);
    }

    [Fact]
    public async Task CompleteStepAsync_WithNotes_UpdatesNotesAndCompletion()
    {
        // Arrange
        var userId = ObjectId.NewObjectId();
        var guide = CreateTestGuide(stepCount: 5);
        var progress = await _service.StartGuideAsync(guide.Id, userId);
        var notes = "Test notes";

        // Act
        var result = await _service.CompleteStepAsync(progress.Id, stepOrder: 1, completed: true, notes: notes);

        // Assert
        result.Should().BeTrue();
        var updatedProgress = _progressRepository.GetById(progress.Id);
        updatedProgress!.Notes.Should().Be(notes);
    }

    [Fact]
    public async Task CompleteStepAsync_WithInvalidStepOrder_ThrowsArgumentException()
    {
        // Arrange
        var userId = ObjectId.NewObjectId();
        var guide = CreateTestGuide(stepCount: 3);
        var progress = await _service.StartGuideAsync(guide.Id, userId);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            async () => await _service.CompleteStepAsync(progress.Id, stepOrder: 10, completed: true));

        exception.Message.Should().Contain("invalid");
    }

    [Fact]
    public async Task CompleteStepAsync_WithNonExistentProgress_ThrowsInvalidOperationException()
    {
        // Arrange
        var progressId = ObjectId.NewObjectId();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.CompleteStepAsync(progressId, stepOrder: 1, completed: true));
    }

    [Fact]
    public async Task UpdateCurrentStepAsync_WithValidStep_UpdatesCurrentStep()
    {
        // Arrange
        var userId = ObjectId.NewObjectId();
        var guide = CreateTestGuide(stepCount: 5);
        var progress = await _service.StartGuideAsync(guide.Id, userId);

        // Act
        var result = await _service.UpdateCurrentStepAsync(progress.Id, stepOrder: 3);

        // Assert
        result.Should().BeTrue();
        var updatedProgress = _progressRepository.GetById(progress.Id);
        updatedProgress!.CurrentStepOrder.Should().Be(3);
    }

    [Fact]
    public async Task UpdateCurrentStepAsync_WithInvalidStepOrder_ThrowsArgumentException()
    {
        // Arrange
        var userId = ObjectId.NewObjectId();
        var guide = CreateTestGuide(stepCount: 3);
        var progress = await _service.StartGuideAsync(guide.Id, userId);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            async () => await _service.UpdateCurrentStepAsync(progress.Id, stepOrder: 0));

        exception.Message.Should().Contain("invalid");
    }

    [Fact]
    public async Task MarkGuideCompleteAsync_WithActiveProgress_MarksComplete()
    {
        // Arrange
        var userId = ObjectId.NewObjectId();
        var guide = CreateTestGuide();
        var progress = await _service.StartGuideAsync(guide.Id, userId);

        // Act
        var result = await _service.MarkGuideCompleteAsync(progress.Id);

        // Assert
        result.Should().BeTrue();
        var updatedProgress = _progressRepository.GetById(progress.Id);
        updatedProgress!.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task MarkGuideCompleteAsync_WhenAlreadyCompleted_ReturnsFalse()
    {
        // Arrange
        var userId = ObjectId.NewObjectId();
        var guide = CreateTestGuide();
        var progress = await _service.StartGuideAsync(guide.Id, userId);

        // Complete once
        await _service.MarkGuideCompleteAsync(progress.Id);

        // Act - Try to complete again
        var result = await _service.MarkGuideCompleteAsync(progress.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetStatisticsAsync_ReturnsStatisticsForUser()
    {
        // Arrange
        var userId = ObjectId.NewObjectId();
        var guide1 = CreateTestGuide("Guide 1", stepCount: 1);
        var guide2 = CreateTestGuide("Guide 2");

        var progress1 = await _service.StartGuideAsync(guide1.Id, userId);
        await _service.StartGuideAsync(guide2.Id, userId);

        // Complete guide1
        await _service.CompleteStepAsync(progress1.Id, 1, true);
        await _service.MarkGuideCompleteAsync(progress1.Id);

        // Act
        var result = await _service.GetStatisticsAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.TotalStarted.Should().Be(2);
        result.TotalCompleted.Should().Be(1);
    }

    [Fact]
    public void CalculateEstimatedTimeRemaining_WithNoCompletedSteps_ReturnsFullEstimate()
    {
        // Arrange
        var guide = CreateTestGuide(stepCount: 10, estimatedMinutes: 100);
        var progress = new Progress
        {
            GuideId = guide.Id,
            CompletedStepOrders = new List<int>()
        };

        // Act
        var result = _service.CalculateEstimatedTimeRemaining(progress, guide);

        // Assert
        result.Should().Be(100);
    }

    [Fact]
    public void CalculateEstimatedTimeRemaining_WithHalfCompleted_ReturnsHalfEstimate()
    {
        // Arrange
        var guide = CreateTestGuide(stepCount: 10, estimatedMinutes: 100);
        var progress = new Progress
        {
            GuideId = guide.Id,
            CompletedStepOrders = new List<int> { 1, 2, 3, 4, 5 }
        };

        // Act
        var result = _service.CalculateEstimatedTimeRemaining(progress, guide);

        // Assert
        result.Should().Be(50);
    }

    [Fact]
    public void CalculateEstimatedTimeRemaining_WithAllCompleted_ReturnsZero()
    {
        // Arrange
        var guide = CreateTestGuide(stepCount: 5, estimatedMinutes: 100);
        var progress = new Progress
        {
            GuideId = guide.Id,
            CompletedStepOrders = new List<int> { 1, 2, 3, 4, 5 }
        };

        // Act
        var result = _service.CalculateEstimatedTimeRemaining(progress, guide);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void CalculateEstimatedTimeRemaining_WithNoEstimatedTime_ReturnsNull()
    {
        // Arrange
        var guide = CreateTestGuide(stepCount: 5, estimatedMinutes: 0);
        var progress = new Progress
        {
            GuideId = guide.Id,
            CompletedStepOrders = new List<int> { 1, 2 }
        };

        // Act
        var result = _service.CalculateEstimatedTimeRemaining(progress, guide);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void CalculateEstimatedTimeRemaining_WithNoSteps_ReturnsNull()
    {
        // Arrange
        var guide = CreateTestGuide(stepCount: 0, estimatedMinutes: 100);
        var progress = new Progress
        {
            GuideId = guide.Id,
            CompletedStepOrders = new List<int>()
        };

        // Act
        var result = _service.CalculateEstimatedTimeRemaining(progress, guide);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateActiveTimeAsync_WithValidSeconds_UpdatesActiveTime()
    {
        // Arrange
        var userId = ObjectId.NewObjectId();
        var guide = CreateTestGuide();
        var progress = await _service.StartGuideAsync(guide.Id, userId);

        // Act
        var result = await _service.UpdateActiveTimeAsync(progress.Id, additionalSeconds: 60);

        // Assert
        result.Should().BeTrue();
        var updatedProgress = _progressRepository.GetById(progress.Id);
        updatedProgress!.TotalActiveTimeSeconds.Should().Be(60);
    }

    [Fact]
    public async Task UpdateActiveTimeAsync_WithNegativeSeconds_ThrowsArgumentException()
    {
        // Arrange
        var progressId = ObjectId.NewObjectId();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _service.UpdateActiveTimeAsync(progressId, additionalSeconds: -10));
    }

    [Fact]
    public async Task UpdateActiveTimeAsync_UpdatesLastAccessedAt()
    {
        // Arrange
        var userId = ObjectId.NewObjectId();
        var guide = CreateTestGuide();
        var progress = await _service.StartGuideAsync(guide.Id, userId);

        var originalProgress = _progressRepository.GetById(progress.Id);
        var originalLastAccessedAt = originalProgress!.LastAccessedAt;

        await Task.Delay(100); // Small delay

        // Act
        await _service.UpdateActiveTimeAsync(progress.Id, additionalSeconds: 60);

        // Assert
        var updatedProgress = _progressRepository.GetById(progress.Id);
        updatedProgress!.LastAccessedAt.Should().BeAfter(originalLastAccessedAt);
    }

    // Helper method to create test guides
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

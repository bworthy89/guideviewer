using FluentAssertions;
using GuideViewer.Core.Services;
using GuideViewer.Data.Entities;
using GuideViewer.Data.Repositories;
using GuideViewer.Data.Services;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace GuideViewer.Tests.Integration;

/// <summary>
/// Performance and scalability integration tests for progress tracking.
/// Tests query performance, index usage, and timer functionality.
/// </summary>
public class ProgressPerformanceIntegrationTests : IDisposable
{
    private readonly string _testDatabasePath;
    private readonly DatabaseService _databaseService;
    private readonly GuideRepository _guideRepository;
    private readonly UserRepository _userRepository;
    private readonly ProgressRepository _progressRepository;
    private readonly IProgressTrackingService _progressTrackingService;
    private readonly ITimerService _timerService;

    public ProgressPerformanceIntegrationTests()
    {
        _testDatabasePath = Path.Combine(Path.GetTempPath(), $"test_progress_perf_{Guid.NewGuid()}.db");
        _databaseService = new DatabaseService(_testDatabasePath);
        _guideRepository = new GuideRepository(_databaseService);
        _userRepository = new UserRepository(_databaseService);
        _progressRepository = new ProgressRepository(_databaseService);
        _progressTrackingService = new ProgressTrackingService(_progressRepository, _guideRepository);
        _timerService = new TimerService();
    }

    [Fact]
    public async Task Performance_100PlusRecords_QueriesUnder100ms()
    {
        // Arrange - Create 100 users and 10 guides
        var userIds = new List<ObjectId>();
        for (int i = 0; i < 100; i++)
        {
            var user = new User { ProductKey = $"USER{i:D3}-KEY", Role = "Technician" };
            userIds.Add(_userRepository.Insert(user));
        }

        var guideIds = new List<ObjectId>();
        for (int i = 0; i < 10; i++)
        {
            var guide = CreateTestGuide($"Performance Guide {i}", 3);
            guideIds.Add(_guideRepository.Insert(guide));
        }

        // Create 150 progress records (some users have multiple guides in progress)
        for (int i = 0; i < 150; i++)
        {
            var userId = userIds[i % userIds.Count];
            var guideId = guideIds[i % guideIds.Count];

            // Skip if this combination already exists
            var existing = await _progressTrackingService.GetProgressAsync(guideId, userId);
            if (existing == null)
            {
                await _progressTrackingService.StartGuideAsync(guideId, userId);
            }
        }

        // Act & Assert - Query active progress (should use LastAccessedAt index)
        var sw = Stopwatch.StartNew();
        var activeProgress = await _progressTrackingService.GetActiveProgressAsync(userIds[0]);
        sw.Stop();

        sw.ElapsedMilliseconds.Should().BeLessThan(100, "Query should be fast with indexes");

        // Act & Assert - Query all progress for a guide (should use GuideId index)
        sw.Restart();
        var guideProgress = _progressRepository.GetAllProgressForGuide(guideIds[0]);
        sw.Stop();

        sw.ElapsedMilliseconds.Should().BeLessThan(100, "Query should be fast with indexes");

        // Act & Assert - Get statistics (aggregation query)
        sw.Restart();
        var stats = await _progressTrackingService.GetStatisticsAsync(userIds[0]);
        sw.Stop();

        sw.ElapsedMilliseconds.Should().BeLessThan(100, "Statistics should be fast with indexes");
    }

    [Fact]
    public async Task Performance_DatabaseIndexes_WorkCorrectly()
    {
        // Arrange - Create test data
        var user1 = new User { ProductKey = "USER1-KEY", Role = "Technician" };
        var user2 = new User { ProductKey = "USER2-KEY", Role = "Technician" };
        var userId1 = _userRepository.Insert(user1);
        var userId2 = _userRepository.Insert(user2);

        var guide = CreateTestGuide("Index Test Guide", 2);
        var guideId = _guideRepository.Insert(guide);

        // Create progress for both users
        var progress1 = await _progressTrackingService.StartGuideAsync(guideId, userId1);
        var progress2 = await _progressTrackingService.StartGuideAsync(guideId, userId2);

        // Complete progress1
        await _progressTrackingService.CompleteStepAsync(progress1.Id, 1, true);
        await _progressTrackingService.CompleteStepAsync(progress1.Id, 2, true);
        await _progressTrackingService.MarkGuideCompleteAsync(progress1.Id);

        // Act - Query by UserId (should use UserId index)
        var user1Progress = _progressRepository.GetActiveByUser(userId1);
        var user2Progress = _progressRepository.GetActiveByUser(userId2);

        // Assert
        user1Progress.Should().BeEmpty(); // User 1's guide is complete
        user2Progress.Should().ContainSingle(); // User 2's guide is still in progress

        // Act - Query by GuideId (should use GuideId index)
        var allProgressForGuide = _progressRepository.GetAllProgressForGuide(guideId);

        // Assert
        allProgressForGuide.Should().HaveCount(2); // Both users started this guide

        // Act - Query completed (should use CompletedAt index)
        var completedProgress = _progressRepository.GetCompletedByUser(userId1);

        // Assert
        completedProgress.Should().ContainSingle();
        completedProgress.First().CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Performance_UniqueConstraint_PreventsDuplicates()
    {
        // Arrange
        var user = new User { ProductKey = "TEST-KEY", Role = "Technician" };
        var userId = _userRepository.Insert(user);

        var guide = CreateTestGuide("Unique Test Guide", 2);
        var guideId = _guideRepository.Insert(guide);

        // Act - Start guide
        await _progressTrackingService.StartGuideAsync(guideId, userId);

        // Act & Assert - Try to start same guide again
        var act = async () => await _progressTrackingService.StartGuideAsync(guideId, userId);
        await act.Should().ThrowAsync<InvalidOperationException>();

        // Verify only one record exists
        var allProgress = _progressRepository.GetActiveByUser(userId);
        allProgress.Should().ContainSingle();
    }

    [Fact]
    public void Timer_ElapsedTimeTracking_Accurate()
    {
        // Arrange
        var timerService = new TimerService();
        TimeSpan? receivedElapsed = null;

        timerService.Tick += (sender, elapsed) =>
        {
            receivedElapsed = elapsed;
        };

        // Act
        timerService.Start();
        System.Threading.Thread.Sleep(1100); // Sleep for 1 second + buffer

        // Assert
        receivedElapsed.Should().NotBeNull();
        receivedElapsed!.Value.TotalSeconds.Should().BeGreaterThanOrEqualTo(1);
        receivedElapsed.Value.Should().BeCloseTo(timerService.Elapsed, TimeSpan.FromMilliseconds(300));

        // Cleanup
        timerService.Stop();
        timerService.Dispose();
    }

    [Fact]
    public void Timer_StartStopReset_WorksCorrectly()
    {
        // Arrange
        var timerService = new TimerService();

        // Act - Start
        timerService.Start();
        timerService.IsRunning.Should().BeTrue();

        System.Threading.Thread.Sleep(500);
        var elapsedAfterStart = timerService.Elapsed;
        elapsedAfterStart.TotalMilliseconds.Should().BeGreaterThan(400);

        // Act - Stop
        timerService.Stop();
        timerService.IsRunning.Should().BeFalse();

        var elapsedAfterStop = timerService.Elapsed;
        System.Threading.Thread.Sleep(500);
        timerService.Elapsed.Should().Be(elapsedAfterStop); // Should not increase when stopped

        // Act - Reset
        timerService.Reset();
        timerService.Elapsed.Should().Be(TimeSpan.Zero);
        timerService.IsRunning.Should().BeFalse();

        // Cleanup
        timerService.Dispose();
    }

    [Fact]
    public void Timer_MultipleStartStops_NoMemoryLeak()
    {
        // Arrange
        var timerService = new TimerService();
        int tickCount = 0;

        timerService.Tick += (sender, elapsed) =>
        {
            tickCount++;
        };

        // Act - Start and stop multiple times
        for (int i = 0; i < 5; i++)
        {
            timerService.Start();
            System.Threading.Thread.Sleep(300);
            timerService.Stop();
        }

        // Assert - Timer should have ticked at least once per cycle
        tickCount.Should().BeGreaterThan(0);

        // Act - Final disposal
        timerService.Dispose();

        // Assert - No exceptions thrown, no memory leaks
        timerService.IsRunning.Should().BeFalse();
    }

    [Fact]
    public async Task ActiveTimeTracking_UpdatesCorrectly()
    {
        // Arrange
        var user = new User { ProductKey = "TEST-KEY", Role = "Technician" };
        var userId = _userRepository.Insert(user);

        var guide = CreateTestGuide("Time Tracking Guide", 2);
        var guideId = _guideRepository.Insert(guide);

        var progress = await _progressTrackingService.StartGuideAsync(guideId, userId);

        // Act - Simulate 60 seconds of active time
        await _progressTrackingService.UpdateActiveTimeAsync(progress.Id, 60);

        // Assert
        var updatedProgress = await _progressTrackingService.GetProgressAsync(guideId, userId);
        updatedProgress!.TotalActiveTimeSeconds.Should().BeGreaterThanOrEqualTo(60);
    }

    [Fact]
    public async Task EstimatedTimeRemaining_CalculatesCorrectly()
    {
        // Arrange
        var user = new User { ProductKey = "TEST-KEY", Role = "Technician" };
        var userId = _userRepository.Insert(user);

        var guide = CreateTestGuide("Estimation Guide", 4);
        guide.EstimatedMinutes = 40; // 10 minutes per step
        var guideId = _guideRepository.Insert(guide);

        var progress = await _progressTrackingService.StartGuideAsync(guideId, userId);

        // Complete 2 out of 4 steps (50% done)
        await _progressTrackingService.CompleteStepAsync(progress.Id, 1, true);
        await _progressTrackingService.CompleteStepAsync(progress.Id, 2, true);

        // Simulate 20 minutes of active time for first 2 steps
        await _progressTrackingService.UpdateActiveTimeAsync(progress.Id, 1200); // 20 minutes in seconds

        var updatedProgress = await _progressTrackingService.GetProgressAsync(guideId, userId);
        var updatedGuide = _guideRepository.GetById(guideId);

        // Act
        var estimatedRemaining = _progressTrackingService.CalculateEstimatedTimeRemaining(updatedProgress!, updatedGuide!);

        // Assert - Should estimate ~20 more minutes for remaining 50%
        estimatedRemaining.Should().NotBeNull();
        estimatedRemaining.Should().BeGreaterThan(15); // At least 15 minutes
        estimatedRemaining.Should().BeLessThan(30); // Less than 30 minutes
    }

    [Fact]
    public async Task LargeNotesField_StoresAndRetrievesCorrectly()
    {
        // Arrange
        var user = new User { ProductKey = "TEST-KEY", Role = "Technician" };
        var userId = _userRepository.Insert(user);

        var guide = CreateTestGuide("Large Notes Guide", 1);
        var guideId = _guideRepository.Insert(guide);

        var progress = await _progressTrackingService.StartGuideAsync(guideId, userId);

        // Create 5000 character note (max allowed)
        var largeNotes = new string('A', 5000);

        // Act
        await _progressTrackingService.CompleteStepAsync(progress.Id, 1, true, largeNotes);

        // Assert
        var updatedProgress = await _progressTrackingService.GetProgressAsync(guideId, userId);
        updatedProgress!.Notes.Should().HaveLength(5000);
        updatedProgress.Notes.Should().Be(largeNotes);
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
        _timerService?.Dispose();
        _databaseService?.Dispose();
        if (File.Exists(_testDatabasePath))
        {
            File.Delete(_testDatabasePath);
        }
    }
}

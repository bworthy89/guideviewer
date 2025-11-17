using GuideViewer.Data.Entities;
using GuideViewer.Data.Models;
using GuideViewer.Data.Repositories;
using LiteDB;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuideViewer.Core.Services;

/// <summary>
/// Service for managing user progress through guides.
/// Handles business logic and validation for progress tracking.
/// </summary>
public class ProgressTrackingService : IProgressTrackingService
{
    private readonly ProgressRepository _progressRepository;
    private readonly GuideRepository _guideRepository;

    public ProgressTrackingService(
        ProgressRepository progressRepository,
        GuideRepository guideRepository)
    {
        _progressRepository = progressRepository ?? throw new ArgumentNullException(nameof(progressRepository));
        _guideRepository = guideRepository ?? throw new ArgumentNullException(nameof(guideRepository));
    }

    /// <summary>
    /// Starts a new progress record for a user on a specific guide.
    /// </summary>
    public async Task<Progress> StartGuideAsync(ObjectId guideId, ObjectId userId)
    {
        return await Task.Run(() =>
        {
            // Validate guide exists
            var guide = _guideRepository.GetById(guideId);
            if (guide == null)
            {
                throw new InvalidOperationException($"Guide with ID {guideId} not found.");
            }

            // Check if progress already exists
            var existingProgress = _progressRepository.GetByUserAndGuide(userId, guideId);
            if (existingProgress != null)
            {
                throw new InvalidOperationException(
                    $"Progress already exists for user {userId} on guide {guideId}. " +
                    $"Cannot start a guide that is already in progress.");
            }

            // Validate guide has steps
            if (guide.Steps == null || guide.Steps.Count == 0)
            {
                throw new InvalidOperationException($"Guide {guide.Title} has no steps.");
            }

            // Create new progress record
            var progress = new Progress
            {
                UserId = userId,
                GuideId = guideId,
                CurrentStepOrder = 1,
                StartedAt = DateTime.UtcNow,
                LastAccessedAt = DateTime.UtcNow
            };

            progress.Id = _progressRepository.Insert(progress);
            Log.Information("Started progress tracking for user {UserId} on guide {GuideId}",
                userId, guideId);

            return progress;
        });
    }

    /// <summary>
    /// Gets the progress record for a specific user and guide.
    /// </summary>
    public async Task<Progress?> GetProgressAsync(ObjectId guideId, ObjectId userId)
    {
        return await Task.Run(() =>
        {
            return _progressRepository.GetByUserAndGuide(userId, guideId);
        });
    }

    /// <summary>
    /// Gets all active (not completed) progress records for a user.
    /// </summary>
    public async Task<IEnumerable<Progress>> GetActiveProgressAsync(ObjectId userId)
    {
        return await Task.Run(() =>
        {
            return _progressRepository.GetActiveByUser(userId);
        });
    }

    /// <summary>
    /// Gets all completed progress records for a user.
    /// </summary>
    public async Task<IEnumerable<Progress>> GetCompletedProgressAsync(ObjectId userId)
    {
        return await Task.Run(() =>
        {
            return _progressRepository.GetCompletedByUser(userId);
        });
    }

    /// <summary>
    /// Marks a step as complete or incomplete.
    /// </summary>
    public async Task<bool> CompleteStepAsync(ObjectId progressId, int stepOrder, bool completed, string? notes = null)
    {
        return await Task.Run(() =>
        {
            // Get progress record
            var progress = _progressRepository.GetById(progressId);
            if (progress == null)
            {
                throw new InvalidOperationException($"Progress record {progressId} not found.");
            }

            // Get guide to validate step order
            var guide = _guideRepository.GetById(progress.GuideId);
            if (guide == null)
            {
                throw new InvalidOperationException($"Guide {progress.GuideId} not found.");
            }

            // Validate step order
            if (stepOrder < 1 || stepOrder > guide.Steps.Count)
            {
                throw new ArgumentException(
                    $"Step order {stepOrder} is invalid. Guide has {guide.Steps.Count} steps.",
                    nameof(stepOrder));
            }

            // Update step completion
            var result = _progressRepository.UpdateStepCompletion(progressId, stepOrder, completed);

            // Add notes if provided
            if (!string.IsNullOrWhiteSpace(notes))
            {
                // Get fresh progress record after step completion update
                progress = _progressRepository.GetById(progressId);
                if (progress != null)
                {
                    progress.Notes = notes;
                    _progressRepository.Update(progress);
                }
            }

            if (result)
            {
                Log.Information("Updated step {StepOrder} completion to {Completed} for progress {ProgressId}",
                    stepOrder, completed, progressId);
            }

            return result;
        });
    }

    /// <summary>
    /// Updates the current step the user is viewing.
    /// </summary>
    public async Task<bool> UpdateCurrentStepAsync(ObjectId progressId, int stepOrder)
    {
        return await Task.Run(() =>
        {
            // Get progress record
            var progress = _progressRepository.GetById(progressId);
            if (progress == null)
            {
                throw new InvalidOperationException($"Progress record {progressId} not found.");
            }

            // Get guide to validate step order
            var guide = _guideRepository.GetById(progress.GuideId);
            if (guide == null)
            {
                throw new InvalidOperationException($"Guide {progress.GuideId} not found.");
            }

            // Validate step order
            if (stepOrder < 1 || stepOrder > guide.Steps.Count)
            {
                throw new ArgumentException(
                    $"Step order {stepOrder} is invalid. Guide has {guide.Steps.Count} steps.",
                    nameof(stepOrder));
            }

            var result = _progressRepository.UpdateCurrentStep(progressId, stepOrder);

            if (result)
            {
                Log.Information("Updated current step to {StepOrder} for progress {ProgressId}",
                    stepOrder, progressId);
            }

            return result;
        });
    }

    /// <summary>
    /// Marks a guide as complete.
    /// </summary>
    public async Task<bool> MarkGuideCompleteAsync(ObjectId progressId)
    {
        return await Task.Run(() =>
        {
            // Get progress record
            var progress = _progressRepository.GetById(progressId);
            if (progress == null)
            {
                throw new InvalidOperationException($"Progress record {progressId} not found.");
            }

            // Check if already completed
            if (progress.CompletedAt.HasValue)
            {
                Log.Warning("Progress {ProgressId} is already marked as complete.", progressId);
                return false;
            }

            var result = _progressRepository.MarkGuideComplete(progressId);

            if (result)
            {
                Log.Information("Marked guide complete for progress {ProgressId}", progressId);
            }

            return result;
        });
    }

    /// <summary>
    /// Gets completion statistics for a user.
    /// </summary>
    public async Task<ProgressStatistics> GetStatisticsAsync(ObjectId userId)
    {
        return await Task.Run(() =>
        {
            return _progressRepository.GetStatistics(userId);
        });
    }

    /// <summary>
    /// Calculates estimated time remaining for a guide based on progress.
    /// </summary>
    public int? CalculateEstimatedTimeRemaining(Progress progress, Guide guide)
    {
        if (progress == null)
            throw new ArgumentNullException(nameof(progress));
        if (guide == null)
            throw new ArgumentNullException(nameof(guide));

        // If no estimated time, can't calculate
        if (guide.EstimatedMinutes <= 0)
            return null;

        // If no steps, can't calculate
        if (guide.Steps == null || guide.Steps.Count == 0)
            return null;

        // Calculate completion percentage based on completed steps
        var totalSteps = guide.Steps.Count;
        var completedSteps = progress.CompletedStepOrders.Count;

        if (completedSteps == 0)
        {
            // No steps completed, return full estimated time
            return guide.EstimatedMinutes;
        }

        if (completedSteps >= totalSteps)
        {
            // All steps completed, no time remaining
            return 0;
        }

        // Calculate percentage remaining
        var percentageRemaining = (double)(totalSteps - completedSteps) / totalSteps;
        var estimatedMinutesRemaining = (int)Math.Ceiling(guide.EstimatedMinutes * percentageRemaining);

        return estimatedMinutesRemaining;
    }

    /// <summary>
    /// Updates the active time spent on a guide.
    /// </summary>
    public async Task<bool> UpdateActiveTimeAsync(ObjectId progressId, int additionalSeconds)
    {
        return await Task.Run(() =>
        {
            if (additionalSeconds < 0)
            {
                throw new ArgumentException("Additional seconds cannot be negative.", nameof(additionalSeconds));
            }

            // Get progress record
            var progress = _progressRepository.GetById(progressId);
            if (progress == null)
            {
                throw new InvalidOperationException($"Progress record {progressId} not found.");
            }

            // Update active time
            progress.TotalActiveTimeSeconds += additionalSeconds;
            progress.LastAccessedAt = DateTime.UtcNow;

            var result = _progressRepository.Update(progress);

            if (result)
            {
                Log.Debug("Updated active time for progress {ProgressId}: +{Seconds}s (total: {TotalSeconds}s)",
                    progressId, additionalSeconds, progress.TotalActiveTimeSeconds);
            }

            return result;
        });
    }
}

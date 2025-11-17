using GuideViewer.Data.Entities;
using GuideViewer.Data.Models;
using GuideViewer.Data.Services;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GuideViewer.Data.Repositories;

/// <summary>
/// Repository for managing user progress through guides.
/// </summary>
public class ProgressRepository : Repository<Progress>
{
    public ProgressRepository(DatabaseService databaseService) : base(databaseService, "progress")
    {
    }

    /// <summary>
    /// Gets the progress record for a specific user and guide combination.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="guideId">The guide ID.</param>
    /// <returns>The progress record, or null if not found.</returns>
    public Progress? GetByUserAndGuide(ObjectId userId, ObjectId guideId)
    {
        return Collection.FindOne(p => p.UserId == userId && p.GuideId == guideId);
    }

    /// <summary>
    /// Gets all active (not completed) progress records for a user,
    /// ordered by most recently accessed first.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>Active progress records ordered by LastAccessedAt descending.</returns>
    public IEnumerable<Progress> GetActiveByUser(ObjectId userId)
    {
        return Collection
            .Find(p => p.UserId == userId && p.CompletedAt == null)
            .OrderByDescending(p => p.LastAccessedAt);
    }

    /// <summary>
    /// Gets all completed progress records for a user,
    /// ordered by completion date (newest first).
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>Completed progress records ordered by CompletedAt descending.</returns>
    public IEnumerable<Progress> GetCompletedByUser(ObjectId userId)
    {
        return Collection
            .Find(p => p.UserId == userId && p.CompletedAt != null)
            .OrderByDescending(p => p.CompletedAt);
    }

    /// <summary>
    /// Gets all progress records for a specific guide (all users).
    /// Used by admins to see who has started/completed a guide.
    /// </summary>
    /// <param name="guideId">The guide ID.</param>
    /// <returns>All progress records for the guide.</returns>
    public IEnumerable<Progress> GetAllProgressForGuide(ObjectId guideId)
    {
        return Collection.Find(p => p.GuideId == guideId);
    }

    /// <summary>
    /// Calculates statistics about a user's progress across all guides.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>Progress statistics including completion rates and average times.</returns>
    public ProgressStatistics GetStatistics(ObjectId userId)
    {
        var allProgress = Collection.Find(p => p.UserId == userId).ToList();
        var completed = allProgress.Where(p => p.CompletedAt.HasValue).ToList();

        var statistics = new ProgressStatistics
        {
            TotalStarted = allProgress.Count,
            TotalCompleted = completed.Count,
            CurrentlyInProgress = allProgress.Count - completed.Count
        };

        // Calculate average completion time for completed guides
        if (completed.Any())
        {
            var totalMinutes = completed
                .Select(p => (p.CompletedAt!.Value - p.StartedAt).TotalMinutes)
                .Average();
            statistics.AverageCompletionTimeMinutes = Math.Round(totalMinutes, 2);
        }

        // Calculate completion rate
        if (statistics.TotalStarted > 0)
        {
            statistics.CompletionRate = Math.Round(
                (statistics.TotalCompleted / (double)statistics.TotalStarted) * 100, 2);
        }

        return statistics;
    }

    /// <summary>
    /// Updates the completion status of a specific step.
    /// </summary>
    /// <param name="progressId">The progress record ID.</param>
    /// <param name="stepOrder">The step order number.</param>
    /// <param name="completed">True to mark complete, false to mark incomplete.</param>
    /// <returns>True if the update succeeded.</returns>
    public bool UpdateStepCompletion(ObjectId progressId, int stepOrder, bool completed)
    {
        var progress = GetById(progressId);
        if (progress == null)
        {
            return false;
        }

        if (completed)
        {
            // Add to completed list if not already there
            if (!progress.CompletedStepOrders.Contains(stepOrder))
            {
                progress.CompletedStepOrders.Add(stepOrder);
            }
        }
        else
        {
            // Remove from completed list
            progress.CompletedStepOrders.Remove(stepOrder);
        }

        // Update last accessed timestamp
        progress.LastAccessedAt = DateTime.UtcNow;

        return Update(progress);
    }

    /// <summary>
    /// Updates the current step the user is on.
    /// </summary>
    /// <param name="progressId">The progress record ID.</param>
    /// <param name="stepOrder">The new current step order.</param>
    /// <returns>True if the update succeeded.</returns>
    public bool UpdateCurrentStep(ObjectId progressId, int stepOrder)
    {
        var progress = GetById(progressId);
        if (progress == null)
        {
            return false;
        }

        progress.CurrentStepOrder = stepOrder;
        progress.LastAccessedAt = DateTime.UtcNow;

        return Update(progress);
    }

    /// <summary>
    /// Marks a guide as complete by setting the CompletedAt timestamp.
    /// </summary>
    /// <param name="progressId">The progress record ID.</param>
    /// <returns>True if the update succeeded.</returns>
    public bool MarkGuideComplete(ObjectId progressId)
    {
        var progress = GetById(progressId);
        if (progress == null)
        {
            return false;
        }

        progress.CompletedAt = DateTime.UtcNow;
        progress.LastAccessedAt = DateTime.UtcNow;

        return Update(progress);
    }
}

using GuideViewer.Data.Entities;
using GuideViewer.Data.Models;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuideViewer.Core.Services;

/// <summary>
/// Service for managing user progress through guides.
/// Handles business logic and validation for progress tracking.
/// </summary>
public interface IProgressTrackingService
{
    /// <summary>
    /// Starts a new progress record for a user on a specific guide.
    /// </summary>
    /// <param name="guideId">The guide ID.</param>
    /// <param name="userId">The user ID.</param>
    /// <returns>The created progress record.</returns>
    /// <exception cref="InvalidOperationException">If progress already exists for this user+guide.</exception>
    Task<Progress> StartGuideAsync(ObjectId guideId, ObjectId userId);

    /// <summary>
    /// Gets the progress record for a specific user and guide.
    /// </summary>
    /// <param name="guideId">The guide ID.</param>
    /// <param name="userId">The user ID.</param>
    /// <returns>The progress record, or null if not found.</returns>
    Task<Progress?> GetProgressAsync(ObjectId guideId, ObjectId userId);

    /// <summary>
    /// Gets all active (not completed) progress records for a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>List of active progress records ordered by most recently accessed.</returns>
    Task<IEnumerable<Progress>> GetActiveProgressAsync(ObjectId userId);

    /// <summary>
    /// Gets all completed progress records for a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>List of completed progress records ordered by completion date.</returns>
    Task<IEnumerable<Progress>> GetCompletedProgressAsync(ObjectId userId);

    /// <summary>
    /// Marks a step as complete or incomplete.
    /// </summary>
    /// <param name="progressId">The progress record ID.</param>
    /// <param name="stepOrder">The step order number.</param>
    /// <param name="completed">True to mark complete, false to mark incomplete.</param>
    /// <param name="notes">Optional notes for this step completion.</param>
    /// <returns>True if successful.</returns>
    /// <exception cref="ArgumentException">If step order is invalid.</exception>
    Task<bool> CompleteStepAsync(ObjectId progressId, int stepOrder, bool completed, string? notes = null);

    /// <summary>
    /// Updates the current step the user is viewing.
    /// </summary>
    /// <param name="progressId">The progress record ID.</param>
    /// <param name="stepOrder">The new current step order.</param>
    /// <returns>True if successful.</returns>
    Task<bool> UpdateCurrentStepAsync(ObjectId progressId, int stepOrder);

    /// <summary>
    /// Marks a guide as complete.
    /// </summary>
    /// <param name="progressId">The progress record ID.</param>
    /// <returns>True if successful.</returns>
    Task<bool> MarkGuideCompleteAsync(ObjectId progressId);

    /// <summary>
    /// Gets completion statistics for a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>Statistics including completion rates and average times.</returns>
    Task<ProgressStatistics> GetStatisticsAsync(ObjectId userId);

    /// <summary>
    /// Calculates estimated time remaining for a guide based on progress.
    /// </summary>
    /// <param name="progress">The progress record.</param>
    /// <param name="guide">The guide being tracked.</param>
    /// <returns>Estimated minutes remaining, or null if cannot calculate.</returns>
    int? CalculateEstimatedTimeRemaining(Progress progress, Guide guide);

    /// <summary>
    /// Updates the active time spent on a guide.
    /// </summary>
    /// <param name="progressId">The progress record ID.</param>
    /// <param name="additionalSeconds">Seconds to add to total active time.</param>
    /// <returns>True if successful.</returns>
    Task<bool> UpdateActiveTimeAsync(ObjectId progressId, int additionalSeconds);
}

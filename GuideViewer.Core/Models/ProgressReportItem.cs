using LiteDB;
using System;

namespace GuideViewer.Core.Models;

/// <summary>
/// Represents a progress report item for admin monitoring.
/// Combines user, guide, and progress information for display.
/// </summary>
public class ProgressReportItem
{
    public required ObjectId ProgressId { get; set; }
    public required ObjectId UserId { get; set; }
    public required string UserName { get; set; }
    public required ObjectId GuideId { get; set; }
    public required string GuideTitle { get; set; }
    public required string GuideCategory { get; set; }
    public required int CurrentStep { get; set; }
    public required int CompletedSteps { get; set; }
    public required int TotalSteps { get; set; }
    public required DateTime StartedAt { get; set; }
    public required DateTime LastAccessedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public required int TotalTimeMinutes { get; set; }

    /// <summary>
    /// Gets the completion percentage (0-100).
    /// </summary>
    public double CompletionPercentage =>
        TotalSteps > 0 ? (double)CompletedSteps / TotalSteps * 100 : 0;

    /// <summary>
    /// Gets whether the guide is completed.
    /// </summary>
    public bool IsCompleted => CompletedAt.HasValue;

    /// <summary>
    /// Gets the status text (In Progress or Completed).
    /// </summary>
    public string Status => IsCompleted ? "Completed" : "In Progress";

    /// <summary>
    /// Gets a user-friendly display string for progress.
    /// </summary>
    public string ProgressDisplayText => $"{CompletedSteps}/{TotalSteps} steps";

    /// <summary>
    /// Gets a user-friendly display string for time spent.
    /// </summary>
    public string TimeSpentDisplayText
    {
        get
        {
            if (TotalTimeMinutes < 60)
                return $"{TotalTimeMinutes} min";

            var hours = TotalTimeMinutes / 60;
            var minutes = TotalTimeMinutes % 60;
            return minutes > 0 ? $"{hours}h {minutes}m" : $"{hours}h";
        }
    }

    /// <summary>
    /// Gets a user-friendly display string for last accessed time.
    /// </summary>
    public string LastAccessedDisplayText
    {
        get
        {
            var timeSpan = DateTime.UtcNow - LastAccessedAt;
            if (timeSpan.TotalMinutes < 1)
                return "Just now";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} min ago";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours}h ago";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays}d ago";
            return LastAccessedAt.ToLocalTime().ToString("MMM d, yyyy");
        }
    }

    /// <summary>
    /// Gets a user-friendly display string for started date.
    /// </summary>
    public string StartedDisplayText => StartedAt.ToLocalTime().ToString("MMM d, yyyy h:mm tt");

    /// <summary>
    /// Gets a user-friendly display string for completion date.
    /// </summary>
    public string CompletedDisplayText =>
        CompletedAt.HasValue
            ? CompletedAt.Value.ToLocalTime().ToString("MMM d, yyyy h:mm tt")
            : "Not completed";
}

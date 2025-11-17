namespace GuideViewer.Data.Models;

/// <summary>
/// Statistics about a user's progress across all guides.
/// </summary>
public class ProgressStatistics
{
    /// <summary>
    /// Total number of guides started by the user.
    /// </summary>
    public int TotalStarted { get; set; }

    /// <summary>
    /// Total number of guides completed by the user.
    /// </summary>
    public int TotalCompleted { get; set; }

    /// <summary>
    /// Number of guides currently in progress (not completed).
    /// </summary>
    public int CurrentlyInProgress { get; set; }

    /// <summary>
    /// Average completion time in minutes across all completed guides.
    /// Returns 0 if no guides have been completed.
    /// </summary>
    public double AverageCompletionTimeMinutes { get; set; }

    /// <summary>
    /// Percentage of started guides that have been completed.
    /// Range: 0.0 to 100.0
    /// </summary>
    public double CompletionRate { get; set; }
}

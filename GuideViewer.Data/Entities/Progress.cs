using LiteDB;
using System;
using System.Collections.Generic;

namespace GuideViewer.Data.Entities;

/// <summary>
/// Represents a user's progress through a guide.
/// Tracks which steps are completed, current position, and time spent.
/// </summary>
public class Progress
{
    /// <summary>
    /// Unique identifier for this progress record.
    /// </summary>
    [BsonId]
    public ObjectId Id { get; set; }

    /// <summary>
    /// ID of the guide being tracked.
    /// </summary>
    public ObjectId GuideId { get; set; }

    /// <summary>
    /// ID of the user tracking this progress.
    /// </summary>
    public ObjectId UserId { get; set; }

    /// <summary>
    /// The order number of the current step the user is on.
    /// </summary>
    public int CurrentStepOrder { get; set; }

    /// <summary>
    /// List of step order numbers that have been completed.
    /// Allows non-linear completion (users can skip/backtrack).
    /// </summary>
    public List<int> CompletedStepOrders { get; set; } = new List<int>();

    /// <summary>
    /// When the user started this guide.
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// Last time the user accessed this guide.
    /// Used for sorting "recently accessed" lists.
    /// </summary>
    public DateTime LastAccessedAt { get; set; }

    /// <summary>
    /// When the user finished this guide (null if not finished).
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// User notes for this progress session.
    /// Maximum 5000 characters.
    /// </summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Total active time spent on this guide in seconds.
    /// Used to track actual work time (excludes pauses).
    /// </summary>
    public int TotalActiveTimeSeconds { get; set; }
}

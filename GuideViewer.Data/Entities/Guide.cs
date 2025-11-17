using LiteDB;
using System;
using System.Collections.Generic;

namespace GuideViewer.Data.Entities;

/// <summary>
/// Represents an installation guide with multiple steps.
/// </summary>
public class Guide
{
    /// <summary>
    /// Gets or sets the unique identifier for this guide.
    /// </summary>
    [BsonId]
    public ObjectId Id { get; set; } = ObjectId.Empty;

    /// <summary>
    /// Gets or sets the title of the guide.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description/summary of the guide.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the category name this guide belongs to.
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the estimated completion time in minutes.
    /// </summary>
    public int EstimatedMinutes { get; set; }

    /// <summary>
    /// Gets or sets the list of steps in this guide.
    /// Steps are embedded documents, not separate collection entries.
    /// </summary>
    public List<Step> Steps { get; set; } = new List<Step>();

    /// <summary>
    /// Gets or sets when this guide was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when this guide was last modified.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the user role or ID who created this guide.
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets the total number of steps in this guide.
    /// </summary>
    [BsonIgnore]
    public int StepCount => Steps?.Count ?? 0;

    /// <summary>
    /// Gets a value indicating whether this guide has any steps.
    /// </summary>
    [BsonIgnore]
    public bool HasSteps => StepCount > 0;
}

using System;
using System.Collections.Generic;

namespace GuideViewer.Data.Entities;

/// <summary>
/// Represents a single step within an installation guide.
/// Steps are embedded within Guide documents (not a separate collection).
/// </summary>
public class Step
{
    /// <summary>
    /// Gets or sets the unique identifier for this step.
    /// Uses string (GUID) for easy embedding in LiteDB documents.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the order/position of this step in the guide (1-based).
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Gets or sets the title/name of this step.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the rich text content for this step.
    /// Stored as RTF (RichEditBox native format) or plain text.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of image file IDs stored in LiteDB FileStorage.
    /// Each string is a LiteDB file ID that can be used to retrieve the image.
    /// </summary>
    public List<string> ImageIds { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets when this step was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when this step was last modified.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

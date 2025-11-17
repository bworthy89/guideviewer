using LiteDB;
using System;

namespace GuideViewer.Data.Entities;

/// <summary>
/// Represents a category for organizing installation guides.
/// </summary>
public class Category
{
    /// <summary>
    /// Gets or sets the unique identifier for this category.
    /// </summary>
    [BsonId]
    public ObjectId Id { get; set; } = ObjectId.Empty;

    /// <summary>
    /// Gets or sets the name of the category.
    /// Must be unique across all categories.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of this category.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Segoe Fluent Icon glyph for this category.
    /// Example: "\uE8F1" for a document icon.
    /// </summary>
    public string IconGlyph { get; set; } = "\uE8F1"; // Default: document icon

    /// <summary>
    /// Gets or sets the color for the category badge (hex format).
    /// Example: "#0078D4" for Windows blue.
    /// </summary>
    public string Color { get; set; } = "#0078D4"; // Default: Windows blue

    /// <summary>
    /// Gets or sets when this category was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when this category was last modified.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

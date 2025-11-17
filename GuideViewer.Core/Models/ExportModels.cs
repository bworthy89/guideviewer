using System;
using System.Collections.Generic;

namespace GuideViewer.Core.Models;

/// <summary>
/// Root export container for guide export.
/// </summary>
public class GuideExport
{
    /// <summary>
    /// Gets or sets the export format version.
    /// </summary>
    public string Version { get; set; } = "1.0";

    /// <summary>
    /// Gets or sets when this export was created.
    /// </summary>
    public DateTime ExportDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the exported guide.
    /// </summary>
    public GuideExportData? Guide { get; set; }
}

/// <summary>
/// Root export container for multiple guides.
/// </summary>
public class GuidesExport
{
    /// <summary>
    /// Gets or sets the export format version.
    /// </summary>
    public string Version { get; set; } = "1.0";

    /// <summary>
    /// Gets or sets when this export was created.
    /// </summary>
    public DateTime ExportDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the number of guides exported.
    /// </summary>
    public int GuideCount { get; set; }

    /// <summary>
    /// Gets or sets the exported guides.
    /// </summary>
    public List<GuideExportData> Guides { get; set; } = new();
}

/// <summary>
/// Guide data for export (serializable, no LiteDB dependencies).
/// </summary>
public class GuideExportData
{
    /// <summary>
    /// Gets or sets the guide ID (for reference only, new ID generated on import).
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the category name.
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the estimated completion time in minutes.
    /// </summary>
    public int EstimatedMinutes { get; set; }

    /// <summary>
    /// Gets or sets the steps.
    /// </summary>
    public List<StepExportData> Steps { get; set; } = new();

    /// <summary>
    /// Gets or sets when the guide was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets when the guide was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets who created the guide.
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;
}

/// <summary>
/// Step data for export (serializable, no LiteDB dependencies).
/// </summary>
public class StepExportData
{
    /// <summary>
    /// Gets or sets the step ID (for reference only).
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the order/position.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the content (RTF or plain text).
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the images as Base64 strings (optional, for embedded export).
    /// Dictionary key is the original image ID, value is Base64 data.
    /// </summary>
    public Dictionary<string, string>? ImagesBase64 { get; set; }

    /// <summary>
    /// Gets or sets the image file names (for ZIP export).
    /// Maps original ImageId to filename in ZIP.
    /// </summary>
    public Dictionary<string, string>? ImageFileNames { get; set; }

    /// <summary>
    /// Gets or sets when the step was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets when the step was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}

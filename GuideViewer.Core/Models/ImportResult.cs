using LiteDB;
using System.Collections.Generic;
using System.Linq;

namespace GuideViewer.Core.Models;

/// <summary>
/// Result of a guide import operation.
/// </summary>
public class ImportResult
{
    /// <summary>
    /// Gets or sets whether the import was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the IDs of successfully imported guides.
    /// </summary>
    public List<ObjectId> ImportedGuideIds { get; set; } = new();

    /// <summary>
    /// Gets or sets error messages encountered during import.
    /// </summary>
    public List<string> ErrorMessages { get; set; } = new();

    /// <summary>
    /// Gets or sets warning messages (non-fatal issues).
    /// </summary>
    public List<string> WarningMessages { get; set; } = new();

    /// <summary>
    /// Gets or sets the number of duplicate guides skipped.
    /// </summary>
    public int DuplicatesSkipped { get; set; }

    /// <summary>
    /// Gets or sets the number of guides successfully imported.
    /// </summary>
    public int GuidesImported { get; set; }

    /// <summary>
    /// Gets or sets the number of images imported.
    /// </summary>
    public int ImagesImported { get; set; }

    /// <summary>
    /// Gets a value indicating whether there are any errors.
    /// </summary>
    public bool HasErrors => ErrorMessages.Any();

    /// <summary>
    /// Gets a value indicating whether there are any warnings.
    /// </summary>
    public bool HasWarnings => WarningMessages.Any();

    /// <summary>
    /// Creates a successful import result.
    /// </summary>
    public static ImportResult CreateSuccess(List<ObjectId> guideIds, int imagesImported = 0)
    {
        return new ImportResult
        {
            Success = true,
            ImportedGuideIds = guideIds,
            GuidesImported = guideIds.Count,
            ImagesImported = imagesImported
        };
    }

    /// <summary>
    /// Creates a failed import result.
    /// </summary>
    public static ImportResult CreateFailure(string errorMessage)
    {
        return new ImportResult
        {
            Success = false,
            ErrorMessages = new List<string> { errorMessage }
        };
    }

    /// <summary>
    /// Creates a partial success result (some imports succeeded, some failed).
    /// </summary>
    public static ImportResult CreatePartialSuccess(List<ObjectId> guideIds, List<string> errors, List<string> warnings)
    {
        return new ImportResult
        {
            Success = guideIds.Any(),
            ImportedGuideIds = guideIds,
            GuidesImported = guideIds.Count,
            ErrorMessages = errors,
            WarningMessages = warnings
        };
    }

    /// <summary>
    /// Gets a summary message for display to the user.
    /// </summary>
    public string GetSummaryMessage()
    {
        if (!Success && !ImportedGuideIds.Any())
        {
            return $"Import failed: {string.Join(", ", ErrorMessages)}";
        }

        var parts = new List<string>();

        if (GuidesImported > 0)
        {
            parts.Add($"{GuidesImported} guide(s) imported successfully");
        }

        if (ImagesImported > 0)
        {
            parts.Add($"{ImagesImported} image(s) imported");
        }

        if (DuplicatesSkipped > 0)
        {
            parts.Add($"{DuplicatesSkipped} duplicate(s) skipped");
        }

        if (HasWarnings)
        {
            parts.Add($"{WarningMessages.Count} warning(s)");
        }

        if (HasErrors)
        {
            parts.Add($"{ErrorMessages.Count} error(s)");
        }

        return string.Join(", ", parts);
    }
}

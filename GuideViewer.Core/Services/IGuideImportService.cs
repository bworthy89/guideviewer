using GuideViewer.Core.Models;
using System.Threading.Tasks;

namespace GuideViewer.Core.Services;

/// <summary>
/// Service for importing guides from various formats (JSON, ZIP).
/// </summary>
public interface IGuideImportService
{
    /// <summary>
    /// Imports a guide from JSON string.
    /// </summary>
    /// <param name="json">The JSON string containing guide data.</param>
    /// <param name="duplicateHandling">How to handle duplicate guides (skip, overwrite, rename).</param>
    /// <returns>Import result with success/failure details.</returns>
    Task<ImportResult> ImportGuideFromJsonAsync(string json, DuplicateHandling duplicateHandling = DuplicateHandling.Skip);

    /// <summary>
    /// Imports guides from a JSON file.
    /// </summary>
    /// <param name="filePath">The file path to import from.</param>
    /// <param name="duplicateHandling">How to handle duplicate guides.</param>
    /// <returns>Import result with success/failure details.</returns>
    Task<ImportResult> ImportGuidesFromFileAsync(string filePath, DuplicateHandling duplicateHandling = DuplicateHandling.Skip);

    /// <summary>
    /// Imports a guide from a ZIP package.
    /// </summary>
    /// <param name="zipData">The ZIP file contents.</param>
    /// <param name="duplicateHandling">How to handle duplicate guides.</param>
    /// <returns>Import result with success/failure details.</returns>
    Task<ImportResult> ImportGuideFromZipAsync(byte[] zipData, DuplicateHandling duplicateHandling = DuplicateHandling.Skip);

    /// <summary>
    /// Validates an import file without importing.
    /// </summary>
    /// <param name="filePath">The file path to validate.</param>
    /// <returns>True if file is valid for import, false otherwise.</returns>
    Task<bool> ValidateImportFileAsync(string filePath);
}

/// <summary>
/// How to handle duplicate guides during import.
/// </summary>
public enum DuplicateHandling
{
    /// <summary>
    /// Skip duplicate guides (don't import).
    /// </summary>
    Skip,

    /// <summary>
    /// Overwrite existing guides with imported data.
    /// </summary>
    Overwrite,

    /// <summary>
    /// Rename imported guide (append number to title).
    /// </summary>
    Rename
}

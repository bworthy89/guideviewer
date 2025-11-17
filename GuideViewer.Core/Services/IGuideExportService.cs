using LiteDB;
using System.Threading.Tasks;

namespace GuideViewer.Core.Services;

/// <summary>
/// Service for exporting guides to various formats (JSON, ZIP).
/// </summary>
public interface IGuideExportService
{
    /// <summary>
    /// Exports a single guide to JSON format.
    /// </summary>
    /// <param name="guideId">The guide ID to export.</param>
    /// <param name="includeImages">Whether to include images as Base64 in JSON.</param>
    /// <returns>JSON string representation of the guide.</returns>
    /// <exception cref="ArgumentException">Thrown if guide not found.</exception>
    Task<string> ExportGuideToJsonAsync(ObjectId guideId, bool includeImages = true);

    /// <summary>
    /// Exports all guides to JSON format.
    /// </summary>
    /// <param name="includeImages">Whether to include images as Base64 in JSON.</param>
    /// <returns>JSON array string containing all guides.</returns>
    Task<string> ExportAllGuidesToJsonAsync(bool includeImages = true);

    /// <summary>
    /// Exports a single guide to a JSON file.
    /// </summary>
    /// <param name="guideId">The guide ID to export.</param>
    /// <param name="filePath">The destination file path.</param>
    /// <param name="includeImages">Whether to include images as Base64.</param>
    /// <returns>True if successful, false otherwise.</returns>
    Task<bool> ExportGuideToFileAsync(ObjectId guideId, string filePath, bool includeImages = true);

    /// <summary>
    /// Exports all guides to a JSON file.
    /// </summary>
    /// <param name="filePath">The destination file path.</param>
    /// <param name="includeImages">Whether to include images as Base64.</param>
    /// <returns>True if successful, false otherwise.</returns>
    Task<bool> ExportAllGuidesToFileAsync(string filePath, bool includeImages = true);

    /// <summary>
    /// Exports a guide with images to a ZIP package.
    /// The ZIP contains a JSON file and separate image files.
    /// </summary>
    /// <param name="guideId">The guide ID to export.</param>
    /// <returns>ZIP file contents as byte array.</returns>
    /// <exception cref="ArgumentException">Thrown if guide not found.</exception>
    Task<byte[]> ExportGuideWithImagesAsync(ObjectId guideId);
}

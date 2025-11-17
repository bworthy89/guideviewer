using System.IO;
using System.Threading.Tasks;

namespace GuideViewer.Core.Services;

/// <summary>
/// Service for managing image storage in LiteDB FileStorage.
/// </summary>
public interface IImageStorageService
{
    /// <summary>
    /// Uploads an image to LiteDB FileStorage.
    /// </summary>
    /// <param name="imageStream">The image stream.</param>
    /// <param name="fileName">The original file name.</param>
    /// <returns>The file ID assigned by LiteDB.</returns>
    /// <exception cref="ArgumentException">Thrown if image is invalid.</exception>
    Task<string> UploadImageAsync(Stream imageStream, string fileName);

    /// <summary>
    /// Gets an image from LiteDB FileStorage.
    /// </summary>
    /// <param name="fileId">The file ID.</param>
    /// <returns>The image stream, or null if not found.</returns>
    Task<Stream?> GetImageAsync(string fileId);

    /// <summary>
    /// Deletes an image from LiteDB FileStorage.
    /// </summary>
    /// <param name="fileId">The file ID to delete.</param>
    /// <returns>True if deleted, false if not found.</returns>
    Task<bool> DeleteImageAsync(string fileId);

    /// <summary>
    /// Gets metadata about an image.
    /// </summary>
    /// <param name="fileId">The file ID.</param>
    /// <returns>Image metadata (size, type, upload date), or null if not found.</returns>
    Task<ImageMetadata?> GetImageMetadataAsync(string fileId);

    /// <summary>
    /// Validates an image stream (size and format).
    /// </summary>
    /// <param name="imageStream">The image stream to validate.</param>
    /// <param name="fileName">The file name for format detection.</param>
    /// <returns>Validation result with error message if invalid.</returns>
    Task<ImageValidationResult> ValidateImageAsync(Stream imageStream, string fileName);
}

/// <summary>
/// Metadata about a stored image.
/// </summary>
public class ImageMetadata
{
    /// <summary>
    /// Gets or sets the file ID.
    /// </summary>
    public string FileId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the original file name.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file size in bytes.
    /// </summary>
    public long SizeInBytes { get; set; }

    /// <summary>
    /// Gets or sets the MIME type.
    /// </summary>
    public string MimeType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the image was uploaded.
    /// </summary>
    public DateTime UploadedAt { get; set; }
}

/// <summary>
/// Result of image validation.
/// </summary>
public class ImageValidationResult
{
    /// <summary>
    /// Gets or sets whether the image is valid.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Gets or sets the error message if invalid.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    public static ImageValidationResult Success() => new ImageValidationResult { IsValid = true };

    /// <summary>
    /// Creates a failed validation result.
    /// </summary>
    public static ImageValidationResult Fail(string errorMessage) => new ImageValidationResult
    {
        IsValid = false,
        ErrorMessage = errorMessage
    };
}

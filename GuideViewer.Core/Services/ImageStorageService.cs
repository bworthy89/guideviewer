using GuideViewer.Data.Services;
using LiteDB;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GuideViewer.Core.Services;

/// <summary>
/// Service for managing image storage in LiteDB FileStorage.
/// </summary>
public class ImageStorageService : IImageStorageService
{
    private readonly DatabaseService _databaseService;
    private const long MaxImageSizeBytes = 10 * 1024 * 1024; // 10MB
    private static readonly string[] AllowedExtensions = { ".png", ".jpg", ".jpeg", ".bmp" };
    private static readonly string[] AllowedMimeTypes = { "image/png", "image/jpeg", "image/bmp" };

    public ImageStorageService(DatabaseService databaseService)
    {
        _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
    }

    /// <inheritdoc/>
    public async Task<string> UploadImageAsync(Stream imageStream, string fileName)
    {
        if (imageStream == null)
        {
            throw new ArgumentNullException(nameof(imageStream));
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("File name cannot be empty.", nameof(fileName));
        }

        // Validate the image
        var validationResult = await ValidateImageAsync(imageStream, fileName);
        if (!validationResult.IsValid)
        {
            throw new ArgumentException(validationResult.ErrorMessage);
        }

        // Reset stream position after validation
        if (imageStream.CanSeek)
        {
            imageStream.Position = 0;
        }

        try
        {
            // Generate unique file ID
            var fileId = $"img_{Guid.NewGuid():N}";

            // Upload to LiteDB FileStorage
            var fileInfo = _databaseService.Database.FileStorage.Upload(fileId, fileName, imageStream);

            Log.Information("Image uploaded: {FileId}, Size: {Size} bytes", fileId, fileInfo.Length);

            return fileId;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to upload image: {FileName}", fileName);
            throw;
        }
    }

    /// <inheritdoc/>
    public Task<Stream?> GetImageAsync(string fileId)
    {
        if (string.IsNullOrWhiteSpace(fileId))
        {
            throw new ArgumentException("File ID cannot be empty.", nameof(fileId));
        }

        try
        {
            var fileInfo = _databaseService.Database.FileStorage.FindById(fileId);
            if (fileInfo == null)
            {
                Log.Warning("Image not found: {FileId}", fileId);
                return Task.FromResult<Stream?>(null);
            }

            var memoryStream = new MemoryStream();
            fileInfo.CopyTo(memoryStream);
            memoryStream.Position = 0;

            return Task.FromResult<Stream?>(memoryStream);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve image: {FileId}", fileId);
            throw;
        }
    }

    /// <inheritdoc/>
    public Task<bool> DeleteImageAsync(string fileId)
    {
        if (string.IsNullOrWhiteSpace(fileId))
        {
            throw new ArgumentException("File ID cannot be empty.", nameof(fileId));
        }

        try
        {
            var deleted = _databaseService.Database.FileStorage.Delete(fileId);

            if (deleted)
            {
                Log.Information("Image deleted: {FileId}", fileId);
            }
            else
            {
                Log.Warning("Image not found for deletion: {FileId}", fileId);
            }

            return Task.FromResult(deleted);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to delete image: {FileId}", fileId);
            throw;
        }
    }

    /// <inheritdoc/>
    public Task<ImageMetadata?> GetImageMetadataAsync(string fileId)
    {
        if (string.IsNullOrWhiteSpace(fileId))
        {
            throw new ArgumentException("File ID cannot be empty.", nameof(fileId));
        }

        try
        {
            var fileInfo = _databaseService.Database.FileStorage.FindById(fileId);
            if (fileInfo == null)
            {
                return Task.FromResult<ImageMetadata?>(null);
            }

            var metadata = new ImageMetadata
            {
                FileId = fileInfo.Id,
                FileName = fileInfo.Filename,
                SizeInBytes = fileInfo.Length,
                MimeType = fileInfo.MimeType,
                UploadedAt = fileInfo.UploadDate
            };

            return Task.FromResult<ImageMetadata?>(metadata);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to get image metadata: {FileId}", fileId);
            throw;
        }
    }

    /// <inheritdoc/>
    public Task<ImageValidationResult> ValidateImageAsync(Stream imageStream, string fileName)
    {
        if (imageStream == null)
        {
            return Task.FromResult(ImageValidationResult.Fail("Image stream is null."));
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            return Task.FromResult(ImageValidationResult.Fail("File name is empty."));
        }

        // Check file extension
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            return Task.FromResult(ImageValidationResult.Fail(
                $"Invalid file format. Allowed formats: {string.Join(", ", AllowedExtensions)}"));
        }

        // Check file size
        if (imageStream.Length > MaxImageSizeBytes)
        {
            var maxSizeMB = MaxImageSizeBytes / (1024 * 1024);
            return Task.FromResult(ImageValidationResult.Fail(
                $"Image size exceeds maximum allowed size of {maxSizeMB}MB."));
        }

        // Check if stream is readable
        if (!imageStream.CanRead)
        {
            return Task.FromResult(ImageValidationResult.Fail("Image stream is not readable."));
        }

        // Basic validation passed
        return Task.FromResult(ImageValidationResult.Success());
    }
}

using GuideViewer.Core.Models;
using GuideViewer.Data.Entities;
using GuideViewer.Data.Repositories;
using LiteDB;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace GuideViewer.Core.Services;

/// <summary>
/// Service for exporting guides to various formats.
/// </summary>
public class GuideExportService : IGuideExportService
{
    private readonly GuideRepository _guideRepository;
    private readonly IImageStorageService _imageStorageService;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true, // Pretty-print for readability
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="GuideExportService"/> class.
    /// </summary>
    public GuideExportService(GuideRepository guideRepository, IImageStorageService imageStorageService)
    {
        _guideRepository = guideRepository ?? throw new ArgumentNullException(nameof(guideRepository));
        _imageStorageService = imageStorageService ?? throw new ArgumentNullException(nameof(imageStorageService));
    }

    /// <inheritdoc/>
    public async Task<string> ExportGuideToJsonAsync(ObjectId guideId, bool includeImages = true)
    {
        Log.Information("Exporting guide {GuideId} to JSON (includeImages={IncludeImages})", guideId, includeImages);

        var guide = _guideRepository.GetById(guideId);
        if (guide == null)
        {
            Log.Error("Guide {GuideId} not found for export", guideId);
            throw new ArgumentException($"Guide with ID {guideId} not found.", nameof(guideId));
        }

        var exportData = await ConvertToExportDataAsync(guide, includeImages);
        var export = new GuideExport
        {
            Version = "1.0",
            ExportDate = DateTime.UtcNow,
            Guide = exportData
        };

        var json = System.Text.Json.JsonSerializer.Serialize(export, _jsonOptions);
        Log.Information("Successfully exported guide '{Title}' to JSON ({Length} characters)", guide.Title, json.Length);
        return json;
    }

    /// <inheritdoc/>
    public async Task<string> ExportAllGuidesToJsonAsync(bool includeImages = true)
    {
        Log.Information("Exporting all guides to JSON (includeImages={IncludeImages})", includeImages);

        var guides = _guideRepository.GetAll().ToList();
        Log.Information("Found {Count} guides to export", guides.Count);

        var exportTasks = guides.Select(g => ConvertToExportDataAsync(g, includeImages));
        var exportedGuides = await Task.WhenAll(exportTasks);

        var export = new GuidesExport
        {
            Version = "1.0",
            ExportDate = DateTime.UtcNow,
            GuideCount = guides.Count,
            Guides = exportedGuides.ToList()
        };

        var json = System.Text.Json.JsonSerializer.Serialize(export, _jsonOptions);
        Log.Information("Successfully exported {Count} guides to JSON ({Length} characters)", guides.Count, json.Length);
        return json;
    }

    /// <inheritdoc/>
    public async Task<bool> ExportGuideToFileAsync(ObjectId guideId, string filePath, bool includeImages = true)
    {
        try
        {
            Log.Information("Exporting guide {GuideId} to file: {FilePath}", guideId, filePath);

            var json = await ExportGuideToJsonAsync(guideId, includeImages);
            await File.WriteAllTextAsync(filePath, json);

            Log.Information("Successfully exported guide to file: {FilePath}", filePath);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to export guide {GuideId} to file: {FilePath}", guideId, filePath);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ExportAllGuidesToFileAsync(string filePath, bool includeImages = true)
    {
        try
        {
            Log.Information("Exporting all guides to file: {FilePath}", filePath);

            var json = await ExportAllGuidesToJsonAsync(includeImages);
            await File.WriteAllTextAsync(filePath, json);

            Log.Information("Successfully exported all guides to file: {FilePath}", filePath);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to export all guides to file: {FilePath}", filePath);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<byte[]> ExportGuideWithImagesAsync(ObjectId guideId)
    {
        Log.Information("Exporting guide {GuideId} to ZIP package with images", guideId);

        var guide = _guideRepository.GetById(guideId);
        if (guide == null)
        {
            Log.Error("Guide {GuideId} not found for ZIP export", guideId);
            throw new ArgumentException($"Guide with ID {guideId} not found.", nameof(guideId));
        }

        using var memoryStream = new MemoryStream();
        int imageCounter = 0;
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            // Export guide data to JSON (without Base64 images, we'll include them as files)
            var exportData = await ConvertToExportDataAsync(guide, includeImages: false, includeImageFileNames: true);
            var export = new GuideExport
            {
                Version = "1.0",
                ExportDate = DateTime.UtcNow,
                Guide = exportData
            };

            // Add guide JSON to ZIP
            var guideEntry = archive.CreateEntry("guide.json");
            using (var entryStream = guideEntry.Open())
            {
                var json = System.Text.Json.JsonSerializer.Serialize(export, _jsonOptions);
                using var writer = new StreamWriter(entryStream);
                await writer.WriteAsync(json);
            }

            // Add images to ZIP
            foreach (var step in guide.Steps)
            {
                foreach (var imageId in step.ImageIds)
                {
                    imageCounter++;
                    var imageStream = await _imageStorageService.GetImageAsync(imageId);
                    if (imageStream != null)
                    {
                        var metadata = await _imageStorageService.GetImageMetadataAsync(imageId);
                        var extension = GetExtensionFromMimeType(metadata?.MimeType ?? "image/png");
                        var fileName = $"images/step_{step.Order}_image_{imageCounter}{extension}";

                        var imageEntry = archive.CreateEntry(fileName);
                        using var imageEntryStream = imageEntry.Open();
                        await imageStream.CopyToAsync(imageEntryStream);
                        imageStream.Dispose();

                        Log.Debug("Added image {FileName} to ZIP (ImageId: {ImageId})", fileName, imageId);
                    }
                    else
                    {
                        Log.Warning("Image {ImageId} not found for step {StepOrder}", imageId, step.Order);
                    }
                }
            }
        }

        var zipBytes = memoryStream.ToArray();
        Log.Information("Successfully exported guide '{Title}' to ZIP package ({Size} bytes, {Images} images)",
            guide.Title, zipBytes.Length, imageCounter);
        return zipBytes;
    }

    /// <summary>
    /// Converts a Guide entity to export data.
    /// </summary>
    private async Task<GuideExportData> ConvertToExportDataAsync(Guide guide, bool includeImages, bool includeImageFileNames = false)
    {
        var exportData = new GuideExportData
        {
            Id = guide.Id.ToString(),
            Title = guide.Title,
            Description = guide.Description,
            Category = guide.Category,
            EstimatedMinutes = guide.EstimatedMinutes,
            CreatedAt = guide.CreatedAt,
            UpdatedAt = guide.UpdatedAt,
            CreatedBy = guide.CreatedBy,
            Steps = new List<StepExportData>()
        };

        var imageFileCounter = 0;
        foreach (var step in guide.Steps)
        {
            var stepData = new StepExportData
            {
                Id = step.Id,
                Order = step.Order,
                Title = step.Title,
                Content = step.Content,
                CreatedAt = step.CreatedAt,
                UpdatedAt = step.UpdatedAt
            };

            // Include images as Base64 or file names
            if ((includeImages || includeImageFileNames) && step.ImageIds.Count > 0)
            {
                if (includeImages)
                {
                    stepData.ImagesBase64 = new Dictionary<string, string>();
                }

                if (includeImageFileNames)
                {
                    stepData.ImageFileNames = new Dictionary<string, string>();
                }

                foreach (var imageId in step.ImageIds)
                {
                    imageFileCounter++;
                    var imageStream = await _imageStorageService.GetImageAsync(imageId);
                    if (imageStream != null)
                    {
                        if (includeImages)
                        {
                            // Convert to Base64
                            using var memoryStream = new MemoryStream();
                            await imageStream.CopyToAsync(memoryStream);
                            var base64 = Convert.ToBase64String(memoryStream.ToArray());
                            stepData.ImagesBase64![imageId] = base64;
                        }

                        if (includeImageFileNames)
                        {
                            var metadata = await _imageStorageService.GetImageMetadataAsync(imageId);
                            var extension = GetExtensionFromMimeType(metadata?.MimeType ?? "image/png");
                            var fileName = $"step_{step.Order}_image_{imageFileCounter}{extension}";
                            stepData.ImageFileNames![imageId] = fileName;
                        }

                        imageStream.Dispose();
                    }
                    else
                    {
                        Log.Warning("Image {ImageId} not found in step '{StepTitle}'", imageId, step.Title);
                    }
                }
            }

            exportData.Steps.Add(stepData);
        }

        return exportData;
    }

    /// <summary>
    /// Gets file extension from MIME type.
    /// </summary>
    private static string GetExtensionFromMimeType(string mimeType)
    {
        return mimeType.ToLowerInvariant() switch
        {
            "image/png" => ".png",
            "image/jpeg" => ".jpg",
            "image/jpg" => ".jpg",
            "image/bmp" => ".bmp",
            "image/gif" => ".gif",
            _ => ".png" // Default to PNG
        };
    }
}

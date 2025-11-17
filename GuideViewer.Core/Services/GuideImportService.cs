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
/// Service for importing guides from various formats.
/// </summary>
public class GuideImportService : IGuideImportService
{
    private readonly GuideRepository _guideRepository;
    private readonly CategoryRepository _categoryRepository;
    private readonly IImageStorageService _imageStorageService;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="GuideImportService"/> class.
    /// </summary>
    public GuideImportService(
        GuideRepository guideRepository,
        CategoryRepository categoryRepository,
        IImageStorageService imageStorageService)
    {
        _guideRepository = guideRepository ?? throw new ArgumentNullException(nameof(guideRepository));
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        _imageStorageService = imageStorageService ?? throw new ArgumentNullException(nameof(imageStorageService));
    }

    /// <inheritdoc/>
    public async Task<ImportResult> ImportGuideFromJsonAsync(string json, DuplicateHandling duplicateHandling = DuplicateHandling.Skip)
    {
        Log.Information("Importing guide(s) from JSON (duplicateHandling={DuplicateHandling})", duplicateHandling);

        try
        {
            // Try to parse as single guide first
            if (TryDeserialize<GuideExport>(json, out var singleGuideExport) && singleGuideExport?.Guide != null)
            {
                Log.Information("Detected single guide export format");
                return await ImportGuideDataAsync(singleGuideExport.Guide, duplicateHandling);
            }

            // Try to parse as multiple guides
            if (TryDeserialize<GuidesExport>(json, out var multipleGuidesExport) && multipleGuidesExport?.Guides != null)
            {
                Log.Information("Detected multiple guides export format ({Count} guides)", multipleGuidesExport.Guides.Count);
                return await ImportMultipleGuidesAsync(multipleGuidesExport.Guides, duplicateHandling);
            }

            // Try to parse as raw GuideExportData
            if (TryDeserialize<GuideExportData>(json, out var rawGuideData) && rawGuideData != null)
            {
                Log.Information("Detected raw guide data format");
                return await ImportGuideDataAsync(rawGuideData, duplicateHandling);
            }

            Log.Error("Failed to parse JSON: Unrecognized format");
            return ImportResult.CreateFailure("Invalid JSON format. Expected GuideExport, GuidesExport, or GuideExportData.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to import guide from JSON");
            return ImportResult.CreateFailure($"Import failed: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<ImportResult> ImportGuidesFromFileAsync(string filePath, DuplicateHandling duplicateHandling = DuplicateHandling.Skip)
    {
        Log.Information("Importing guides from file: {FilePath}", filePath);

        try
        {
            if (!File.Exists(filePath))
            {
                return ImportResult.CreateFailure($"File not found: {filePath}");
            }

            var extension = Path.GetExtension(filePath).ToLowerInvariant();

            if (extension == ".zip")
            {
                var zipData = await File.ReadAllBytesAsync(filePath);
                return await ImportGuideFromZipAsync(zipData, duplicateHandling);
            }
            else if (extension == ".json")
            {
                var json = await File.ReadAllTextAsync(filePath);
                return await ImportGuideFromJsonAsync(json, duplicateHandling);
            }
            else
            {
                return ImportResult.CreateFailure($"Unsupported file format: {extension}. Expected .json or .zip");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to import guides from file: {FilePath}", filePath);
            return ImportResult.CreateFailure($"Import failed: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<ImportResult> ImportGuideFromZipAsync(byte[] zipData, DuplicateHandling duplicateHandling = DuplicateHandling.Skip)
    {
        Log.Information("Importing guide from ZIP package");

        try
        {
            using var memoryStream = new MemoryStream(zipData);
            using var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read);

            // Find guide.json in ZIP
            var guideEntry = archive.GetEntry("guide.json");
            if (guideEntry == null)
            {
                return ImportResult.CreateFailure("ZIP package does not contain guide.json");
            }

            // Read guide JSON
            using var guideStream = guideEntry.Open();
            using var reader = new StreamReader(guideStream);
            var json = await reader.ReadToEndAsync();

            // Parse guide data
            var guideExport = System.Text.Json.JsonSerializer.Deserialize<GuideExport>(json, _jsonOptions);
            if (guideExport?.Guide == null)
            {
                return ImportResult.CreateFailure("Invalid guide.json format");
            }

            // Import images from ZIP
            var imageMap = new Dictionary<string, string>(); // Maps: oldImageFileName -> newImageId
            var imagesFolder = archive.Entries.Where(e => e.FullName.StartsWith("images/", StringComparison.OrdinalIgnoreCase));

            foreach (var imageEntry in imagesFolder)
            {
                if (imageEntry.Length == 0) continue; // Skip directories

                using var imageStream = imageEntry.Open();
                using var imageMemoryStream = new MemoryStream();
                await imageStream.CopyToAsync(imageMemoryStream);
                imageMemoryStream.Position = 0;

                var fileName = Path.GetFileName(imageEntry.FullName);
                var newImageId = await _imageStorageService.UploadImageAsync(imageMemoryStream, fileName);
                imageMap[fileName] = newImageId;

                Log.Debug("Imported image {FileName} -> {ImageId}", fileName, newImageId);
            }

            // Map image file names to new IDs in guide data
            foreach (var step in guideExport.Guide.Steps)
            {
                if (step.ImageFileNames != null)
                {
                    foreach (var (oldImageId, fileName) in step.ImageFileNames)
                    {
                        if (imageMap.TryGetValue(fileName, out var newImageId))
                        {
                            // We'll replace ImageIds during guide creation
                            // For now, store mapping in ImagesBase64 (repurpose this field)
                            step.ImagesBase64 ??= new Dictionary<string, string>();
                            step.ImagesBase64[oldImageId] = newImageId; // Store newImageId instead of Base64
                        }
                    }
                }
            }

            // Import guide
            var result = await ImportGuideDataAsync(guideExport.Guide, duplicateHandling, isZipImport: true);
            result.ImagesImported = imageMap.Count;

            Log.Information("Successfully imported guide from ZIP ({Images} images)", imageMap.Count);
            return result;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to import guide from ZIP");
            return ImportResult.CreateFailure($"ZIP import failed: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ValidateImportFileAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return false;
            }

            var extension = Path.GetExtension(filePath).ToLowerInvariant();

            if (extension == ".json")
            {
                var json = await File.ReadAllTextAsync(filePath);
                return TryDeserialize<GuideExport>(json, out _) ||
                       TryDeserialize<GuidesExport>(json, out _) ||
                       TryDeserialize<GuideExportData>(json, out _);
            }
            else if (extension == ".zip")
            {
                var zipData = await File.ReadAllBytesAsync(filePath);
                using var memoryStream = new MemoryStream(zipData);
                using var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read);
                return archive.GetEntry("guide.json") != null;
            }

            return false;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to validate import file: {FilePath}", filePath);
            return false;
        }
    }

    /// <summary>
    /// Imports multiple guides.
    /// </summary>
    private async Task<ImportResult> ImportMultipleGuidesAsync(List<GuideExportData> guides, DuplicateHandling duplicateHandling)
    {
        var importedIds = new List<ObjectId>();
        var errors = new List<string>();
        var warnings = new List<string>();
        var duplicatesSkipped = 0;

        foreach (var guideData in guides)
        {
            try
            {
                var importResult = await ImportGuideDataAsync(guideData, duplicateHandling);
                if (importResult.Success)
                {
                    importedIds.AddRange(importResult.ImportedGuideIds);
                }
                else
                {
                    errors.AddRange(importResult.ErrorMessages);
                }

                warnings.AddRange(importResult.WarningMessages);
                duplicatesSkipped += importResult.DuplicatesSkipped;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to import guide '{Title}'", guideData.Title);
                errors.Add($"Failed to import '{guideData.Title}': {ex.Message}");
            }
        }

        var result = ImportResult.CreatePartialSuccess(importedIds, errors, warnings);
        result.DuplicatesSkipped = duplicatesSkipped;
        return result;
    }

    /// <summary>
    /// Imports a single guide from export data.
    /// </summary>
    private async Task<ImportResult> ImportGuideDataAsync(GuideExportData guideData, DuplicateHandling duplicateHandling, bool isZipImport = false)
    {
        Log.Information("Importing guide: {Title}", guideData.Title);

        // Validate required fields
        if (string.IsNullOrWhiteSpace(guideData.Title))
        {
            return ImportResult.CreateFailure("Guide title is required");
        }

        // Check for duplicate
        var existingGuide = _guideRepository.GetAll()
            .FirstOrDefault(g => g.Title.Equals(guideData.Title, StringComparison.OrdinalIgnoreCase));

        if (existingGuide != null)
        {
            switch (duplicateHandling)
            {
                case DuplicateHandling.Skip:
                    Log.Information("Skipping duplicate guide: {Title}", guideData.Title);
                    return new ImportResult
                    {
                        Success = false,
                        DuplicatesSkipped = 1,
                        WarningMessages = new List<string> { $"Skipped duplicate guide: {guideData.Title}" }
                    };

                case DuplicateHandling.Overwrite:
                    Log.Information("Overwriting existing guide: {Title}", guideData.Title);
                    _guideRepository.Delete(existingGuide.Id);
                    break;

                case DuplicateHandling.Rename:
                    guideData.Title = GenerateUniqueName(guideData.Title);
                    Log.Information("Renamed duplicate guide to: {Title}", guideData.Title);
                    break;
            }
        }

        // Ensure category exists
        await EnsureCategoryExistsAsync(guideData.Category);

        // Convert to Guide entity
        var guide = new Guide
        {
            Id = ObjectId.NewObjectId(),
            Title = guideData.Title,
            Description = guideData.Description,
            Category = guideData.Category,
            EstimatedMinutes = guideData.EstimatedMinutes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = "Imported",
            Steps = new List<Step>()
        };

        var imagesImported = 0;

        // Import steps
        foreach (var stepData in guideData.Steps)
        {
            var step = new Step
            {
                Id = Guid.NewGuid().ToString(),
                Order = stepData.Order,
                Title = stepData.Title,
                Content = stepData.Content,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ImageIds = new List<string>()
            };

            // Import images
            if (isZipImport && stepData.ImagesBase64 != null)
            {
                // For ZIP import, ImagesBase64 contains the new ImageIds (we repurposed this field)
                foreach (var (oldId, newId) in stepData.ImagesBase64)
                {
                    step.ImageIds.Add(newId);
                    imagesImported++;
                }
            }
            else if (stepData.ImagesBase64 != null)
            {
                // For JSON import with Base64 images
                foreach (var (imageId, base64Data) in stepData.ImagesBase64)
                {
                    try
                    {
                        var imageBytes = Convert.FromBase64String(base64Data);
                        using var imageStream = new MemoryStream(imageBytes);
                        var newImageId = await _imageStorageService.UploadImageAsync(imageStream, $"imported_image_{imageId}.png");
                        step.ImageIds.Add(newImageId);
                        imagesImported++;
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "Failed to import image {ImageId} for step {StepTitle}", imageId, stepData.Title);
                    }
                }
            }

            guide.Steps.Add(step);
        }

        // Save guide
        _guideRepository.Insert(guide);

        Log.Information("Successfully imported guide '{Title}' with {Steps} steps and {Images} images",
            guide.Title, guide.Steps.Count, imagesImported);

        var result = ImportResult.CreateSuccess(new List<ObjectId> { guide.Id }, imagesImported);
        return result;
    }

    /// <summary>
    /// Ensures a category exists, creating it if necessary.
    /// </summary>
    private async Task EnsureCategoryExistsAsync(string categoryName)
    {
        if (string.IsNullOrWhiteSpace(categoryName))
        {
            return;
        }

        var existingCategory = _categoryRepository.GetAll()
            .FirstOrDefault(c => c.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase));

        if (existingCategory == null)
        {
            var newCategory = new Category
            {
                Id = ObjectId.NewObjectId(),
                Name = categoryName,
                IconGlyph = "\uE8F1", // Default icon (Document)
                Color = "#0078D4", // Default color (blue)
                CreatedAt = DateTime.UtcNow
            };

            _categoryRepository.Insert(newCategory);
            Log.Information("Created new category: {CategoryName}", categoryName);
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Generates a unique name by appending a number.
    /// </summary>
    private string GenerateUniqueName(string baseName)
    {
        var counter = 1;
        var newName = $"{baseName} ({counter})";

        while (_guideRepository.GetAll().Any(g => g.Title.Equals(newName, StringComparison.OrdinalIgnoreCase)))
        {
            counter++;
            newName = $"{baseName} ({counter})";
        }

        return newName;
    }

    /// <summary>
    /// Tries to deserialize JSON to a specific type.
    /// </summary>
    private static bool TryDeserialize<T>(string json, out T? result) where T : class
    {
        try
        {
            result = System.Text.Json.JsonSerializer.Deserialize<T>(json, _jsonOptions);
            return result != null;
        }
        catch
        {
            result = null;
            return false;
        }
    }
}

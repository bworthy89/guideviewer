using FluentAssertions;
using GuideViewer.Core.Models;
using GuideViewer.Core.Services;
using GuideViewer.Data.Entities;
using GuideViewer.Data.Repositories;
using GuideViewer.Data.Services;
using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace GuideViewer.Tests.Services;

/// <summary>
/// Unit tests for GuideImportService.
/// </summary>
public class GuideImportServiceTests : IDisposable
{
    private readonly string _testDatabasePath;
    private readonly DatabaseService _databaseService;
    private readonly GuideRepository _guideRepository;
    private readonly CategoryRepository _categoryRepository;
    private readonly ImageStorageService _imageStorageService;
    private readonly GuideImportService _importService;

    public GuideImportServiceTests()
    {
        _testDatabasePath = Path.Combine(Path.GetTempPath(), $"test_import_{Guid.NewGuid()}.db");
        _databaseService = new DatabaseService(_testDatabasePath);
        _guideRepository = new GuideRepository(_databaseService);
        _categoryRepository = new CategoryRepository(_databaseService);
        _imageStorageService = new ImageStorageService(_databaseService);
        _importService = new GuideImportService(_guideRepository, _categoryRepository, _imageStorageService);
    }

    [Fact]
    public async Task ImportGuideFromJsonAsync_WithValidJson_ShouldImportGuide()
    {
        // Arrange
        var json = CreateTestGuideJson("Import Test Guide");

        // Act
        var result = await _importService.ImportGuideFromJsonAsync(json);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.GuidesImported.Should().Be(1);
        result.ImportedGuideIds.Should().HaveCount(1);

        var importedGuide = _guideRepository.GetById(result.ImportedGuideIds[0]);
        importedGuide.Should().NotBeNull();
        importedGuide!.Title.Should().Be("Import Test Guide");
    }

    [Fact]
    public async Task ImportGuideFromJsonAsync_WithInvalidJson_ShouldReturnFailure()
    {
        // Arrange
        var invalidJson = "{ this is not valid json }";

        // Act
        var result = await _importService.ImportGuideFromJsonAsync(invalidJson);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.HasErrors.Should().BeTrue();
    }

    [Fact]
    public async Task ImportGuideFromJsonAsync_WithDuplicateTitle_SkipHandling_ShouldSkip()
    {
        // Arrange
        var existingGuide = CreateTestGuide("Duplicate Guide");
        _guideRepository.Insert(existingGuide);

        var json = CreateTestGuideJson("Duplicate Guide");

        // Act
        var result = await _importService.ImportGuideFromJsonAsync(json, DuplicateHandling.Skip);

        // Assert
        result.Should().NotBeNull();
        result.DuplicatesSkipped.Should().Be(1);
        result.GuidesImported.Should().Be(0);
        result.HasWarnings.Should().BeTrue();
    }

    [Fact]
    public async Task ImportGuideFromJsonAsync_WithDuplicateTitle_OverwriteHandling_ShouldOverwrite()
    {
        // Arrange
        var existingGuide = CreateTestGuide("Overwrite Guide");
        existingGuide.Description = "Old Description";
        _guideRepository.Insert(existingGuide);

        var json = CreateTestGuideJson("Overwrite Guide", "New Description");

        // Act
        var result = await _importService.ImportGuideFromJsonAsync(json, DuplicateHandling.Overwrite);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.GuidesImported.Should().Be(1);

        var guides = _guideRepository.GetAll().Where(g => g.Title == "Overwrite Guide").ToList();
        guides.Should().HaveCount(1);
        guides[0].Description.Should().Be("New Description");
    }

    [Fact]
    public async Task ImportGuideFromJsonAsync_WithDuplicateTitle_RenameHandling_ShouldRename()
    {
        // Arrange
        var existingGuide = CreateTestGuide("Rename Guide");
        _guideRepository.Insert(existingGuide);

        var json = CreateTestGuideJson("Rename Guide");

        // Act
        var result = await _importService.ImportGuideFromJsonAsync(json, DuplicateHandling.Rename);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.GuidesImported.Should().Be(1);

        var guides = _guideRepository.GetAll().ToList();
        guides.Should().HaveCount(2);
        guides.Should().Contain(g => g.Title == "Rename Guide");
        guides.Should().Contain(g => g.Title == "Rename Guide (1)");
    }

    [Fact]
    public async Task ImportGuideFromJsonAsync_WithMissingCategory_ShouldCreateCategory()
    {
        // Arrange
        var json = CreateTestGuideJson("Category Test", category: "New Category");

        // Act
        var result = await _importService.ImportGuideFromJsonAsync(json);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();

        var category = _categoryRepository.GetAll().FirstOrDefault(c => c.Name == "New Category");
        category.Should().NotBeNull();
    }

    [Fact]
    public async Task ImportGuideFromJsonAsync_WithBase64Images_ShouldImportImages()
    {
        // Arrange
        var imageData = Convert.ToBase64String(new byte[] { 1, 2, 3, 4, 5 });
        var json = CreateTestGuideJsonWithImages("Image Import", imageData);

        // Act
        var result = await _importService.ImportGuideFromJsonAsync(json);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.ImagesImported.Should().BeGreaterThan(0);

        var importedGuide = _guideRepository.GetById(result.ImportedGuideIds[0]);
        importedGuide!.Steps[0].ImageIds.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ImportGuidesFromFileAsync_WithJsonFile_ShouldImport()
    {
        // Arrange
        var json = CreateTestGuideJson("File Import Test");
        var filePath = Path.Combine(Path.GetTempPath(), $"import_test_{Guid.NewGuid()}.json");
        await File.WriteAllTextAsync(filePath, json);

        try
        {
            // Act
            var result = await _importService.ImportGuidesFromFileAsync(filePath);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.GuidesImported.Should().Be(1);
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    [Fact]
    public async Task ImportGuidesFromFileAsync_WithNonExistentFile_ShouldReturnFailure()
    {
        // Arrange
        var filePath = Path.Combine(Path.GetTempPath(), $"nonexistent_{Guid.NewGuid()}.json");

        // Act
        var result = await _importService.ImportGuidesFromFileAsync(filePath);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.HasErrors.Should().BeTrue();
    }

    [Fact]
    public async Task ImportGuidesFromFileAsync_WithUnsupportedExtension_ShouldReturnFailure()
    {
        // Arrange
        var filePath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.txt");
        await File.WriteAllTextAsync(filePath, "test content");

        try
        {
            // Act
            var result = await _importService.ImportGuidesFromFileAsync(filePath);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.ErrorMessages.Should().Contain(m => m.Contains("Unsupported file format"));
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    [Fact]
    public async Task ValidateImportFileAsync_WithValidJsonFile_ShouldReturnTrue()
    {
        // Arrange
        var json = CreateTestGuideJson("Validation Test");
        var filePath = Path.Combine(Path.GetTempPath(), $"validate_test_{Guid.NewGuid()}.json");
        await File.WriteAllTextAsync(filePath, json);

        try
        {
            // Act
            var isValid = await _importService.ValidateImportFileAsync(filePath);

            // Assert
            isValid.Should().BeTrue();
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    [Fact]
    public async Task ValidateImportFileAsync_WithInvalidJsonFile_ShouldReturnFalse()
    {
        // Arrange
        var filePath = Path.Combine(Path.GetTempPath(), $"invalid_test_{Guid.NewGuid()}.json");
        await File.WriteAllTextAsync(filePath, "{ invalid json }");

        try
        {
            // Act
            var isValid = await _importService.ValidateImportFileAsync(filePath);

            // Assert
            isValid.Should().BeFalse();
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    [Fact]
    public async Task ImportGuideFromJsonAsync_WithMultipleGuides_ShouldImportAll()
    {
        // Arrange
        var json = CreateMultipleGuidesJson(new[] { "Guide 1", "Guide 2", "Guide 3" });

        // Act
        var result = await _importService.ImportGuideFromJsonAsync(json);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.GuidesImported.Should().Be(3);
        result.ImportedGuideIds.Should().HaveCount(3);
    }

    [Fact]
    public void ImportResult_GetSummaryMessage_ShouldReturnFormattedString()
    {
        // Arrange
        var result = new ImportResult
        {
            Success = true,
            GuidesImported = 5,
            ImagesImported = 10,
            DuplicatesSkipped = 2
        };

        // Act
        var summary = result.GetSummaryMessage();

        // Assert
        summary.Should().Contain("5 guide(s) imported");
        summary.Should().Contain("10 image(s) imported");
        summary.Should().Contain("2 duplicate(s) skipped");
    }

    // Helper Methods

    private Guide CreateTestGuide(string title, string description = "Test Description")
    {
        return new Guide
        {
            Id = ObjectId.NewObjectId(),
            Title = title,
            Description = description,
            Category = "Test Category",
            EstimatedMinutes = 30,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = "TestUser",
            Steps = new List<Step>
            {
                new Step
                {
                    Id = Guid.NewGuid().ToString(),
                    Order = 1,
                    Title = "Step 1",
                    Content = "Content 1"
                }
            }
        };
    }

    private string CreateTestGuideJson(string title, string description = "Test Description", string category = "Test Category")
    {
        var export = new GuideExport
        {
            Version = "1.0",
            ExportDate = DateTime.UtcNow,
            Guide = new GuideExportData
            {
                Title = title,
                Description = description,
                Category = category,
                EstimatedMinutes = 30,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "TestUser",
                Steps = new List<StepExportData>
                {
                    new StepExportData
                    {
                        Order = 1,
                        Title = "Step 1",
                        Content = "Content 1",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                }
            }
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return System.Text.Json.JsonSerializer.Serialize(export, options);
    }

    private string CreateTestGuideJsonWithImages(string title, string base64Image)
    {
        var export = new GuideExport
        {
            Version = "1.0",
            ExportDate = DateTime.UtcNow,
            Guide = new GuideExportData
            {
                Title = title,
                Description = "Test",
                Category = "Test",
                EstimatedMinutes = 30,
                Steps = new List<StepExportData>
                {
                    new StepExportData
                    {
                        Order = 1,
                        Title = "Step 1",
                        Content = "Content",
                        ImagesBase64 = new Dictionary<string, string>
                        {
                            { "img1", base64Image }
                        }
                    }
                }
            }
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return System.Text.Json.JsonSerializer.Serialize(export, options);
    }

    private string CreateMultipleGuidesJson(string[] titles)
    {
        var export = new GuidesExport
        {
            Version = "1.0",
            ExportDate = DateTime.UtcNow,
            GuideCount = titles.Length,
            Guides = titles.Select(title => new GuideExportData
            {
                Title = title,
                Description = $"Description for {title}",
                Category = "Test",
                EstimatedMinutes = 30,
                Steps = new List<StepExportData>
                {
                    new StepExportData
                    {
                        Order = 1,
                        Title = "Step 1",
                        Content = "Content"
                    }
                }
            }).ToList()
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return System.Text.Json.JsonSerializer.Serialize(export, options);
    }

    public void Dispose()
    {
        _databaseService?.Dispose();
        if (File.Exists(_testDatabasePath))
            File.Delete(_testDatabasePath);
    }
}

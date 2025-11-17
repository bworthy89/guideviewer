using FluentAssertions;
using GuideViewer.Core.Services;
using GuideViewer.Data.Entities;
using GuideViewer.Data.Repositories;
using GuideViewer.Data.Services;
using LiteDB;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace GuideViewer.Tests.Services;

/// <summary>
/// Unit tests for GuideExportService.
/// </summary>
public class GuideExportServiceTests : IDisposable
{
    private readonly string _testDatabasePath;
    private readonly DatabaseService _databaseService;
    private readonly GuideRepository _guideRepository;
    private readonly ImageStorageService _imageStorageService;
    private readonly GuideExportService _exportService;

    public GuideExportServiceTests()
    {
        _testDatabasePath = Path.Combine(Path.GetTempPath(), $"test_export_{Guid.NewGuid()}.db");
        _databaseService = new DatabaseService(_testDatabasePath);
        _guideRepository = new GuideRepository(_databaseService);
        _imageStorageService = new ImageStorageService(_databaseService);
        _exportService = new GuideExportService(_guideRepository, _imageStorageService);
    }

    [Fact]
    public async Task ExportGuideToJsonAsync_WithValidGuide_ShouldReturnJson()
    {
        // Arrange
        var guide = CreateTestGuide("Test Guide");
        _guideRepository.Insert(guide);

        // Act
        var json = await _exportService.ExportGuideToJsonAsync(guide.Id, includeImages: false);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("Test Guide");
        json.Should().Contain("\"version\": \"1.0\"");
    }

    [Fact]
    public async Task ExportGuideToJsonAsync_WithInvalidGuideId_ShouldThrowException()
    {
        // Arrange
        var invalidId = ObjectId.NewObjectId();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _exportService.ExportGuideToJsonAsync(invalidId));
    }

    [Fact]
    public async Task ExportGuideToJsonAsync_WithImages_ShouldIncludeBase64()
    {
        // Arrange
        var guide = CreateTestGuide("Guide with Images");
        _guideRepository.Insert(guide);
        var imageId = await _imageStorageService.UploadImageAsync(
            CreateTestImageStream(1024), "test.png");
        guide.Steps[0].ImageIds.Add(imageId);
        _guideRepository.Update(guide);

        // Act
        var json = await _exportService.ExportGuideToJsonAsync(guide.Id, includeImages: true);

        // Assert
        json.Should().Contain("imagesBase64");
    }

    [Fact]
    public async Task ExportAllGuidesToJsonAsync_WithMultipleGuides_ShouldReturnJsonArray()
    {
        // Arrange
        _guideRepository.Insert(CreateTestGuide("Guide 1"));
        _guideRepository.Insert(CreateTestGuide("Guide 2"));
        _guideRepository.Insert(CreateTestGuide("Guide 3"));

        // Act
        var json = await _exportService.ExportAllGuidesToJsonAsync(includeImages: false);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("Guide 1");
        json.Should().Contain("Guide 2");
        json.Should().Contain("Guide 3");
        json.Should().Contain("\"guideCount\": 3");
    }

    [Fact]
    public async Task ExportAllGuidesToJsonAsync_WithEmptyDatabase_ShouldReturnEmptyArray()
    {
        // Act
        var json = await _exportService.ExportAllGuidesToJsonAsync(includeImages: false);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("\"guideCount\": 0");
        json.Should().Contain("\"guides\": []");
    }

    [Fact]
    public async Task ExportGuideToFileAsync_WithValidPath_ShouldCreateFile()
    {
        // Arrange
        var guide = CreateTestGuide("File Export Test");
        _guideRepository.Insert(guide);
        var filePath = Path.Combine(Path.GetTempPath(), $"export_test_{Guid.NewGuid()}.json");

        try
        {
            // Act
            var success = await _exportService.ExportGuideToFileAsync(guide.Id, filePath, includeImages: false);

            // Assert
            success.Should().BeTrue();
            File.Exists(filePath).Should().BeTrue();
            var content = await File.ReadAllTextAsync(filePath);
            content.Should().Contain("File Export Test");
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    [Fact]
    public async Task ExportAllGuidesToFileAsync_WithValidPath_ShouldCreateFile()
    {
        // Arrange
        _guideRepository.Insert(CreateTestGuide("Guide A"));
        _guideRepository.Insert(CreateTestGuide("Guide B"));
        var filePath = Path.Combine(Path.GetTempPath(), $"export_all_{Guid.NewGuid()}.json");

        try
        {
            // Act
            var success = await _exportService.ExportAllGuidesToFileAsync(filePath, includeImages: false);

            // Assert
            success.Should().BeTrue();
            File.Exists(filePath).Should().BeTrue();
            var content = await File.ReadAllTextAsync(filePath);
            content.Should().Contain("Guide A");
            content.Should().Contain("Guide B");
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    [Fact]
    public async Task ExportGuideWithImagesAsync_WithImages_ShouldCreateZipWithFiles()
    {
        // Arrange
        var guide = CreateTestGuide("ZIP Export Test");
        _guideRepository.Insert(guide);
        var imageId1 = await _imageStorageService.UploadImageAsync(
            CreateTestImageStream(1024), "image1.png");
        var imageId2 = await _imageStorageService.UploadImageAsync(
            CreateTestImageStream(2048), "image2.jpg");
        guide.Steps[0].ImageIds.Add(imageId1);
        guide.Steps[1].ImageIds.Add(imageId2);
        _guideRepository.Update(guide);

        // Act
        var zipData = await _exportService.ExportGuideWithImagesAsync(guide.Id);

        // Assert
        zipData.Should().NotBeNull();
        zipData.Length.Should().BeGreaterThan(0);

        // Verify ZIP contents
        using var memoryStream = new MemoryStream(zipData);
        using var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read);

        archive.Entries.Should().Contain(e => e.Name == "guide.json");
        archive.Entries.Should().Contain(e => e.FullName.StartsWith("images/"));
        archive.Entries.Count(e => e.FullName.StartsWith("images/")).Should().Be(2);
    }

    [Fact]
    public async Task ExportGuideWithImagesAsync_WithoutImages_ShouldCreateZipWithOnlyJson()
    {
        // Arrange
        var guide = CreateTestGuide("ZIP No Images");
        _guideRepository.Insert(guide);

        // Act
        var zipData = await _exportService.ExportGuideWithImagesAsync(guide.Id);

        // Assert
        zipData.Should().NotBeNull();

        using var memoryStream = new MemoryStream(zipData);
        using var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read);

        archive.Entries.Should().Contain(e => e.Name == "guide.json");
        archive.Entries.Should().NotContain(e => e.FullName.StartsWith("images/"));
    }

    [Fact]
    public async Task ExportGuideToJsonAsync_ShouldIncludeMetadata()
    {
        // Arrange
        var guide = CreateTestGuide("Metadata Test");
        guide.CreatedBy = "TestUser";
        guide.EstimatedMinutes = 45;
        _guideRepository.Insert(guide);

        // Act
        var json = await _exportService.ExportGuideToJsonAsync(guide.Id, includeImages: false);

        // Assert
        json.Should().Contain("TestUser");
        json.Should().Contain("\"estimatedMinutes\": 45");
        json.Should().Contain("exportDate");
    }

    // Helper Methods

    private Guide CreateTestGuide(string title)
    {
        return new Guide
        {
            Id = ObjectId.NewObjectId(),
            Title = title,
            Description = $"Description for {title}",
            Category = "Test Category",
            EstimatedMinutes = 30,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = "TestUser",
            Steps = new System.Collections.Generic.List<Step>
            {
                new Step
                {
                    Id = Guid.NewGuid().ToString(),
                    Order = 1,
                    Title = "Step 1",
                    Content = "Step 1 content",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Step
                {
                    Id = Guid.NewGuid().ToString(),
                    Order = 2,
                    Title = "Step 2",
                    Content = "Step 2 content",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            }
        };
    }

    private MemoryStream CreateTestImageStream(int sizeInBytes)
    {
        var data = new byte[sizeInBytes];
        new Random().NextBytes(data);
        return new MemoryStream(data);
    }

    public void Dispose()
    {
        _databaseService?.Dispose();
        if (File.Exists(_testDatabasePath))
            File.Delete(_testDatabasePath);
    }
}

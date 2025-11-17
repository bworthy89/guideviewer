using FluentAssertions;
using GuideViewer.Core.Services;
using GuideViewer.Data.Entities;
using GuideViewer.Data.Repositories;
using GuideViewer.Data.Services;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace GuideViewer.Tests.Integration;

/// <summary>
/// Integration tests for data management workflows (export, import, backup).
/// </summary>
public class DataManagementIntegrationTests : IDisposable
{
    private readonly string _testDatabasePath;
    private readonly DatabaseService _databaseService;
    private readonly GuideRepository _guideRepository;
    private readonly CategoryRepository _categoryRepository;
    private readonly UserRepository _userRepository;
    private readonly ProgressRepository _progressRepository;
    private readonly ImageStorageService _imageStorageService;
    private readonly GuideExportService _exportService;
    private readonly GuideImportService _importService;
    private readonly DatabaseBackupService _backupService;

    public DataManagementIntegrationTests()
    {
        _testDatabasePath = Path.Combine(Path.GetTempPath(), $"test_integration_{Guid.NewGuid()}.db");
        _databaseService = new DatabaseService(_testDatabasePath);
        _guideRepository = new GuideRepository(_databaseService);
        _categoryRepository = new CategoryRepository(_databaseService);
        _userRepository = new UserRepository(_databaseService);
        _progressRepository = new ProgressRepository(_databaseService);
        _imageStorageService = new ImageStorageService(_databaseService);
        _exportService = new GuideExportService(_guideRepository, _imageStorageService);
        _importService = new GuideImportService(_guideRepository, _categoryRepository, _imageStorageService);
        _backupService = new DatabaseBackupService(
            _databaseService,
            _guideRepository,
            _userRepository,
            _progressRepository,
            _categoryRepository);
    }

    [Fact]
    public async Task ExportImportWorkflow_GuideWithoutImages_ShouldPreserveAllData()
    {
        // Arrange - Create a guide
        var originalGuide = CreateCompleteGuide("Original Guide");
        _guideRepository.Insert(originalGuide);

        // Act - Export to JSON
        var json = await _exportService.ExportGuideToJsonAsync(originalGuide.Id, includeImages: false);

        // Clear database
        _guideRepository.Delete(originalGuide.Id);
        _guideRepository.GetAll().Should().BeEmpty();

        // Act - Import from JSON
        var importResult = await _importService.ImportGuideFromJsonAsync(json);

        // Assert
        importResult.Success.Should().BeTrue();
        importResult.GuidesImported.Should().Be(1);

        var importedGuide = _guideRepository.GetById(importResult.ImportedGuideIds[0]);
        importedGuide.Should().NotBeNull();
        importedGuide!.Title.Should().Be(originalGuide.Title);
        importedGuide.Description.Should().Be(originalGuide.Description);
        importedGuide.Category.Should().Be(originalGuide.Category);
        importedGuide.EstimatedMinutes.Should().Be(originalGuide.EstimatedMinutes);
        importedGuide.Steps.Should().HaveCount(originalGuide.Steps.Count);
        importedGuide.Steps[0].Title.Should().Be(originalGuide.Steps[0].Title);
        importedGuide.Steps[0].Content.Should().Be(originalGuide.Steps[0].Content);
    }

    [Fact]
    public async Task ExportImportWorkflow_GuideWithImages_ShouldPreserveImages()
    {
        // Arrange - Create a guide with images
        var originalGuide = CreateCompleteGuide("Guide with Images");
        _guideRepository.Insert(originalGuide);
        var imageId1 = await _imageStorageService.UploadImageAsync(
            CreateTestImageStream(1024), "test1.png");
        var imageId2 = await _imageStorageService.UploadImageAsync(
            CreateTestImageStream(2048), "test2.jpg");

        originalGuide.Steps[0].ImageIds.Add(imageId1);
        originalGuide.Steps[1].ImageIds.Add(imageId2);
        _guideRepository.Update(originalGuide);

        // Act - Export to JSON with images
        var json = await _exportService.ExportGuideToJsonAsync(originalGuide.Id, includeImages: true);

        // Clear database
        _guideRepository.Delete(originalGuide.Id);
        await _imageStorageService.DeleteImageAsync(imageId1);
        await _imageStorageService.DeleteImageAsync(imageId2);

        // Act - Import from JSON
        var importResult = await _importService.ImportGuideFromJsonAsync(json);

        // Assert
        importResult.Success.Should().BeTrue();
        importResult.ImagesImported.Should().Be(2);

        var importedGuide = _guideRepository.GetById(importResult.ImportedGuideIds[0]);
        importedGuide!.Steps[0].ImageIds.Should().HaveCount(1);
        importedGuide.Steps[1].ImageIds.Should().HaveCount(1);

        // Verify images can be retrieved
        var image1Stream = await _imageStorageService.GetImageAsync(importedGuide.Steps[0].ImageIds[0]);
        var image2Stream = await _imageStorageService.GetImageAsync(importedGuide.Steps[1].ImageIds[0]);

        image1Stream.Should().NotBeNull();
        image2Stream.Should().NotBeNull();

        image1Stream!.Dispose();
        image2Stream!.Dispose();
    }

    [Fact]
    public async Task ExportImportWorkflow_MultipleGuides_ShouldImportAll()
    {
        // Arrange - Create multiple guides
        var guide1 = CreateCompleteGuide("Guide One");
        var guide2 = CreateCompleteGuide("Guide Two");
        var guide3 = CreateCompleteGuide("Guide Three");

        _guideRepository.Insert(guide1);
        _guideRepository.Insert(guide2);
        _guideRepository.Insert(guide3);

        // Act - Export all to JSON
        var json = await _exportService.ExportAllGuidesToJsonAsync(includeImages: false);

        // Clear database
        foreach (var guide in _guideRepository.GetAll().ToList())
        {
            _guideRepository.Delete(guide.Id);
        }

        // Act - Import from JSON
        var importResult = await _importService.ImportGuideFromJsonAsync(json);

        // Assert
        importResult.Success.Should().BeTrue();
        importResult.GuidesImported.Should().Be(3);

        var importedGuides = _guideRepository.GetAll().ToList();
        importedGuides.Should().HaveCount(3);
        importedGuides.Should().Contain(g => g.Title == "Guide One");
        importedGuides.Should().Contain(g => g.Title == "Guide Two");
        importedGuides.Should().Contain(g => g.Title == "Guide Three");
    }

    [Fact]
    public async Task ExportImportWorkflow_WithMissingCategory_ShouldCreateCategory()
    {
        // Arrange
        var guide = CreateCompleteGuide("Category Test", category: "New Category");
        _guideRepository.Insert(guide);

        var categoriesBeforeExport = _categoryRepository.GetAll().ToList();

        // Act - Export
        var json = await _exportService.ExportGuideToJsonAsync(guide.Id, includeImages: false);

        // Delete guide and category
        _guideRepository.Delete(guide.Id);
        foreach (var cat in _categoryRepository.GetAll().ToList())
        {
            _categoryRepository.Delete(cat.Id);
        }

        // Act - Import (should auto-create category)
        var importResult = await _importService.ImportGuideFromJsonAsync(json);

        // Assert
        importResult.Success.Should().BeTrue();

        var category = _categoryRepository.GetAll().FirstOrDefault(c => c.Name == "New Category");
        category.Should().NotBeNull();
    }

    [Fact]
    public async Task BackupRestoreWorkflow_ShouldPreserveAllData()
    {
        // Arrange - Seed comprehensive test data
        SeedComprehensiveTestData();

        var originalGuideCount = _guideRepository.GetAll().Count();
        var originalUserCount = _userRepository.GetAll().Count();
        var originalCategoryCount = _categoryRepository.GetAll().Count();

        var backupPath = Path.Combine(Path.GetTempPath(), $"backup_restore_test_{Guid.NewGuid()}.zip");

        try
        {
            // Act - Create backup
            var backupSuccess = await _backupService.CreateBackupAsync(backupPath);
            backupSuccess.Should().BeTrue();

            // Verify backup metadata
            var backupInfo = await _backupService.GetBackupInfoAsync(backupPath);
            backupInfo.Should().NotBeNull();
            backupInfo!.GuideCount.Should().Be(originalGuideCount);
            backupInfo.UserCount.Should().Be(originalUserCount);
            backupInfo.CategoryCount.Should().Be(originalCategoryCount);

            // Clear all data
            foreach (var guide in _guideRepository.GetAll().ToList())
            {
                _guideRepository.Delete(guide.Id);
            }
            foreach (var user in _userRepository.GetAll().ToList())
            {
                _userRepository.Delete(user.Id);
            }
            foreach (var category in _categoryRepository.GetAll().ToList())
            {
                _categoryRepository.Delete(category.Id);
            }

            _guideRepository.GetAll().Should().BeEmpty();
            _userRepository.GetAll().Should().BeEmpty();
            _categoryRepository.GetAll().Should().BeEmpty();

            // Note: RestoreBackupAsync closes the database and restarts the app
            // For testing, we'll manually extract and verify the backup contents
            var extractPath = Path.Combine(Path.GetTempPath(), $"extract_{Guid.NewGuid()}");
            System.IO.Compression.ZipFile.ExtractToDirectory(backupPath, extractPath);

            try
            {
                var restoredDbPath = Path.Combine(extractPath, "data.db");
                File.Exists(restoredDbPath).Should().BeTrue();

                // Verify restored database
                using var restoredDbService = new DatabaseService(restoredDbPath);
                var restoredGuideRepo = new GuideRepository(restoredDbService);
                var restoredUserRepo = new UserRepository(restoredDbService);
                var restoredCategoryRepo = new CategoryRepository(restoredDbService);

                restoredGuideRepo.GetAll().Count().Should().Be(originalGuideCount);
                restoredUserRepo.GetAll().Count().Should().Be(originalUserCount);
                restoredCategoryRepo.GetAll().Count().Should().Be(originalCategoryCount);
            }
            finally
            {
                if (Directory.Exists(extractPath))
                    Directory.Delete(extractPath, recursive: true);
            }
        }
        finally
        {
            if (File.Exists(backupPath))
                File.Delete(backupPath);
        }
    }

    [Fact]
    public async Task ExportImportPerformance_50Guides_ShouldCompleteUnder5Seconds()
    {
        // Arrange - Create 50 guides
        for (int i = 1; i <= 50; i++)
        {
            var guide = CreateCompleteGuide($"Performance Test Guide {i}");
            _guideRepository.Insert(guide);
        }

        var exportStopwatch = Stopwatch.StartNew();

        // Act - Export all guides
        var json = await _exportService.ExportAllGuidesToJsonAsync(includeImages: false);

        exportStopwatch.Stop();

        // Clear database
        foreach (var guide in _guideRepository.GetAll().ToList())
        {
            _guideRepository.Delete(guide.Id);
        }

        var importStopwatch = Stopwatch.StartNew();

        // Act - Import all guides
        var importResult = await _importService.ImportGuideFromJsonAsync(json);

        importStopwatch.Stop();

        // Assert
        exportStopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(5));
        importStopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(5));
        importResult.GuidesImported.Should().Be(50);
    }

    [Fact]
    public async Task BackupPerformance_100Guides_ShouldCompleteUnder10Seconds()
    {
        // Arrange - Create 100 guides
        for (int i = 1; i <= 100; i++)
        {
            var guide = CreateCompleteGuide($"Backup Performance Test {i}");
            _guideRepository.Insert(guide);
        }

        var backupPath = Path.Combine(Path.GetTempPath(), $"perf_backup_{Guid.NewGuid()}.zip");
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Act - Create backup
            var success = await _backupService.CreateBackupAsync(backupPath);

            stopwatch.Stop();

            // Assert
            success.Should().BeTrue();
            stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(10));

            var backupInfo = await _backupService.GetBackupInfoAsync(backupPath);
            backupInfo!.GuideCount.Should().Be(100);
        }
        finally
        {
            if (File.Exists(backupPath))
                File.Delete(backupPath);
        }
    }

    [Fact]
    public async Task ImportDuplicateHandling_SkipMode_ShouldNotCreateDuplicates()
    {
        // Arrange
        var guide = CreateCompleteGuide("Duplicate Test");
        _guideRepository.Insert(guide);

        var json = await _exportService.ExportGuideToJsonAsync(guide.Id, includeImages: false);

        // Act - Try to import duplicate with Skip mode
        var importResult = await _importService.ImportGuideFromJsonAsync(json, DuplicateHandling.Skip);

        // Assert
        importResult.DuplicatesSkipped.Should().Be(1);
        importResult.GuidesImported.Should().Be(0);
        _guideRepository.GetAll().Count().Should().Be(1);
    }

    [Fact]
    public async Task ImportDuplicateHandling_RenameMode_ShouldCreateBothVersions()
    {
        // Arrange
        var guide = CreateCompleteGuide("Rename Test");
        _guideRepository.Insert(guide);

        var json = await _exportService.ExportGuideToJsonAsync(guide.Id, includeImages: false);

        // Act - Import duplicate with Rename mode
        var importResult = await _importService.ImportGuideFromJsonAsync(json, DuplicateHandling.Rename);

        // Assert
        importResult.Success.Should().BeTrue();
        importResult.GuidesImported.Should().Be(1);

        var guides = _guideRepository.GetAll().ToList();
        guides.Should().HaveCount(2);
        guides.Should().Contain(g => g.Title == "Rename Test");
        guides.Should().Contain(g => g.Title == "Rename Test (1)");
    }

    // Helper Methods

    private Guide CreateCompleteGuide(string title, string category = "Test Category")
    {
        return new Guide
        {
            Id = ObjectId.NewObjectId(),
            Title = title,
            Description = $"Comprehensive description for {title}",
            Category = category,
            EstimatedMinutes = 45,
            CreatedAt = DateTime.UtcNow.AddDays(-7),
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = "IntegrationTest",
            Steps = new List<Step>
            {
                new Step
                {
                    Id = Guid.NewGuid().ToString(),
                    Order = 1,
                    Title = "Step 1: Preparation",
                    Content = "Detailed preparation instructions with RTF content",
                    CreatedAt = DateTime.UtcNow.AddDays(-7),
                    UpdatedAt = DateTime.UtcNow
                },
                new Step
                {
                    Id = Guid.NewGuid().ToString(),
                    Order = 2,
                    Title = "Step 2: Installation",
                    Content = "Installation procedure with detailed steps",
                    CreatedAt = DateTime.UtcNow.AddDays(-7),
                    UpdatedAt = DateTime.UtcNow
                },
                new Step
                {
                    Id = Guid.NewGuid().ToString(),
                    Order = 3,
                    Title = "Step 3: Verification",
                    Content = "Verification and testing procedures",
                    CreatedAt = DateTime.UtcNow.AddDays(-7),
                    UpdatedAt = DateTime.UtcNow
                }
            }
        };
    }

    private void SeedComprehensiveTestData()
    {
        // Add users
        _userRepository.Insert(new User
        {
            Id = ObjectId.NewObjectId(),
            ProductKey = "ADMIN-TEST-1234-5678",
            Role = "Admin",
            ActivatedAt = DateTime.UtcNow
        });

        _userRepository.Insert(new User
        {
            Id = ObjectId.NewObjectId(),
            ProductKey = "TECH-TEST-ABCD-EFGH",
            Role = "Technician",
            ActivatedAt = DateTime.UtcNow
        });

        // Add categories
        _categoryRepository.Insert(new Category
        {
            Id = ObjectId.NewObjectId(),
            Name = "Installation",
            IconGlyph = "\uE8F1",
            Color = "#0078D4",
            CreatedAt = DateTime.UtcNow
        });

        _categoryRepository.Insert(new Category
        {
            Id = ObjectId.NewObjectId(),
            Name = "Maintenance",
            IconGlyph = "\uE90F",
            Color = "#00A818",
            CreatedAt = DateTime.UtcNow
        });

        // Add guides
        _guideRepository.Insert(CreateCompleteGuide("Server Installation", "Installation"));
        _guideRepository.Insert(CreateCompleteGuide("Network Setup", "Installation"));
        _guideRepository.Insert(CreateCompleteGuide("System Maintenance", "Maintenance"));
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

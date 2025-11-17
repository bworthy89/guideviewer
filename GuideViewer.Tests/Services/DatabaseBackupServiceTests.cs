using FluentAssertions;
using GuideViewer.Core.Services;
using GuideViewer.Data.Entities;
using GuideViewer.Data.Repositories;
using GuideViewer.Data.Services;
using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace GuideViewer.Tests.Services;

/// <summary>
/// Unit tests for DatabaseBackupService.
/// </summary>
public class DatabaseBackupServiceTests : IDisposable
{
    private readonly string _testDatabasePath;
    private readonly DatabaseService _databaseService;
    private readonly GuideRepository _guideRepository;
    private readonly UserRepository _userRepository;
    private readonly ProgressRepository _progressRepository;
    private readonly CategoryRepository _categoryRepository;
    private readonly DatabaseBackupService _backupService;

    public DatabaseBackupServiceTests()
    {
        _testDatabasePath = Path.Combine(Path.GetTempPath(), $"test_backup_{Guid.NewGuid()}.db");
        _databaseService = new DatabaseService(_testDatabasePath);
        _guideRepository = new GuideRepository(_databaseService);
        _userRepository = new UserRepository(_databaseService);
        _progressRepository = new ProgressRepository(_databaseService);
        _categoryRepository = new CategoryRepository(_databaseService);
        _backupService = new DatabaseBackupService(
            _databaseService,
            _guideRepository,
            _userRepository,
            _progressRepository,
            _categoryRepository);
    }

    [Fact]
    public async Task CreateBackupAsync_WithValidPath_ShouldCreateBackup()
    {
        // Arrange
        SeedTestData();
        var backupPath = Path.Combine(Path.GetTempPath(), $"backup_test_{Guid.NewGuid()}.zip");

        try
        {
            // Act
            var success = await _backupService.CreateBackupAsync(backupPath);

            // Assert
            success.Should().BeTrue();
            File.Exists(backupPath).Should().BeTrue();

            var fileInfo = new FileInfo(backupPath);
            fileInfo.Length.Should().BeGreaterThan(0);
        }
        finally
        {
            if (File.Exists(backupPath))
                File.Delete(backupPath);
        }
    }

    [Fact]
    public async Task CreateBackupAsync_ShouldIncludeMetadataFile()
    {
        // Arrange
        SeedTestData();
        var backupPath = Path.Combine(Path.GetTempPath(), $"backup_metadata_{Guid.NewGuid()}.zip");

        try
        {
            // Act
            var success = await _backupService.CreateBackupAsync(backupPath);

            // Assert
            success.Should().BeTrue();

            using var archive = ZipFile.OpenRead(backupPath);
            archive.Entries.Should().Contain(e => e.Name == "data.db");
            archive.Entries.Should().Contain(e => e.Name == "metadata.json");
        }
        finally
        {
            if (File.Exists(backupPath))
                File.Delete(backupPath);
        }
    }

    [Fact]
    public async Task CreateBackupAsync_MetadataShouldContainCounts()
    {
        // Arrange
        SeedTestData();
        var backupPath = Path.Combine(Path.GetTempPath(), $"backup_counts_{Guid.NewGuid()}.zip");

        try
        {
            // Act
            await _backupService.CreateBackupAsync(backupPath);

            // Assert
            var backupInfo = await _backupService.GetBackupInfoAsync(backupPath);
            backupInfo.Should().NotBeNull();
            backupInfo!.GuideCount.Should().Be(2);
            backupInfo.UserCount.Should().Be(1);
            backupInfo.CategoryCount.Should().Be(1);
        }
        finally
        {
            if (File.Exists(backupPath))
                File.Delete(backupPath);
        }
    }

    [Fact]
    public async Task ValidateBackupAsync_WithValidBackup_ShouldReturnTrue()
    {
        // Arrange
        SeedTestData();
        var backupPath = Path.Combine(Path.GetTempPath(), $"backup_validate_{Guid.NewGuid()}.zip");
        await _backupService.CreateBackupAsync(backupPath);

        try
        {
            // Act
            var isValid = await _backupService.ValidateBackupAsync(backupPath);

            // Assert
            isValid.Should().BeTrue();
        }
        finally
        {
            if (File.Exists(backupPath))
                File.Delete(backupPath);
        }
    }

    [Fact]
    public async Task ValidateBackupAsync_WithInvalidBackup_ShouldReturnFalse()
    {
        // Arrange
        var invalidBackupPath = Path.Combine(Path.GetTempPath(), $"invalid_backup_{Guid.NewGuid()}.zip");

        // Create empty ZIP
        using (var archive = ZipFile.Open(invalidBackupPath, ZipArchiveMode.Create))
        {
            archive.CreateEntry("random.txt");
        }

        try
        {
            // Act
            var isValid = await _backupService.ValidateBackupAsync(invalidBackupPath);

            // Assert
            isValid.Should().BeFalse();
        }
        finally
        {
            if (File.Exists(invalidBackupPath))
                File.Delete(invalidBackupPath);
        }
    }

    [Fact]
    public async Task ValidateBackupAsync_WithNonExistentFile_ShouldReturnFalse()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), $"nonexistent_{Guid.NewGuid()}.zip");

        // Act
        var isValid = await _backupService.ValidateBackupAsync(nonExistentPath);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public async Task GetBackupInfoAsync_WithValidBackup_ShouldReturnInfo()
    {
        // Arrange
        SeedTestData();
        var backupPath = Path.Combine(Path.GetTempPath(), $"backup_info_{Guid.NewGuid()}.zip");
        await _backupService.CreateBackupAsync(backupPath);

        try
        {
            // Act
            var backupInfo = await _backupService.GetBackupInfoAsync(backupPath);

            // Assert
            backupInfo.Should().NotBeNull();
            backupInfo!.BackupDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
            backupInfo.AppVersion.Should().NotBeNullOrEmpty();
            backupInfo.DatabaseSize.Should().BeGreaterThan(0);
            backupInfo.IsValid.Should().BeTrue();
        }
        finally
        {
            if (File.Exists(backupPath))
                File.Delete(backupPath);
        }
    }

    [Fact]
    public async Task GetBackupInfoAsync_WithInvalidBackup_ShouldReturnNull()
    {
        // Arrange
        var invalidPath = Path.Combine(Path.GetTempPath(), $"invalid_{Guid.NewGuid()}.zip");
        using (var archive = ZipFile.Open(invalidPath, ZipArchiveMode.Create))
        {
            // Empty ZIP
        }

        try
        {
            // Act
            var backupInfo = await _backupService.GetBackupInfoAsync(invalidPath);

            // Assert
            backupInfo.Should().BeNull();
        }
        finally
        {
            if (File.Exists(invalidPath))
                File.Delete(invalidPath);
        }
    }

    [Fact]
    public async Task GetAvailableBackupsAsync_WithMultipleBackups_ShouldReturnSortedList()
    {
        // Arrange
        var testDir = Path.Combine(Path.GetTempPath(), $"backups_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(testDir);

        try
        {
            SeedTestData();

            var backup1 = Path.Combine(testDir, "backup1.zip");
            var backup2 = Path.Combine(testDir, "backup2.zip");
            var backup3 = Path.Combine(testDir, "backup3.zip");

            await _backupService.CreateBackupAsync(backup1);
            await Task.Delay(100); // Ensure different timestamps
            await _backupService.CreateBackupAsync(backup2);
            await Task.Delay(100);
            await _backupService.CreateBackupAsync(backup3);

            // Act
            var backups = await _backupService.GetAvailableBackupsAsync(testDir);

            // Assert
            backups.Should().HaveCount(3);
            // Should be sorted newest first
            backups[0].Should().Be(backup3);
            backups[1].Should().Be(backup2);
            backups[2].Should().Be(backup1);
        }
        finally
        {
            if (Directory.Exists(testDir))
                Directory.Delete(testDir, recursive: true);
        }
    }

    [Fact]
    public async Task GetAvailableBackupsAsync_WithNonExistentDirectory_ShouldReturnEmptyList()
    {
        // Arrange
        var nonExistentDir = Path.Combine(Path.GetTempPath(), $"nonexistent_{Guid.NewGuid()}");

        // Act
        var backups = await _backupService.GetAvailableBackupsAsync(nonExistentDir);

        // Assert
        backups.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAvailableBackupsAsync_ShouldFilterOutInvalidBackups()
    {
        // Arrange
        var testDir = Path.Combine(Path.GetTempPath(), $"filter_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(testDir);

        try
        {
            SeedTestData();

            // Create valid backup
            var validBackup = Path.Combine(testDir, "valid.zip");
            await _backupService.CreateBackupAsync(validBackup);

            // Create invalid backup
            var invalidBackup = Path.Combine(testDir, "invalid.zip");
            using (var archive = ZipFile.Open(invalidBackup, ZipArchiveMode.Create))
            {
                archive.CreateEntry("random.txt");
            }

            // Act
            var backups = await _backupService.GetAvailableBackupsAsync(testDir);

            // Assert
            backups.Should().HaveCount(1);
            backups[0].Should().Be(validBackup);
        }
        finally
        {
            if (Directory.Exists(testDir))
                Directory.Delete(testDir, recursive: true);
        }
    }

    [Fact]
    public void BackupInfo_GetSummary_ShouldReturnFormattedString()
    {
        // Arrange
        var backupInfo = new Core.Models.BackupInfo
        {
            BackupDate = new DateTime(2025, 11, 17, 10, 30, 0),
            GuideCount = 10,
            UserCount = 5,
            ProgressCount = 20,
            DatabaseSize = 2048000 // ~2MB
        };

        // Act
        var summary = backupInfo.GetSummary();

        // Assert
        summary.Should().Contain("2025-11-17 10:30");
        summary.Should().Contain("10 guides");
        summary.Should().Contain("5 users");
        summary.Should().Contain("20 progress records");
        summary.Should().Contain("2,000 KB");
    }

    // Helper Methods

    private void SeedTestData()
    {
        // Add user
        var user = new User
        {
            Id = ObjectId.NewObjectId(),
            ProductKey = "TEST-KEY-1234-5678",
            Role = "Admin",
            ActivatedAt = DateTime.UtcNow
        };
        _userRepository.Insert(user);

        // Add category
        var category = new Category
        {
            Id = ObjectId.NewObjectId(),
            Name = "Test Category",
            IconGlyph = "\uE8F1",
            Color = "#0078D4",
            CreatedAt = DateTime.UtcNow
        };
        _categoryRepository.Insert(category);

        // Add guides
        var guide1 = new Guide
        {
            Id = ObjectId.NewObjectId(),
            Title = "Test Guide 1",
            Description = "Description 1",
            Category = "Test Category",
            EstimatedMinutes = 30,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
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
        _guideRepository.Insert(guide1);

        var guide2 = new Guide
        {
            Id = ObjectId.NewObjectId(),
            Title = "Test Guide 2",
            Description = "Description 2",
            Category = "Test Category",
            EstimatedMinutes = 45,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
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
        _guideRepository.Insert(guide2);
    }

    public void Dispose()
    {
        _databaseService?.Dispose();
        if (File.Exists(_testDatabasePath))
            File.Delete(_testDatabasePath);
    }
}

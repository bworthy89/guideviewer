using GuideViewer.Core.Models;
using GuideViewer.Data.Repositories;
using GuideViewer.Data.Services;
using LiteDB;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace GuideViewer.Core.Services;

/// <summary>
/// Service for creating and restoring database backups.
/// </summary>
public class DatabaseBackupService : IDatabaseBackupService
{
    private readonly DatabaseService _databaseService;
    private readonly GuideRepository _guideRepository;
    private readonly UserRepository _userRepository;
    private readonly ProgressRepository _progressRepository;
    private readonly CategoryRepository _categoryRepository;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseBackupService"/> class.
    /// </summary>
    public DatabaseBackupService(
        DatabaseService databaseService,
        GuideRepository guideRepository,
        UserRepository userRepository,
        ProgressRepository progressRepository,
        CategoryRepository categoryRepository)
    {
        _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
        _guideRepository = guideRepository ?? throw new ArgumentNullException(nameof(guideRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _progressRepository = progressRepository ?? throw new ArgumentNullException(nameof(progressRepository));
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
    }

    /// <inheritdoc/>
    public async Task<bool> CreateBackupAsync(string backupPath)
    {
        Log.Information("Creating database backup: {BackupPath}", backupPath);

        try
        {
            // Create temp directory for backup files
            var tempDir = Path.Combine(Path.GetTempPath(), $"guideviewer_backup_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);

            try
            {
                // Create database backup using DatabaseService
                var tempDbPath = Path.Combine(tempDir, "data.db");
                _databaseService.Backup(tempDbPath);

                // Create backup metadata
                var metadata = await CreateBackupMetadataAsync(tempDbPath);
                var metadataJson = System.Text.Json.JsonSerializer.Serialize(metadata, _jsonOptions);
                var metadataPath = Path.Combine(tempDir, "metadata.json");
                await File.WriteAllTextAsync(metadataPath, metadataJson);

                // Create ZIP archive
                if (File.Exists(backupPath))
                {
                    File.Delete(backupPath);
                }

                ZipFile.CreateFromDirectory(tempDir, backupPath, CompressionLevel.Optimal, includeBaseDirectory: false);

                Log.Information("Successfully created backup: {BackupPath} ({Size} bytes)",
                    backupPath, new FileInfo(backupPath).Length);

                return true;
            }
            finally
            {
                // Clean up temp directory
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, recursive: true);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create database backup: {BackupPath}", backupPath);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> RestoreBackupAsync(string backupPath)
    {
        Log.Information("Restoring database from backup: {BackupPath}", backupPath);

        try
        {
            if (!File.Exists(backupPath))
            {
                Log.Error("Backup file not found: {BackupPath}", backupPath);
                return false;
            }

            // Validate backup first
            if (!await ValidateBackupAsync(backupPath))
            {
                Log.Error("Invalid backup file: {BackupPath}", backupPath);
                return false;
            }

            // Create temp directory for extraction
            var tempDir = Path.Combine(Path.GetTempPath(), $"guideviewer_restore_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);

            try
            {
                // Extract backup
                ZipFile.ExtractToDirectory(backupPath, tempDir);

                var tempDbPath = Path.Combine(tempDir, "data.db");
                if (!File.Exists(tempDbPath))
                {
                    Log.Error("Backup does not contain data.db: {BackupPath}", backupPath);
                    return false;
                }

                // Get the original database path
                // NOTE: We can't directly access _databasePath from DatabaseService, so we'll use the default path
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var appFolder = Path.Combine(appDataPath, "GuideViewer");
                var currentDbPath = Path.Combine(appFolder, "data.db");

                // Close the current database connection
                // WARNING: App should restart after restore!
                _databaseService.Dispose();

                // Backup current database (just in case)
                var backupOfCurrent = currentDbPath + $".backup_{DateTime.Now:yyyyMMddHHmmss}";
                if (File.Exists(currentDbPath))
                {
                    File.Copy(currentDbPath, backupOfCurrent, overwrite: true);
                    Log.Information("Created backup of current database: {BackupPath}", backupOfCurrent);
                }

                // Copy restored database to app location
                File.Copy(tempDbPath, currentDbPath, overwrite: true);

                Log.Information("Successfully restored database from backup: {BackupPath}", backupPath);
                Log.Warning("Application restart required to use restored database");

                return true;
            }
            finally
            {
                // Clean up temp directory
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, recursive: true);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to restore database from backup: {BackupPath}", backupPath);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ValidateBackupAsync(string backupPath)
    {
        try
        {
            if (!File.Exists(backupPath))
            {
                return false;
            }

            // Check if it's a valid ZIP file
            using var archive = ZipFile.OpenRead(backupPath);

            // Check for required files
            var hasDataDb = archive.Entries.Any(e => e.Name == "data.db");
            var hasMetadata = archive.Entries.Any(e => e.Name == "metadata.json");

            return hasDataDb && hasMetadata;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to validate backup: {BackupPath}", backupPath);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<BackupInfo?> GetBackupInfoAsync(string backupPath)
    {
        try
        {
            if (!File.Exists(backupPath))
            {
                return null;
            }

            using var archive = ZipFile.OpenRead(backupPath);

            // Find metadata.json
            var metadataEntry = archive.Entries.FirstOrDefault(e => e.Name == "metadata.json");
            if (metadataEntry == null)
            {
                return null;
            }

            // Read and deserialize metadata
            using var stream = metadataEntry.Open();
            var metadata = await System.Text.Json.JsonSerializer.DeserializeAsync<BackupInfo>(stream, _jsonOptions);

            if (metadata != null)
            {
                metadata.IsValid = await ValidateBackupAsync(backupPath);
            }

            return metadata;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to get backup info: {BackupPath}", backupPath);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<List<string>> GetAvailableBackupsAsync(string directory)
    {
        try
        {
            if (!Directory.Exists(directory))
            {
                return new List<string>();
            }

            var backupFiles = Directory.GetFiles(directory, "*.zip", SearchOption.TopDirectoryOnly);

            // Filter to only valid backups
            var validBackups = new List<string>();
            foreach (var file in backupFiles)
            {
                if (await ValidateBackupAsync(file))
                {
                    validBackups.Add(file);
                }
            }

            // Sort by date (newest first)
            return validBackups.OrderByDescending(f => File.GetCreationTime(f)).ToList();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to get available backups from directory: {Directory}", directory);
            return new List<string>();
        }
    }

    /// <summary>
    /// Creates backup metadata.
    /// </summary>
    private async Task<BackupInfo> CreateBackupMetadataAsync(string databasePath)
    {
        var metadata = new BackupInfo
        {
            BackupDate = DateTime.UtcNow,
            AppVersion = GetAppVersion(),
            GuideCount = _guideRepository.GetAll().Count(),
            UserCount = _userRepository.GetAll().Count(),
            ProgressCount = _progressRepository.GetAll().Count(),
            CategoryCount = _categoryRepository.GetAll().Count(),
            DatabaseSize = new FileInfo(databasePath).Length,
            IsValid = true
        };

        await Task.CompletedTask;
        return metadata;
    }

    /// <summary>
    /// Gets the application version.
    /// </summary>
    private static string GetAppVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        return version?.ToString() ?? "1.0.0";
    }
}

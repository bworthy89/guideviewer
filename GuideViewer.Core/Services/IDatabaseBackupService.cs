using GuideViewer.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuideViewer.Core.Services;

/// <summary>
/// Service for creating and restoring database backups.
/// </summary>
public interface IDatabaseBackupService
{
    /// <summary>
    /// Creates a backup of the entire database.
    /// </summary>
    /// <param name="backupPath">The destination path for the backup file (ZIP).</param>
    /// <returns>True if successful, false otherwise.</returns>
    Task<bool> CreateBackupAsync(string backupPath);

    /// <summary>
    /// Restores a database from a backup file.
    /// WARNING: This will replace the current database. App restart required.
    /// </summary>
    /// <param name="backupPath">The backup file path to restore from.</param>
    /// <returns>True if successful, false otherwise.</returns>
    Task<bool> RestoreBackupAsync(string backupPath);

    /// <summary>
    /// Validates a backup file.
    /// </summary>
    /// <param name="backupPath">The backup file path to validate.</param>
    /// <returns>True if valid, false otherwise.</returns>
    Task<bool> ValidateBackupAsync(string backupPath);

    /// <summary>
    /// Gets information about a backup file.
    /// </summary>
    /// <param name="backupPath">The backup file path.</param>
    /// <returns>Backup metadata, or null if invalid.</returns>
    Task<BackupInfo?> GetBackupInfoAsync(string backupPath);

    /// <summary>
    /// Gets a list of available backup files in a directory.
    /// </summary>
    /// <param name="directory">The directory to search.</param>
    /// <returns>List of backup file paths.</returns>
    Task<List<string>> GetAvailableBackupsAsync(string directory);
}

using System;

namespace GuideViewer.Core.Models;

/// <summary>
/// Information about a database backup.
/// </summary>
public class BackupInfo
{
    /// <summary>
    /// Gets or sets when the backup was created.
    /// </summary>
    public DateTime BackupDate { get; set; }

    /// <summary>
    /// Gets or sets the application version that created the backup.
    /// </summary>
    public string AppVersion { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of guides in the backup.
    /// </summary>
    public int GuideCount { get; set; }

    /// <summary>
    /// Gets or sets the number of users in the backup.
    /// </summary>
    public int UserCount { get; set; }

    /// <summary>
    /// Gets or sets the number of progress records in the backup.
    /// </summary>
    public int ProgressCount { get; set; }

    /// <summary>
    /// Gets or sets the number of categories in the backup.
    /// </summary>
    public int CategoryCount { get; set; }

    /// <summary>
    /// Gets or sets the database file size in bytes.
    /// </summary>
    public long DatabaseSize { get; set; }

    /// <summary>
    /// Gets or sets whether the backup is valid.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Gets a user-friendly display of the backup info.
    /// </summary>
    public string GetSummary()
    {
        return $"Backup from {BackupDate:yyyy-MM-dd HH:mm} - " +
               $"{GuideCount} guides, {UserCount} users, {ProgressCount} progress records - " +
               $"{DatabaseSize / 1024.0:N0} KB";
    }
}

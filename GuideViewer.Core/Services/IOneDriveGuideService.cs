using GuideViewer.Core.Models;

namespace GuideViewer.Core.Services;

/// <summary>
/// Service for discovering and importing guides from OneDrive sync folder.
/// Monitors local OneDrive folder for new/updated guide packages without requiring SharePoint API.
/// </summary>
public interface IOneDriveGuideService
{
    /// <summary>
    /// Gets the detected OneDrive sync folder path for GuideViewer guides.
    /// Returns null if OneDrive folder cannot be detected.
    /// </summary>
    string? GetOneDriveFolderPath();

    /// <summary>
    /// Checks if OneDrive sync folder is accessible and contains guide content.
    /// </summary>
    bool IsOneDriveFolderAvailable();

    /// <summary>
    /// Scans the OneDrive sync folder for available guide packages.
    /// </summary>
    Task<List<OneDriveGuideInfo>> GetAvailableGuidesAsync();

    /// <summary>
    /// Checks for new or updated guides by comparing with local database.
    /// </summary>
    Task<List<GuideUpdateInfo>> CheckForGuideUpdatesAsync();

    /// <summary>
    /// Imports a guide package from OneDrive sync folder into local database.
    /// </summary>
    Task<ImportResult> ImportGuideFromOneDriveAsync(OneDriveGuideInfo guideInfo);

    /// <summary>
    /// Starts background monitoring of OneDrive folder for changes.
    /// Raises events when new guides are detected.
    /// </summary>
    void StartMonitoring();

    /// <summary>
    /// Stops background monitoring.
    /// </summary>
    void StopMonitoring();

    /// <summary>
    /// Event raised when new guides are detected in OneDrive folder.
    /// </summary>
    event EventHandler<GuideUpdateDetectedEventArgs>? GuideUpdatesDetected;
}

/// <summary>
/// Information about a guide package in OneDrive sync folder.
/// </summary>
public class OneDriveGuideInfo
{
    public required string FileName { get; init; }
    public required string FullPath { get; init; }
    public required long FileSize { get; init; }
    public required DateTime LastModified { get; init; }
    public string? GuideId { get; set; }
    public string? Title { get; set; }
    public string? Version { get; set; }
}

/// <summary>
/// Information about a guide update available from OneDrive.
/// </summary>
public class GuideUpdateInfo
{
    public required OneDriveGuideInfo OneDriveGuide { get; init; }
    public required GuideUpdateType UpdateType { get; init; }
    public DateTime? LocalVersion { get; init; }
}

public enum GuideUpdateType
{
    New,      // Guide doesn't exist locally
    Updated   // Guide exists but OneDrive version is newer
}

public class GuideUpdateDetectedEventArgs : EventArgs
{
    public required List<GuideUpdateInfo> Updates { get; init; }
}

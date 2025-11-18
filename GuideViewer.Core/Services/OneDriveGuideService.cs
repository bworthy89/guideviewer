using GuideViewer.Core.Models;
using GuideViewer.Data.Repositories;
using Microsoft.Win32;
using Serilog;
using System.IO.Compression;
using System.Text.Json;

namespace GuideViewer.Core.Services;

/// <summary>
/// Service for discovering and importing guides from OneDrive sync folder.
/// Uses local file system access only - no SharePoint API required.
/// </summary>
public class OneDriveGuideService : IOneDriveGuideService, IDisposable
{
    private readonly IGuideImportService _importService;
    private readonly IGuideRepository _guideRepository;
    private FileSystemWatcher? _watcher;
    private Timer? _debounceTimer;
    private readonly object _lock = new();

    // OneDrive folder structure: OneDrive - Glory Global\GuideViewer_Guides\
    private const string GUIDE_FOLDER_NAME = "GuideViewer_Guides";
    private const string GUIDES_SUBFOLDER = "Guides";

    public event EventHandler<GuideUpdateDetectedEventArgs>? GuideUpdatesDetected;

    public OneDriveGuideService(
        IGuideImportService importService,
        IGuideRepository guideRepository)
    {
        _importService = importService;
        _guideRepository = guideRepository;
    }

    /// <summary>
    /// Detects OneDrive sync folder path using multiple methods.
    /// Priority: Registry > Environment Variables > Known Paths
    /// </summary>
    public string? GetOneDriveFolderPath()
    {
        try
        {
            // Method 1: Check registry for OneDrive Business folder
            var commercialPath = GetOneDrivePathFromRegistry("Business1");
            if (!string.IsNullOrEmpty(commercialPath))
            {
                var guidePath = Path.Combine(commercialPath, GUIDE_FOLDER_NAME);
                if (Directory.Exists(guidePath))
                {
                    Log.Information("OneDrive guide folder found via registry: {Path}", guidePath);
                    return guidePath;
                }
            }

            // Method 2: Check environment variable
            var envPath = Environment.GetEnvironmentVariable("OneDriveCommercial");
            if (!string.IsNullOrEmpty(envPath))
            {
                var guidePath = Path.Combine(envPath, GUIDE_FOLDER_NAME);
                if (Directory.Exists(guidePath))
                {
                    Log.Information("OneDrive guide folder found via environment: {Path}", guidePath);
                    return guidePath;
                }
            }

            // Method 3: Check common locations
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var commonPaths = new[]
            {
                Path.Combine(userProfile, "OneDrive - Glory Global", GUIDE_FOLDER_NAME),
                Path.Combine(userProfile, "OneDrive", GUIDE_FOLDER_NAME)
            };

            foreach (var path in commonPaths)
            {
                if (Directory.Exists(path))
                {
                    Log.Information("OneDrive guide folder found at common path: {Path}", path);
                    return path;
                }
            }

            Log.Warning("OneDrive guide folder not found at any known location");
            return null;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error detecting OneDrive folder path");
            return null;
        }
    }

    /// <summary>
    /// Gets OneDrive path from Windows Registry.
    /// </summary>
    private string? GetOneDrivePathFromRegistry(string accountType)
    {
        try
        {
            var keyPath = $@"Software\Microsoft\OneDrive\Accounts\{accountType}";
            using var key = Registry.CurrentUser.OpenSubKey(keyPath);
            return key?.GetValue("UserFolder") as string;
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "Could not read OneDrive path from registry for account type: {AccountType}", accountType);
            return null;
        }
    }

    public bool IsOneDriveFolderAvailable()
    {
        var folderPath = GetOneDriveFolderPath();
        if (string.IsNullOrEmpty(folderPath))
            return false;

        try
        {
            var guidesPath = Path.Combine(folderPath, GUIDES_SUBFOLDER);
            return Directory.Exists(guidesPath);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Scans OneDrive sync folder for guide ZIP packages.
    /// </summary>
    public async Task<List<OneDriveGuideInfo>> GetAvailableGuidesAsync()
    {
        var folderPath = GetOneDriveFolderPath();
        if (string.IsNullOrEmpty(folderPath))
        {
            Log.Warning("Cannot scan for guides: OneDrive folder not available");
            return new List<OneDriveGuideInfo>();
        }

        var guidesPath = Path.Combine(folderPath, GUIDES_SUBFOLDER);
        if (!Directory.Exists(guidesPath))
        {
            Log.Warning("Guides subfolder not found: {Path}", guidesPath);
            return new List<OneDriveGuideInfo>();
        }

        var guides = new List<OneDriveGuideInfo>();

        await Task.Run(() =>
        {
            try
            {
                var zipFiles = Directory.GetFiles(guidesPath, "*.zip");

                foreach (var zipFile in zipFiles)
                {
                    try
                    {
                        var fileInfo = new FileInfo(zipFile);
                        var guideInfo = new OneDriveGuideInfo
                        {
                            FileName = fileInfo.Name,
                            FullPath = fileInfo.FullName,
                            FileSize = fileInfo.Length,
                            LastModified = fileInfo.LastWriteTime
                        };

                        // Try to extract metadata from ZIP
                        ExtractGuideMetadata(zipFile, guideInfo);

                        guides.Add(guideInfo);
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "Could not process guide file: {File}", zipFile);
                    }
                }

                Log.Information("Found {Count} guide packages in OneDrive folder", guides.Count);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error scanning OneDrive guides folder");
            }
        });

        return guides;
    }

    /// <summary>
    /// Extracts guide metadata (title, version, ID) from ZIP package without full extraction.
    /// </summary>
    private void ExtractGuideMetadata(string zipPath, OneDriveGuideInfo guideInfo)
    {
        try
        {
            using var archive = ZipFile.OpenRead(zipPath);
            var metadataEntry = archive.GetEntry("metadata.json");

            if (metadataEntry != null)
            {
                using var stream = metadataEntry.Open();
                using var reader = new StreamReader(stream);
                var json = reader.ReadToEnd();

                var metadata = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
                if (metadata != null)
                {
                    if (metadata.TryGetValue("title", out var title))
                        guideInfo.Title = title.GetString();

                    if (metadata.TryGetValue("version", out var version))
                        guideInfo.Version = version.GetString();

                    if (metadata.TryGetValue("id", out var id))
                        guideInfo.GuideId = id.GetString();
                }
            }
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "Could not extract metadata from guide package: {File}", zipPath);
        }
    }

    /// <summary>
    /// Checks for new or updated guides by comparing with local database.
    /// </summary>
    public async Task<List<GuideUpdateInfo>> CheckForGuideUpdatesAsync()
    {
        var availableGuides = await GetAvailableGuidesAsync();
        var updates = new List<GuideUpdateInfo>();

        foreach (var oneDriveGuide in availableGuides)
        {
            // Try to find matching guide in local database
            var localGuide = _guideRepository.GetAll()
                .FirstOrDefault(g =>
                    g.Id.ToString() == oneDriveGuide.GuideId ||
                    g.Title == oneDriveGuide.Title);

            if (localGuide == null)
            {
                // New guide not in database
                updates.Add(new GuideUpdateInfo
                {
                    OneDriveGuide = oneDriveGuide,
                    UpdateType = GuideUpdateType.New,
                    LocalVersion = null
                });
            }
            else if (oneDriveGuide.LastModified > localGuide.UpdatedAt)
            {
                // Existing guide with newer version
                updates.Add(new GuideUpdateInfo
                {
                    OneDriveGuide = oneDriveGuide,
                    UpdateType = GuideUpdateType.Updated,
                    LocalVersion = localGuide.UpdatedAt
                });
            }
        }

        Log.Information("Found {NewCount} new and {UpdatedCount} updated guides",
            updates.Count(u => u.UpdateType == GuideUpdateType.New),
            updates.Count(u => u.UpdateType == GuideUpdateType.Updated));

        return updates;
    }

    /// <summary>
    /// Imports a guide package from OneDrive into local database.
    /// </summary>
    public async Task<ImportResult> ImportGuideFromOneDriveAsync(OneDriveGuideInfo guideInfo)
    {
        try
        {
            Log.Information("Importing guide from OneDrive: {File}", guideInfo.FileName);

            // Read the file into memory
            byte[] fileData = await File.ReadAllBytesAsync(guideInfo.FullPath);

            // Use existing import service
            var result = await _importService.ImportGuideAsync(
                fileData,
                guideInfo.FileName);

            if (result.Success)
            {
                Log.Information("Successfully imported guide: {Title}", result.ImportedGuide?.Title);
            }
            else
            {
                Log.Warning("Failed to import guide: {Error}", result.ErrorMessage);
            }

            return result;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error importing guide from OneDrive: {File}", guideInfo.FileName);
            return new ImportResult
            {
                Success = false,
                ErrorMessage = $"Import failed: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Starts monitoring OneDrive folder for changes.
    /// Uses FileSystemWatcher with debouncing to avoid multiple triggers.
    /// </summary>
    public void StartMonitoring()
    {
        var folderPath = GetOneDriveFolderPath();
        if (string.IsNullOrEmpty(folderPath))
        {
            Log.Warning("Cannot start monitoring: OneDrive folder not available");
            return;
        }

        var guidesPath = Path.Combine(folderPath, GUIDES_SUBFOLDER);
        if (!Directory.Exists(guidesPath))
        {
            Log.Warning("Cannot start monitoring: Guides folder not found at {Path}", guidesPath);
            return;
        }

        try
        {
            _watcher = new FileSystemWatcher(guidesPath)
            {
                Filter = "*.zip",
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size,
                EnableRaisingEvents = true
            };

            _watcher.Created += OnGuideFileChanged;
            _watcher.Changed += OnGuideFileChanged;
            _watcher.Renamed += OnGuideFileRenamed;

            Log.Information("Started monitoring OneDrive guide folder: {Path}", guidesPath);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error starting OneDrive folder monitoring");
        }
    }

    private void OnGuideFileChanged(object sender, FileSystemEventArgs e)
    {
        lock (_lock)
        {
            // Debounce: Reset timer on each event
            _debounceTimer?.Dispose();
            _debounceTimer = new Timer(async _ => await CheckAndNotifyUpdatesAsync(), null, 2000, Timeout.Infinite);
        }
    }

    private void OnGuideFileRenamed(object sender, RenamedEventArgs e)
    {
        OnGuideFileChanged(sender, e);
    }

    private async Task CheckAndNotifyUpdatesAsync()
    {
        try
        {
            var updates = await CheckForGuideUpdatesAsync();

            if (updates.Any())
            {
                GuideUpdatesDetected?.Invoke(this, new GuideUpdateDetectedEventArgs
                {
                    Updates = updates
                });
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error checking for guide updates");
        }
    }

    public void StopMonitoring()
    {
        if (_watcher != null)
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();
            _watcher = null;
            Log.Information("Stopped monitoring OneDrive guide folder");
        }

        _debounceTimer?.Dispose();
        _debounceTimer = null;
    }

    public void Dispose()
    {
        StopMonitoring();
    }
}

using System.Text.Json;
using GuideViewer.Core.Models;
using GuideViewer.Data.Repositories;

namespace GuideViewer.Core.Services;

/// <summary>
/// Service for managing application settings with JSON persistence.
/// </summary>
public class SettingsService : ISettingsService
{
    private const string SettingsKey = "AppSettings";
    private readonly SettingsRepository _settingsRepository;
    private AppSettings? _cachedSettings;

    /// <summary>
    /// Initializes a new instance of the SettingsService.
    /// </summary>
    public SettingsService(SettingsRepository settingsRepository)
    {
        _settingsRepository = settingsRepository ?? throw new ArgumentNullException(nameof(settingsRepository));
    }

    /// <inheritdoc/>
    public AppSettings LoadSettings()
    {
        if (_cachedSettings != null)
        {
            return _cachedSettings;
        }

        var json = _settingsRepository.GetValue(SettingsKey);

        if (string.IsNullOrEmpty(json))
        {
            _cachedSettings = AppSettings.CreateDefault();
            SaveSettings(_cachedSettings);
            return _cachedSettings;
        }

        try
        {
            _cachedSettings = JsonSerializer.Deserialize<AppSettings>(json);
            return _cachedSettings ?? AppSettings.CreateDefault();
        }
        catch (JsonException)
        {
            // If deserialization fails, return default settings
            _cachedSettings = AppSettings.CreateDefault();
            SaveSettings(_cachedSettings);
            return _cachedSettings;
        }
    }

    /// <inheritdoc/>
    public void SaveSettings(AppSettings settings)
    {
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        _settingsRepository.SetValue(SettingsKey, json);
        _cachedSettings = settings;
    }

    /// <inheritdoc/>
    public string GetTheme()
    {
        var settings = LoadSettings();
        return settings.Theme;
    }

    /// <inheritdoc/>
    public void SetTheme(string theme)
    {
        var settings = LoadSettings();
        settings.Theme = theme;
        SaveSettings(settings);
    }

    /// <inheritdoc/>
    public (double Width, double Height, double X, double Y, bool IsMaximized) GetWindowState()
    {
        var settings = LoadSettings();
        return (settings.WindowWidth, settings.WindowHeight, settings.WindowX, settings.WindowY, settings.IsMaximized);
    }

    /// <inheritdoc/>
    public void SaveWindowState(double width, double height, double x, double y, bool isMaximized)
    {
        var settings = LoadSettings();
        settings.WindowWidth = width;
        settings.WindowHeight = height;
        settings.WindowX = x;
        settings.WindowY = y;
        settings.IsMaximized = isMaximized;
        SaveSettings(settings);
    }

    /// <inheritdoc/>
    public T? GetValue<T>(string key, T? defaultValue = default)
    {
        var json = _settingsRepository.GetValue(key);

        if (string.IsNullOrEmpty(json))
        {
            return defaultValue;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(json);
        }
        catch (JsonException)
        {
            return defaultValue;
        }
    }

    /// <inheritdoc/>
    public void SetValue<T>(string key, T value)
    {
        var json = JsonSerializer.Serialize(value);
        _settingsRepository.SetValue(key, json);
    }
}

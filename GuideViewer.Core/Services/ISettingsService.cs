using GuideViewer.Core.Models;

namespace GuideViewer.Core.Services;

/// <summary>
/// Interface for application settings management.
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Loads the application settings.
    /// </summary>
    AppSettings LoadSettings();

    /// <summary>
    /// Saves the application settings.
    /// </summary>
    void SaveSettings(AppSettings settings);

    /// <summary>
    /// Gets the current theme setting.
    /// </summary>
    string GetTheme();

    /// <summary>
    /// Sets the application theme.
    /// </summary>
    void SetTheme(string theme);

    /// <summary>
    /// Gets the current window state.
    /// </summary>
    (double Width, double Height, double X, double Y, bool IsMaximized) GetWindowState();

    /// <summary>
    /// Saves the window state.
    /// </summary>
    void SaveWindowState(double width, double height, double x, double y, bool isMaximized);

    /// <summary>
    /// Gets a setting value by key.
    /// </summary>
    T? GetValue<T>(string key, T? defaultValue = default);

    /// <summary>
    /// Sets a setting value by key.
    /// </summary>
    void SetValue<T>(string key, T value);
}

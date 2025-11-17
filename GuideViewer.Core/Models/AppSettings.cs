namespace GuideViewer.Core.Models;

/// <summary>
/// Application settings model.
/// </summary>
public class AppSettings
{
    /// <summary>
    /// Gets or sets the application theme (Light, Dark, System).
    /// </summary>
    public string Theme { get; set; } = "System";

    /// <summary>
    /// Gets or sets the window width.
    /// </summary>
    public double WindowWidth { get; set; } = 1200;

    /// <summary>
    /// Gets or sets the window height.
    /// </summary>
    public double WindowHeight { get; set; } = 800;

    /// <summary>
    /// Gets or sets the window X position.
    /// </summary>
    public double WindowX { get; set; } = -1; // -1 means center on screen

    /// <summary>
    /// Gets or sets the window Y position.
    /// </summary>
    public double WindowY { get; set; } = -1; // -1 means center on screen

    /// <summary>
    /// Gets or sets whether the window is maximized.
    /// </summary>
    public bool IsMaximized { get; set; }

    /// <summary>
    /// Gets or sets the last opened guide ID.
    /// </summary>
    public string? LastOpenedGuideId { get; set; }

    /// <summary>
    /// Gets or sets whether to show welcome screen on startup.
    /// </summary>
    public bool ShowWelcomeScreen { get; set; } = true;

    /// <summary>
    /// Creates default settings.
    /// </summary>
    public static AppSettings CreateDefault()
    {
        return new AppSettings();
    }
}

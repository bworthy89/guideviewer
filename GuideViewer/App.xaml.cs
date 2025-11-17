using GuideViewer.Core.Services;
using GuideViewer.Data.Repositories;
using GuideViewer.Data.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Serilog;

namespace GuideViewer;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    private Window? m_window;

    /// <summary>
    /// Gets the current application instance.
    /// </summary>
    public static new App Current => (App)Application.Current;

    /// <summary>
    /// Gets the service provider for dependency injection.
    /// </summary>
    public IServiceProvider Services { get; }

    /// <summary>
    /// Initializes the singleton application object. This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();

        // Configure logging
        ConfigureLogging();

        // Build dependency injection container
        Services = ConfigureServices();

        // Log application startup
        Log.Information("GuideViewer application starting...");
    }

    /// <summary>
    /// Configures the Serilog logger.
    /// </summary>
    private void ConfigureLogging()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var logPath = Path.Combine(appDataPath, "GuideViewer", "logs", "app-.log");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(
                logPath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }

    /// <summary>
    /// Configures dependency injection services.
    /// </summary>
    private IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Data layer - Singleton for database connection
        services.AddSingleton<DatabaseService>();

        // Repositories - Transient as they're lightweight
        services.AddTransient<UserRepository>();
        services.AddTransient<SettingsRepository>();

        // Core services - Singleton for application lifetime
        services.AddSingleton<LicenseValidator>();
        services.AddSingleton<ISettingsService, SettingsService>();

        // Logging
        services.AddSingleton(Log.Logger);

        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        try
        {
            m_window = new MainWindow();
            m_window.Activate();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to launch main window");
            throw;
        }
    }

    /// <summary>
    /// Gets a service from the dependency injection container.
    /// </summary>
    public static T GetService<T>() where T : class
    {
        return Current.Services.GetRequiredService<T>();
    }
}

/// <summary>
/// Extension methods for dependency injection.
/// </summary>
public static class ServiceExtensions
{
    /// <summary>
    /// Gets a service from the application's service provider.
    /// </summary>
    public static T GetService<T>(this Application app) where T : class
    {
        return ((App)app).Services.GetRequiredService<T>();
    }
}

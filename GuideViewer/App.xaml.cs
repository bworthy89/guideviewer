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
    /// Gets the main application window.
    /// </summary>
    public static Window? MainWindow { get; internal set; }

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

        // Set up global error handling
        this.UnhandledException += OnUnhandledException;

        // Log application startup
        Log.Information("GuideViewer application starting...");

        // Seed sample data for testing (only if database is empty)
        SeedSampleDataIfNeeded();
    }

    /// <summary>
    /// Seeds sample data into the database if it's empty.
    /// </summary>
    private void SeedSampleDataIfNeeded()
    {
        try
        {
            var guideRepository = Services.GetService<GuideViewer.Data.Repositories.GuideRepository>();
            var categoryRepository = Services.GetService<GuideViewer.Data.Repositories.CategoryRepository>();

            if (guideRepository != null && categoryRepository != null)
            {
                GuideViewer.Core.Utilities.SampleDataSeeder.SeedSampleData(categoryRepository, guideRepository);
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to seed sample data");
        }
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
        services.AddTransient<GuideViewer.Data.Repositories.GuideRepository>();
        services.AddTransient<GuideViewer.Data.Repositories.CategoryRepository>();
        services.AddTransient<GuideViewer.Data.Repositories.ProgressRepository>();

        // Core services - Singleton for application lifetime
        services.AddSingleton<LicenseValidator>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<GuideViewer.Core.Services.IImageStorageService, GuideViewer.Core.Services.ImageStorageService>();
        services.AddSingleton<GuideViewer.Core.Services.IGuideExportService, GuideViewer.Core.Services.GuideExportService>();
        services.AddSingleton<GuideViewer.Core.Services.IGuideImportService, GuideViewer.Core.Services.GuideImportService>();
        services.AddSingleton<GuideViewer.Core.Services.IDatabaseBackupService, GuideViewer.Core.Services.DatabaseBackupService>();

        // UI services - Singleton for application lifetime
        services.AddSingleton<GuideViewer.Services.NavigationService>();
        services.AddSingleton<GuideViewer.Services.IKeyboardShortcutService, GuideViewer.Services.KeyboardShortcutService>();
        services.AddSingleton<GuideViewer.Core.Services.IErrorHandlingService, GuideViewer.Core.Services.ErrorHandlingService>();
        services.AddSingleton<GuideViewer.Core.Services.IPerformanceMonitoringService, GuideViewer.Core.Services.PerformanceMonitoringService>();

        // ViewModels - Singleton for application lifetime
        services.AddSingleton<GuideViewer.ViewModels.MainViewModel>();

        // Editor services - Transient (one per editor instance)
        services.AddTransient<GuideViewer.Core.Services.IAutoSaveService, GuideViewer.Core.Services.AutoSaveService>();

        // Progress tracking services - Transient (one per usage)
        services.AddTransient<GuideViewer.Core.Services.ITimerService, GuideViewer.Core.Services.TimerService>();
        services.AddTransient<GuideViewer.Core.Services.IProgressTrackingService, GuideViewer.Core.Services.ProgressTrackingService>();

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
            // Check if user is already activated
            var userRepository = GetService<UserRepository>();
            var currentUser = userRepository.GetCurrentUser();

            if (currentUser != null)
            {
                // User already activated, show main window
                Log.Information("User already activated with role: {Role}", currentUser.Role);
                m_window = new MainWindow();
                MainWindow = m_window;
            }
            else
            {
                // First run, show activation window
                Log.Information("First run detected, showing activation window");
                m_window = new Views.ActivationWindow();
                MainWindow = m_window;
            }

            m_window.Activate();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to launch application window");
            throw;
        }
    }

    /// <summary>
    /// Handles unhandled exceptions at the application level.
    /// </summary>
    private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        try
        {
            // Get the error handling service
            var errorHandlingService = Services.GetService<GuideViewer.Core.Services.IErrorHandlingService>();

            if (errorHandlingService != null)
            {
                // Handle the exception
                var errorInfo = errorHandlingService.HandleException(e.Exception, "Unhandled Application Error");

                // Mark as handled if recoverable to prevent app crash
                if (errorInfo.IsRecoverable)
                {
                    e.Handled = true;
                    Log.Information("Unhandled exception was marked as recoverable and handled");

                    // Show error dialog on UI thread
                    MainWindow?.DispatcherQueue.TryEnqueue(async () =>
                    {
                        await errorHandlingService.ShowErrorDialogAsync(errorInfo);
                    });
                }
                else
                {
                    // Log fatal error and let app crash
                    Log.Fatal(e.Exception, "Unrecoverable error occurred. Application will terminate.");
                }
            }
            else
            {
                // Fallback logging if service not available
                Log.Error(e.Exception, "Unhandled exception occurred and error handling service is not available");
            }
        }
        catch (Exception ex)
        {
            // Last resort logging
            Log.Fatal(ex, "Failed to handle unhandled exception");
        }
    }

    /// <summary>
    /// Gets a service from the dependency injection container.
    /// </summary>
    public static T GetService<T>() where T : class
    {
        return ((App)Application.Current).Services.GetRequiredService<T>();
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

using GuideViewer.Services;
using GuideViewer.ViewModels;
using GuideViewer.Views.Pages;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Runtime.InteropServices;
using WinRT;

namespace GuideViewer;

/// <summary>
/// The main window of the GuideViewer application.
/// </summary>
public sealed partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; }
    private readonly NavigationService _navigationService;

    public MainWindow()
    {
        this.InitializeComponent();

        // Get services from DI
        _navigationService = App.GetService<NavigationService>();
        ViewModel = App.GetService<MainViewModel>();

        // Set DataContext for bindings
        if (this.Content is FrameworkElement rootElement)
        {
            rootElement.DataContext = ViewModel;
        }

        // Set window size using AppWindow API
        var appWindow = AppWindow;
        if (appWindow != null)
        {
            appWindow.Resize(new Windows.Graphics.SizeInt32 { Width = 1200, Height = 800 });
        }

        // Apply Mica background material
        TrySetMicaBackdrop();

        // Register pages with navigation service
        RegisterPages();

        // Set up navigation frame
        _navigationService.Frame = ContentFrame;

        // Navigate to home page by default
        _navigationService.NavigateTo(PageKeys.Home);
        NavigationViewControl.SelectedItem = HomeNavItem;
    }

    /// <summary>
    /// Registers all pages with the navigation service.
    /// </summary>
    private void RegisterPages()
    {
        _navigationService.RegisterPage<HomePage>(PageKeys.Home);
        _navigationService.RegisterPage<GuidesPage>(PageKeys.Guides);
        _navigationService.RegisterPage<GuideEditorPage>(PageKeys.GuideEditor);
        _navigationService.RegisterPage<GuideDetailPage>(PageKeys.GuideDetail);
        _navigationService.RegisterPage<ProgressPage>(PageKeys.Progress);
        _navigationService.RegisterPage<ActiveGuideProgressPage>(PageKeys.ActiveGuide);
        _navigationService.RegisterPage<SettingsPage>(PageKeys.Settings);
    }

    /// <summary>
    /// Handles navigation view selection changes.
    /// </summary>
    private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem item)
        {
            var pageKey = item.Tag?.ToString();
            if (!string.IsNullOrEmpty(pageKey))
            {
                _navigationService.NavigateTo(pageKey);
            }
        }
    }

    #region Mica Background

    private WindowsSystemDispatcherQueueHelper? m_wsdqHelper;
    private MicaController? m_micaController;
    private SystemBackdropConfiguration? m_configurationSource;

    private bool TrySetMicaBackdrop()
    {
        if (MicaController.IsSupported())
        {
            m_wsdqHelper = new WindowsSystemDispatcherQueueHelper();
            m_wsdqHelper.EnsureWindowsSystemDispatcherQueueController();

            // Create the policy object
            m_configurationSource = new SystemBackdropConfiguration();
            this.Activated += Window_Activated;
            this.Closed += Window_Closed;

            // Initial configuration state
            m_configurationSource.IsInputActive = true;
            SetConfigurationSourceTheme();

            m_micaController = new MicaController();
            m_micaController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
            m_micaController.SetSystemBackdropConfiguration(m_configurationSource);
            return true;
        }

        return false;
    }

    private void Window_Activated(object sender, WindowActivatedEventArgs args)
    {
        if (m_configurationSource != null)
        {
            m_configurationSource.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;
        }
    }

    private void Window_Closed(object sender, WindowEventArgs args)
    {
        if (m_micaController != null)
        {
            m_micaController.Dispose();
            m_micaController = null;
        }
        this.Activated -= Window_Activated;
        m_configurationSource = null;
    }

    private void SetConfigurationSourceTheme()
    {
        if (m_configurationSource != null)
        {
            m_configurationSource.Theme = this.Content switch
            {
                FrameworkElement { ActualTheme: ElementTheme.Dark } => SystemBackdropTheme.Dark,
                FrameworkElement { ActualTheme: ElementTheme.Light } => SystemBackdropTheme.Light,
                FrameworkElement { ActualTheme: ElementTheme.Default } => SystemBackdropTheme.Default,
                _ => SystemBackdropTheme.Default
            };
        }
    }

    #endregion
}

// Helper for Mica backdrop
class WindowsSystemDispatcherQueueHelper
{
    [StructLayout(LayoutKind.Sequential)]
    struct DispatcherQueueOptions
    {
        internal int dwSize;
        internal int threadType;
        internal int apartmentType;
    }

    [DllImport("CoreMessaging.dll")]
    private static extern int CreateDispatcherQueueController([In] DispatcherQueueOptions options, [In, Out, MarshalAs(UnmanagedType.IUnknown)] ref object? dispatcherQueueController);

    object? m_dispatcherQueueController = null;
    public void EnsureWindowsSystemDispatcherQueueController()
    {
        if (Windows.System.DispatcherQueue.GetForCurrentThread() != null)
        {
            return;
        }

        if (m_dispatcherQueueController == null)
        {
            DispatcherQueueOptions options;
            options.dwSize = Marshal.SizeOf(typeof(DispatcherQueueOptions));
            options.threadType = 2;    // DQTYPE_THREAD_CURRENT
            options.apartmentType = 2; // DQTAT_COM_STA

            CreateDispatcherQueueController(options, ref m_dispatcherQueueController);
        }
    }
}

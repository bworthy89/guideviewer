using GuideViewer.Core.Services;
using GuideViewer.Services;
using GuideViewer.ViewModels;
using GuideViewer.Views.Pages;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System.Runtime.InteropServices;
using Windows.System;
using WinRT;

namespace GuideViewer;

/// <summary>
/// The main window of the GuideViewer application.
/// </summary>
public sealed partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; }
    private readonly NavigationService _navigationService;
    private readonly IKeyboardShortcutService _keyboardShortcutService;

    public MainWindow()
    {
        this.InitializeComponent();

        // Get services from DI
        _navigationService = App.GetService<NavigationService>();
        _keyboardShortcutService = App.GetService<IKeyboardShortcutService>();
        ViewModel = App.GetService<MainViewModel>();

        // Connect error handling service to UI
        SetupErrorHandling();

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

        // Register keyboard shortcuts
        RegisterKeyboardShortcuts();

        // Hook up keyboard event handler
        if (this.Content is UIElement rootUIElement)
        {
            rootUIElement.KeyDown += OnKeyDown;
        }

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
        _navigationService.RegisterPage<AboutPage>(PageKeys.About);
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

    #region Keyboard Shortcuts

    /// <summary>
    /// Registers all global keyboard shortcuts.
    /// </summary>
    private void RegisterKeyboardShortcuts()
    {
        // Ctrl+N: New Guide (admin only)
        _keyboardShortcutService.RegisterShortcut(
            VirtualKey.N,
            VirtualKeyModifiers.Control,
            () =>
            {
                if (ViewModel.IsAdmin)
                {
                    _navigationService.NavigateTo(PageKeys.GuideEditor);
                }
            },
            "Create New Guide (Admin)");

        // Ctrl+F: Focus search (navigate to Guides page with search focused)
        _keyboardShortcutService.RegisterShortcut(
            VirtualKey.F,
            VirtualKeyModifiers.Control,
            () => _navigationService.NavigateTo(PageKeys.Guides),
            "Go to Guides");

        // Ctrl+B: Navigate to Settings (for backup)
        _keyboardShortcutService.RegisterShortcut(
            VirtualKey.B,
            VirtualKeyModifiers.Control,
            () => _navigationService.NavigateTo(PageKeys.Settings),
            "Go to Settings (Backup)");

        // Ctrl+E: Navigate to Settings (for export)
        _keyboardShortcutService.RegisterShortcut(
            VirtualKey.E,
            VirtualKeyModifiers.Control,
            () => _navigationService.NavigateTo(PageKeys.Settings),
            "Go to Settings (Export)");

        // Ctrl+I: Navigate to Settings (for import)
        _keyboardShortcutService.RegisterShortcut(
            VirtualKey.I,
            VirtualKeyModifiers.Control,
            () => _navigationService.NavigateTo(PageKeys.Settings),
            "Go to Settings (Import)");

        // F1: Navigate to About page
        _keyboardShortcutService.RegisterShortcut(
            VirtualKey.F1,
            VirtualKeyModifiers.None,
            () => _navigationService.NavigateTo(PageKeys.About),
            "About");

        // Escape: Go back
        _keyboardShortcutService.RegisterShortcut(
            VirtualKey.Escape,
            VirtualKeyModifiers.None,
            () =>
            {
                if (_navigationService.CanGoBack)
                {
                    _navigationService.GoBack();
                }
            },
            "Go Back");

        // F2: Show keyboard shortcuts help
        _keyboardShortcutService.RegisterShortcut(
            VirtualKey.F2,
            VirtualKeyModifiers.None,
            () => ShowKeyboardShortcutsHelp(),
            "Show Keyboard Shortcuts");
    }

    /// <summary>
    /// Handles keyboard input for shortcuts.
    /// </summary>
    private void OnKeyDown(object sender, KeyRoutedEventArgs e)
    {
        // Get current modifiers
        var modifiers = VirtualKeyModifiers.None;
        var coreWindow = Microsoft.UI.Xaml.Window.Current?.CoreWindow;

        if (coreWindow != null)
        {
            var ctrlState = coreWindow.GetKeyState(VirtualKey.Control);
            var shiftState = coreWindow.GetKeyState(VirtualKey.Shift);
            var altState = coreWindow.GetKeyState(VirtualKey.Menu);

            if (ctrlState.HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
                modifiers |= VirtualKeyModifiers.Control;
            if (shiftState.HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
                modifiers |= VirtualKeyModifiers.Shift;
            if (altState.HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
                modifiers |= VirtualKeyModifiers.Menu;
        }

        // Process the shortcut
        if (_keyboardShortcutService.ProcessKeyPress(e.Key, modifiers))
        {
            e.Handled = true;
        }
    }

    /// <summary>
    /// Shows the keyboard shortcuts help dialog.
    /// </summary>
    private async void ShowKeyboardShortcutsHelp()
    {
        var shortcuts = _keyboardShortcutService.GetRegisteredShortcuts();
        var shortcutText = string.Join("\n", shortcuts.Select(kvp => $"{kvp.Key}: {kvp.Value}"));

        var dialog = new ContentDialog
        {
            Title = "Keyboard Shortcuts",
            Content = new ScrollViewer
            {
                Content = new TextBlock
                {
                    Text = shortcutText,
                    TextWrapping = TextWrapping.Wrap
                },
                MaxHeight = 400
            },
            CloseButtonText = "Close",
            XamlRoot = this.Content.XamlRoot
        };

        await dialog.ShowAsync();
    }

    #endregion

    #region Error Handling

    /// <summary>
    /// Sets up the error handling service to show dialogs in the UI.
    /// </summary>
    private void SetupErrorHandling()
    {
        var errorHandlingService = App.GetService<IErrorHandlingService>();
        if (errorHandlingService is GuideViewer.Core.Services.ErrorHandlingService service)
        {
            service.SetShowDialogFunction(ShowErrorDialogAsync);
        }
    }

    /// <summary>
    /// Shows an error dialog with detailed error information.
    /// </summary>
    private async System.Threading.Tasks.Task ShowErrorDialogAsync(GuideViewer.Core.Models.ErrorInfo errorInfo)
    {
        try
        {
            var content = new StackPanel { Spacing = 12 };

            // Error message
            content.Children.Add(new TextBlock
            {
                Text = errorInfo.UserMessage,
                TextWrapping = TextWrapping.Wrap
            });

            // Suggested actions
            if (errorInfo.SuggestedActions.Count > 0)
            {
                content.Children.Add(new TextBlock
                {
                    Text = "Suggested actions:",
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                    Margin = new Thickness(0, 8, 0, 4)
                });

                foreach (var action in errorInfo.SuggestedActions)
                {
                    content.Children.Add(new TextBlock
                    {
                        Text = $"• {action}",
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(12, 0, 0, 0)
                    });
                }
            }

            var dialog = new ContentDialog
            {
                Title = $"Error: {errorInfo.Category}",
                Content = content,
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };

            await dialog.ShowAsync();
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "Failed to show error dialog");
        }
    }

    #endregion

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

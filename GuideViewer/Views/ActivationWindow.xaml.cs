using GuideViewer.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace GuideViewer.Views;

/// <summary>
/// Activation window where users enter their product key.
/// </summary>
public sealed partial class ActivationWindow : Window
{
    public ActivationViewModel ViewModel { get; }

    public ActivationWindow()
    {
        this.InitializeComponent();

        // Get ViewModel from DI
        ViewModel = new ActivationViewModel(
            App.GetService<GuideViewer.Core.Services.LicenseValidator>(),
            App.GetService<GuideViewer.Data.Repositories.UserRepository>()
        );

        // Set DataContext for bindings (cast to FrameworkElement for WinUI 3)
        if (this.Content is Microsoft.UI.Xaml.FrameworkElement rootElement)
        {
            rootElement.DataContext = ViewModel;
        }

        // Subscribe to activation success event
        ViewModel.ActivationSucceeded += ViewModel_ActivationSucceeded;

        // Set window size
        var appWindow = AppWindow;
        if (appWindow != null)
        {
            appWindow.Resize(new Windows.Graphics.SizeInt32 { Width = 600, Height = 700 });
        }

        // Focus first segment on load
        this.Activated += (s, e) => Segment1TextBox.Focus(FocusState.Programmatic);
    }

    /// <summary>
    /// Handles text changed event to auto-advance to next segment.
    /// </summary>
    private void SegmentTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is not TextBox textBox)
            return;

        // Auto-advance to next segment when 4 characters are entered
        if (textBox.Text.Length == 4)
        {
            if (textBox.Name == nameof(Segment1TextBox))
                Segment2TextBox.Focus(FocusState.Programmatic);
            else if (textBox.Name == nameof(Segment2TextBox))
                Segment3TextBox.Focus(FocusState.Programmatic);
            else if (textBox.Name == nameof(Segment3TextBox))
                Segment4TextBox.Focus(FocusState.Programmatic);
            else if (textBox.Name == nameof(Segment4TextBox))
                ActivateButton.Focus(FocusState.Programmatic);
        }
    }

    /// <summary>
    /// Handles key down events for backspace navigation.
    /// </summary>
    private void SegmentTextBox_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (sender is not TextBox textBox)
            return;

        // Handle backspace to move to previous segment
        if (e.Key == Windows.System.VirtualKey.Back && string.IsNullOrEmpty(textBox.Text))
        {
            if (textBox.Name == nameof(Segment4TextBox))
            {
                Segment3TextBox.Focus(FocusState.Programmatic);
                Segment3TextBox.SelectionStart = Segment3TextBox.Text.Length;
            }
            else if (textBox.Name == nameof(Segment3TextBox))
            {
                Segment2TextBox.Focus(FocusState.Programmatic);
                Segment2TextBox.SelectionStart = Segment2TextBox.Text.Length;
            }
            else if (textBox.Name == nameof(Segment2TextBox))
            {
                Segment1TextBox.Focus(FocusState.Programmatic);
                Segment1TextBox.SelectionStart = Segment1TextBox.Text.Length;
            }
        }
        // Handle Enter key to activate
        else if (e.Key == Windows.System.VirtualKey.Enter)
        {
            if (ViewModel.ActivateCommand.CanExecute(null))
            {
                ViewModel.ActivateCommand.Execute(null);
            }
        }
    }

    /// <summary>
    /// Handles paste event to auto-split product key.
    /// </summary>
    private void SegmentTextBox_Paste(object sender, TextControlPasteEventArgs e)
    {
        // Get clipboard content
        var dataPackage = Windows.ApplicationModel.DataTransfer.Clipboard.GetContent();
        if (dataPackage.Contains(Windows.ApplicationModel.DataTransfer.StandardDataFormats.Text))
        {
            var task = dataPackage.GetTextAsync().AsTask();
            task.Wait();
            var pastedText = task.Result;

            // Let ViewModel handle the paste
            ViewModel.HandlePaste(pastedText);

            // Prevent default paste behavior
            e.Handled = true;

            // Focus the last segment
            Segment4TextBox.Focus(FocusState.Programmatic);
        }
    }

    /// <summary>
    /// Handles activation success event.
    /// </summary>
    private void ViewModel_ActivationSucceeded(object? sender, GuideViewer.Core.Models.UserRole role)
    {
        // Close activation window and open main window
        var mainWindow = new MainWindow();
        App.MainWindow = mainWindow;
        mainWindow.Activate();
        this.Close();
    }

    /// <summary>
    /// Handles error InfoBar close event.
    /// </summary>
    private void ErrorInfoBar_Closed(InfoBar sender, InfoBarClosedEventArgs args)
    {
        ViewModel.ClearError();
    }
}

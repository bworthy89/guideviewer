using GuideViewer.Core.Services;
using GuideViewer.Data.Entities;
using GuideViewer.Data.Repositories;
using GuideViewer.Services;
using GuideViewer.ViewModels;
using LiteDB;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace GuideViewer.Views.Pages;

public sealed partial class GuideEditorPage : Page
{
    public GuideEditorViewModel ViewModel { get; private set; }

    public GuideEditorPage()
    {
        this.InitializeComponent();

        // Initialize ViewModel with dependencies
        var guideRepository = App.GetService<GuideRepository>();
        var categoryRepository = App.GetService<CategoryRepository>();
        var userRepository = App.GetService<UserRepository>();
        var imageStorageService = App.GetService<IImageStorageService>();
        var autoSaveService = App.GetService<IAutoSaveService>();
        var navigationService = App.GetService<NavigationService>();

        ViewModel = new GuideEditorViewModel(
            guideRepository,
            categoryRepository,
            userRepository,
            imageStorageService,
            autoSaveService,
            navigationService,
            this.DispatcherQueue);

        this.DataContext = ViewModel;

        // Set page name for ElementName binding
        this.Name = "PageRoot";
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        // Parameter can be null (new guide) or ObjectId (edit existing guide)
        ObjectId? guideId = null;
        if (e.Parameter is ObjectId objectId)
        {
            guideId = objectId;
        }
        else if (e.Parameter != null)
        {
            Log.Warning("Invalid parameter type passed to GuideEditorPage: {ParameterType}", e.Parameter.GetType().Name);
        }

        await ViewModel.InitializeAsync(guideId);
    }

    /// <summary>
    /// Handles Edit Step button click - shows the step editor panel.
    /// </summary>
    private void EditStep_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Step step)
        {
            ViewModel.SelectedStep = step;
            LoadStepContentIntoEditor(step);
            StepEditorPanel.Visibility = Visibility.Visible;

            // Scroll to editor
            var scrollViewer = FindParentScrollViewer(StepEditorPanel);
            scrollViewer?.ChangeView(null, scrollViewer.ScrollableHeight, null);
        }
    }

    /// <summary>
    /// Handles Close Editor button click - hides the step editor panel.
    /// </summary>
    private void CloseStepEditor_Click(object sender, RoutedEventArgs e)
    {
        SaveStepContentFromEditor();
        StepEditorPanel.Visibility = Visibility.Collapsed;
        ViewModel.SelectedStep = null;
    }

    /// <summary>
    /// Loads step content into the RichEditBox.
    /// </summary>
    private void LoadStepContentIntoEditor(Step step)
    {
        if (string.IsNullOrEmpty(step.Content))
        {
            StepContentEditor.Document.SetText(Microsoft.UI.Text.TextSetOptions.None, string.Empty);
        }
        else
        {
            try
            {
                // Try loading as RTF first
                using var stream = new MemoryStream();
                using var writer = new StreamWriter(stream);
                writer.Write(step.Content);
                writer.Flush();
                stream.Position = 0;
                using var randomAccessStream = stream.AsRandomAccessStream();

                StepContentEditor.Document.LoadFromStream(
                    Microsoft.UI.Text.TextSetOptions.FormatRtf,
                    randomAccessStream);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Invalid RTF content for step, falling back to plain text");
                // Fall back to plain text
                StepContentEditor.Document.SetText(
                    Microsoft.UI.Text.TextSetOptions.None,
                    step.Content);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error loading step content");
                // Fall back to plain text
                StepContentEditor.Document.SetText(
                    Microsoft.UI.Text.TextSetOptions.None,
                    step.Content);
            }
        }
    }

    /// <summary>
    /// Saves step content from the RichEditBox back to the step.
    /// </summary>
    private void SaveStepContentFromEditor()
    {
        if (ViewModel.SelectedStep == null) return;

        try
        {
            // Save as RTF
            using var stream = new MemoryStream();
            using var randomAccessStream = stream.AsRandomAccessStream();

            StepContentEditor.Document.SaveToStream(
                Microsoft.UI.Text.TextGetOptions.FormatRtf,
                randomAccessStream);

            stream.Position = 0;
            using var reader = new StreamReader(stream);
            ViewModel.SelectedStep.Content = reader.ReadToEnd();
            ViewModel.SelectedStep.UpdatedAt = DateTime.UtcNow;

            // Mark as dirty for auto-save
            ViewModel.HasUnsavedChanges = true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save step content");
        }
    }

    /// <summary>
    /// Handles RichEditBox text changed - auto-save trigger.
    /// </summary>
    private void StepContentEditor_TextChanged(object sender, RoutedEventArgs e)
    {
        // Save content on every change (debounced by auto-save service)
        SaveStepContentFromEditor();
    }

    /// <summary>
    /// Handles Upload Image button click - opens file picker.
    /// </summary>
    private async void UploadImage_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedStep == null)
        {
            ViewModel.ValidationMessage = "Please select a step first.";
            ViewModel.HasValidationError = true;
            return;
        }

        try
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".bmp");

            // Get the window handle for the picker
            if (App.MainWindow == null)
            {
                Log.Error("MainWindow is null, cannot show file picker");
                return;
            }

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                using var stream = await file.OpenStreamForReadAsync();
                stream.Position = 0; // Ensure we're at the start
                await ViewModel.UploadImageAsync(stream, file.Name);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to upload image");
            ViewModel.ValidationMessage = "Failed to upload image. Please try again.";
            ViewModel.HasValidationError = true;
        }
    }

    /// <summary>
    /// Loads image from LiteDB FileStorage when Image control is loaded.
    /// </summary>
    private async void StepImage_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is Image image && image.Tag is string fileId)
        {
            try
            {
                var imageStorageService = App.GetService<IImageStorageService>();
                var imageStream = await imageStorageService.GetImageAsync(fileId);

                if (imageStream != null)
                {
                    using (imageStream)
                    {
                        var bitmap = new BitmapImage();
                        await bitmap.SetSourceAsync(imageStream.AsRandomAccessStream());

                        // Ensure we're on the UI thread
                        if (this.DispatcherQueue.HasThreadAccess)
                        {
                            image.Source = bitmap;
                        }
                        else
                        {
                            this.DispatcherQueue.TryEnqueue(() =>
                            {
                                image.Source = bitmap;
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to load image: {FileId}", fileId);
            }
        }
    }

    /// <summary>
    /// Finds the parent ScrollViewer of an element.
    /// </summary>
    private ScrollViewer? FindParentScrollViewer(DependencyObject element)
    {
        var parent = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetParent(element);
        while (parent != null)
        {
            if (parent is ScrollViewer scrollViewer)
                return scrollViewer;

            parent = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetParent(parent);
        }
        return null;
    }
}

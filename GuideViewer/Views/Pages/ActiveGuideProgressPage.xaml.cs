using GuideViewer.Core.Services;
using GuideViewer.Data.Entities;
using GuideViewer.Data.Repositories;
using GuideViewer.Services;
using GuideViewer.ViewModels;
using LiteDB;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Serilog;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace GuideViewer.Views.Pages;

/// <summary>
/// Active guide progress page - full-screen step-by-step guide tracking.
/// </summary>
public sealed partial class ActiveGuideProgressPage : Page
{
    public ActiveGuideProgressViewModel ViewModel { get; }

    private NavigationService? _navigationService;

    public ActiveGuideProgressPage()
    {
        this.InitializeComponent();

        // Initialize ViewModel with dependencies
        ViewModel = new ActiveGuideProgressViewModel(
            App.GetService<IProgressTrackingService>(),
            App.GetService<ITimerService>(),
            App.GetService<IAutoSaveService>(),
            App.GetService<GuideRepository>(),
            this.DispatcherQueue);

        this.DataContext = ViewModel;

        // Subscribe to property changes with named method to prevent memory leak
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;

        // Get navigation service
        _navigationService = App.GetService<NavigationService>();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        // Expect parameters: (ObjectId guideId, ObjectId? progressId)
        if (e.Parameter is ValueTuple<ObjectId, ObjectId?> parameters)
        {
            var (guideId, progressId) = parameters;

            // Get current user
            var userRepository = App.GetService<UserRepository>();
            var currentUser = userRepository.GetCurrentUser();

            if (currentUser == null)
            {
                Log.Error("No current user found for active guide progress");
                return;
            }

            await ViewModel.InitializeAsync(currentUser.Id, guideId, progressId);
        }
        else
        {
            Log.Error("Invalid navigation parameters for ActiveGuideProgressPage");
        }
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Load RTF and image when current step changes
        if (e.PropertyName == nameof(ViewModel.CurrentStep))
        {
            LoadCurrentStepContent();
        }

        // Navigate back when guide is finished
        if (e.PropertyName == nameof(ViewModel.CanFinishGuide) && ViewModel.CanFinishGuide)
        {
            // The user will click "Finish Guide" button
            // We'll handle navigation in the FinishGuide command
        }
    }

    private void LoadCurrentStepContent()
    {
        if (ViewModel.CurrentStep == null)
        {
            return;
        }

        // Load RTF content
        LoadStepInstructions(ViewModel.CurrentStep);

        // Load image if exists
        LoadStepImage(ViewModel.CurrentStep);
    }

    private void LoadStepInstructions(Step step)
    {
        if (string.IsNullOrWhiteSpace(step.Content))
        {
            // No content, clear the RichEditBox
            StepContentRichEditBox.Document.SetText(TextSetOptions.None, string.Empty);
            return;
        }

        try
        {
            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream);
            writer.Write(step.Content);
            writer.Flush();
            stream.Position = 0;

            StepContentRichEditBox.Document.LoadFromStream(TextSetOptions.FormatRtf, stream.AsRandomAccessStream());
            Log.Debug("Loaded RTF content for step {StepOrder}", step.Order);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load RTF content for step {StepOrder}", step.Order);
            // Fallback: just set plain text
            StepContentRichEditBox.Document.SetText(TextSetOptions.None, "Failed to load instructions.");
        }
    }

    private async void LoadStepImage(Step step)
    {
        if (step.ImageIds == null || step.ImageIds.Count == 0)
        {
            // No images, hide the image border
            this.DispatcherQueue.TryEnqueue(() =>
            {
                StepImageBorder.Visibility = Visibility.Collapsed;
                StepImage.Source = null;
            });
            return;
        }

        try
        {
            // Load the first image (in a real app, you might want a gallery for multiple images)
            var firstImageId = step.ImageIds[0];

            await Task.Run(() =>
            {
                var databaseService = App.GetService<Data.Services.DatabaseService>();
                var fileInfo = databaseService.Database.FileStorage.FindById(firstImageId);

                if (fileInfo == null)
                {
                    Log.Warning("Image file {FileId} not found for step {StepOrder}", firstImageId, step.Order);
                    this.DispatcherQueue.TryEnqueue(() =>
                    {
                        StepImageBorder.Visibility = Visibility.Collapsed;
                        StepImage.Source = null;
                    });
                    return;
                }

                using var stream = new MemoryStream();
                fileInfo.CopyTo(stream);
                stream.Position = 0;

                // Load image on UI thread
                this.DispatcherQueue.TryEnqueue(async () =>
                {
                    try
                    {
                        var bitmap = new BitmapImage();
                        await bitmap.SetSourceAsync(stream.AsRandomAccessStream());
                        StepImage.Source = bitmap;
                        StepImageBorder.Visibility = Visibility.Visible;
                        Log.Debug("Loaded image for step {StepOrder}", step.Order);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Failed to set bitmap source for step {StepOrder}", step.Order);
                        StepImageBorder.Visibility = Visibility.Collapsed;
                    }
                });
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load image for step {StepOrder}", step.Order);
            this.DispatcherQueue.TryEnqueue(() =>
            {
                StepImageBorder.Visibility = Visibility.Collapsed;
                StepImage.Source = null;
            });
        }
    }

    private async void ExitButton_Click(object sender, RoutedEventArgs e)
    {
        // Check for unsaved notes
        if (ViewModel.HasUnsavedNotes)
        {
            var dialog = new ContentDialog
            {
                Title = "Unsaved Notes",
                Content = "You have unsaved notes. Do you want to save them before exiting?",
                PrimaryButtonText = "Save & Exit",
                SecondaryButtonText = "Exit Without Saving",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                // Save notes before exiting
                await ViewModel.SaveNotesCommand.ExecuteAsync(null);
            }
            else if (result == ContentDialogResult.None)
            {
                // Cancel - don't exit
                return;
            }
            // Secondary = exit without saving, continue below
        }

        // Dispose of ViewModel and navigate back
        ViewModel.Dispose();

        // Navigate back to ProgressPage
        _navigationService?.NavigateTo(PageKeys.Progress);
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);

        // Unsubscribe from property changes to prevent memory leak
        ViewModel.PropertyChanged -= ViewModel_PropertyChanged;

        // Note: We don't dispose here because the user might navigate back
        // Disposal is handled explicitly in ExitButton_Click or when finishing the guide
    }
}

using GuideViewer.Core.Services;
using GuideViewer.Data.Entities;
using GuideViewer.Data.Repositories;
using GuideViewer.Services;
using LiteDB;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Serilog;
using System;
using System.IO;
using System.Linq;
using Windows.Storage.Streams;

namespace GuideViewer.Views.Pages;

/// <summary>
/// Page for viewing guide details in read-only mode.
/// </summary>
public sealed partial class GuideDetailPage : Page
{
    private readonly GuideRepository _guideRepository;
    private readonly CategoryRepository _categoryRepository;
    private readonly NavigationService _navigationService;
    private Guide? _guide;

    public GuideDetailPage()
    {
        this.InitializeComponent();

        // Get dependencies from DI
        _guideRepository = App.GetService<GuideRepository>();
        _categoryRepository = App.GetService<CategoryRepository>();
        _navigationService = App.GetService<NavigationService>();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        // Show loading overlay
        LoadingOverlay.Visibility = Visibility.Visible;

        try
        {
            // Get guide ID from navigation parameter
            if (e.Parameter is ObjectId guideId)
            {
                // Load guide
                _guide = await System.Threading.Tasks.Task.Run(() => _guideRepository.GetById(guideId));

                if (_guide != null)
                {
                    // Update UI
                    UpdateUI();
                }
                else
                {
                    Log.Warning("Guide not found: {GuideId}", guideId);
                    ShowError("Guide not found");
                }
            }
            else
            {
                Log.Warning("Invalid parameter type passed to GuideDetailPage: {ParameterType}",
                    e.Parameter?.GetType().Name ?? "null");
                ShowError("Invalid guide reference");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load guide details");
            ShowError("Failed to load guide");
        }
        finally
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;
        }
    }

    private void UpdateUI()
    {
        if (_guide == null) return;

        // Update header
        TitleTextBlock.Text = _guide.Title;
        DescriptionTextBlock.Text = _guide.Description;
        CategoryTextBlock.Text = _guide.Category;
        CreatedAtTextBlock.Text = $"Created: {_guide.CreatedAt:g}";
        UpdatedAtTextBlock.Text = $"Updated: {_guide.UpdatedAt:g}";

        // Update category badge
        var category = _categoryRepository.GetByName(_guide.Category);
        if (category != null)
        {
            CategoryIcon.Glyph = category.IconGlyph;
            CategoryBadge.Background = new SolidColorBrush(ParseHexColor(category.Color));
        }

        // Update steps
        if (_guide.Steps != null && _guide.Steps.Count > 0)
        {
            StepsRepeater.ItemsSource = _guide.Steps.OrderBy(s => s.Order).ToList();
            EmptyState.Visibility = Visibility.Collapsed;
        }
        else
        {
            StepsRepeater.ItemsSource = null;
            EmptyState.Visibility = Visibility.Visible;
        }
    }

    private void StepInstructionsBox_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is RichEditBox richEditBox && richEditBox.Tag is string rtfContent)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(rtfContent))
                {
                    using var stream = new MemoryStream();
                    using var writer = new StreamWriter(stream);
                    writer.Write(rtfContent);
                    writer.Flush();
                    stream.Position = 0;

                    using var randomAccessStream = stream.AsRandomAccessStream();
                    richEditBox.Document.LoadFromStream(Microsoft.UI.Text.TextSetOptions.FormatRtf, randomAccessStream);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load step instructions RTF content");
            }
        }
    }

    private async void StepImage_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is Image image && image.Tag is string fileId && !string.IsNullOrWhiteSpace(fileId))
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

                        // Ensure UI update is on dispatcher thread
                        if (this.DispatcherQueue.HasThreadAccess)
                        {
                            image.Source = bitmap;
                        }
                        else
                        {
                            this.DispatcherQueue.TryEnqueue(() => { image.Source = bitmap; });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load step image: {FileId}", fileId);
            }
        }
    }

    private void EditGuide_Click(object sender, RoutedEventArgs e)
    {
        if (_guide != null)
        {
            // Navigate to editor with guide ID
            _navigationService.NavigateTo(PageKeys.GuideEditor, _guide.Id);
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        // Navigate back to guides list
        _navigationService.NavigateTo(PageKeys.Guides);
    }

    private void ShowError(string message)
    {
        TitleTextBlock.Text = "Error";
        DescriptionTextBlock.Text = message;
        StepsRepeater.ItemsSource = null;
        EmptyState.Visibility = Visibility.Visible;
    }

    private Windows.UI.Color ParseHexColor(string hex)
    {
        hex = hex.TrimStart('#');
        return Windows.UI.Color.FromArgb(
            255,
            Convert.ToByte(hex.Substring(0, 2), 16),
            Convert.ToByte(hex.Substring(2, 2), 16),
            Convert.ToByte(hex.Substring(4, 2), 16)
        );
    }
}

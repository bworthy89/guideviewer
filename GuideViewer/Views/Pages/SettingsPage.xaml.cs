using GuideViewer.Core.Services;
using GuideViewer.Data.Entities;
using GuideViewer.ViewModels;
using GuideViewer.Views.Dialogs;
using LiteDB;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace GuideViewer.Views.Pages;

/// <summary>
/// Settings page - application configuration and preferences.
/// </summary>
public sealed partial class SettingsPage : Page
{
    public CategoryManagementViewModel ViewModel { get; }
    public ProgressReportViewModel ProgressReportViewModel { get; }

    public SettingsPage()
    {
        this.InitializeComponent();

        // Get dependencies from DI
        ViewModel = new CategoryManagementViewModel(
            App.GetService<Data.Repositories.CategoryRepository>(),
            App.GetService<Data.Repositories.GuideRepository>(),
            this.DispatcherQueue);

        ProgressReportViewModel = new ProgressReportViewModel(
            App.GetService<Data.Repositories.ProgressRepository>(),
            App.GetService<Data.Repositories.UserRepository>(),
            App.GetService<Data.Repositories.GuideRepository>(),
            this.DispatcherQueue);

        // Bind categories to ItemsRepeater
        CategoriesRepeater.ItemsSource = ViewModel.Categories;

        // Bind progress reports to the section's DataContext
        ProgressReportsSection.DataContext = ProgressReportViewModel;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        // Reload categories when navigating to this page
        await ViewModel.LoadCategoriesCommand.ExecuteAsync(null);

        // Load progress reports if user is admin
        var mainViewModel = App.GetService<MainViewModel>();
        if (mainViewModel?.IsAdmin == true)
        {
            await ProgressReportViewModel.InitializeAsync();
        }

        // Check OneDrive status
        UpdateOneDriveStatus();
    }

    /// <summary>
    /// Updates the OneDrive status display.
    /// </summary>
    private void UpdateOneDriveStatus()
    {
        try
        {
            var oneDriveService = App.GetService<IOneDriveGuideService>();
            var folderPath = oneDriveService.GetOneDriveFolderPath();

            if (string.IsNullOrEmpty(folderPath))
            {
                OneDriveStatusTextBlock.Text = "OneDrive guide folder not found";
            }
            else if (oneDriveService.IsOneDriveFolderAvailable())
            {
                OneDriveStatusTextBlock.Text = $"Connected to: {folderPath}";
            }
            else
            {
                OneDriveStatusTextBlock.Text = $"OneDrive folder found but not accessible: {folderPath}";
            }
        }
        catch
        {
            OneDriveStatusTextBlock.Text = "Error checking OneDrive status";
        }
    }

    private async void AddCategory_Click(object sender, RoutedEventArgs e)
    {
        // Create new category with defaults
        var newCategory = ViewModel.CreateNewCategory();

        // Show editor dialog
        var dialog = new CategoryEditorDialog(newCategory)
        {
            XamlRoot = this.XamlRoot
        };

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            // Save the category
            await ViewModel.SaveCategoryCommand.ExecuteAsync(dialog.Category);
        }
    }

    private async void EditCategory_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Category category)
        {
            // Create a copy to edit (to avoid modifying the original until save)
            var categoryToEdit = new Category
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IconGlyph = category.IconGlyph,
                Color = category.Color,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };

            // Show editor dialog
            var dialog = new CategoryEditorDialog(categoryToEdit)
            {
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                // Save the category
                await ViewModel.SaveCategoryCommand.ExecuteAsync(dialog.Category);
            }
        }
    }

    private async void DeleteCategory_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Category category)
        {
            // Show confirmation dialog
            var confirmDialog = new ContentDialog
            {
                Title = "Delete Category",
                Content = $"Are you sure you want to delete the category '{category.Name}'?\n\nThis action cannot be undone.",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            var result = await confirmDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                await ViewModel.DeleteCategoryCommand.ExecuteAsync(category);

                // Show error if deletion failed
                if (ViewModel.HasValidationError)
                {
                    var errorDialog = new ContentDialog
                    {
                        Title = "Cannot Delete Category",
                        Content = ViewModel.ValidationMessage,
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await errorDialog.ShowAsync();

                    // Clear error
                    ViewModel.HasValidationError = false;
                    ViewModel.ValidationMessage = string.Empty;
                }
            }
        }
    }

    private void CategoryBadge_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is Border border && border.Tag is string colorHex)
        {
            border.Background = new SolidColorBrush(ParseHexColor(colorHex));
        }
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

    private void ProgressSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            ProgressReportViewModel.SearchQuery = sender.Text;
        }
    }

    // Data Management Event Handlers

    private async void ExportSingleGuideButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var guideRepository = App.GetService<Data.Repositories.GuideRepository>();
            var guides = guideRepository.GetAll().ToList();

            if (!guides.Any())
            {
                await ShowMessageAsync("No Guides", "There are no guides to export.", "OK");
                return;
            }

            // Show guide picker dialog
            var guide = await ShowGuideSelectorDialogAsync(guides);
            if (guide == null) return;

            // Show file save picker
            var savePicker = new FileSavePicker();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

            savePicker.SuggestedFileName = $"{guide.Title.Replace(" ", "_")}_export";
            savePicker.FileTypeChoices.Add("JSON File", new List<string> { ".json" });
            savePicker.FileTypeChoices.Add("ZIP Package (with images)", new List<string> { ".zip" });

            var file = await savePicker.PickSaveFileAsync();
            if (file == null) return;

            var exportService = App.GetService<IGuideExportService>();
            var success = false;

            if (file.FileType == ".zip")
            {
                var zipData = await exportService.ExportGuideWithImagesAsync(guide.Id);
                await FileIO.WriteBytesAsync(file, zipData);
                success = true;
            }
            else
            {
                success = await exportService.ExportGuideToFileAsync(guide.Id, file.Path, includeImages: true);
            }

            if (success)
            {
                await ShowMessageAsync("Export Successful", $"Guide '{guide.Title}' exported successfully to {file.Name}", "OK");
            }
            else
            {
                await ShowMessageAsync("Export Failed", "Failed to export guide. Check logs for details.", "OK");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to export single guide");
            await ShowMessageAsync("Export Error", $"An error occurred: {ex.Message}", "OK");
        }
    }

    private async void ExportAllGuidesButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var guideRepository = App.GetService<Data.Repositories.GuideRepository>();
            var guideCount = guideRepository.GetAll().Count();

            if (guideCount == 0)
            {
                await ShowMessageAsync("No Guides", "There are no guides to export.", "OK");
                return;
            }

            // Show file save picker
            var savePicker = new FileSavePicker();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

            savePicker.SuggestedFileName = $"all_guides_export_{DateTime.Now:yyyyMMdd}";
            savePicker.FileTypeChoices.Add("JSON File", new List<string> { ".json" });

            var file = await savePicker.PickSaveFileAsync();
            if (file == null) return;

            var exportService = App.GetService<IGuideExportService>();
            var success = await exportService.ExportAllGuidesToFileAsync(file.Path, includeImages: true);

            if (success)
            {
                await ShowMessageAsync("Export Successful", $"All {guideCount} guides exported successfully to {file.Name}", "OK");
            }
            else
            {
                await ShowMessageAsync("Export Failed", "Failed to export guides. Check logs for details.", "OK");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to export all guides");
            await ShowMessageAsync("Export Error", $"An error occurred: {ex.Message}", "OK");
        }
    }

    private async void ImportGuidesButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Show file open picker
            var openPicker = new FileOpenPicker();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hwnd);

            openPicker.FileTypeFilter.Add(".json");
            openPicker.FileTypeFilter.Add(".zip");

            var file = await openPicker.PickSingleFileAsync();
            if (file == null) return;

            // Get duplicate handling preference
            var duplicateHandling = DuplicateHandlingComboBox.SelectedIndex switch
            {
                0 => DuplicateHandling.Skip,
                1 => DuplicateHandling.Overwrite,
                2 => DuplicateHandling.Rename,
                _ => DuplicateHandling.Skip
            };

            // Show progress
            ImportProgressRing.Visibility = Visibility.Visible;
            ImportGuidesButton.IsEnabled = false;

            var importService = App.GetService<IGuideImportService>();
            var result = await importService.ImportGuidesFromFileAsync(file.Path, duplicateHandling);

            // Hide progress
            ImportProgressRing.Visibility = Visibility.Collapsed;
            ImportGuidesButton.IsEnabled = true;

            // Show result
            if (result.Success)
            {
                ImportResultInfoBar.Title = "Import Successful";
                ImportResultInfoBar.Message = result.GetSummaryMessage();
                ImportResultInfoBar.Severity = InfoBarSeverity.Success;
            }
            else
            {
                ImportResultInfoBar.Title = "Import Failed";
                ImportResultInfoBar.Message = result.GetSummaryMessage();
                ImportResultInfoBar.Severity = InfoBarSeverity.Error;
            }

            ImportResultInfoBar.Visibility = Visibility.Visible;
            ImportResultInfoBar.IsOpen = true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to import guides");
            ImportProgressRing.Visibility = Visibility.Collapsed;
            ImportGuidesButton.IsEnabled = true;

            ImportResultInfoBar.Title = "Import Error";
            ImportResultInfoBar.Message = $"An error occurred: {ex.Message}";
            ImportResultInfoBar.Severity = InfoBarSeverity.Error;
            ImportResultInfoBar.Visibility = Visibility.Visible;
            ImportResultInfoBar.IsOpen = true;
        }
    }

    private void ImportResultInfoBar_Closed(InfoBar sender, InfoBarClosedEventArgs args)
    {
        ImportResultInfoBar.Visibility = Visibility.Collapsed;
    }

    private async void CreateBackupButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Show folder picker
            var folderPicker = new FolderPicker();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);

            folderPicker.FileTypeFilter.Add("*");

            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder == null) return;

            var backupFileName = $"guideviewer_backup_{DateTime.Now:yyyyMMdd_HHmmss}.zip";
            var backupPath = System.IO.Path.Combine(folder.Path, backupFileName);

            var backupService = App.GetService<IDatabaseBackupService>();
            var success = await backupService.CreateBackupAsync(backupPath);

            if (success)
            {
                LastBackupTextBlock.Text = $"Last backup: {DateTime.Now:yyyy-MM-dd HH:mm}";

                BackupResultInfoBar.Title = "Backup Created";
                BackupResultInfoBar.Message = $"Backup created successfully: {backupFileName}";
                BackupResultInfoBar.Severity = InfoBarSeverity.Success;
                BackupResultInfoBar.Visibility = Visibility.Visible;
                BackupResultInfoBar.IsOpen = true;
            }
            else
            {
                BackupResultInfoBar.Title = "Backup Failed";
                BackupResultInfoBar.Message = "Failed to create backup. Check logs for details.";
                BackupResultInfoBar.Severity = InfoBarSeverity.Error;
                BackupResultInfoBar.Visibility = Visibility.Visible;
                BackupResultInfoBar.IsOpen = true;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create backup");
            BackupResultInfoBar.Title = "Backup Error";
            BackupResultInfoBar.Message = $"An error occurred: {ex.Message}";
            BackupResultInfoBar.Severity = InfoBarSeverity.Error;
            BackupResultInfoBar.Visibility = Visibility.Visible;
            BackupResultInfoBar.IsOpen = true;
        }
    }

    private async void RestoreBackupButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Show warning dialog
            var confirmDialog = new ContentDialog
            {
                Title = "Restore Backup - Warning",
                Content = "Restoring a backup will replace your current database.\n\n" +
                         "The application will close after restore. You'll need to restart it manually.\n\n" +
                         "Make sure you've created a backup of your current data first!\n\n" +
                         "Do you want to continue?",
                PrimaryButtonText = "Restore",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            var result = await confirmDialog.ShowAsync();
            if (result != ContentDialogResult.Primary) return;

            // Show file open picker
            var openPicker = new FileOpenPicker();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hwnd);

            openPicker.FileTypeFilter.Add(".zip");

            var file = await openPicker.PickSingleFileAsync();
            if (file == null) return;

            var backupService = App.GetService<IDatabaseBackupService>();

            // Validate backup first
            if (!await backupService.ValidateBackupAsync(file.Path))
            {
                await ShowMessageAsync("Invalid Backup", "The selected file is not a valid backup.", "OK");
                return;
            }

            // Restore backup
            var success = await backupService.RestoreBackupAsync(file.Path);

            if (success)
            {
                // Show success dialog and close app
                var successDialog = new ContentDialog
                {
                    Title = "Backup Restored",
                    Content = "Backup restored successfully!\n\n" +
                             "The application will close now. Please restart it to use the restored database.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                await successDialog.ShowAsync();

                // Close the application
                Application.Current.Exit();
            }
            else
            {
                await ShowMessageAsync("Restore Failed", "Failed to restore backup. Check logs for details.", "OK");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to restore backup");
            await ShowMessageAsync("Restore Error", $"An error occurred: {ex.Message}", "OK");
        }
    }

    private void BackupResultInfoBar_Closed(InfoBar sender, InfoBarClosedEventArgs args)
    {
        BackupResultInfoBar.Visibility = Visibility.Collapsed;
    }

    // Helper Methods

    private async Task<Guide?> ShowGuideSelectorDialogAsync(List<Guide> guides)
    {
        var dialog = new ContentDialog
        {
            Title = "Select Guide to Export",
            PrimaryButtonText = "Export",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = this.XamlRoot
        };

        var listView = new ListView
        {
            ItemsSource = guides,
            DisplayMemberPath = "Title",
            SelectedIndex = 0
        };

        dialog.Content = listView;

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary && listView.SelectedItem is Guide selectedGuide)
        {
            return selectedGuide;
        }

        return null;
    }

    private async Task ShowMessageAsync(string title, string content, string closeButtonText)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = content,
            CloseButtonText = closeButtonText,
            XamlRoot = this.XamlRoot
        };

        await dialog.ShowAsync();
    }

    private async void SeedSampleDataButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            SeedSampleDataButton.IsEnabled = false;

            // Get repositories
            var categoryRepository = App.GetService<Data.Repositories.CategoryRepository>();
            var guideRepository = App.GetService<Data.Repositories.GuideRepository>();

            // Check if data already exists
            var existingGuides = guideRepository.GetAll();
            if (existingGuides.Any())
            {
                SeedDataInfoBar.Severity = InfoBarSeverity.Warning;
                SeedDataInfoBar.Message = "Sample data already exists. The database is not empty.";
                SeedDataInfoBar.Visibility = Visibility.Visible;
                SeedDataInfoBar.IsOpen = true;
                return;
            }

            // Seed sample data
            await Task.Run(() =>
            {
                Core.Utilities.SampleDataSeeder.SeedSampleData(categoryRepository, guideRepository);
            });

            // Reload categories to show new data
            await ViewModel.LoadCategoriesCommand.ExecuteAsync(null);

            // Show success message
            SeedDataInfoBar.Severity = InfoBarSeverity.Success;
            SeedDataInfoBar.Message = "Sample data created successfully! 5 guides and 4 categories have been added.";
            SeedDataInfoBar.Visibility = Visibility.Visible;
            SeedDataInfoBar.IsOpen = true;

            Log.Information("Sample data seeded successfully from Settings page");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to seed sample data from Settings page");

            SeedDataInfoBar.Severity = InfoBarSeverity.Error;
            SeedDataInfoBar.Message = $"Failed to seed sample data: {ex.Message}";
            SeedDataInfoBar.Visibility = Visibility.Visible;
            SeedDataInfoBar.IsOpen = true;
        }
        finally
        {
            SeedSampleDataButton.IsEnabled = true;
        }
    }

    private void SeedDataInfoBar_Closed(InfoBar sender, InfoBarClosedEventArgs args)
    {
        SeedDataInfoBar.Visibility = Visibility.Collapsed;
    }

    /// <summary>
    /// Checks for guide updates from OneDrive sync folder.
    /// </summary>
    private async void CheckForGuideUpdatesButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            CheckForGuideUpdatesButton.IsEnabled = false;
            GuideUpdateProgressRing.IsActive = true;
            GuideUpdateProgressRing.Visibility = Visibility.Visible;
            GuideUpdateResultInfoBar.Visibility = Visibility.Collapsed;

            var oneDriveService = App.GetService<IOneDriveGuideService>();

            // First, check if OneDrive folder is available
            if (!oneDriveService.IsOneDriveFolderAvailable())
            {
                var folderPath = oneDriveService.GetOneDriveFolderPath();
                var message = string.IsNullOrEmpty(folderPath)
                    ? "OneDrive guide folder not found. Please make sure OneDrive is syncing the 'GuideViewer_Guides' folder."
                    : $"OneDrive guide folder not accessible at: {folderPath}";

                GuideUpdateResultInfoBar.Title = "OneDrive Not Available";
                GuideUpdateResultInfoBar.Message = message;
                GuideUpdateResultInfoBar.Severity = InfoBarSeverity.Warning;
                GuideUpdateResultInfoBar.Visibility = Visibility.Visible;
                GuideUpdateResultInfoBar.IsOpen = true;

                OneDriveStatusTextBlock.Text = "OneDrive folder not found";
                return;
            }

            // Update status
            var oneDrivePath = oneDriveService.GetOneDriveFolderPath();
            OneDriveStatusTextBlock.Text = $"Scanning: {oneDrivePath}";

            // Check for updates
            var updates = await oneDriveService.CheckForGuideUpdatesAsync();

            if (!updates.Any())
            {
                GuideUpdateResultInfoBar.Title = "No Updates Available";
                GuideUpdateResultInfoBar.Message = "All guides are up to date!";
                GuideUpdateResultInfoBar.Severity = InfoBarSeverity.Success;
                GuideUpdateResultInfoBar.Visibility = Visibility.Visible;
                GuideUpdateResultInfoBar.IsOpen = true;

                OneDriveStatusTextBlock.Text = $"Connected to: {oneDrivePath} (No updates available)";
                Log.Information("No guide updates found in OneDrive folder");
                return;
            }

            // Show dialog with available updates
            var dialog = new GuideUpdatesDialog(updates)
            {
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary && dialog.SelectedGuides.Any())
            {
                // Import selected guides
                int successCount = 0;
                int failCount = 0;

                foreach (var update in dialog.SelectedGuides)
                {
                    var importResult = await oneDriveService.ImportGuideFromOneDriveAsync(update.OneDriveGuide);
                    if (importResult.Success)
                        successCount++;
                    else
                        failCount++;
                }

                // Show result
                var resultMessage = successCount > 0 && failCount == 0
                    ? $"Successfully imported {successCount} guide(s)!"
                    : successCount > 0 && failCount > 0
                    ? $"Imported {successCount} guide(s), {failCount} failed."
                    : $"Failed to import {failCount} guide(s).";

                GuideUpdateResultInfoBar.Title = "Import Complete";
                GuideUpdateResultInfoBar.Message = resultMessage;
                GuideUpdateResultInfoBar.Severity = successCount > 0 ? InfoBarSeverity.Success : InfoBarSeverity.Error;
                GuideUpdateResultInfoBar.Visibility = Visibility.Visible;
                GuideUpdateResultInfoBar.IsOpen = true;

                OneDriveStatusTextBlock.Text = $"Connected to: {oneDrivePath} (Last checked: {DateTime.Now:g})";

                Log.Information("Guide import completed: {SuccessCount} succeeded, {FailCount} failed",
                    successCount, failCount);
            }
            else
            {
                OneDriveStatusTextBlock.Text = $"Connected to: {oneDrivePath} (Last checked: {DateTime.Now:g})";
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error checking for guide updates from OneDrive");

            GuideUpdateResultInfoBar.Title = "Error";
            GuideUpdateResultInfoBar.Message = $"Failed to check for updates: {ex.Message}";
            GuideUpdateResultInfoBar.Severity = InfoBarSeverity.Error;
            GuideUpdateResultInfoBar.Visibility = Visibility.Visible;
            GuideUpdateResultInfoBar.IsOpen = true;

            OneDriveStatusTextBlock.Text = "Error checking for updates";
        }
        finally
        {
            CheckForGuideUpdatesButton.IsEnabled = true;
            GuideUpdateProgressRing.IsActive = false;
            GuideUpdateProgressRing.Visibility = Visibility.Collapsed;
        }
    }

    private void GuideUpdateResultInfoBar_Closed(InfoBar sender, InfoBarClosedEventArgs args)
    {
        GuideUpdateResultInfoBar.Visibility = Visibility.Collapsed;
    }
}

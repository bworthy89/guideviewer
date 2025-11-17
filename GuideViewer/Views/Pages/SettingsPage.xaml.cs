using GuideViewer.Data.Entities;
using GuideViewer.ViewModels;
using GuideViewer.Views.Dialogs;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Threading.Tasks;

namespace GuideViewer.Views.Pages;

/// <summary>
/// Settings page - application configuration and preferences.
/// </summary>
public sealed partial class SettingsPage : Page
{
    public CategoryManagementViewModel ViewModel { get; }

    public SettingsPage()
    {
        this.InitializeComponent();

        // Get dependencies from DI
        ViewModel = new CategoryManagementViewModel(
            App.GetService<Data.Repositories.CategoryRepository>(),
            App.GetService<Data.Repositories.GuideRepository>(),
            this.DispatcherQueue);

        // Bind categories to ItemsRepeater
        CategoriesRepeater.ItemsSource = ViewModel.Categories;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        // Reload categories when navigating to this page
        await ViewModel.LoadCategoriesCommand.ExecuteAsync(null);
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
}

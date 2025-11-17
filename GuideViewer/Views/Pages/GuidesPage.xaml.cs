using GuideViewer.Data.Entities;
using GuideViewer.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Linq;

namespace GuideViewer.Views.Pages;

/// <summary>
/// Page displaying the list of installation guides with search and filtering.
/// </summary>
public sealed partial class GuidesPage : Page
{
    public GuidesViewModel ViewModel { get; }

    public GuidesPage()
    {
        this.InitializeComponent();

        // Get dependencies from DI
        ViewModel = new GuidesViewModel(
            App.GetService<Data.Repositories.GuideRepository>(),
            App.GetService<Data.Repositories.CategoryRepository>(),
            App.GetService<Services.NavigationService>(),
            App.GetService<Data.Repositories.UserRepository>(),
            this.DispatcherQueue);

        // Set DataContext
        this.DataContext = ViewModel;

        // Set name for ElementName bindings
        this.Name = "PageRoot";
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        // Reload guides when navigating back to this page
        // This ensures newly created/edited guides appear in the list
        await ViewModel.LoadDataCommand.ExecuteAsync(null);
    }

    private async void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        await ViewModel.SearchCommand.ExecuteAsync(null);
    }

    private async void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Only trigger filter if ViewModel is initialized
        if (ViewModel != null && e.AddedItems.Count > 0)
        {
            await ViewModel.FilterByCategoryCommand.ExecuteAsync(null);
        }
    }

    private void DeleteCancel_Click(object sender, RoutedEventArgs e)
    {
        // Close the flyout
        if (sender is Button button &&
            button.Parent is StackPanel panel &&
            panel.Parent is Flyout flyout)
        {
            flyout.Hide();
        }
    }

    private void DeleteConfirm_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Guide guide)
        {
            // Execute delete command
            _ = ViewModel.DeleteGuideCommand.ExecuteAsync(guide);

            // Close the flyout
            if (button.Parent is StackPanel panel &&
                panel.Parent is Flyout flyout)
            {
                flyout.Hide();
            }
        }
    }
}

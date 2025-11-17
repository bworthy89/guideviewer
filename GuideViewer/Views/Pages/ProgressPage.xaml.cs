using GuideViewer.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace GuideViewer.Views.Pages;

/// <summary>
/// Progress page - displays user's progress dashboard.
/// </summary>
public sealed partial class ProgressPage : Page
{
    public ProgressDashboardViewModel ViewModel { get; }

    public ProgressPage()
    {
        this.InitializeComponent();

        // Initialize ViewModel with services from DI container
        ViewModel = new ProgressDashboardViewModel(
            App.GetService<GuideViewer.Core.Services.IProgressTrackingService>(),
            App.GetService<GuideViewer.Data.Repositories.GuideRepository>(),
            App.GetService<GuideViewer.Data.Repositories.UserRepository>(),
            App.GetService<GuideViewer.Services.NavigationService>(),
            this.DispatcherQueue);

        DataContext = ViewModel;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        // Load dashboard data when page is navigated to
        await ViewModel.InitializeAsync();
    }
}

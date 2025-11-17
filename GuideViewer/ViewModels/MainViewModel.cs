using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GuideViewer.Core.Models;
using GuideViewer.Data.Repositories;
using GuideViewer.Services;
using Serilog;

namespace GuideViewer.ViewModels;

/// <summary>
/// ViewModel for the main window with navigation and role-based UI.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly NavigationService _navigationService;
    private readonly UserRepository _userRepository;

    [ObservableProperty]
    private string currentUserRole = "Unknown";

    [ObservableProperty]
    private bool isAdmin = false;

    [ObservableProperty]
    private string selectedPage = PageKeys.Home;

    public MainViewModel(NavigationService navigationService, UserRepository userRepository)
    {
        _navigationService = navigationService;
        _userRepository = userRepository;

        // Subscribe to navigation events
        _navigationService.Navigated += OnNavigated;

        // Load current user and role
        LoadCurrentUser();
    }

    /// <summary>
    /// Loads the current user and determines their role.
    /// </summary>
    private void LoadCurrentUser()
    {
        try
        {
            var user = _userRepository.GetCurrentUser();
            if (user != null)
            {
                CurrentUserRole = user.Role;
                IsAdmin = user.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase);

                Log.Information("Main window loaded for user with role: {Role}", user.Role);
            }
            else
            {
                Log.Warning("No current user found in MainViewModel");
                CurrentUserRole = "Unknown";
                IsAdmin = false;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error loading current user in MainViewModel");
            CurrentUserRole = "Unknown";
            IsAdmin = false;
        }
    }

    /// <summary>
    /// Navigates to a page.
    /// </summary>
    [RelayCommand]
    private void NavigateToPage(string pageKey)
    {
        try
        {
            _navigationService.NavigateTo(pageKey);
            SelectedPage = pageKey;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error navigating to page: {PageKey}", pageKey);
        }
    }

    /// <summary>
    /// Handles navigation events.
    /// </summary>
    private void OnNavigated(object? sender, string pageKey)
    {
        SelectedPage = pageKey;
    }

    /// <summary>
    /// Gets the display name for the current user role.
    /// </summary>
    public string RoleDisplayName =>
        IsAdmin ? "Administrator" : "Technician";

    /// <summary>
    /// Gets the icon glyph for the current user role.
    /// </summary>
    public string RoleIconGlyph =>
        IsAdmin ? "\uE7EF" : "\uE77B"; // Admin: AdminStar, Technician: Contact
}

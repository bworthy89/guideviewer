using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GuideViewer.Data.Entities;
using GuideViewer.Data.Repositories;
using GuideViewer.Services;
using LiteDB;
using Microsoft.UI.Dispatching;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace GuideViewer.ViewModels;

/// <summary>
/// ViewModel for the guides list page with search and filtering.
/// </summary>
public partial class GuidesViewModel : ObservableObject
{
    private readonly GuideRepository _guideRepository;
    private readonly CategoryRepository _categoryRepository;
    private readonly NavigationService _navigationService;
    private readonly UserRepository _userRepository;
    private readonly DispatcherQueue _dispatcherQueue;

    [ObservableProperty]
    private ObservableCollection<Guide> guides = new();

    [ObservableProperty]
    private ObservableCollection<Category> categories = new();

    [ObservableProperty]
    private string searchQuery = string.Empty;

    [ObservableProperty]
    private Category? selectedCategory;

    /// <summary>
    /// Gets whether a search query is active.
    /// </summary>
    public bool HasSearchQuery => !string.IsNullOrWhiteSpace(SearchQuery);

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private bool isAdmin = false;

    [ObservableProperty]
    private string emptyStateMessage = "No guides found";

    public GuidesViewModel(
        GuideRepository guideRepository,
        CategoryRepository categoryRepository,
        NavigationService navigationService,
        UserRepository userRepository,
        DispatcherQueue dispatcherQueue)
    {
        _guideRepository = guideRepository ?? throw new ArgumentNullException(nameof(guideRepository));
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _dispatcherQueue = dispatcherQueue ?? throw new ArgumentNullException(nameof(dispatcherQueue));

        // Subscribe to collection changes to update HasGuides property
        Guides.CollectionChanged += (s, e) => OnPropertyChanged(nameof(HasGuides));

        // Check if current user is admin
        LoadUserRole();

        // Load data on initialization
        _ = LoadDataAsync();
    }

    /// <summary>
    /// Loads the current user's role to determine admin status.
    /// </summary>
    private void LoadUserRole()
    {
        try
        {
            var currentUser = _userRepository.GetCurrentUser();
            IsAdmin = currentUser?.Role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) ?? false;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load user role in GuidesViewModel");
            IsAdmin = false;
        }
    }

    /// <summary>
    /// Loads all guides and categories from the database.
    /// </summary>
    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsLoading = true;

        try
        {
            await Task.Run(() =>
            {
                // Load categories
                var categoriesList = _categoryRepository.GetAll().ToList();

                // Load guides
                var guidesList = _guideRepository.GetAll().ToList();

                // Update UI on main thread
                _dispatcherQueue.TryEnqueue(() =>
                {
                    Categories.Clear();
                    Categories.Add(new Category { Name = "All Categories", Id = ObjectId.Empty });
                    foreach (var category in categoriesList)
                    {
                        Categories.Add(category);
                    }

                    Guides.Clear();
                    foreach (var guide in guidesList)
                    {
                        Guides.Add(guide);
                    }

                    // Select "All Categories" by default
                    SelectedCategory = Categories.FirstOrDefault();

                    UpdateEmptyStateMessage();
                });
            });

            Log.Information("Loaded {GuideCount} guides and {CategoryCount} categories", Guides.Count, Categories.Count - 1);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load guides and categories");
            EmptyStateMessage = "Failed to load guides. Please try again.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Performs a search based on the current search query.
    /// </summary>
    [RelayCommand]
    private async Task SearchAsync()
    {
        IsLoading = true;

        try
        {
            await Task.Run(() =>
            {
                IEnumerable<Guide> results;

                // Search with current query
                if (!string.IsNullOrWhiteSpace(SearchQuery))
                {
                    results = _guideRepository.Search(SearchQuery);
                }
                else if (SelectedCategory != null && SelectedCategory.Id != ObjectId.Empty)
                {
                    // Filter by category only
                    results = _guideRepository.GetByCategory(SelectedCategory.Name);
                }
                else
                {
                    // Show all
                    results = _guideRepository.GetAll();
                }

                var resultsList = results.ToList();

                _dispatcherQueue.TryEnqueue(() =>
                {
                    Guides.Clear();
                    foreach (var guide in resultsList)
                    {
                        Guides.Add(guide);
                    }

                    UpdateEmptyStateMessage();
                });
            });

            Log.Information("Search completed: '{Query}', {ResultCount} results", SearchQuery, Guides.Count);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Search failed for query: {Query}", SearchQuery);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Filters guides by the selected category.
    /// </summary>
    [RelayCommand]
    private async Task FilterByCategoryAsync()
    {
        IsLoading = true;

        try
        {
            await Task.Run(() =>
            {
                IEnumerable<Guide> results;

                if (SelectedCategory == null || SelectedCategory.Id == ObjectId.Empty)
                {
                    // All categories - show all guides or search results
                    if (!string.IsNullOrWhiteSpace(SearchQuery))
                    {
                        results = _guideRepository.Search(SearchQuery);
                    }
                    else
                    {
                        results = _guideRepository.GetAll();
                    }
                }
                else
                {
                    // Specific category
                    results = _guideRepository.GetByCategory(SelectedCategory.Name);

                    // Apply search filter if query exists
                    if (!string.IsNullOrWhiteSpace(SearchQuery))
                    {
                        var query = SearchQuery.ToLowerInvariant();
                        results = results.Where(g =>
                            g.Title.ToLowerInvariant().Contains(query) ||
                            g.Description.ToLowerInvariant().Contains(query));
                    }
                }

                var resultsList = results.ToList();

                _dispatcherQueue.TryEnqueue(() =>
                {
                    Guides.Clear();
                    foreach (var guide in resultsList)
                    {
                        Guides.Add(guide);
                    }

                    UpdateEmptyStateMessage();
                });
            });

            Log.Information("Filtered by category: {Category}, {ResultCount} results",
                SelectedCategory?.Name ?? "All", Guides.Count);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Category filter failed");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Creates a new guide (navigates to editor in create mode).
    /// </summary>
    [RelayCommand]
    private void CreateGuide()
    {
        if (!IsAdmin)
        {
            Log.Warning("Non-admin user attempted to create guide");
            return;
        }

        Log.Information("Creating new guide");
        _navigationService.NavigateTo(PageKeys.GuideEditor);
    }

    /// <summary>
    /// Edits an existing guide (navigates to editor with guide ID).
    /// </summary>
    [RelayCommand]
    private void EditGuide(Guide guide)
    {
        if (!IsAdmin)
        {
            Log.Warning("Non-admin user attempted to edit guide");
            return;
        }

        if (guide == null)
        {
            return;
        }

        Log.Information("Editing guide: {GuideId} - {GuideTitle}", guide.Id, guide.Title);
        _navigationService.NavigateTo(PageKeys.GuideEditor, guide.Id);
    }

    /// <summary>
    /// Views a guide in read-only mode (for technicians and admins).
    /// </summary>
    [RelayCommand]
    private void ViewGuide(Guide guide)
    {
        if (guide == null)
        {
            return;
        }

        Log.Information("Viewing guide: {GuideId} - {GuideTitle}", guide.Id, guide.Title);
        _navigationService.NavigateTo(PageKeys.GuideDetail, guide.Id);
    }

    /// <summary>
    /// Starts or resumes progress tracking for a guide.
    /// </summary>
    [RelayCommand]
    private void StartGuide(Guide guide)
    {
        if (guide == null)
        {
            return;
        }

        Log.Information("Starting guide: {GuideId} - {GuideTitle}", guide.Id, guide.Title);

        // Navigate to active guide progress page
        // Pass guideId and null progressId (page will check for existing progress)
        _navigationService.NavigateTo(PageKeys.ActiveGuide, (guide.Id, (ObjectId?)null));
    }

    /// <summary>
    /// Deletes a guide after confirmation.
    /// </summary>
    [RelayCommand]
    private async Task DeleteGuideAsync(Guide guide)
    {
        if (!IsAdmin)
        {
            Log.Warning("Non-admin user attempted to delete guide");
            return;
        }

        if (guide == null)
        {
            return;
        }

        // TODO: Show ContentDialog for confirmation
        // For now, just delete directly (confirmation dialog will be added in UI)
        try
        {
            var deleted = _guideRepository.Delete(guide.Id);

            if (deleted)
            {
                Guides.Remove(guide);
                UpdateEmptyStateMessage();
                Log.Information("Guide deleted: {GuideId} - {GuideTitle}", guide.Id, guide.Title);
            }
            else
            {
                Log.Warning("Failed to delete guide: {GuideId}", guide.Id);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error deleting guide: {GuideId}", guide.Id);
        }
    }

    /// <summary>
    /// Clears the search query and reloads all guides.
    /// </summary>
    [RelayCommand]
    private async Task ClearSearchAsync()
    {
        SearchQuery = string.Empty;
        await SearchAsync();
    }

    /// <summary>
    /// Called when SearchQuery property changes to notify HasSearchQuery.
    /// </summary>
    partial void OnSearchQueryChanged(string value)
    {
        OnPropertyChanged(nameof(HasSearchQuery));
    }

    /// <summary>
    /// Updates the empty state message based on current filters.
    /// </summary>
    private void UpdateEmptyStateMessage()
    {
        if (Guides.Count == 0)
        {
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                EmptyStateMessage = $"No guides found for '{SearchQuery}'";
            }
            else if (SelectedCategory != null && SelectedCategory.Id != ObjectId.Empty)
            {
                EmptyStateMessage = $"No guides in '{SelectedCategory.Name}' category";
            }
            else
            {
                EmptyStateMessage = IsAdmin
                    ? "No guides yet. Click 'New Guide' to create one."
                    : "No guides available.";
            }
        }
    }

    /// <summary>
    /// Gets whether any guides are available.
    /// </summary>
    public bool HasGuides => Guides.Count > 0;
}

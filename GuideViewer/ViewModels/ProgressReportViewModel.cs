using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GuideViewer.Core.Models;
using GuideViewer.Data.Repositories;
using LiteDB;
using Microsoft.UI.Dispatching;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace GuideViewer.ViewModels;

/// <summary>
/// ViewModel for the admin progress reports section.
/// Displays all users' progress across all guides.
/// </summary>
public partial class ProgressReportViewModel : ObservableObject
{
    private readonly ProgressRepository _progressRepository;
    private readonly UserRepository _userRepository;
    private readonly GuideRepository _guideRepository;
    private readonly DispatcherQueue _dispatcherQueue;

    [ObservableProperty]
    private ObservableCollection<ProgressReportItem> allProgress = new();

    [ObservableProperty]
    private ObservableCollection<ProgressReportItem> filteredProgress = new();

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private bool hasProgress = false;

    [ObservableProperty]
    private string searchQuery = string.Empty;

    [ObservableProperty]
    private string selectedUserFilter = "All Users";

    [ObservableProperty]
    private string selectedGuideFilter = "All Guides";

    [ObservableProperty]
    private ObservableCollection<string> availableUsers = new();

    [ObservableProperty]
    private ObservableCollection<string> availableGuides = new();

    public ProgressReportViewModel(
        ProgressRepository progressRepository,
        UserRepository userRepository,
        GuideRepository guideRepository,
        DispatcherQueue dispatcherQueue)
    {
        _progressRepository = progressRepository ?? throw new ArgumentNullException(nameof(progressRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _guideRepository = guideRepository ?? throw new ArgumentNullException(nameof(guideRepository));
        _dispatcherQueue = dispatcherQueue ?? throw new ArgumentNullException(nameof(dispatcherQueue));

        // Subscribe to collection changes
        FilteredProgress.CollectionChanged += (s, e) => OnPropertyChanged(nameof(HasProgress));
    }

    /// <summary>
    /// Initializes the report by loading all progress data.
    /// </summary>
    public async Task InitializeAsync()
    {
        await LoadAllProgressAsync();
    }

    /// <summary>
    /// Loads all progress records with user and guide information.
    /// </summary>
    [RelayCommand]
    private async Task LoadAllProgressAsync()
    {
        IsLoading = true;

        try
        {
            await Task.Run(() =>
            {
                // Get all progress records
                var allProgressRecords = _progressRepository.GetAll().ToList();
                Log.Information("Loading {Count} progress records for admin report", allProgressRecords.Count);

                // Create report items by joining with users and guides
                var reportItems = allProgressRecords
                    .Select(CreateProgressReportItem)
                    .Where(item => item != null)
                    .OrderByDescending(item => item!.LastAccessedAt)
                    .ToList();

                // Get unique users and guides for filters
                var users = reportItems
                    .Select(item => item!.UserName)
                    .Distinct()
                    .OrderBy(name => name)
                    .ToList();

                var guides = reportItems
                    .Select(item => item!.GuideTitle)
                    .Distinct()
                    .OrderBy(title => title)
                    .ToList();

                // Update UI on main thread
                _dispatcherQueue.TryEnqueue(() =>
                {
                    AllProgress.Clear();
                    foreach (var item in reportItems)
                    {
                        if (item != null)
                        {
                            AllProgress.Add(item);
                        }
                    }

                    // Update filter dropdowns
                    AvailableUsers.Clear();
                    AvailableUsers.Add("All Users");
                    foreach (var user in users)
                    {
                        AvailableUsers.Add(user);
                    }

                    AvailableGuides.Clear();
                    AvailableGuides.Add("All Guides");
                    foreach (var guide in guides)
                    {
                        AvailableGuides.Add(guide);
                    }

                    // Initial filter
                    ApplyFilters();
                });
            });

            Log.Information("Admin progress report loaded successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load admin progress report");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Creates a ProgressReportItem from a Progress record.
    /// </summary>
    private ProgressReportItem? CreateProgressReportItem(Data.Entities.Progress progress)
    {
        try
        {
            var user = _userRepository.GetById(progress.UserId);
            if (user == null)
            {
                Log.Warning("User {UserId} not found for progress {ProgressId}", progress.UserId, progress.Id);
                return null;
            }

            var guide = _guideRepository.GetById(progress.GuideId);
            if (guide == null)
            {
                Log.Warning("Guide {GuideId} not found for progress {ProgressId}", progress.GuideId, progress.Id);
                return null;
            }

            return new ProgressReportItem
            {
                ProgressId = progress.Id,
                UserId = user.Id,
                UserName = $"User {user.Id}",  // User doesn't have Username property
                GuideId = guide.Id,
                GuideTitle = guide.Title,
                GuideCategory = guide.Category,
                CurrentStep = progress.CurrentStepOrder,
                CompletedSteps = progress.CompletedStepOrders.Count,
                TotalSteps = guide.Steps.Count,
                StartedAt = progress.StartedAt,
                LastAccessedAt = progress.LastAccessedAt,
                CompletedAt = progress.CompletedAt,
                TotalTimeMinutes = progress.TotalActiveTimeSeconds / 60
            };
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create ProgressReportItem for progress {ProgressId}", progress.Id);
            return null;
        }
    }

    /// <summary>
    /// Applies all active filters (search, user filter, guide filter).
    /// </summary>
    [RelayCommand]
    private void ApplyFilters()
    {
        var filtered = AllProgress.AsEnumerable();

        // Apply user filter
        if (SelectedUserFilter != "All Users")
        {
            filtered = filtered.Where(item => item.UserName == SelectedUserFilter);
        }

        // Apply guide filter
        if (SelectedGuideFilter != "All Guides")
        {
            filtered = filtered.Where(item => item.GuideTitle == SelectedGuideFilter);
        }

        // Apply search query
        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            var query = SearchQuery.ToLowerInvariant();
            filtered = filtered.Where(item =>
                item.UserName.ToLowerInvariant().Contains(query) ||
                item.GuideTitle.ToLowerInvariant().Contains(query) ||
                item.GuideCategory.ToLowerInvariant().Contains(query));
        }

        FilteredProgress.Clear();
        foreach (var item in filtered)
        {
            FilteredProgress.Add(item);
        }

        HasProgress = FilteredProgress.Count > 0;
        Log.Debug("Applied filters: {FilteredCount}/{TotalCount} records", FilteredProgress.Count, AllProgress.Count);
    }

    /// <summary>
    /// Clears the search query and reapplies filters.
    /// </summary>
    [RelayCommand]
    private void ClearSearch()
    {
        SearchQuery = string.Empty;
        ApplyFilters();
    }

    /// <summary>
    /// Triggers when search query changes.
    /// </summary>
    partial void OnSearchQueryChanged(string value)
    {
        ApplyFilters();
    }

    /// <summary>
    /// Triggers when user filter changes.
    /// </summary>
    partial void OnSelectedUserFilterChanged(string value)
    {
        ApplyFilters();
    }

    /// <summary>
    /// Triggers when guide filter changes.
    /// </summary>
    partial void OnSelectedGuideFilterChanged(string value)
    {
        ApplyFilters();
    }
}

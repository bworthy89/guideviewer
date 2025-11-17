using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GuideViewer.Core.Services;
using GuideViewer.Data.Entities;
using GuideViewer.Data.Models;
using GuideViewer.Data.Repositories;
using GuideViewer.Services;
using LiteDB;
using Microsoft.UI.Dispatching;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace GuideViewer.ViewModels;

/// <summary>
/// ViewModel for the Progress Dashboard page.
/// Displays user progress statistics, active guides, and completed guides.
/// </summary>
public partial class ProgressDashboardViewModel : ObservableObject
{
    private readonly IProgressTrackingService _progressTrackingService;
    private readonly GuideRepository _guideRepository;
    private readonly UserRepository _userRepository;
    private readonly Services.NavigationService _navigationService;
    private readonly DispatcherQueue _dispatcherQueue;

    [ObservableProperty]
    private ObservableCollection<ProgressGuideItem> activeGuides = new();

    [ObservableProperty]
    private ObservableCollection<ProgressGuideItem> completedGuides = new();

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private bool hasActiveGuides = false;

    [ObservableProperty]
    private bool hasCompletedGuides = false;

    // Statistics properties
    [ObservableProperty]
    private int totalStarted = 0;

    [ObservableProperty]
    private int totalCompleted = 0;

    [ObservableProperty]
    private int currentlyInProgress = 0;

    [ObservableProperty]
    private double completionRate = 0.0;

    [ObservableProperty]
    private double averageCompletionTimeHours = 0.0;

    public ProgressDashboardViewModel(
        IProgressTrackingService progressTrackingService,
        GuideRepository guideRepository,
        UserRepository userRepository,
        Services.NavigationService navigationService,
        DispatcherQueue dispatcherQueue)
    {
        _progressTrackingService = progressTrackingService ?? throw new ArgumentNullException(nameof(progressTrackingService));
        _guideRepository = guideRepository ?? throw new ArgumentNullException(nameof(guideRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _dispatcherQueue = dispatcherQueue ?? throw new ArgumentNullException(nameof(dispatcherQueue));

        // Subscribe to collection changes to update computed properties
        ActiveGuides.CollectionChanged += (s, e) => OnPropertyChanged(nameof(HasActiveGuides));
        CompletedGuides.CollectionChanged += (s, e) => OnPropertyChanged(nameof(HasCompletedGuides));
    }

    /// <summary>
    /// Initializes the dashboard by loading user progress data.
    /// </summary>
    public async Task InitializeAsync()
    {
        await LoadDashboardDataAsync();
    }

    /// <summary>
    /// Loads all dashboard data (statistics, active guides, completed guides).
    /// </summary>
    [RelayCommand]
    private async Task LoadDashboardDataAsync()
    {
        IsLoading = true;

        try
        {
            await Task.Run(async () =>
            {
                // Get current user
                var currentUser = _userRepository.GetCurrentUser();
                if (currentUser == null)
                {
                    Log.Warning("No current user found when loading progress dashboard");
                    return;
                }

                // Load statistics
                var statistics = await _progressTrackingService.GetStatisticsAsync(currentUser.Id);

                // Load active progress
                var activeProgressList = await _progressTrackingService.GetActiveProgressAsync(currentUser.Id);
                var activeItems = activeProgressList
                    .Select(p => CreateProgressGuideItem(p, currentUser.Id))
                    .Where(item => item != null)
                    .OrderByDescending(item => item!.LastAccessedAt)
                    .Take(10) // Show top 10 most recent
                    .ToList();

                // Load completed progress (most recent 5)
                var completedProgressList = await _progressTrackingService.GetCompletedProgressAsync(currentUser.Id);
                var completedItems = completedProgressList
                    .Select(p => CreateProgressGuideItem(p, currentUser.Id))
                    .Where(item => item != null)
                    .Take(5)
                    .ToList();

                // Update UI on main thread
                _dispatcherQueue.TryEnqueue(() =>
                {
                    // Update statistics
                    TotalStarted = statistics.TotalStarted;
                    TotalCompleted = statistics.TotalCompleted;
                    CurrentlyInProgress = statistics.CurrentlyInProgress;
                    CompletionRate = statistics.CompletionRate;
                    AverageCompletionTimeHours = statistics.AverageCompletionTimeMinutes / 60.0;

                    // Update active guides
                    ActiveGuides.Clear();
                    foreach (var item in activeItems)
                    {
                        if (item != null)
                        {
                            ActiveGuides.Add(item);
                        }
                    }

                    // Update completed guides
                    CompletedGuides.Clear();
                    foreach (var item in completedItems)
                    {
                        if (item != null)
                        {
                            CompletedGuides.Add(item);
                        }
                    }

                    HasActiveGuides = ActiveGuides.Count > 0;
                    HasCompletedGuides = CompletedGuides.Count > 0;
                });
            });

            Log.Information("Progress dashboard loaded successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load progress dashboard data");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Creates a ProgressGuideItem from a Progress record.
    /// </summary>
    private ProgressGuideItem? CreateProgressGuideItem(Progress progress, ObjectId userId)
    {
        try
        {
            var guide = _guideRepository.GetById(progress.GuideId);
            if (guide == null)
            {
                Log.Warning("Guide {GuideId} not found for progress {ProgressId}", progress.GuideId, progress.Id);
                return null;
            }

            var totalSteps = guide.Steps.Count;
            var completedSteps = progress.CompletedStepOrders.Count;
            var progressPercentage = totalSteps > 0 ? (double)completedSteps / totalSteps * 100 : 0;

            var estimatedTimeRemaining = _progressTrackingService.CalculateEstimatedTimeRemaining(progress, guide);

            return new ProgressGuideItem
            {
                ProgressId = progress.Id,
                GuideId = guide.Id,
                GuideTitle = guide.Title,
                GuideCategory = guide.Category,
                CurrentStep = progress.CurrentStepOrder,
                TotalSteps = totalSteps,
                CompletedSteps = completedSteps,
                ProgressPercentage = progressPercentage,
                EstimatedMinutesRemaining = estimatedTimeRemaining,
                TotalActiveTimeMinutes = progress.TotalActiveTimeSeconds / 60,
                LastAccessedAt = progress.LastAccessedAt,
                CompletedAt = progress.CompletedAt,
                IsCompleted = progress.CompletedAt.HasValue
            };
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create ProgressGuideItem for progress {ProgressId}", progress.Id);
            return null;
        }
    }

    /// <summary>
    /// Resumes a guide by navigating to the Active Guide page.
    /// </summary>
    [RelayCommand]
    private void ResumeGuide(ProgressGuideItem item)
    {
        if (item == null) return;

        try
        {
            // Navigate to Active Guide page with the guide and progress IDs
            _navigationService.NavigateTo(PageKeys.ActiveGuide, new { GuideId = item.GuideId, ProgressId = item.ProgressId });
            Log.Information("Resuming guide {GuideTitle} (Progress: {ProgressId})", item.GuideTitle, item.ProgressId);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to resume guide {GuideTitle}", item.GuideTitle);
        }
    }

    /// <summary>
    /// Views a completed guide.
    /// </summary>
    [RelayCommand]
    private void ViewCompletedGuide(ProgressGuideItem item)
    {
        if (item == null) return;

        try
        {
            // Navigate to Guide Detail page (read-only view)
            _navigationService.NavigateTo(PageKeys.GuideDetail, item.GuideId);
            Log.Information("Viewing completed guide {GuideTitle}", item.GuideTitle);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to view guide {GuideTitle}", item.GuideTitle);
        }
    }
}

/// <summary>
/// Represents a guide with progress information for display in the dashboard.
/// </summary>
public class ProgressGuideItem
{
    public required ObjectId ProgressId { get; set; }
    public required ObjectId GuideId { get; set; }
    public required string GuideTitle { get; set; }
    public required string GuideCategory { get; set; }
    public required int CurrentStep { get; set; }
    public required int TotalSteps { get; set; }
    public required int CompletedSteps { get; set; }
    public required double ProgressPercentage { get; set; }
    public int? EstimatedMinutesRemaining { get; set; }
    public required int TotalActiveTimeMinutes { get; set; }
    public required DateTime LastAccessedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public required bool IsCompleted { get; set; }

    /// <summary>
    /// Gets a user-friendly display string for progress.
    /// </summary>
    public string ProgressDisplayText => $"{CompletedSteps} of {TotalSteps} steps completed";

    /// <summary>
    /// Gets a user-friendly display string for estimated time remaining.
    /// </summary>
    public string TimeRemainingDisplayText =>
        EstimatedMinutesRemaining.HasValue
            ? $"~{EstimatedMinutesRemaining.Value} min remaining"
            : "Time unknown";

    /// <summary>
    /// Gets a user-friendly display string for time spent.
    /// </summary>
    public string TimeSpentDisplayText =>
        TotalActiveTimeMinutes > 0
            ? $"{TotalActiveTimeMinutes} min spent"
            : "No time tracked";

    /// <summary>
    /// Gets a user-friendly display string for last accessed time.
    /// </summary>
    public string LastAccessedDisplayText
    {
        get
        {
            var timeSpan = DateTime.UtcNow - LastAccessedAt;
            if (timeSpan.TotalMinutes < 1)
                return "Just now";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} min ago";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} hours ago";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays} days ago";
            return LastAccessedAt.ToLocalTime().ToString("MMM d, yyyy");
        }
    }

    /// <summary>
    /// Gets a user-friendly display string for completion date.
    /// </summary>
    public string CompletedDisplayText =>
        CompletedAt.HasValue
            ? $"Completed {CompletedAt.Value.ToLocalTime():MMM d, yyyy}"
            : string.Empty;
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GuideViewer.Core.Services;
using GuideViewer.Data.Entities;
using GuideViewer.Data.Repositories;
using LiteDB;
using Microsoft.UI.Dispatching;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace GuideViewer.ViewModels;

/// <summary>
/// ViewModel for the active guide progress tracking page.
/// Handles step-by-step navigation, timer, notes, and progress updates.
/// </summary>
public partial class ActiveGuideProgressViewModel : ObservableObject, IDisposable
{
    private readonly IProgressTrackingService _progressTrackingService;
    private readonly ITimerService _timerService;
    private readonly IAutoSaveService _autoSaveService;
    private readonly GuideRepository _guideRepository;
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly object _saveLock = new object();

    private ObjectId _progressId = ObjectId.Empty;
    private ObjectId _guideId = ObjectId.Empty;
    private ObjectId _userId = ObjectId.Empty;

    [ObservableProperty]
    private Guide? currentGuide;

    [ObservableProperty]
    private Progress? currentProgress;

    [ObservableProperty]
    private Step? currentStep;

    [ObservableProperty]
    private ObservableCollection<Step> allSteps = new();

    [ObservableProperty]
    private int currentStepIndex = 0;

    [ObservableProperty]
    private string stepNotes = string.Empty;

    [ObservableProperty]
    private bool isCurrentStepCompleted = false;

    [ObservableProperty]
    private TimeSpan elapsedTime;

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private bool isSaving = false;

    [ObservableProperty]
    private bool hasUnsavedNotes = false;

    [ObservableProperty]
    private string pageTitle = "Guide Progress";

    [ObservableProperty]
    private string progressText = "Step 0 of 0";

    [ObservableProperty]
    private double progressPercentage = 0;

    [ObservableProperty]
    private int completedStepsCount = 0;

    [ObservableProperty]
    private bool isFirstStep = true;

    [ObservableProperty]
    private bool isLastStep = false;

    [ObservableProperty]
    private bool canFinishGuide = false;

    [ObservableProperty]
    private string elapsedTimeDisplay = "00:00:00";

    public ActiveGuideProgressViewModel(
        IProgressTrackingService progressTrackingService,
        ITimerService timerService,
        IAutoSaveService autoSaveService,
        GuideRepository guideRepository,
        DispatcherQueue dispatcherQueue)
    {
        _progressTrackingService = progressTrackingService ?? throw new ArgumentNullException(nameof(progressTrackingService));
        _timerService = timerService ?? throw new ArgumentNullException(nameof(timerService));
        _autoSaveService = autoSaveService ?? throw new ArgumentNullException(nameof(autoSaveService));
        _guideRepository = guideRepository ?? throw new ArgumentNullException(nameof(guideRepository));
        _dispatcherQueue = dispatcherQueue ?? throw new ArgumentNullException(nameof(dispatcherQueue));

        // Subscribe to property changes with named method to prevent memory leak
        PropertyChanged += OnPropertyChanged_TrackNotes;

        // Subscribe to timer tick
        _timerService.Tick += OnTimerTick;
    }

    /// <summary>
    /// Initializes the ViewModel by loading guide and starting/resuming progress.
    /// </summary>
    public async Task InitializeAsync(ObjectId userId, ObjectId guideId, ObjectId? progressId = null)
    {
        IsLoading = true;
        _userId = userId;
        _guideId = guideId;

        try
        {
            // Load guide from database on background thread
            Guide? guide = null;
            await Task.Run(() =>
            {
                guide = _guideRepository.GetById(guideId);
                if (guide == null)
                {
                    Log.Error("Guide {GuideId} not found", guideId);
                }
            });

            if (guide == null)
            {
                return;
            }

            // Set UI-bound properties on UI thread
            CurrentGuide = guide;
            AllSteps.Clear();
            foreach (var step in guide.Steps.OrderBy(s => s.Order))
            {
                AllSteps.Add(step);
            }

            // Start or resume progress
            if (progressId != null)
            {
                // Resume existing progress
                await ResumeProgressAsync((ObjectId)progressId);
            }
            else
            {
                // Start new progress or resume existing
                await StartOrResumeProgressAsync();
            }

            // Start timer
            _timerService.Start();

            // Start auto-save for notes (every 30 seconds)
            _autoSaveService.StartAutoSave(async () => await AutoSaveNotesAsync(), 30);

            PageTitle = CurrentGuide.Title;
            UpdateProgress();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to initialize active guide progress");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task StartOrResumeProgressAsync()
    {
        try
        {
            if (_guideId == ObjectId.Empty || _userId == ObjectId.Empty)
            {
                return;
            }

            // Check if progress already exists
            CurrentProgress = await _progressTrackingService.GetProgressAsync(_guideId, _userId);

            if (CurrentProgress == null)
            {
                // Start new progress
                CurrentProgress = await _progressTrackingService.StartGuideAsync(_guideId, _userId);
                Log.Information("Started new progress for guide {GuideId}", _guideId);

                CurrentStepIndex = 0;
                if (AllSteps.Count > 0)
                {
                    CurrentStep = AllSteps[0];
                    StepNotes = CurrentProgress.Notes ?? string.Empty;
                    IsCurrentStepCompleted = false;
                }
            }
            else
            {
                // Resume existing progress
                Log.Information("Resuming existing progress {ProgressId} for guide {GuideId}", CurrentProgress.Id, _guideId);

                CurrentStepIndex = AllSteps.ToList().FindIndex(s => s.Order == CurrentProgress.CurrentStepOrder);
                if (CurrentStepIndex == -1)
                {
                    CurrentStepIndex = 0;
                }

                CurrentStep = AllSteps[CurrentStepIndex];
                StepNotes = CurrentProgress.Notes ?? string.Empty;
                IsCurrentStepCompleted = CurrentProgress.CompletedStepOrders.Contains(CurrentStep.Order);
                CompletedStepsCount = CurrentProgress.CompletedStepOrders.Count;
            }

            _progressId = CurrentProgress.Id;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to start or resume progress");
            throw;
        }
    }

    private async Task ResumeProgressAsync(ObjectId progressId)
    {
        try
        {
            if (_guideId == ObjectId.Empty || _userId == ObjectId.Empty)
            {
                return;
            }

            _progressId = progressId;
            CurrentProgress = await _progressTrackingService.GetProgressAsync(_guideId, _userId);

            if (CurrentProgress == null)
            {
                Log.Error("Progress {ProgressId} not found", progressId);
                return;
            }

            // Find current step
            CurrentStepIndex = AllSteps.ToList().FindIndex(s => s.Order == CurrentProgress.CurrentStepOrder);
            if (CurrentStepIndex == -1)
            {
                CurrentStepIndex = 0;
            }

            CurrentStep = AllSteps[CurrentStepIndex];
            StepNotes = CurrentProgress.Notes ?? string.Empty;
            IsCurrentStepCompleted = CurrentProgress.CompletedStepOrders.Contains(CurrentStep.Order);
            CompletedStepsCount = CurrentProgress.CompletedStepOrders.Count;

            Log.Information("Resumed progress {ProgressId} at step {StepOrder}", progressId, CurrentStep.Order);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to resume progress");
            throw;
        }
    }

    [RelayCommand]
    private async Task NextStepAsync()
    {
        if (IsLastStep || CurrentStepIndex >= AllSteps.Count - 1 || _progressId == ObjectId.Empty)
        {
            return;
        }

        // Save current step notes before moving
        await SaveNotesAsync();

        CurrentStepIndex++;
        CurrentStep = AllSteps[CurrentStepIndex];

        // Update current step in database
        await _progressTrackingService.UpdateCurrentStepAsync(_progressId, CurrentStep.Order);

        // Load notes for new step (if any)
        // Notes are stored per-progress, not per-step, so we'll keep the same notes
        // In a real app, you might want per-step notes
        IsCurrentStepCompleted = CurrentProgress!.CompletedStepOrders.Contains(CurrentStep.Order);

        UpdateProgress();
        Log.Debug("Moved to step {StepOrder}", CurrentStep.Order);
    }

    [RelayCommand]
    private async Task PreviousStepAsync()
    {
        if (IsFirstStep || CurrentStepIndex <= 0 || _progressId == ObjectId.Empty)
        {
            return;
        }

        // Save current step notes before moving
        await SaveNotesAsync();

        CurrentStepIndex--;
        CurrentStep = AllSteps[CurrentStepIndex];

        // Update current step in database
        await _progressTrackingService.UpdateCurrentStepAsync(_progressId, CurrentStep.Order);

        IsCurrentStepCompleted = CurrentProgress!.CompletedStepOrders.Contains(CurrentStep.Order);

        UpdateProgress();
        Log.Debug("Moved to step {StepOrder}", CurrentStep.Order);
    }

    [RelayCommand]
    private async Task ToggleStepCompletionAsync()
    {
        if (CurrentStep == null || CurrentProgress == null || _progressId == ObjectId.Empty)
        {
            return;
        }

        try
        {
            if (IsCurrentStepCompleted)
            {
                // Uncomplete step
                await _progressTrackingService.CompleteStepAsync(_progressId, CurrentStep.Order, false);
                IsCurrentStepCompleted = false;
                CompletedStepsCount--;
                CurrentProgress.CompletedStepOrders.Remove(CurrentStep.Order);
                Log.Debug("Uncompleted step {StepOrder}", CurrentStep.Order);
            }
            else
            {
                // Complete step
                await _progressTrackingService.CompleteStepAsync(_progressId, CurrentStep.Order, true, StepNotes);
                IsCurrentStepCompleted = true;
                CompletedStepsCount++;
                if (!CurrentProgress.CompletedStepOrders.Contains(CurrentStep.Order))
                {
                    CurrentProgress.CompletedStepOrders.Add(CurrentStep.Order);
                }
                Log.Debug("Completed step {StepOrder}", CurrentStep.Order);
            }

            UpdateProgress();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to toggle step completion");
        }
    }

    [RelayCommand]
    private async Task FinishGuideAsync()
    {
        if (!CanFinishGuide || CurrentProgress == null || _progressId == ObjectId.Empty)
        {
            return;
        }

        try
        {
            // Save notes one last time
            await SaveNotesAsync();

            // Mark guide as complete
            await _progressTrackingService.MarkGuideCompleteAsync(_progressId);

            // Stop timer and auto-save
            _timerService.Stop();
            _autoSaveService.StopAutoSave();

            Log.Information("Finished guide {GuideId}, total time: {ElapsedTime}", _guideId, ElapsedTime);

            // Navigate back to progress dashboard
            // This will be handled by the code-behind
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to finish guide");
        }
    }

    [RelayCommand]
    private async Task SaveNotesAsync()
    {
        if (string.IsNullOrWhiteSpace(StepNotes) || CurrentProgress == null)
        {
            HasUnsavedNotes = false;
            return;
        }

        // Prevent concurrent saves
        lock (_saveLock)
        {
            if (IsSaving) return;
            IsSaving = true;
        }

        try
        {
            await Task.Run(() =>
            {
                // Update progress notes
                CurrentProgress.Notes = StepNotes;
                CurrentProgress.LastAccessedAt = DateTime.UtcNow;

                // This would require a new method in IProgressTrackingService
                // For now, we'll just update the in-memory object
                // In a real implementation, you'd call something like:
                // _progressTrackingService.UpdateNotesAsync(_progressId, StepNotes).Wait();
            });

            HasUnsavedNotes = false;
            _autoSaveService.IsDirty = false;
            Log.Debug("Saved notes for progress {ProgressId}", _progressId);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save notes");
        }
        finally
        {
            IsSaving = false;
        }
    }

    private async Task AutoSaveNotesAsync()
    {
        if (HasUnsavedNotes && !IsSaving)
        {
            await SaveNotesAsync();
        }
    }

    private void UpdateProgress()
    {
        if (AllSteps.Count == 0 || CurrentStep == null)
        {
            return;
        }

        // Update progress indicators
        ProgressText = $"Step {CurrentStepIndex + 1} of {AllSteps.Count}";
        ProgressPercentage = AllSteps.Count > 0 ? (double)CompletedStepsCount / AllSteps.Count * 100 : 0;

        // Update navigation states
        IsFirstStep = CurrentStepIndex == 0;
        IsLastStep = CurrentStepIndex == AllSteps.Count - 1;

        // Can finish guide if all steps are completed
        CanFinishGuide = CompletedStepsCount == AllSteps.Count;

        OnPropertyChanged(nameof(ProgressText));
        OnPropertyChanged(nameof(ProgressPercentage));
        OnPropertyChanged(nameof(IsFirstStep));
        OnPropertyChanged(nameof(IsLastStep));
        OnPropertyChanged(nameof(CanFinishGuide));
    }

    private void OnTimerTick(object? sender, TimeSpan elapsed)
    {
        // Update elapsed time display on UI thread
        _dispatcherQueue.TryEnqueue(() =>
        {
            ElapsedTime = elapsed;
            ElapsedTimeDisplay = elapsed.ToString(@"hh\:mm\:ss");
        });
    }

    private void OnPropertyChanged_TrackNotes(object? sender, PropertyChangedEventArgs e)
    {
        // Track changes for notes only
        if (e.PropertyName == nameof(StepNotes))
        {
            HasUnsavedNotes = true;
            _autoSaveService.IsDirty = true;
        }
    }

    public void Dispose()
    {
        // Stop timer and auto-save
        _timerService.Stop();
        _timerService.Tick -= OnTimerTick;
        _autoSaveService.StopAutoSave();

        // Unsubscribe from property changes to prevent memory leak
        PropertyChanged -= OnPropertyChanged_TrackNotes;

        Log.Debug("Disposed ActiveGuideProgressViewModel");
    }
}

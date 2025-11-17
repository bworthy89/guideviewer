using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GuideViewer.Core.Services;
using GuideViewer.Data.Entities;
using GuideViewer.Data.Repositories;
using GuideViewer.Services;
using LiteDB;
using Microsoft.UI.Dispatching;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GuideViewer.ViewModels;

/// <summary>
/// ViewModel for creating and editing installation guides.
/// </summary>
public partial class GuideEditorViewModel : ObservableObject
{
    private readonly GuideRepository _guideRepository;
    private readonly CategoryRepository _categoryRepository;
    private readonly UserRepository _userRepository;
    private readonly IImageStorageService _imageStorageService;
    private readonly IAutoSaveService _autoSaveService;
    private readonly NavigationService _navigationService;
    private readonly DispatcherQueue _dispatcherQueue;

    private ObjectId? _guideId; // Null for new guide, set for editing
    private readonly object _saveLock = new object(); // Prevents concurrent saves

    #region Observable Properties

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private string category = string.Empty;

    [ObservableProperty]
    private int estimatedMinutes = 30;

    [ObservableProperty]
    private ObservableCollection<Step> steps = new();

    [ObservableProperty]
    private ObservableCollection<Category> availableCategories = new();

    [ObservableProperty]
    private Step? selectedStep;

    /// <summary>
    /// Gets the display text for the selected step order.
    /// </summary>
    public string SelectedStepOrderDisplay => SelectedStep?.Order.ToString() ?? "?";

    partial void OnSelectedStepChanged(Step? value)
    {
        OnPropertyChanged(nameof(SelectedStepOrderDisplay));
    }

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private bool isSaving = false;

    [ObservableProperty]
    private bool isNewGuide = true;

    [ObservableProperty]
    private string pageTitle = "New Guide";

    [ObservableProperty]
    private string validationMessage = string.Empty;

    [ObservableProperty]
    private bool hasValidationError = false;

    [ObservableProperty]
    private DateTime? lastSavedAt;

    [ObservableProperty]
    private bool hasUnsavedChanges = false;

    #endregion

    public GuideEditorViewModel(
        GuideRepository guideRepository,
        CategoryRepository categoryRepository,
        UserRepository userRepository,
        IImageStorageService imageStorageService,
        IAutoSaveService autoSaveService,
        NavigationService navigationService,
        DispatcherQueue dispatcherQueue)
    {
        _guideRepository = guideRepository ?? throw new ArgumentNullException(nameof(guideRepository));
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _imageStorageService = imageStorageService ?? throw new ArgumentNullException(nameof(imageStorageService));
        _autoSaveService = autoSaveService ?? throw new ArgumentNullException(nameof(autoSaveService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _dispatcherQueue = dispatcherQueue ?? throw new ArgumentNullException(nameof(dispatcherQueue));

        // Load categories
        LoadCategories();

        // Start auto-save
        _autoSaveService.StartAutoSave(AutoSaveAsync, intervalSeconds: 30);

        // Track changes - use named method to avoid memory leak
        PropertyChanged += OnPropertyChanged_TrackChanges;
    }

    /// <summary>
    /// Initializes the editor for a new guide or loads an existing guide for editing.
    /// </summary>
    public async Task InitializeAsync(ObjectId? guideId = null)
    {
        _guideId = guideId;
        IsNewGuide = guideId == null || guideId == ObjectId.Empty;

        if (IsNewGuide)
        {
            PageTitle = "New Guide";
            AddStep(); // Start with one empty step
        }
        else
        {
            PageTitle = "Edit Guide";
            // Safe access: guideId is not null here due to IsNewGuide check above
            await LoadGuideAsync((ObjectId)guideId!);
        }
    }

    /// <summary>
    /// Loads categories from the database.
    /// </summary>
    private void LoadCategories()
    {
        try
        {
            var categories = _categoryRepository.GetAll().ToList();
            AvailableCategories.Clear();
            foreach (var cat in categories)
            {
                AvailableCategories.Add(cat);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load categories");
        }
    }

    /// <summary>
    /// Loads an existing guide for editing.
    /// </summary>
    private async Task LoadGuideAsync(ObjectId guideId)
    {
        IsLoading = true;

        try
        {
            await Task.Run(() =>
            {
                var guide = _guideRepository.GetById(guideId);

                if (guide == null)
                {
                    Log.Warning("Guide not found: {GuideId}", guideId);
                    _dispatcherQueue.TryEnqueue(() =>
                    {
                        ValidationMessage = "Guide not found.";
                        HasValidationError = true;
                    });
                    return;
                }

                _dispatcherQueue.TryEnqueue(() =>
                {
                    Title = guide.Title;
                    Description = guide.Description;
                    Category = guide.Category;
                    EstimatedMinutes = guide.EstimatedMinutes;

                    Steps.Clear();
                    foreach (var step in guide.Steps.OrderBy(s => s.Order))
                    {
                        Steps.Add(step);
                    }

                    HasUnsavedChanges = false;
                    _autoSaveService.IsDirty = false;
                });
            });

            Log.Information("Loaded guide for editing: {GuideId}", guideId);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load guide: {GuideId}", guideId);
            ValidationMessage = "Failed to load guide. Please try again.";
            HasValidationError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    #region Step Management Commands

    /// <summary>
    /// Adds a new step to the guide.
    /// </summary>
    [RelayCommand]
    private void AddStep()
    {
        var newStep = new Step
        {
            Order = Steps.Count + 1,
            Title = $"Step {Steps.Count + 1}",
            Content = string.Empty
        };

        Steps.Add(newStep);
        SelectedStep = newStep;
        HasUnsavedChanges = true;
        _autoSaveService.IsDirty = true;

        Log.Information("Added new step: Order {Order}", newStep.Order);
    }

    /// <summary>
    /// Deletes a step from the guide.
    /// </summary>
    [RelayCommand]
    private async Task DeleteStepAsync(Step step)
    {
        if (step == null) return;

        // Delete associated images
        foreach (var imageId in step.ImageIds)
        {
            try
            {
                await _imageStorageService.DeleteImageAsync(imageId);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to delete image: {ImageId}", imageId);
            }
        }

        Steps.Remove(step);

        // Reorder remaining steps
        ReorderSteps();

        HasUnsavedChanges = true;
        _autoSaveService.IsDirty = true;

        Log.Information("Deleted step: {StepId}", step.Id);
    }

    /// <summary>
    /// Moves a step up in the order.
    /// </summary>
    [RelayCommand]
    private void MoveStepUp(Step step)
    {
        if (step == null) return;

        var index = Steps.IndexOf(step);
        if (index > 0)
        {
            Steps.Move(index, index - 1);
            ReorderSteps();
            HasUnsavedChanges = true;
            _autoSaveService.IsDirty = true;
        }
    }

    /// <summary>
    /// Moves a step down in the order.
    /// </summary>
    [RelayCommand]
    private void MoveStepDown(Step step)
    {
        if (step == null) return;

        var index = Steps.IndexOf(step);
        if (index >= 0 && index < Steps.Count - 1)
        {
            Steps.Move(index, index + 1);
            ReorderSteps();
            HasUnsavedChanges = true;
            _autoSaveService.IsDirty = true;
        }
    }

    /// <summary>
    /// Reorders all steps to have sequential Order values.
    /// </summary>
    private void ReorderSteps()
    {
        for (int i = 0; i < Steps.Count; i++)
        {
            Steps[i].Order = i + 1;
        }
    }

    #endregion

    #region Image Management Commands

    /// <summary>
    /// Uploads an image for the current step.
    /// Called from code-behind, not directly bound to command.
    /// </summary>
    public async Task UploadImageAsync(Stream imageStream, string fileName)
    {
        if (SelectedStep == null)
        {
            ValidationMessage = "Please select a step first.";
            HasValidationError = true;
            return;
        }

        try
        {
            // Validate image
            var validation = await _imageStorageService.ValidateImageAsync(imageStream, fileName);
            if (!validation.IsValid)
            {
                ValidationMessage = validation.ErrorMessage ?? "Invalid image.";
                HasValidationError = true;
                return;
            }

            // Upload image
            var fileId = await _imageStorageService.UploadImageAsync(imageStream, fileName);

            // Add to step
            SelectedStep.ImageIds.Add(fileId);
            SelectedStep.UpdatedAt = DateTime.UtcNow;

            HasUnsavedChanges = true;
            _autoSaveService.IsDirty = true;

            Log.Information("Uploaded image {FileName} for step {StepId}", fileName, SelectedStep.Id);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to upload image: {FileName}", fileName);
            ValidationMessage = "Failed to upload image. Please try again.";
            HasValidationError = true;
        }
    }

    /// <summary>
    /// Deletes an image from the current step.
    /// </summary>
    [RelayCommand]
    private async Task DeleteImageAsync(string fileId)
    {
        if (SelectedStep == null || string.IsNullOrEmpty(fileId)) return;

        try
        {
            var deleted = await _imageStorageService.DeleteImageAsync(fileId);
            if (deleted)
            {
                SelectedStep.ImageIds.Remove(fileId);
                SelectedStep.UpdatedAt = DateTime.UtcNow;
                HasUnsavedChanges = true;
                _autoSaveService.IsDirty = true;

                Log.Information("Deleted image: {FileId}", fileId);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to delete image: {FileId}", fileId);
        }
    }

    #endregion

    #region Save Commands

    /// <summary>
    /// Validates the guide before saving.
    /// </summary>
    private bool ValidateGuide()
    {
        HasValidationError = false;
        ValidationMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Title))
        {
            ValidationMessage = "Title is required.";
            HasValidationError = true;
            return false;
        }

        if (string.IsNullOrWhiteSpace(Category))
        {
            ValidationMessage = "Category is required.";
            HasValidationError = true;
            return false;
        }

        if (Steps.Count == 0)
        {
            ValidationMessage = "At least one step is required.";
            HasValidationError = true;
            return false;
        }

        // Validate each step
        foreach (var step in Steps)
        {
            if (string.IsNullOrWhiteSpace(step.Title))
            {
                ValidationMessage = $"Step {step.Order} must have a title.";
                HasValidationError = true;
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Saves the guide to the database.
    /// </summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        if (!ValidateGuide())
        {
            return;
        }

        // Prevent concurrent saves
        lock (_saveLock)
        {
            if (IsSaving)
                return;
            IsSaving = true;
        }

        try
        {
            await Task.Run(() =>
            {
                var currentUser = _userRepository.GetCurrentUser();
                var createdBy = currentUser?.Role ?? "Unknown";

                Guide guide;

                if (IsNewGuide)
                {
                    // Create new guide
                    guide = new Guide
                    {
                        Title = Title,
                        Description = Description,
                        Category = Category,
                        EstimatedMinutes = EstimatedMinutes,
                        Steps = Steps.ToList(),
                        CreatedBy = createdBy,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    var insertedId = _guideRepository.Insert(guide);
                    _guideId = insertedId;
                    guide.Id = insertedId;

                    Log.Information("Created new guide: {GuideId} - {Title}", _guideId, Title);
                }
                else
                {
                    // Update existing guide
                    // _guideId is guaranteed to have a value because IsNewGuide is false
                    guide = _guideRepository.GetById((ObjectId)_guideId!);
                    if (guide != null)
                    {
                        guide.Title = Title;
                        guide.Description = Description;
                        guide.Category = Category;
                        guide.EstimatedMinutes = EstimatedMinutes;
                        guide.Steps = Steps.ToList();
                        guide.UpdatedAt = DateTime.UtcNow;

                        _guideRepository.Update(guide);

                        Log.Information("Updated guide: {GuideId} - {Title}", _guideId, Title);
                    }
                }

                // Ensure category exists in categories collection
                _categoryRepository.EnsureCategory(Category);

                _dispatcherQueue.TryEnqueue(() =>
                {
                    HasUnsavedChanges = false;
                    _autoSaveService.IsDirty = false;
                    LastSavedAt = DateTime.Now;
                    IsNewGuide = false;
                });
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save guide");
            ValidationMessage = "Failed to save guide. Please try again.";
            HasValidationError = true;
        }
        finally
        {
            lock (_saveLock)
            {
                IsSaving = false;
            }
        }
    }

    /// <summary>
    /// Auto-save callback for AutoSaveService.
    /// </summary>
    private async Task AutoSaveAsync()
    {
        if (!HasUnsavedChanges)
        {
            return;
        }

        // Don't start auto-save if manual save is running
        lock (_saveLock)
        {
            if (IsSaving)
                return;
        }

        // Only auto-save if basic fields are filled
        if (!string.IsNullOrWhiteSpace(Title) && !string.IsNullOrWhiteSpace(Category))
        {
            await SaveAsync();
            Log.Information("Auto-saved guide: {Title}", Title);
        }
    }

    /// <summary>
    /// Saves and navigates back to the guides list.
    /// </summary>
    [RelayCommand]
    private async Task SaveAndCloseAsync()
    {
        await SaveAsync();

        if (!HasValidationError)
        {
            _autoSaveService.StopAutoSave();
            _navigationService.NavigateTo(PageKeys.Guides);
        }
    }

    /// <summary>
    /// Cancels editing and navigates back without saving.
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        // TODO: Show confirmation dialog if HasUnsavedChanges
        _autoSaveService.StopAutoSave();
        _navigationService.NavigateTo(PageKeys.Guides);
    }

    #endregion

    /// <summary>
    /// Gets whether a step can be moved up.
    /// </summary>
    public bool CanMoveStepUp(Step? step)
    {
        if (step == null) return false;
        var index = Steps.IndexOf(step);
        return index > 0;
    }

    /// <summary>
    /// Gets whether a step can be moved down.
    /// </summary>
    public bool CanMoveStepDown(Step? step)
    {
        if (step == null) return false;
        var index = Steps.IndexOf(step);
        return index >= 0 && index < Steps.Count - 1;
    }

    /// <summary>
    /// Handles property changes to track unsaved changes.
    /// </summary>
    private void OnPropertyChanged_TrackChanges(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(HasUnsavedChanges) &&
            e.PropertyName != nameof(LastSavedAt) &&
            e.PropertyName != nameof(IsLoading) &&
            e.PropertyName != nameof(IsSaving))
        {
            HasUnsavedChanges = true;
            _autoSaveService.IsDirty = true;
        }
    }
}

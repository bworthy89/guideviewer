using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GuideViewer.Data.Entities;
using GuideViewer.Data.Repositories;
using Microsoft.UI.Dispatching;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace GuideViewer.ViewModels;

/// <summary>
/// ViewModel for managing categories.
/// </summary>
public partial class CategoryManagementViewModel : ObservableObject
{
    private readonly CategoryRepository _categoryRepository;
    private readonly GuideRepository _guideRepository;
    private readonly DispatcherQueue _dispatcherQueue;

    [ObservableProperty]
    private ObservableCollection<Category> categories = new();

    [ObservableProperty]
    private Category? selectedCategory;

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private string validationMessage = string.Empty;

    [ObservableProperty]
    private bool hasValidationError = false;

    public CategoryManagementViewModel(
        CategoryRepository categoryRepository,
        GuideRepository guideRepository,
        DispatcherQueue dispatcherQueue)
    {
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        _guideRepository = guideRepository ?? throw new ArgumentNullException(nameof(guideRepository));
        _dispatcherQueue = dispatcherQueue ?? throw new ArgumentNullException(nameof(dispatcherQueue));

        // Load categories on initialization
        _ = LoadCategoriesAsync();
    }

    /// <summary>
    /// Loads all categories from the database.
    /// </summary>
    [RelayCommand]
    private async Task LoadCategoriesAsync()
    {
        IsLoading = true;

        try
        {
            await Task.Run(() =>
            {
                var categoriesList = _categoryRepository.GetAll().ToList();

                _dispatcherQueue.TryEnqueue(() =>
                {
                    Categories.Clear();
                    foreach (var category in categoriesList)
                    {
                        Categories.Add(category);
                    }
                });
            });

            Log.Information("Loaded {CategoryCount} categories", Categories.Count);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load categories");
            ValidationMessage = "Failed to load categories. Please try again.";
            HasValidationError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Saves a category (insert or update).
    /// </summary>
    [RelayCommand]
    private async Task SaveCategoryAsync(Category category)
    {
        if (category == null) return;

        // Validate
        if (string.IsNullOrWhiteSpace(category.Name))
        {
            ValidationMessage = "Category name is required.";
            HasValidationError = true;
            return;
        }

        try
        {
            await Task.Run(() =>
            {
                // Check for duplicate name (excluding current category)
                if (_categoryRepository.Exists(category.Name, category.Id))
                {
                    _dispatcherQueue.TryEnqueue(() =>
                    {
                        ValidationMessage = $"A category named '{category.Name}' already exists.";
                        HasValidationError = true;
                    });
                    return;
                }

                category.UpdatedAt = DateTime.UtcNow;

                if (category.Id == LiteDB.ObjectId.Empty)
                {
                    // Insert new category
                    category.CreatedAt = DateTime.UtcNow;
                    _categoryRepository.Insert(category);
                    Log.Information("Created new category: {CategoryName}", category.Name);
                }
                else
                {
                    // Update existing category
                    _categoryRepository.Update(category);
                    Log.Information("Updated category: {CategoryName}", category.Name);
                }
            });

            await LoadCategoriesAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save category");
            ValidationMessage = "Failed to save category. Please try again.";
            HasValidationError = true;
        }
    }

    /// <summary>
    /// Deletes a category.
    /// </summary>
    [RelayCommand]
    private async Task DeleteCategoryAsync(Category category)
    {
        if (category == null) return;

        try
        {
            await Task.Run(() =>
            {
                // Check if category is used by any guides
                var guidesInCategory = _guideRepository.GetByCategory(category.Name).ToList();
                if (guidesInCategory.Any())
                {
                    _dispatcherQueue.TryEnqueue(() =>
                    {
                        ValidationMessage = $"Cannot delete category '{category.Name}'. It is used by {guidesInCategory.Count} guide(s).";
                        HasValidationError = true;
                    });
                    return;
                }

                // Delete category
                _categoryRepository.Delete(category.Id);
                Log.Information("Deleted category: {CategoryName}", category.Name);

                _dispatcherQueue.TryEnqueue(() =>
                {
                    Categories.Remove(category);
                });
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to delete category");
            ValidationMessage = "Failed to delete category. Please try again.";
            HasValidationError = true;
        }
    }

    /// <summary>
    /// Creates a new category with default values.
    /// </summary>
    public Category CreateNewCategory()
    {
        return new Category
        {
            Name = "New Category",
            Description = string.Empty,
            IconGlyph = "\uE8F1", // Document icon
            Color = "#0078D4", // Windows blue
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Gets the number of guides using a category.
    /// </summary>
    public int GetGuideCount(string categoryName)
    {
        try
        {
            return _guideRepository.GetByCategory(categoryName).Count();
        }
        catch
        {
            return 0;
        }
    }
}

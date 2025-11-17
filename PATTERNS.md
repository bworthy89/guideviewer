# Development Patterns

This document contains detailed code patterns and examples for common development scenarios in the GuideViewer project.

## Table of Contents
- [MVVM Pattern](#mvvm-pattern)
- [Repository Pattern](#repository-pattern)
- [Dependency Injection](#dependency-injection)
- [Navigation Pattern](#navigation-pattern)
- [Async UI Updates](#async-ui-updates)
- [Value Converters](#value-converters)
- [Search and Filter Pattern](#search-and-filter-pattern)
- [Auto-Save Pattern](#auto-save-pattern)
- [Validation Pattern](#validation-pattern)
- [Adding New Components](#adding-new-components)

---

## MVVM Pattern

Use `CommunityToolkit.Mvvm` source generators for ViewModels.

### Field-based Properties (C# 12 compatible)

```csharp
public partial class ActivationViewModel : ObservableObject
{
    [ObservableProperty]
    private string segment1 = string.Empty; // Generates public Segment1 property

    [ObservableProperty]
    private bool isLoading = false; // Generates public IsLoading property
}
```

### Commands

```csharp
[RelayCommand]
private async Task ActivateAsync()
{
    IsLoading = true;
    // Command logic
    IsLoading = false;
}
// Generates public ActivateCommand : IAsyncRelayCommand
```

### Important Notes
- Use field-based `[ObservableProperty]` (not partial properties) for C# 12 compatibility
- MVVMTK0045 warnings about WinRT/AOT compatibility can be safely ignored for desktop apps
- All ViewModels inherit from `ObservableObject`
- Use `{Binding}` instead of `{x:Bind}` for better WinUI 3 compatibility

---

## Repository Pattern

### Generic Repository Interface

```csharp
public interface IRepository<T>
{
    T? GetById(ObjectId id);
    IEnumerable<T> GetAll();
    IEnumerable<T> Find(Expression<Func<T, bool>> predicate);
    ObjectId Insert(T entity);
    bool Update(T entity);
    bool Delete(ObjectId id);
}
```

### Specialized Repository Example

```csharp
public class GuideRepository : Repository<Guide>
{
    public GuideRepository(DatabaseService databaseService)
        : base(databaseService) { }

    public IEnumerable<Guide> Search(string query)
    {
        return Collection
            .Find(g => g.Title.Contains(query) || g.Description.Contains(query));
    }

    public IEnumerable<Guide> GetByCategory(string category)
    {
        return Collection.Find(g => g.Category == category);
    }

    public IEnumerable<Guide> GetRecentlyModified(int count)
    {
        return Collection
            .Query()
            .OrderByDescending(g => g.UpdatedAt)
            .Limit(count)
            .ToList();
    }
}
```

### Repository Methods by Type
- **UserRepository**: `GetCurrentUser()`, `UpdateLastLogin()`
- **SettingsRepository**: `GetValue(key)`, `SetValue(key, value)`
- **GuideRepository**: `Search(query)`, `GetByCategory(category)`, `GetRecentlyModified(count)`
- **CategoryRepository**: `GetByName(name)`, `Exists(name, excludeId)`, `EnsureCategory(name)`
- **ProgressRepository**: `GetByUserAndGuide()`, `GetActiveByUser()`, `UpdateStepCompletion()`

---

## Dependency Injection

### Service Registration in App.xaml.cs

```csharp
private IServiceProvider ConfigureServices()
{
    var services = new ServiceCollection();

    // Singleton services (shared instance)
    services.AddSingleton<DatabaseService>();
    services.AddSingleton<LicenseValidator>();
    services.AddSingleton<ISettingsService, SettingsService>();
    services.AddSingleton<NavigationService>();
    services.AddSingleton<IImageStorageService, ImageStorageService>();

    // Transient services (new instance each time)
    services.AddTransient<UserRepository>();
    services.AddTransient<GuideRepository>();
    services.AddTransient<CategoryRepository>();
    services.AddTransient<ProgressRepository>();
    services.AddTransient<IAutoSaveService, AutoSaveService>();

    return services.BuildServiceProvider();
}
```

### Accessing Services

```csharp
// From anywhere in the app
var licenseValidator = App.GetService<LicenseValidator>();
var navigationService = App.GetService<NavigationService>();
var guideRepository = App.GetService<GuideRepository>();
```

---

## Navigation Pattern

### Register Pages on Startup

```csharp
var navigationService = App.GetService<NavigationService>();
navigationService.RegisterPage<HomePage>(PageKeys.Home);
navigationService.RegisterPage<GuidesPage>(PageKeys.Guides);
navigationService.RegisterPage<GuideEditorPage>(PageKeys.GuideEditor);
navigationService.RegisterPage<GuideDetailPage>(PageKeys.GuideDetail);
navigationService.RegisterPage<ProgressPage>(PageKeys.Progress);
navigationService.RegisterPage<SettingsPage>(PageKeys.Settings);

navigationService.Frame = ContentFrame; // Set the navigation frame
```

### Navigate to Pages

```csharp
navigationService.NavigateTo(PageKeys.Home);
navigationService.NavigateTo(PageKeys.Guides);
navigationService.NavigateTo(PageKeys.GuideEditor); // Create new guide
navigationService.NavigateTo(PageKeys.GuideEditor, guideId); // Edit existing guide
navigationService.NavigateTo(PageKeys.GuideDetail, guideId); // View guide (read-only)
```

### Page Keys Definition

```csharp
public static class PageKeys
{
    public const string Home = "Home";
    public const string Guides = "Guides";
    public const string GuideEditor = "GuideEditor";
    public const string GuideDetail = "GuideDetail";
    public const string Progress = "Progress";
    public const string Settings = "Settings";
}
```

---

## Async UI Updates

WinUI 3 requires `DispatcherQueue` for updating UI from background threads.

### Inject DispatcherQueue via Constructor

```csharp
public partial class GuidesViewModel : ObservableObject
{
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly GuideRepository _guideRepository;

    public GuidesViewModel(GuideRepository guideRepository, DispatcherQueue dispatcherQueue)
    {
        _guideRepository = guideRepository;
        _dispatcherQueue = dispatcherQueue;
    }

    private async Task LoadDataAsync()
    {
        await Task.Run(() =>
        {
            // Perform work on background thread
            var data = _guideRepository.GetAll().ToList();

            // Update UI on main thread
            _dispatcherQueue.TryEnqueue(() =>
            {
                Items.Clear();
                foreach (var item in data)
                    Items.Add(item);
            });
        });
    }
}
```

### Pass DispatcherQueue from Page

```csharp
// In Page constructor or OnNavigatedTo
ViewModel = new GuidesViewModel(guideRepository, this.DispatcherQueue);
```

**Important**: `App.Current.DispatcherQueue` does NOT exist in WinUI 3 - always use the UI element's DispatcherQueue.

---

## Value Converters

### InverseBooleanConverter

Inverts boolean values (bool → bool):

```xml
<Button IsEnabled="{Binding IsLoading, Converter={StaticResource InverseBooleanConverter}}" />
```

### BooleanToVisibilityConverter

Converts bool to Visibility (True → Visible, False → Collapsed):

```xml
<TextBlock
    Text="Admin Only"
    Visibility="{Binding IsAdmin, Converter={StaticResource BooleanToVisibilityConverter}}" />
```

### InverseBooleanToVisibilityConverter

Converts bool to inverse Visibility (True → Collapsed, False → Visible):

```xml
<TextBlock
    Text="No items found"
    Visibility="{Binding HasGuides, Converter={StaticResource InverseBooleanToVisibilityConverter}}" />
```

### Role-Based UI Visibility

```xml
<NavigationViewItem
    Content="Admin Only Feature"
    Visibility="{Binding IsAdmin, Converter={StaticResource BooleanToVisibilityConverter}}">
    <NavigationViewItem.Icon>
        <FontIcon Glyph="&#xE8F1;"/>
    </NavigationViewItem.Icon>
</NavigationViewItem>
```

---

## Search and Filter Pattern

Pattern used in `GuidesViewModel` for searchable/filterable lists.

```csharp
public partial class GuidesViewModel : ObservableObject
{
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly GuideRepository _guideRepository;

    [ObservableProperty]
    private ObservableCollection<Guide> guides = new();

    [ObservableProperty]
    private string searchQuery = string.Empty;

    [ObservableProperty]
    private Category? selectedCategory;

    // Computed property for Clear button visibility
    public bool HasSearchQuery => !string.IsNullOrWhiteSpace(SearchQuery);

    // Computed property for empty state visibility
    public bool HasGuides => Guides.Count > 0;

    public GuidesViewModel(GuideRepository guideRepository, DispatcherQueue dispatcherQueue)
    {
        _guideRepository = guideRepository;
        _dispatcherQueue = dispatcherQueue;

        // Subscribe to collection changes to update computed properties
        Guides.CollectionChanged += (s, e) => OnPropertyChanged(nameof(HasGuides));
    }

    // Notify UI when SearchQuery changes
    partial void OnSearchQueryChanged(string value)
    {
        OnPropertyChanged(nameof(HasSearchQuery));
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        IsLoading = true;
        try
        {
            await Task.Run(() =>
            {
                // Perform search on background thread
                var results = string.IsNullOrWhiteSpace(SearchQuery)
                    ? _guideRepository.GetAll()
                    : _guideRepository.Search(SearchQuery);

                var resultsList = results.ToList();

                // Update UI on main thread
                _dispatcherQueue.TryEnqueue(() =>
                {
                    Guides.Clear();
                    foreach (var guide in resultsList)
                        Guides.Add(guide);
                });
            });
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ClearSearchAsync()
    {
        SearchQuery = string.Empty;  // Triggers OnSearchQueryChanged
        await SearchAsync();
    }
}
```

### Key Patterns
- Use `ObservableCollection` for UI-bound lists
- Computed properties (`HasGuides`, `HasSearchQuery`) for dynamic visibility
- Subscribe to `CollectionChanged` to notify computed properties
- Use `partial void OnPropertyChanged` to chain notifications
- Offload work to `Task.Run()`, update UI via `DispatcherQueue`
- Set `IsLoading` in try/finally to ensure it's always reset

---

## Auto-Save Pattern

Pattern used in `GuideEditorViewModel` for guide CRUD with auto-save.

```csharp
public partial class GuideEditorViewModel : ObservableObject, IDisposable
{
    private readonly IAutoSaveService _autoSaveService;
    private readonly GuideRepository _guideRepository;
    private readonly object _saveLock = new object(); // Prevent race conditions

    [ObservableProperty]
    private bool hasUnsavedChanges = false;

    [ObservableProperty]
    private DateTime? lastSavedAt;

    public GuideEditorViewModel(
        GuideRepository guideRepository,
        IAutoSaveService autoSaveService)
    {
        _guideRepository = guideRepository;
        _autoSaveService = autoSaveService;

        // Subscribe to property changes to track dirty state
        PropertyChanged += OnPropertyChanged_TrackChanges;
    }

    public async Task InitializeAsync(ObjectId? guideId)
    {
        if (guideId.HasValue)
        {
            // Load existing guide
            await LoadGuideAsync((ObjectId)guideId!);
        }
        else
        {
            // Create new guide
            IsNewGuide = true;
            PageTitle = "New Guide";
        }

        // Start auto-save (every 30 seconds)
        _autoSaveService.StartAutoSave(async () => await AutoSaveAsync(), 30);

        HasUnsavedChanges = false; // Reset after initialization
    }

    private void OnPropertyChanged_TrackChanges(object? sender, PropertyChangedEventArgs e)
    {
        // Track changes for any property except metadata properties
        if (e.PropertyName != nameof(HasUnsavedChanges) &&
            e.PropertyName != nameof(LastSavedAt) &&
            e.PropertyName != nameof(IsLoading) &&
            e.PropertyName != nameof(IsSaving))
        {
            HasUnsavedChanges = true;
            _autoSaveService.IsDirty = true;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        // Prevent concurrent saves with lock
        lock (_saveLock)
        {
            if (IsSaving) return;
            IsSaving = true;
        }

        try
        {
            // Validate before saving
            if (!ValidateGuide())
            {
                IsSaving = false;
                return;
            }

            // Save to repository
            await Task.Run(() =>
            {
                if (IsNewGuide)
                {
                    _guideId = _guideRepository.Insert(guide);
                }
                else
                {
                    _guideRepository.Update(guide);
                }
            });

            HasUnsavedChanges = false;
            LastSavedAt = DateTime.Now;
            _autoSaveService.IsDirty = false;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save guide");
            ValidationMessage = "Failed to save guide";
            HasValidationError = true;
        }
        finally
        {
            IsSaving = false;
        }
    }

    private async Task AutoSaveAsync()
    {
        if (HasUnsavedChanges && !IsSaving)
        {
            await SaveAsync();
        }
    }

    public void Dispose()
    {
        _autoSaveService.StopAutoSave();
        PropertyChanged -= OnPropertyChanged_TrackChanges; // Prevent memory leak
    }
}
```

### Key Patterns
- Use `object _saveLock` to prevent race conditions between manual and auto-save
- Subscribe to `PropertyChanged` with **named method** (not lambda) to prevent memory leaks
- Track dirty state by monitoring all property changes except metadata
- Implement `IDisposable` to stop auto-save and unsubscribe from events
- Use `lock` statement to make save operations thread-safe
- Validate data before saving to prevent invalid data in database

---

## Validation Pattern

Pattern used in `CategoryManagementViewModel` for category CRUD with in-use checking.

```csharp
public partial class CategoryManagementViewModel : ObservableObject
{
    private readonly CategoryRepository _categoryRepository;
    private readonly GuideRepository _guideRepository;
    private readonly DispatcherQueue _dispatcherQueue;

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
                        ValidationMessage = $"Cannot delete category '{category.Name}'. " +
                                          $"It is used by {guidesInCategory.Count} guide(s).";
                        HasValidationError = true;
                    });
                    return;
                }

                // Safe to delete
                _categoryRepository.Delete(category.Id);
                Log.Information("Deleted category: {CategoryName}", category.Name);

                // Update UI
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
}
```

### Key Patterns
- Check referential integrity before deletion (prevent orphaned data)
- Use repository methods to find dependencies
- Show user-friendly error messages with counts
- Update UI on main thread via `DispatcherQueue`
- Log all CRUD operations for debugging

---

## Adding New Components

### Adding a New Service

1. Create interface in `GuideViewer.Core/Services/I{ServiceName}.cs`
2. Implement in `GuideViewer.Core/Services/{ServiceName}.cs`
3. Register in `App.xaml.cs` `ConfigureServices()` method
4. Write unit tests in `GuideViewer.Tests/Services/{ServiceName}Tests.cs`

### Adding a New Entity

1. Create entity in `GuideViewer.Data/Entities/{EntityName}.cs`
2. Add `[BsonId]` attribute to Id property
3. Update `DatabaseService.InitializeCollections()` to add indexes
4. Create repository in `GuideViewer.Data/Repositories/{EntityName}Repository.cs`
5. Register repository in `App.xaml.cs`

### Adding a New Page

1. Create XAML in `GuideViewer/Views/Pages/{PageName}.xaml`
2. Create code-behind in `GuideViewer/Views/Pages/{PageName}.xaml.cs`
3. (Optional) Create ViewModel in `GuideViewer/ViewModels/{PageName}ViewModel.cs`
4. Register page in `MainWindow.xaml.cs`:
   ```csharp
   navigationService.RegisterPage<MyNewPage>("MyNewPageKey");
   ```
5. Add NavigationViewItem to `MainWindow.xaml`:
   ```xml
   <NavigationViewItem Content="My Page" Tag="MyNewPageKey" Icon="Document"/>
   ```

---

## ViewModel Testing in WinUI 3

### Architectural Constraint
WinUI 3 ViewModels (located in the GuideViewer UI project) **cannot** be unit tested using traditional xUnit tests because:

1. **Project Reference Limitation** - Test projects cannot reference WinUI 3 UI projects
2. **DispatcherQueue Dependency** - ViewModels require `DispatcherQueue` which is a WinUI 3 runtime type
3. **UI Thread Requirement** - WinUI 3 components must run on a UI thread with a DispatcherQueue

### Alternative Testing Strategies

**Option 1: Manual UI Testing** (Current Approach)
- Test ViewModels through actual UI interaction in Visual Studio
- Verify functionality with real data and user flows
- Relies on comprehensive service/repository layer tests

**Option 2: Move ViewModels to Separate Project**
- Create `GuideViewer.ViewModels` project
- Reference from both UI and Test projects
- Adds project complexity

**Option 3: UI Automation Tests**
- Use WinAppDriver or similar UI automation
- Test complete user workflows
- Higher maintenance cost

### Recommended Pattern
For this project:
- ✅ **Service/Repository layers**: 188 comprehensive unit tests
- ✅ **ViewModels**: Manual testing via UI + thorough logging
- ✅ **Code quality**: Proper error handling, null validation, DispatcherQueue usage

ViewModels serve as thin presentation layers that coordinate well-tested services. The business logic is tested at the service layer.

---

## Additional Patterns

### Settings Management

```csharp
var settingsService = App.GetService<ISettingsService>();
var settings = settingsService.LoadSettings(); // Cached after first load
settingsService.SetTheme("Dark");
settingsService.SaveWindowState(width, height, x, y, isMaximized);
```

### Product Key Validation

```csharp
var validator = App.GetService<LicenseValidator>();
var licenseInfo = validator.ValidateProductKey(productKey);

if (licenseInfo.IsValid)
{
    var role = licenseInfo.Role; // UserRole.Admin or UserRole.Technician
}
else
{
    var error = licenseInfo.ErrorMessage;
}
```

### Logging

```csharp
Log.Information("Message");
Log.Warning("Warning message");
Log.Error(exception, "Error occurred");
```

### Test Database Cleanup

Always implement `IDisposable` to clean up test databases:

```csharp
public class MyTests : IDisposable
{
    private readonly string _testDatabasePath;
    private readonly DatabaseService _databaseService;

    public MyTests()
    {
        _testDatabasePath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db");
        _databaseService = new DatabaseService(_testDatabasePath);
    }

    public void Dispose()
    {
        _databaseService?.Dispose();
        if (File.Exists(_testDatabasePath))
            File.Delete(_testDatabasePath);
    }
}
```

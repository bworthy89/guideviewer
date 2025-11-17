# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 
Always use context7 when I need code generation, setup or configuration steps, or
library/API documentation. This means you should automatically use the Context7 MCP
tools to resolve library id and get library docs without me having to explicitly ask.

## Project Overview

GuideViewer is a Windows desktop application for service technicians to access and track progress on installation guides. It uses WinUI 3, .NET 8, and follows an offline-first architecture with role-based access control (ADMIN vs TECHNICIAN).

**Target Platform**: Windows 10 1809+ / Windows 11
**Framework**: WinUI 3 (Windows App SDK 1.5+)
**Architecture**: MVVM with dependency injection

**Milestone 1 Status**: ✅ **COMPLETE!** (2025-11-16)
**Milestone 2 Status**: ✅ **COMPLETE!** (2025-11-17) - 100% Complete

## Quick Start

### Running the Application
1. Open `GuideViewer.sln` in **Visual Studio 2022**
2. Build and run (F5)
3. On first run, enter a product key from `TEST_PRODUCT_KEYS.txt`:
   - Admin key: `A04E-02C0-AD82-43C0`
   - Technician key: `TD5A-BB21-A638-C43A`
4. Explore the application with NavigationView

### Testing Different Roles
- **Admin**: See "New Guide" menu item (admin-only)
- **Technician**: "New Guide" menu hidden
- Delete `%LocalAppData%\GuideViewer\data.db` to reset and test with different role

### Running Tests
```bash
dotnet test GuideViewer.Tests/GuideViewer.Tests.csproj
# Expected: 111/111 tests passing (24 Milestone 1 + 75 Milestone 2 unit + 12 Milestone 2 integration)
```

### Sample Data
On first run, the app automatically seeds 5 sample guides in 4 categories:
- Network Installation (2 guides)
- Server Setup (1 guide)
- Software Deployment (1 guide)
- Hardware Maintenance (1 guide)

Delete `%LocalAppData%\GuideViewer\data.db` to reset and re-seed.

## What's New (Latest Session - 2025-11-17)

### Milestone 2 - Phases 4, 5 & 6: Guide Editor + Category Management + Testing ✅ **COMPLETE!**

**Phase 4: Guide Editor UI** (~35% of milestone)
- ✅ **GuideEditorPage** - Full CRUD interface with rich text editing
  - Two-column layout (metadata/steps on left, editor on right)
  - GuideEditorViewModel (~600 lines) with auto-save integration
  - RichEditBox for RTF step instructions
  - Image upload with FileOpenPicker (10MB max, 4 formats)
  - Step reordering with up/down buttons
  - Auto-save every 30 seconds with dirty tracking
  - Unsaved changes warning on navigation
  - Thread-safe image loading with DispatcherQueue
  - Memory leak prevention (named method for PropertyChanged)

- ✅ **7 Critical Bugs Fixed** before testing:
  1. PropertyChanged event memory leak (lambda → named method)
  2. BitmapImage thread safety (added DispatcherQueue checks)
  3. Auto-save race condition (added lock mechanism)
  4. SelectedStep null binding (created fallback property)
  5. RichEditBox stream leak (added using statement)
  6. Image stream position reset
  7. Parameter validation (ObjectId? casting)

**Phase 5: Category Management & Detail View** (~10% of milestone)
- ✅ **CategoryEditorDialog** - Create/edit categories
  - 8 icon choices (Document, Network, Server, Software, Tools, Settings, Phone, Calculator)
  - 7 color choices (Blue, Green, Purple, Red, Orange, Cyan, Gray)
  - Live preview of category badge
  - Validation with duplicate name checking

- ✅ **CategoryManagementViewModel** - Full category CRUD
  - Integrated into SettingsPage with ItemsRepeater
  - Cannot delete categories in use by guides
  - Color badge rendering from hex strings

- ✅ **GuideDetailPage** - Read-only guide viewing
  - All steps displayed with RTF content
  - Category badges with icons and colors
  - Edit button (navigates to editor)
  - Thread-safe image loading

**Files Created** (Phases 4 & 5):
- `GuideEditorPage.xaml` (368 lines), `GuideEditorPage.xaml.cs` (285 lines)
- `GuideEditorViewModel.cs` (~600 lines)
- `CategoryEditorDialog.xaml` (182 lines), `CategoryEditorDialog.xaml.cs` (154 lines)
- `CategoryManagementViewModel.cs` (220 lines)
- `GuideDetailPage.xaml` (209 lines), `GuideDetailPage.xaml.cs` (194 lines)
- Updated `SettingsPage.xaml` with category management UI

**Phase 6: Testing & Polish** (~10% of milestone)
- ✅ **Integration Tests** - 12 new comprehensive tests
  - `GuideWorkflowIntegrationTests.cs` (6 tests)
    * Complete guide CRUD workflow with images
    * Multi-guide category filtering
    * Category deletion prevention with existing guides
    * Multiple images per guide handling
    * Recently modified ordering
  - `CategoryManagementIntegrationTests.cs` (6 tests)
    * Complete category lifecycle
    * Category-guide association validation
    * Duplicate name detection
    * Multi-category organization
    * Case-insensitive lookups

- ✅ **Database Query Optimization**
  - Optimized `GetRecentlyModified` to use UpdatedAt index properly
  - Changed from `Query.All(Query.Descending)` + LINQ to `Query.All("UpdatedAt", Query.Descending)`
  - Performance improvement for retrieving recent guides

**Files Created** (Phase 6):
- `GuideWorkflowIntegrationTests.cs` (290 lines)
- `CategoryManagementIntegrationTests.cs` (260 lines)

**Issues Found & Fixed**:
- **Phase 4**: 12 issues (7 critical bugs + 3 compilation + 2 runtime)
- **Phase 5**: 2 compilation errors (TextSetOptions namespace, ImageStorageService usage)
- **Phase 6**: Database query optimization (1 performance improvement)

**Total Progress**: 111/111 tests passing (24 Milestone 1 + 75 Milestone 2 unit + 12 Milestone 2 integration), 100% of Milestone 2 complete ✅

See `todo.md` for detailed issue documentation and `CLAUDE.md` sections below for architecture details.

## Build & Development Commands

### Building
```bash
# Build entire solution
dotnet build GuideViewer.sln

# Build specific projects
dotnet build GuideViewer.Core/GuideViewer.Core.csproj
dotnet build GuideViewer.Data/GuideViewer.Data.csproj
dotnet build GuideViewer.Tests/GuideViewer.Tests.csproj

# Note: WinUI 3 UI project (GuideViewer.csproj) requires Visual Studio 2022
# The XAML compiler does not work reliably with dotnet CLI
```

### Testing
```bash
# Run all tests
dotnet test GuideViewer.Tests/GuideViewer.Tests.csproj

# Run tests with detailed output
dotnet test GuideViewer.Tests/GuideViewer.Tests.csproj --verbosity detailed

# Run specific test class
dotnet test --filter "FullyQualifiedName~LicenseValidatorTests"

# Run specific test method
dotnet test --filter "FullyQualifiedName~LicenseValidatorTests.ValidateProductKey_WithGeneratedKey_ReturnsValidWithCorrectRole"
```

### Package Management
```bash
# Restore NuGet packages
dotnet restore GuideViewer.sln

# Add package to specific project
dotnet add GuideViewer.Core/GuideViewer.Core.csproj package <PackageName>
```

## Solution Architecture

### Project Structure
```
GuideViewer.sln
├── GuideViewer/              # WinUI 3 application (presentation layer)
│   ├── Converters/          # XAML value converters
│   ├── Services/            # UI services (NavigationService)
│   ├── ViewModels/          # MVVM ViewModels
│   └── Views/               # XAML views and pages
│       ├── Pages/           # Application pages
│       └── ActivationWindow # First-run activation
├── GuideViewer.Core/         # Business logic and services
├── GuideViewer.Data/         # Data access layer (LiteDB)
├── GuideViewer.Tests/        # xUnit tests
└── KeyGenerator/             # Utility for generating product keys
```

### Layered Architecture

**GuideViewer (UI Layer)**
- WinUI 3 XAML views and code-behind
- ViewModels using CommunityToolkit.Mvvm:
  - `ActivationViewModel` - Product key activation
  - `MainViewModel` - Main window and navigation
  - `GuidesViewModel` - Guide list with search/filter
  - `GuideEditorViewModel` - Guide CRUD with auto-save (~600 lines)
  - `CategoryManagementViewModel` - Category CRUD
- Pages:
  - `HomePage` - Landing page
  - `GuidesPage` - Guide list with search/filter (full implementation)
  - `GuideEditorPage` - Guide creation/editing with RTF and images (full implementation)
  - `GuideDetailPage` - Read-only guide viewing (full implementation)
  - `ProgressPage` - Progress tracking (placeholder)
  - `SettingsPage` - Settings with category management (full implementation)
- Dialogs:
  - `CategoryEditorDialog` - Create/edit categories with icon/color picker
- Services: `NavigationService` for page routing
- Converters: `InverseBooleanConverter`, `BooleanToVisibilityConverter`, `InverseBooleanToVisibilityConverter`
- Windows: `ActivationWindow` (first-run), `MainWindow` (NavigationView + Mica)
- Dependency injection configured in App.xaml.cs
- Service access via `App.GetService<T>()`
- DispatcherQueue injection for async UI updates

**GuideViewer.Core (Business Logic)**
- Services: `LicenseValidator`, `ISettingsService`/`SettingsService`, `IImageStorageService`/`ImageStorageService`, `IAutoSaveService`/`AutoSaveService`
- Models: `UserRole`, `LicenseInfo`, `AppSettings`, `ImageValidationResult`, `ImageMetadata`
- Utilities: `ProductKeyGenerator`, `SampleDataSeeder`
- **No dependencies on Data or UI layers**

**GuideViewer.Data (Data Access)**
- `DatabaseService`: LiteDB initialization and management (including FileStorage for images)
- Repository pattern: `IRepository<T>`, `Repository<T>`, `UserRepository`, `SettingsRepository`, `GuideRepository`, `CategoryRepository`
- Entities: `User`, `AppSetting`, `Guide`, `Step`, `Category`
- Indexes on Guide.Title, Guide.Category, Guide.UpdatedAt, Category.Name (unique)
- **No dependencies on Core or UI layers**

**Dependency Flow**: UI → Core → Data (strict layering enforced)

## Key Architectural Patterns

### Dependency Injection
Services are registered in `App.xaml.cs`:
- **Singleton**: `DatabaseService`, `LicenseValidator`, `ISettingsService`, `NavigationService`, `ImageStorageService`
- **Transient**: `UserRepository`, `SettingsRepository`, `GuideRepository`, `CategoryRepository`, `AutoSaveService`

Access services anywhere:
```csharp
var licenseValidator = App.GetService<LicenseValidator>();
var navigationService = App.GetService<NavigationService>();
```

### Repository Pattern
Generic repository for all entities:
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

Specialized repositories inherit from `Repository<T>`:
- `UserRepository`: Adds `GetCurrentUser()`, `UpdateLastLogin()`
- `SettingsRepository`: Adds `GetValue(key)`, `SetValue(key, value)`
- `GuideRepository`: Adds `Search(query)`, `GetByCategory(category)`, `GetRecentlyModified(count)`, `GetDistinctCategories()`
- `CategoryRepository`: Adds `GetByName(name)`, `Exists(name, excludeId)`, `InsertIfNotExists(name)`, `EnsureCategory(name)`

### MVVM Pattern
Use `CommunityToolkit.Mvvm` source generators for ViewModels:

**Field-based Properties** (C# 12 compatible):
```csharp
public partial class ActivationViewModel : ObservableObject
{
    [ObservableProperty]
    private string segment1 = string.Empty; // Generates public Segment1 property

    [ObservableProperty]
    private bool isLoading = false; // Generates public IsLoading property
}
```

**Commands**:
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

**Important Notes**:
- Use field-based `[ObservableProperty]` (not partial properties) for C# 12 compatibility
- MVVMTK0045 warnings about WinRT/AOT compatibility can be safely ignored for desktop apps
- All ViewModels inherit from `ObservableObject`
- Use `{Binding}` instead of `{x:Bind}` for better WinUI 3 compatibility

### Navigation Pattern
Frame-based navigation using `NavigationService`:

**Register pages on startup**:
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

**Navigate to a page**:
```csharp
navigationService.NavigateTo(PageKeys.Home);
navigationService.NavigateTo(PageKeys.Guides);
navigationService.NavigateTo(PageKeys.GuideEditor); // Create new guide
navigationService.NavigateTo(PageKeys.GuideEditor, guideId); // Edit existing guide
navigationService.NavigateTo(PageKeys.GuideDetail, guideId); // View guide (read-only)
```

**Page keys are defined in `PageKeys` class**:
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

### Value Converters
XAML converters registered in `App.xaml`:

**InverseBooleanConverter**: Inverts boolean values (bool → bool)
```xml
IsEnabled="{Binding IsLoading, Converter={StaticResource InverseBooleanConverter}}"
```

**BooleanToVisibilityConverter**: Converts bool to Visibility (True → Visible, False → Collapsed)
```xml
Visibility="{Binding IsAdmin, Converter={StaticResource BooleanToVisibilityConverter}}"
```

**InverseBooleanToVisibilityConverter**: Converts bool to inverse Visibility (True → Collapsed, False → Visible)
```xml
Visibility="{Binding HasGuides, Converter={StaticResource InverseBooleanToVisibilityConverter}}"
```

### Async UI Updates with DispatcherQueue
WinUI 3 requires `DispatcherQueue` for updating UI from background threads. Inject via constructor:

```csharp
public partial class GuidesViewModel : ObservableObject
{
    private readonly DispatcherQueue _dispatcherQueue;

    public GuidesViewModel(..., DispatcherQueue dispatcherQueue)
    {
        _dispatcherQueue = dispatcherQueue;
    }

    private async Task LoadDataAsync()
    {
        await Task.Run(() =>
        {
            var data = _repository.GetAll().ToList();

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

Pass `Page.DispatcherQueue` to ViewModel:
```csharp
ViewModel = new GuidesViewModel(..., this.DispatcherQueue);
```

**Important**: `App.Current.DispatcherQueue` does NOT exist in WinUI 3 - use UI element's DispatcherQueue instead.

## Data Storage

### LiteDB Database
**Location**: `%LocalAppData%\GuideViewer\data.db`
**Collections**:
- `users`: User authentication and role information
- `settings`: Application settings (JSON serialized)
- `guides`: Installation guides with embedded steps (✅ implemented)
- `categories`: Guide categories with icons and colors (✅ implemented)
- `progress`: User progress tracking (to be implemented in Milestone 3)

**FileStorage**: Images stored in LiteDB FileStorage (10MB max per image)
- File ID format: `img_{Guid}`
- Supported formats: PNG, JPG, JPEG, BMP
- Automatically deleted when parent guide is deleted

### Settings Management
Settings are JSON-serialized and cached in memory:
```csharp
var settingsService = App.GetService<ISettingsService>();
var settings = settingsService.LoadSettings(); // Cached after first load
settingsService.SetTheme("Dark");
settingsService.SaveWindowState(width, height, x, y, isMaximized);
```

## Authentication & Licensing

### Product Key Format
`XXXX-XXXX-XXXX-XXXX` (16 characters, uppercase alphanumeric)

**Structure**:
- First character: Role prefix (`A` = Admin, `T` = Technician)
- Characters 1-11: Payload
- Characters 12-15: HMAC-SHA256 checksum (first 4 chars)

**Secret Salt**: Defined in `LicenseValidator.cs` (change in production)

### Generating Product Keys

**Method 1: Using KeyGenerator utility** (Recommended):
```bash
dotnet run --project KeyGenerator/KeyGenerator.csproj
```
Generates 5 admin and 5 technician keys and displays them.

**Method 2: Programmatically**:
```csharp
var validator = new LicenseValidator();
var adminKey = validator.GenerateProductKey(UserRole.Admin);
var techKey = validator.GenerateProductKey(UserRole.Technician);
```

**Method 3: Using ProductKeyGenerator utility**:
```csharp
ProductKeyGenerator.PrintKeys(adminCount: 5, techCount: 5);
```

**Test Keys Available**: See `TEST_PRODUCT_KEYS.txt` for pre-generated keys.

### Validating Product Keys
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

## Logging

**Framework**: Serilog
**Location**: `%LocalAppData%\GuideViewer\logs\app-*.log`
**Retention**: 7 days (rolling daily)
**Level**: Information

Access logger:
```csharp
Log.Information("Message");
Log.Error(exception, "Error occurred");
```

## Testing Strategy

**Framework**: xUnit, FluentAssertions, Moq
**Target Coverage**: 80%+ for Core project
**Current Status**: 24 tests passing

### Test Organization
```
GuideViewer.Tests/
└── Services/
    ├── LicenseValidatorTests.cs    # 11 tests
    └── SettingsServiceTests.cs      # 13 tests
```

### Test Database
Tests use temporary SQLite databases created in `%TEMP%`:
```csharp
_testDatabasePath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db");
_databaseService = new DatabaseService(_testDatabasePath);
```

Always implement `IDisposable` to clean up test databases:
```csharp
public void Dispose()
{
    _databaseService?.Dispose();
    if (File.Exists(_testDatabasePath))
        File.Delete(_testDatabasePath);
}
```

## WinUI 3 Specifics

### XAML Compilation
The WinUI 3 XAML compiler (`XamlCompiler.exe`) does not work reliably via `dotnet build` CLI. **Always use Visual Studio 2022** to build the GuideViewer UI project.

**Important WinUI 3 vs WPF differences**:
- Window properties like `MinWidth`, `MinHeight`, `Title` don't exist in WinUI 3
- Use `AppWindow.Resize()` API instead for window sizing
- Use `{Binding}` instead of `{x:Bind}` to avoid compilation issues
- Set DataContext on `FrameworkElement` (cast from `this.Content`)

### Windows App SDK Deployment
For development, the project uses **unpackaged deployment**:

```xml
<WindowsPackageType>None</WindowsPackageType>
<WindowsAppSDKSelfContained>true</WindowsAppSDKSelfContained>
```

This bundles the Windows App SDK runtime DLLs with the application, eliminating the need for separate runtime installation during development.

### Mica Background Material
MainWindow implements Windows 11 Mica backdrop:

```csharp
private MicaController? m_micaController;
private SystemBackdropConfiguration? m_configurationSource;

private bool TrySetMicaBackdrop()
{
    if (MicaController.IsSupported())
    {
        m_micaController = new MicaController();
        m_micaController.AddSystemBackdropTarget(this.As<ICompositionSupportsSystemBackdrop>());
        m_configurationSource = new SystemBackdropConfiguration();
        m_micaController.SetSystemBackdropConfiguration(m_configurationSource);
        return true;
    }
    return false;
}
```

**Graceful fallback** if Mica is not supported (Windows 10).

### Fluent Design Requirements
- ✅ Windows 11 Fluent Design System components
- ✅ Mica background material for main window (implemented)
- ✅ NavigationView for navigation
- ✅ Minimum touch targets: 44x44 pixels
- ✅ Support both light and dark themes

### Performance Targets
- Startup time: <2 seconds
- Guide list load: <500ms
- Step navigation: <100ms
- Memory usage: <150MB

## Common Development Patterns

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

### Implementing Role-Based UI Visibility
Use the `IsAdmin` property from `MainViewModel` with `BooleanToVisibilityConverter`:

```xml
<NavigationViewItem
    Content="Admin Only Feature"
    Visibility="{Binding IsAdmin, Converter={StaticResource BooleanToVisibilityConverter}}">
    <NavigationViewItem.Icon>
        <FontIcon Glyph="&#xE8F1;"/>
    </NavigationViewItem.Icon>
</NavigationViewItem>
```

The menu item automatically shows/hides based on user role.

### Implementing Search and Filter in ViewModels
Pattern used in `GuidesViewModel` for searchable/filterable lists:

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

    public GuidesViewModel(..., DispatcherQueue dispatcherQueue)
    {
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

**Key Patterns**:
- Use `ObservableCollection` for UI-bound lists
- Computed properties (`HasGuides`, `HasSearchQuery`) for dynamic visibility
- Subscribe to `CollectionChanged` to notify computed properties
- Use `partial void OnPropertyChanged` to chain notifications
- Offload work to `Task.Run()`, update UI via `DispatcherQueue`
- Set `IsLoading` in try/finally to ensure it's always reset

### Implementing Guide Editor with Auto-Save
Pattern used in `GuideEditorViewModel` for guide CRUD with auto-save:

```csharp
public partial class GuideEditorViewModel : ObservableObject, IDisposable
{
    private readonly IAutoSaveService _autoSaveService;
    private readonly object _saveLock = new object(); // Prevent race conditions

    [ObservableProperty]
    private bool hasUnsavedChanges = false;

    [ObservableProperty]
    private DateTime? lastSavedAt;

    public GuideEditorViewModel(IAutoSaveService autoSaveService, ...)
    {
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

**Key Patterns**:
- Use `object _saveLock` to prevent race conditions between manual and auto-save
- Subscribe to `PropertyChanged` with **named method** (not lambda) to prevent memory leaks
- Track dirty state by monitoring all property changes except metadata
- Implement `IDisposable` to stop auto-save and unsubscribe from events
- Use `lock` statement to make save operations thread-safe
- Validate data before saving to prevent invalid data in database

### Implementing Category Management with Validation
Pattern used in `CategoryManagementViewModel` for category CRUD with in-use checking:

```csharp
public partial class CategoryManagementViewModel : ObservableObject
{
    private readonly CategoryRepository _categoryRepository;
    private readonly GuideRepository _guideRepository;

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

**Key Patterns**:
- Check referential integrity before deletion (prevent orphaned data)
- Use `_guideRepository.GetByCategory()` to find dependencies
- Show user-friendly error messages with counts
- Update UI on main thread via `DispatcherQueue`
- Log all CRUD operations for debugging

## Project Status

**Milestone 1 (Foundation)**: ✅ **COMPLETE!** (2025-11-16)

### Completed Features
- ✅ Project structure and dependencies (4 projects + KeyGenerator)
- ✅ Data layer with LiteDB (DatabaseService, Repositories, Entities)
- ✅ Product key validation with HMAC-SHA256 (11 tests passing)
- ✅ Settings service with JSON persistence (13 tests passing)
- ✅ Dependency injection fully configured
- ✅ Logging with Serilog to %LocalAppData%
- ✅ 24/24 unit tests passing, 80%+ code coverage
- ✅ **ActivationWindow** - Complete first-run experience
  - 4-segment product key input with auto-advance
  - Paste support (auto-splits keys)
  - Keyboard navigation (Tab, Enter, Backspace)
  - Error handling with InfoBar
  - Loading states with ProgressRing
- ✅ **MainWindow** - Full NavigationView implementation
  - NavigationView with 4 pages (Home, Guides, Progress, Settings)
  - Admin-only "New Guide" menu item (role-based visibility)
  - User role badge in navigation footer
  - Mica background material (Windows 11 Fluent Design)
  - NavigationService with Frame-based routing
- ✅ **Role-based access control** - Tested with Admin + Technician roles
- ✅ **Windows App SDK deployment** - Unpackaged mode for development
- ✅ **WinUI 3 build issues resolved** - All compilation errors fixed

### Test Results
- ✅ Admin role: "New Guide" menu visible
- ✅ Technician role: "New Guide" menu hidden
- ✅ All pages navigate correctly
- ✅ User role persisted across app restarts
- ✅ No crashes during normal operation

**Milestone 2 (Guide Data Model & Admin CRUD)**: ✅ **COMPLETE!** - 100% Complete (2025-11-17)

### Completed Features (Phases 1-6) ✅
- ✅ **Data Layer** (Phase 1) - Complete guide management infrastructure
  - Guide, Step, Category entities with LiteDB persistence
  - GuideRepository with search, filter, GetByCategory, GetRecentlyModified
  - CategoryRepository with uniqueness enforcement and EnsureCategory
  - 33 repository tests passing (19 Guide + 14 Category)

- ✅ **Services Layer** (Phase 2) - Image storage and auto-save
  - ImageStorageService with 10MB max, PNG/JPG/JPEG/BMP validation
  - AutoSaveService with configurable intervals and dirty tracking
  - 42 service tests passing (26 ImageStorage + 16 AutoSave)

- ✅ **Guide List UI** (Phase 3) - Modern card-based interface
  - Search by title, description, or category
  - Category filtering with "All Categories" option
  - Responsive grid layout (ItemsRepeater with UniformGridLayout)
  - Role-based Edit/Delete/View buttons (admin only for Edit/Delete)
  - Delete confirmation flyout
  - Loading states and contextual empty states
  - Sample data seeding (5 guides, 4 categories)
  - GuidesViewModel with async search/filter logic

- ✅ **Guide Editor UI** (Phase 4) - Full CRUD with rich text editing
  - GuideEditorPage with two-column layout (368 lines XAML)
  - GuideEditorViewModel with auto-save integration (~600 lines)
  - RichEditBox for RTF step instructions
  - Image upload with FileOpenPicker and validation
  - Step reordering with up/down buttons
  - Auto-save every 30 seconds with dirty tracking and lock mechanism
  - Unsaved changes warning with ContentDialog
  - Thread-safe image loading with DispatcherQueue
  - 7 critical bugs fixed before testing (memory leaks, race conditions, null bindings)

- ✅ **Category Management & Detail View** (Phase 5)
  - CategoryEditorDialog with 8 icons and 7 colors
  - CategoryManagementViewModel integrated into SettingsPage
  - Live preview of category badge
  - Cannot delete categories in use by guides
  - GuideDetailPage for read-only guide viewing
  - All steps displayed with RTF content and images
  - Edit button navigates to GuideEditorPage

- ✅ **Testing & Polish** (Phase 6) - Integration tests and optimization
  - 12 integration tests for complete guide CRUD workflow
  - Database query optimization (GetRecentlyModified)
  - All 111 tests passing
  - Documentation fully updated

**Testing Results**:
- ✅ 111/111 tests passing (24 Milestone 1 + 75 Milestone 2 unit + 12 Milestone 2 integration)
- ✅ All CRUD operations working correctly
- ✅ Role-based UI visibility verified (admin vs technician)
- ✅ 15 issues found and fixed during Phases 4, 5 & 6
- ✅ Build successful, all features tested in Visual Studio
- ✅ Integration tests validate end-to-end workflows
- ✅ Database queries optimized for performance

## Important Files

### Documentation
- `spec.md`: Complete product specification with requirements
- `todo.md`: Milestone 2 task list with detailed progress (100% complete ✅)
- `CLAUDE.md`: This file - development guide and codebase documentation
- `TEST_PRODUCT_KEYS.txt`: 10 test product keys (5 admin, 5 tech)

### Core Business Logic
- `GuideViewer.Core/Services/LicenseValidator.cs`: Product key validation (HMAC-SHA256)
- `GuideViewer.Core/Services/ISettingsService.cs` + `SettingsService.cs`: Settings persistence
- `GuideViewer.Core/Services/IImageStorageService.cs` + `ImageStorageService.cs`: Image management (LiteDB FileStorage)
- `GuideViewer.Core/Services/IAutoSaveService.cs` + `AutoSaveService.cs`: Auto-save mechanism
- `GuideViewer.Core/Models/UserRole.cs`: Admin/Technician enum
- `GuideViewer.Core/Models/LicenseInfo.cs`: License validation result
- `GuideViewer.Core/Models/AppSettings.cs`: Settings model
- `GuideViewer.Core/Models/ImageValidationResult.cs`: Image validation result
- `GuideViewer.Core/Models/ImageMetadata.cs`: Image metadata model
- `GuideViewer.Core/Utilities/ProductKeyGenerator.cs`: Key generation utility
- `GuideViewer.Core/Utilities/SampleDataSeeder.cs`: Sample data generation

### Data Layer
- `GuideViewer.Data/Services/DatabaseService.cs`: LiteDB initialization + indexes
- `GuideViewer.Data/Repositories/Repository.cs`: Generic repository base
- `GuideViewer.Data/Repositories/UserRepository.cs`: User data access
- `GuideViewer.Data/Repositories/SettingsRepository.cs`: Settings data access
- `GuideViewer.Data/Repositories/GuideRepository.cs`: Guide CRUD + search + filter
- `GuideViewer.Data/Repositories/CategoryRepository.cs`: Category CRUD + uniqueness
- `GuideViewer.Data/Entities/User.cs`: User entity
- `GuideViewer.Data/Entities/AppSetting.cs`: Setting entity
- `GuideViewer.Data/Entities/Guide.cs`: Guide entity with embedded steps
- `GuideViewer.Data/Entities/Step.cs`: Step entity (embedded in Guide)
- `GuideViewer.Data/Entities/Category.cs`: Category entity

### UI Layer
- `GuideViewer/App.xaml.cs`: DI configuration, logging, first-run detection, sample data seeding
- `GuideViewer/App.xaml`: Application resources, value converters
- `GuideViewer/MainWindow.xaml`: NavigationView + Mica background
- `GuideViewer/MainWindow.xaml.cs`: Navigation setup, Mica implementation, page registration
- `GuideViewer/Views/ActivationWindow.xaml`: Product key entry UI
- `GuideViewer/Views/ActivationWindow.xaml.cs`: Activation logic

**Pages**:
- `GuideViewer/Views/Pages/HomePage.xaml`: Landing page
- `GuideViewer/Views/Pages/GuidesPage.xaml`: Full guide list with search/filter (252 lines)
- `GuideViewer/Views/Pages/GuidesPage.xaml.cs`: Event handlers for search, filter, delete
- `GuideViewer/Views/Pages/GuideEditorPage.xaml`: Guide editor with RTF and images (368 lines)
- `GuideViewer/Views/Pages/GuideEditorPage.xaml.cs`: File picker, RTF loading, image loading (285 lines)
- `GuideViewer/Views/Pages/GuideDetailPage.xaml`: Read-only guide viewing (209 lines)
- `GuideViewer/Views/Pages/GuideDetailPage.xaml.cs`: Guide loading, RTF rendering (194 lines)
- `GuideViewer/Views/Pages/ProgressPage.xaml`: Progress dashboard (placeholder)
- `GuideViewer/Views/Pages/SettingsPage.xaml`: Settings with category management
- `GuideViewer/Views/Pages/SettingsPage.xaml.cs`: Category CRUD event handlers

**Dialogs**:
- `GuideViewer/Views/Dialogs/CategoryEditorDialog.xaml`: Category editor with icon/color picker (182 lines)
- `GuideViewer/Views/Dialogs/CategoryEditorDialog.xaml.cs`: Live preview, validation (154 lines)

**ViewModels**:
- `GuideViewer/ViewModels/ActivationViewModel.cs`: Activation logic
- `GuideViewer/ViewModels/MainViewModel.cs`: Main window + role detection
- `GuideViewer/ViewModels/GuidesViewModel.cs`: Guide list with search/filter logic (409 lines)
- `GuideViewer/ViewModels/GuideEditorViewModel.cs`: Guide CRUD with auto-save (~600 lines)
- `GuideViewer/ViewModels/CategoryManagementViewModel.cs`: Category CRUD (220 lines)

**Services & Converters**:
- `GuideViewer/Services/NavigationService.cs`: Page navigation with PageKeys
- `GuideViewer/Converters/InverseBooleanConverter.cs`: Boolean inversion
- `GuideViewer/Converters/BooleanToVisibilityConverter.cs`: Bool → Visibility
- `GuideViewer/Converters/InverseBooleanToVisibilityConverter.cs`: Bool → Inverse Visibility

### Testing
**Unit Tests** (99 tests):
- `GuideViewer.Tests/Services/LicenseValidatorTests.cs`: 11 tests
- `GuideViewer.Tests/Services/SettingsServiceTests.cs`: 13 tests
- `GuideViewer.Tests/Services/ImageStorageServiceTests.cs`: 26 tests
- `GuideViewer.Tests/Services/AutoSaveServiceTests.cs`: 16 tests
- `GuideViewer.Tests/Repositories/GuideRepositoryTests.cs`: 19 tests
- `GuideViewer.Tests/Repositories/CategoryRepositoryTests.cs`: 14 tests

**Integration Tests** (12 tests):
- `GuideViewer.Tests/Integration/GuideWorkflowIntegrationTests.cs`: 6 tests (complete guide CRUD workflow)
- `GuideViewer.Tests/Integration/CategoryManagementIntegrationTests.cs`: 6 tests (category management with validation)

### Utilities
- `KeyGenerator/Program.cs`: Console app for generating product keys

## References

- [WinUI 3 Documentation](https://learn.microsoft.com/en-us/windows/apps/winui/winui3/)
- [LiteDB Documentation](https://www.litedb.org/)
- [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
- [Serilog](https://serilog.net/)

# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

GuideViewer is a Windows desktop application for service technicians to access and track progress on installation guides. It uses WinUI 3, .NET 8, and follows an offline-first architecture with role-based access control (ADMIN vs TECHNICIAN).

**Target Platform**: Windows 10 1809+ / Windows 11
**Framework**: WinUI 3 (Windows App SDK 1.5+)
**Architecture**: MVVM with dependency injection

**Milestone 1 Status**: ✅ **COMPLETE!** (2025-11-16)
**Milestone 2 Status**: 🔵 **IN PROGRESS** - 45% Complete (Phase 3 done)

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
# Expected: 75/75 tests passing (24 Milestone 1 + 51 Milestone 2)
```

### Sample Data
On first run, the app automatically seeds 5 sample guides in 4 categories:
- Network Installation (2 guides)
- Server Setup (1 guide)
- Software Deployment (1 guide)
- Hardware Maintenance (1 guide)

Delete `%LocalAppData%\GuideViewer\data.db` to reset and re-seed.

## What's New (Latest Session - 2025-11-16)

### Milestone 2 - Phase 3: Guide List UI ✅ **COMPLETE!**

**Major Achievements**:
- ✅ **Data Layer** - Complete guide management infrastructure
  - `Guide`, `Step`, `Category` entities with LiteDB persistence
  - `GuideRepository` with search, filter, and category queries
  - `CategoryRepository` with uniqueness enforcement
  - 33 repository tests passing (19 Guide + 14 Category)

- ✅ **Services Layer** - Image storage and auto-save
  - `ImageStorageService` with validation (10MB max, PNG/JPG/JPEG/BMP)
  - `AutoSaveService` with configurable intervals and dirty tracking
  - 42 service tests passing (26 ImageStorage + 16 AutoSave)

- ✅ **Guide List UI** - Modern card-based interface
  - Search by title, description, or category
  - Category filtering with "All Categories" option
  - Responsive grid layout (1-3 columns, ItemsRepeater)
  - Role-based visibility (Edit/Delete for admins only)
  - Delete confirmation flyout
  - Loading states and contextual empty states
  - Sample data seeding utility

**Files Created** (Milestone 2):
- **Entities**: `Guide.cs`, `Step.cs`, `Category.cs`
- **Repositories**: `GuideRepository.cs`, `CategoryRepository.cs`
- **Services**: `ImageStorageService.cs`, `AutoSaveService.cs`
- **ViewModels**: `GuidesViewModel.cs` (393 lines)
- **Pages**: Updated `GuidesPage.xaml` (252 lines), `GuidesPage.xaml.cs`
- **Utilities**: `SampleDataSeeder.cs`
- **Tests**: `GuideRepositoryTests.cs`, `CategoryRepositoryTests.cs`, `ImageStorageServiceTests.cs`, `AutoSaveServiceTests.cs`
- **Converters**: `InverseBooleanToVisibilityConverter.cs`

**Issues Found & Fixed During Testing**:
1. ✅ DispatcherQueue access error (WinUI 3 limitation) - Fixed with DI injection
2. ✅ Clear button not appearing - Fixed with `HasSearchQuery` computed property
3. ✅ Delete flyout issues - Added cancel handler, increased width
4. ✅ Empty state always visible - Fixed with `InverseBooleanToVisibilityConverter` + collection change notification

**Total Progress**: 75/75 tests passing, 45% of Milestone 2 complete

See `todo.md` for detailed phase breakdown and `CLAUDE.md` sections below for architecture details.

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
- ViewModels using CommunityToolkit.Mvvm (`ActivationViewModel`, `MainViewModel`, `GuidesViewModel`)
- Pages: `HomePage`, `GuidesPage` (full implementation), `ProgressPage`, `SettingsPage`
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
navigationService.RegisterPage<ProgressPage>(PageKeys.Progress);
navigationService.RegisterPage<SettingsPage>(PageKeys.Settings);

navigationService.Frame = ContentFrame; // Set the navigation frame
```

**Navigate to a page**:
```csharp
navigationService.NavigateTo(PageKeys.Home);
navigationService.NavigateTo(PageKeys.Guides);
```

**Page keys are defined in `PageKeys` class**:
```csharp
public static class PageKeys
{
    public const string Home = "Home";
    public const string Guides = "Guides";
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

**Milestone 2 (Guide Data Model & Admin CRUD)**: 🔵 **IN PROGRESS** - 45% Complete

### Completed Features (Phases 1-3)
- ✅ **Data Layer** - Complete guide management infrastructure
  - Guide, Step, Category entities with LiteDB persistence
  - GuideRepository with search, filter, GetByCategory, GetRecentlyModified
  - CategoryRepository with uniqueness enforcement and EnsureCategory
  - 33 repository tests passing (19 Guide + 14 Category)

- ✅ **Services Layer** - Image storage and auto-save
  - ImageStorageService with 10MB max, PNG/JPG/JPEG/BMP validation
  - AutoSaveService with configurable intervals and dirty tracking
  - 42 service tests passing (26 ImageStorage + 16 AutoSave)

- ✅ **Guide List UI** - Modern card-based interface
  - Search by title, description, or category
  - Category filtering with "All Categories" option
  - Responsive grid layout (ItemsRepeater with UniformGridLayout)
  - Role-based Edit/Delete buttons (admin only)
  - Delete confirmation flyout
  - Loading states and contextual empty states
  - Sample data seeding (5 guides, 4 categories)
  - GuidesViewModel with async search/filter logic

**Testing Results**:
- ✅ 75/75 unit tests passing
- ✅ Search and filter working correctly
- ✅ Role-based UI visibility verified (admin vs technician)
- ✅ 4 runtime issues found and fixed during testing
- ✅ Sample data seeds on first run

### Remaining Work (Phases 4-6)
- [ ] Phase 4: Guide Editor UI with RichEditBox (~35%)
- [ ] Phase 5: Category Management & Detail View (~10%)
- [ ] Phase 6: Testing & Polish (~10%)

## Important Files

### Documentation
- `spec.md`: Complete product specification with requirements
- `todo.md`: Milestone 2 task list with detailed progress (45% complete)
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
- `GuideViewer/MainWindow.xaml.cs`: Navigation setup, Mica implementation
- `GuideViewer/Views/ActivationWindow.xaml`: Product key entry UI
- `GuideViewer/Views/ActivationWindow.xaml.cs`: Activation logic
- `GuideViewer/Views/Pages/HomePage.xaml`: Landing page
- `GuideViewer/Views/Pages/GuidesPage.xaml`: Full guide list with search/filter (252 lines)
- `GuideViewer/Views/Pages/GuidesPage.xaml.cs`: Event handlers for search, filter, delete
- `GuideViewer/Views/Pages/ProgressPage.xaml`: Progress dashboard
- `GuideViewer/Views/Pages/SettingsPage.xaml`: Settings page
- `GuideViewer/ViewModels/ActivationViewModel.cs`: Activation logic
- `GuideViewer/ViewModels/MainViewModel.cs`: Main window + role detection
- `GuideViewer/ViewModels/GuidesViewModel.cs`: Guide list with search/filter logic (409 lines)
- `GuideViewer/Services/NavigationService.cs`: Page navigation
- `GuideViewer/Converters/InverseBooleanConverter.cs`: Boolean inversion
- `GuideViewer/Converters/BooleanToVisibilityConverter.cs`: Bool → Visibility
- `GuideViewer/Converters/InverseBooleanToVisibilityConverter.cs`: Bool → Inverse Visibility

### Testing
- `GuideViewer.Tests/Services/LicenseValidatorTests.cs`: 11 tests
- `GuideViewer.Tests/Services/SettingsServiceTests.cs`: 13 tests
- `GuideViewer.Tests/Services/ImageStorageServiceTests.cs`: 26 tests
- `GuideViewer.Tests/Services/AutoSaveServiceTests.cs`: 16 tests
- `GuideViewer.Tests/Repositories/GuideRepositoryTests.cs`: 19 tests
- `GuideViewer.Tests/Repositories/CategoryRepositoryTests.cs`: 14 tests

### Utilities
- `KeyGenerator/Program.cs`: Console app for generating product keys

## References

- [WinUI 3 Documentation](https://learn.microsoft.com/en-us/windows/apps/winui/winui3/)
- [LiteDB Documentation](https://www.litedb.org/)
- [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
- [Serilog](https://serilog.net/)

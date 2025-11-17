# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

GuideViewer is a Windows desktop application for service technicians to access and track progress on installation guides. It uses WinUI 3, .NET 8, and follows an offline-first architecture with role-based access control (ADMIN vs TECHNICIAN).

**Target Platform**: Windows 10 1809+ / Windows 11
**Framework**: WinUI 3 (Windows App SDK 1.5+)
**Architecture**: MVVM with dependency injection

**Milestone 1 Status**: ✅ **COMPLETE!** (2025-11-16)

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
# Expected: 24/24 tests passing
```

## What's New (Latest Session - 2025-11-16)

### Major Achievements
- ✅ **ActivationWindow** - Complete first-run product key activation flow
- ✅ **MainWindow** - Full NavigationView with 4 pages and Mica background
- ✅ **Role-based UI** - Admin/Technician visibility tested and working
- ✅ **NavigationService** - Frame-based page routing system
- ✅ **Value Converters** - InverseBooleanConverter, BooleanToVisibilityConverter
- ✅ **Windows App SDK** - Unpackaged deployment configured
- ✅ **WinUI 3 Issues** - All build and runtime issues resolved

### Files Created (17 new files)
- NavigationService, ActivationViewModel, MainViewModel
- 4 placeholder pages (Home, Guides, Progress, Settings)
- ActivationWindow with complete UI and logic
- Value converters for data binding
- KeyGenerator utility project

### Known Issues Resolved
1. ✅ WinUI 3 XAML compiler errors (invalid WPF properties)
2. ✅ Windows App SDK runtime DLL not found (unpackaged deployment)
3. ✅ C# 13 partial property syntax errors (reverted to field-based)

See `todo.md` for complete session details.

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
- ViewModels using CommunityToolkit.Mvvm (`ActivationViewModel`, `MainViewModel`)
- Pages: `HomePage`, `GuidesPage`, `ProgressPage`, `SettingsPage`
- Services: `NavigationService` for page routing
- Converters: `InverseBooleanConverter`, `BooleanToVisibilityConverter`
- Windows: `ActivationWindow` (first-run), `MainWindow` (NavigationView + Mica)
- Dependency injection configured in App.xaml.cs
- Service access via `App.GetService<T>()`

**GuideViewer.Core (Business Logic)**
- Services: `LicenseValidator`, `ISettingsService`/`SettingsService`
- Models: `UserRole`, `LicenseInfo`, `AppSettings`
- Utilities: `ProductKeyGenerator`
- **No dependencies on Data or UI layers**

**GuideViewer.Data (Data Access)**
- `DatabaseService`: LiteDB initialization and management
- Repository pattern: `IRepository<T>`, `Repository<T>`, `UserRepository`, `SettingsRepository`
- Entities: `User`, `AppSetting`
- **No dependencies on Core or UI layers**

**Dependency Flow**: UI → Core → Data (strict layering enforced)

## Key Architectural Patterns

### Dependency Injection
Services are registered in `App.xaml.cs`:
- **Singleton**: `DatabaseService`, `LicenseValidator`, `ISettingsService`, `NavigationService`
- **Transient**: `UserRepository`, `SettingsRepository`

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

**InverseBooleanConverter**: Inverts boolean values
```xml
IsEnabled="{Binding IsLoading, Converter={StaticResource InverseBooleanConverter}}"
```

**BooleanToVisibilityConverter**: Converts bool to Visibility
```xml
Visibility="{Binding IsAdmin, Converter={StaticResource BooleanToVisibilityConverter}}"
```

## Data Storage

### LiteDB Database
**Location**: `%LocalAppData%\GuideViewer\data.db`
**Collections**:
- `users`: User authentication and role information
- `settings`: Application settings (JSON serialized)
- `guides`: Installation guides (to be implemented)
- `progress`: User progress tracking (to be implemented)

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

### Next Milestone (2)
- Guide creation and editing (Admin only)
- Guide viewer with step-by-step navigation
- Progress tracking and completion status
- SharePoint synchronization
- Offline-first data sync

## Important Files

### Documentation
- `spec.md`: Complete product specification with requirements
- `todo.md`: Milestone 1 task list with detailed progress (COMPLETE!)
- `CLAUDE.md`: This file - development guide and codebase documentation
- `TEST_PRODUCT_KEYS.txt`: 10 test product keys (5 admin, 5 tech)

### Core Business Logic
- `GuideViewer.Core/Services/LicenseValidator.cs`: Product key validation (HMAC-SHA256)
- `GuideViewer.Core/Services/ISettingsService.cs`: Settings interface
- `GuideViewer.Core/Services/SettingsService.cs`: Settings implementation
- `GuideViewer.Core/Models/UserRole.cs`: Admin/Technician enum
- `GuideViewer.Core/Models/LicenseInfo.cs`: License validation result
- `GuideViewer.Core/Utilities/ProductKeyGenerator.cs`: Key generation utility

### Data Layer
- `GuideViewer.Data/Services/DatabaseService.cs`: LiteDB initialization
- `GuideViewer.Data/Repositories/Repository.cs`: Generic repository base
- `GuideViewer.Data/Repositories/UserRepository.cs`: User data access
- `GuideViewer.Data/Repositories/SettingsRepository.cs`: Settings data access
- `GuideViewer.Data/Entities/User.cs`: User entity
- `GuideViewer.Data/Entities/AppSetting.cs`: Setting entity

### UI Layer
- `GuideViewer/App.xaml.cs`: DI configuration, logging, first-run detection
- `GuideViewer/App.xaml`: Application resources, value converters
- `GuideViewer/MainWindow.xaml`: NavigationView + Mica background
- `GuideViewer/MainWindow.xaml.cs`: Navigation setup, Mica implementation
- `GuideViewer/Views/ActivationWindow.xaml`: Product key entry UI
- `GuideViewer/Views/ActivationWindow.xaml.cs`: Activation logic
- `GuideViewer/Views/Pages/HomePage.xaml`: Landing page
- `GuideViewer/Views/Pages/GuidesPage.xaml`: Guides browser
- `GuideViewer/Views/Pages/ProgressPage.xaml`: Progress dashboard
- `GuideViewer/Views/Pages/SettingsPage.xaml`: Settings page
- `GuideViewer/ViewModels/ActivationViewModel.cs`: Activation logic
- `GuideViewer/ViewModels/MainViewModel.cs`: Main window + role detection
- `GuideViewer/Services/NavigationService.cs`: Page navigation
- `GuideViewer/Converters/InverseBooleanConverter.cs`: Boolean inversion
- `GuideViewer/Converters/BooleanToVisibilityConverter.cs`: Bool → Visibility

### Testing
- `GuideViewer.Tests/Services/LicenseValidatorTests.cs`: 11 tests
- `GuideViewer.Tests/Services/SettingsServiceTests.cs`: 13 tests

### Utilities
- `KeyGenerator/Program.cs`: Console app for generating product keys

## References

- [WinUI 3 Documentation](https://learn.microsoft.com/en-us/windows/apps/winui/winui3/)
- [LiteDB Documentation](https://www.litedb.org/)
- [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
- [Serilog](https://serilog.net/)

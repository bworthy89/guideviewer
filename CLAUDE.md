# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

GuideViewer is a Windows desktop application for service technicians to access and track progress on installation guides. It uses WinUI 3, .NET 8, and follows an offline-first architecture with role-based access control (ADMIN vs TECHNICIAN).

**Target Platform**: Windows 10 1809+ / Windows 11
**Framework**: WinUI 3 (Windows App SDK 1.5+)
**Architecture**: MVVM with dependency injection

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
├── GuideViewer.Core/         # Business logic and services
├── GuideViewer.Data/         # Data access layer (LiteDB)
└── GuideViewer.Tests/        # xUnit tests
```

### Layered Architecture

**GuideViewer (UI Layer)**
- WinUI 3 XAML views and code-behind
- ViewModels using CommunityToolkit.Mvvm
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
- **Singleton**: `DatabaseService`, `LicenseValidator`, `ISettingsService`
- **Transient**: `UserRepository`, `SettingsRepository`

Access services anywhere:
```csharp
var licenseValidator = App.GetService<LicenseValidator>();
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
- Use `CommunityToolkit.Mvvm` source generators
- ViewModels should use `[ObservableProperty]` and `[RelayCommand]` attributes
- All ViewModels should inherit from `ObservableObject`

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
```csharp
var validator = new LicenseValidator();
var adminKey = validator.GenerateProductKey(UserRole.Admin);
var techKey = validator.GenerateProductKey(UserRole.Technician);
```

Or use utility:
```csharp
ProductKeyGenerator.PrintKeys(adminCount: 5, techCount: 5);
```

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

### XAML Compilation Issue
The WinUI 3 XAML compiler (`XamlCompiler.exe`) does not work reliably via `dotnet build` CLI. **Always use Visual Studio 2022** to build the GuideViewer UI project.

### Fluent Design Requirements
- Use Windows 11 Fluent Design System components
- Mica background material for main window
- Acrylic for navigation panes
- Minimum touch targets: 44x44 pixels
- Support both light and dark themes

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
1. Create XAML in `GuideViewer/Views/{PageName}.xaml`
2. Create ViewModel in `GuideViewer/ViewModels/{PageName}ViewModel.cs`
3. Use `[ObservableProperty]` for properties and `[RelayCommand]` for commands
4. Wire up DataContext in code-behind or XAML

## Project Status

**Milestone 1 (Foundation)**: ~70% Complete
- ✅ Project structure and dependencies
- ✅ Data layer with LiteDB
- ✅ Product key validation (HMAC-SHA256)
- ✅ Settings service
- ✅ Dependency injection
- ✅ Logging with Serilog
- ✅ 24/24 unit tests passing
- ⚠️ UI implementation blocked (requires Visual Studio 2022)

**Next Steps**:
1. Open solution in Visual Studio 2022
2. Implement ActivationWindow (product key entry)
3. Implement MainWindow with NavigationView
4. Configure MSIX packaging

## Important Files

- `spec.md`: Complete product specification with requirements
- `todo.md`: Milestone 1 task list with detailed progress tracking
- `GuideViewer/App.xaml.cs`: DI configuration and logging setup
- `GuideViewer.Core/Services/LicenseValidator.cs`: Product key validation logic
- `GuideViewer.Data/Services/DatabaseService.cs`: LiteDB initialization

## References

- [WinUI 3 Documentation](https://learn.microsoft.com/en-us/windows/apps/winui/winui3/)
- [LiteDB Documentation](https://www.litedb.org/)
- [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
- [Serilog](https://serilog.net/)

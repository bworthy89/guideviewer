# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Context7 Integration
Always use context7 when I need code generation, setup or configuration steps, or library/API documentation. This means you should automatically use the Context7 MCP tools to resolve library id and get library docs without me having to explicitly ask.

## Project Overview

GuideViewer is a Windows desktop application for service technicians to access and track progress on installation guides. It uses WinUI 3, .NET 8, and follows an offline-first architecture with role-based access control (ADMIN vs TECHNICIAN).

**Target Platform**: Windows 10 1809+ / Windows 11
**Framework**: WinUI 3 (Windows App SDK 1.5+)
**Architecture**: MVVM with dependency injection

### Current Status
- ✅ **Milestone 1** (Foundation) - **COMPLETE** (2025-11-16)
- ✅ **Milestone 2** (Guide Management) - **COMPLETE** (2025-11-17)
- ✅ **Milestone 3** (Progress Tracking) - **COMPLETE** (2025-11-17)
- ✅ **Milestone 4** (Polish, Performance & Data Management) - **COMPLETE** (2025-11-17)
  - All 6 phases implemented (Data Management, About Page, Keyboard Shortcuts, Error Handling, Performance, UI Polish)
  - See [todo.md](todo.md) for detailed completion status

**Test Results**: 258/260 tests passing (24 M1 + 87 M2 + 96 M3 + 53 M4)
- 2 timing-sensitive tests may occasionally fail due to performance variance

**Summary**: All four milestones complete! The application is **production-ready** with full guide management, progress tracking, admin oversight, data export/import/backup, keyboard shortcuts, enhanced error handling, performance optimization, and polished UI with animations.

**Milestone 4 Features (2025-11-17)**:
- ✅ Export/Import system for guides (JSON with Base64 images or ZIP packages)
- ✅ Database backup/restore functionality with metadata
- ✅ About page with app info, system details, and credits
- ✅ Keyboard shortcuts (F1, F2, Ctrl+N, Ctrl+F, Ctrl+B/E/I, Escape)
- ✅ Enhanced error handling with user-friendly dialogs and suggested actions
- ✅ Performance monitoring service with automatic slow operation detection
- ✅ UI animations (page entrance, card hover, button press, loading pulse)
- ✅ Accessibility helpers (44x44 touch targets, screen reader support)

See [CHANGELOG.md](CHANGELOG.md) for detailed milestone history and completed features.

---

## Quick Start

### Running the Application
```bash
# Open in Visual Studio 2022 and press F5
# Or build the solution
dotnet build GuideViewer.sln
```

**First Run**: Enter a product key from `TEST_PRODUCT_KEYS.txt`:
- Admin key: `A04E-02C0-AD82-43C0`
- Technician key: `TD5A-BB21-A638-C43A`

**Reset Database**: Delete `%LocalAppData%\GuideViewer\data.db`

### Running Tests
```bash
dotnet test GuideViewer.Tests/GuideViewer.Tests.csproj
# Expected: 258/260 tests passing (2 timing tests may fail occasionally)
```

### Build Commands
```bash
# Build entire solution
dotnet build GuideViewer.sln

# Build specific projects
dotnet build GuideViewer.Core/GuideViewer.Core.csproj
dotnet build GuideViewer.Data/GuideViewer.Data.csproj
dotnet build GuideViewer.Tests/GuideViewer.Tests.csproj

# Note: WinUI 3 UI project requires Visual Studio 2022
```

---

## Solution Architecture

### Project Structure
```
GuideViewer.sln
├── GuideViewer/              # WinUI 3 application (presentation layer)
│   ├── Converters/          # XAML value converters
│   ├── Services/            # NavigationService
│   ├── ViewModels/          # MVVM ViewModels
│   └── Views/               # XAML views and pages
├── GuideViewer.Core/         # Business logic and services
├── GuideViewer.Data/         # Data access layer (LiteDB)
├── GuideViewer.Tests/        # xUnit tests
└── KeyGenerator/             # Product key generator utility
```

### Layered Architecture

**GuideViewer (UI Layer)**
- WinUI 3 XAML views with MVVM pattern
- ViewModels: `ActivationViewModel`, `MainViewModel`, `GuidesViewModel`, `GuideEditorViewModel`, `CategoryManagementViewModel`
- Pages: `HomePage`, `GuidesPage`, `GuideEditorPage`, `GuideDetailPage`, `ProgressPage`, `SettingsPage`, `AboutPage`
- Services: `NavigationService`, `IKeyboardShortcutService`
- Helpers: `AnimationHelper`, `AccessibilityHelper`
- Converters: `InverseBooleanConverter`, `BooleanToVisibilityConverter`, `InverseBooleanToVisibilityConverter`
- Resources: `Animations.xaml`, `Styles.xaml` (5 animations, 9 enhanced styles)

**GuideViewer.Core (Business Logic)**
- Services: `LicenseValidator`, `ISettingsService`, `IImageStorageService`, `IAutoSaveService`, `IProgressTrackingService`, `ITimerService`, `IGuideExportService`, `IGuideImportService`, `IDatabaseBackupService`, `IErrorHandlingService`, `IPerformanceMonitoringService`, `IOneDriveGuideService`
- Models: `UserRole`, `LicenseInfo`, `AppSettings`, `ImageValidationResult`, `ImageMetadata`, `ProgressStatistics`, `ProgressReportItem`, `GuideExport`, `ImportResult`, `BackupInfo`, `ErrorInfo`, `ErrorCategory`, `PerformanceMetric`, `OneDriveGuideInfo`, `GuideUpdateInfo`, `GuideUpdateType`
- Utilities: `ProductKeyGenerator`, `SampleDataSeeder`
- **No dependencies on Data or UI layers**

**GuideViewer.Data (Data Access)**
- `DatabaseService`: LiteDB initialization and management
- Repository pattern: `IRepository<T>`, specialized repositories
- Entities: `User`, `AppSetting`, `Guide`, `Step`, `Category`, `Progress`
- Models: `ProgressStatistics`
- **No dependencies on Core or UI layers**

**Dependency Flow**: UI → Core → Data (strict layering enforced)

See [PATTERNS.md](PATTERNS.md) for detailed architectural patterns and code examples.

---

## Data Storage

### LiteDB Database
**Location**: `%LocalAppData%\GuideViewer\data.db`

**Collections**:
- `users` - User authentication and role information
- `settings` - Application settings (JSON serialized)
- `guides` - Installation guides with embedded steps ✅
- `categories` - Guide categories with icons and colors ✅
- `progress` - User progress tracking ✅ (Phase 1)

**FileStorage**: Images stored in LiteDB (10MB max, PNG/JPG/JPEG/BMP)

**Indexes**:
- Guide.Title, Guide.Category, Guide.UpdatedAt
- Category.Name (unique)
- Progress: (UserId, GuideId) composite unique, LastAccessedAt

---

## Authentication & Licensing

### Product Key Format
`XXXX-XXXX-XXXX-XXXX` (16 characters, uppercase alphanumeric)

**Structure**:
- First character: Role prefix (`A` = Admin, `T` = Technician)
- Characters 1-11: Payload
- Characters 12-15: HMAC-SHA256 checksum

### Generating Product Keys
```bash
# Using KeyGenerator utility (Recommended)
dotnet run --project KeyGenerator/KeyGenerator.csproj

# Or programmatically
var validator = new LicenseValidator();
var adminKey = validator.GenerateProductKey(UserRole.Admin);
```

See `TEST_PRODUCT_KEYS.txt` for pre-generated test keys.

---

## Guide Distribution System

### OneDrive-Based Guide Updates

**Problem**: SharePoint REST API access is blocked by IT restrictions.

**Solution**: OneDrive sync folder monitoring with local file system access (no API required).

### How It Works

```
IT Admin uploads guides to SharePoint
        ↓ (OneDrive syncs automatically)
Local: C:\Users\[User]\OneDrive - Glory Global\GuideViewer_Guides\
        ↓ (GuideViewer monitors folder)
Detects new/updated guides → Imports into database
```

### Key Components

**OneDriveGuideService** (`GuideViewer.Core/Services/`)
- Detects OneDrive sync folder location (registry/environment variables)
- Scans for guide ZIP packages
- Compares with local database to identify new/updated guides
- Imports guides via `IGuideImportService`
- Optional: FileSystemWatcher for real-time monitoring

**Settings Page UI**
- "Check for Guide Updates" button
- OneDrive connection status display
- `GuideUpdatesDialog` for selecting which guides to import

### User Workflow

**For Technicians**:
1. Open GuideViewer → Settings
2. Click "Check for Guide Updates"
3. Dialog shows available guides (all pre-selected)
4. Click "Import Selected"
5. New guides appear in Guides list

**For IT Admins**:
1. Upload guide ZIPs to SharePoint: `GuideViewer_Guides/Guides/`
2. OneDrive syncs automatically to all field PCs
3. Notify technicians to check for updates

### Advantages

✅ **No API Access Required** - Pure file system operations
✅ **No Admin Rights Needed** - OneDrive syncs to user folder
✅ **Automatic Sync** - OneDrive handles cloud synchronization
✅ **Offline-First** - Files are local, works offline
✅ **Simple Deployment** - Just sync a SharePoint folder

See [ONEDRIVE_GUIDE_UPDATES.md](ONEDRIVE_GUIDE_UPDATES.md) for complete documentation, setup instructions, and troubleshooting.

---

## Application Updates (Planned)

### Squirrel.Windows Auto-Update System

**Status**: 🚧 Planned for implementation

**Purpose**: Automatic application updates (not guide content - that uses OneDrive)

**Why Separate Systems?**:
- App updates: Infrequent (monthly), large packages, requires restart
- Guide updates: Frequent (weekly), small packages, no restart

**Architecture**:
```
IT Admin publishes app updates to SharePoint/file share
        ↓
Squirrel.Windows checks for updates
        ↓
Downloads delta packages (only changed files)
        ↓
Applies update on next app restart
```

**Features**:
- Delta updates (only download changed files)
- Automatic background checks
- No admin rights required (%LocalAppData% install)
- Rollback support
- No AppLocker restrictions (confirmed)

**Setup Required**:
1. Install Squirrel tooling: `dotnet tool install --global Clowd.Squirrel`
2. Add NuGet package: `Clowd.Squirrel` to GuideViewer.csproj
3. Create `UpdateService.cs` implementation
4. Add update UI to Settings page
5. Create deployment script for packaging releases

See `todo.md` for implementation tasks.

---

## Common Development Tasks

### Adding a New Service
1. Create `I{ServiceName}.cs` in `GuideViewer.Core/Services/`
2. Implement `{ServiceName}.cs` in `GuideViewer.Core/Services/`
3. Register in `App.xaml.cs` `ConfigureServices()`
4. Write tests in `GuideViewer.Tests/Services/`

### Adding a New Entity
1. Create entity in `GuideViewer.Data/Entities/`
2. Add `[BsonId]` attribute to Id property
3. Update `DatabaseService.InitializeCollections()` for indexes
4. Create repository in `GuideViewer.Data/Repositories/`
5. Register repository in `App.xaml.cs`

### Adding a New Page
1. Create XAML in `GuideViewer/Views/Pages/`
2. Create code-behind `.xaml.cs`
3. (Optional) Create ViewModel in `GuideViewer/ViewModels/`
4. Register page in `MainWindow.xaml.cs`
5. Add NavigationViewItem to `MainWindow.xaml`

See [PATTERNS.md](PATTERNS.md) for detailed code examples.

---

## Key Technologies

### Dependency Injection
Services registered in `App.xaml.cs`:
- **Singleton**: `DatabaseService`, `LicenseValidator`, `ISettingsService`, `NavigationService`, `ImageStorageService`
- **Transient**: All repositories, `AutoSaveService`

Access via `App.GetService<T>()`

### MVVM with CommunityToolkit.Mvvm
```csharp
public partial class MyViewModel : ObservableObject
{
    [ObservableProperty]
    private string title = string.Empty; // Generates public Title property

    [RelayCommand]
    private async Task SaveAsync() { } // Generates SaveCommand
}
```

### Navigation
```csharp
var nav = App.GetService<NavigationService>();
nav.NavigateTo(PageKeys.Guides);
nav.NavigateTo(PageKeys.GuideEditor, guideId); // With parameter
```

### Async UI Updates
WinUI 3 requires `DispatcherQueue` for UI updates from background threads:
```csharp
_dispatcherQueue.TryEnqueue(() => { /* UI update */ });
```

**Important**: `App.Current.DispatcherQueue` does NOT exist - use page's `DispatcherQueue`

**Threading Pattern for ObservableObject Properties**:
```csharp
// ❌ Wrong - causes COM marshalling error (RPC_E_WRONG_THREAD)
await Task.Run(() =>
{
    CurrentGuide = _repository.GetById(id); // ObservableObject property set on background thread
});

// ✅ Correct - load on background thread, assign on UI thread
Guide? guide = null;
await Task.Run(() =>
{
    guide = _repository.GetById(id); // Local variable on background thread
});
CurrentGuide = guide; // ObservableObject property set on UI thread
```

---

## WinUI 3 Specifics

### XAML Compilation
**Always use Visual Studio 2022** to build the UI project. The XAML compiler doesn't work reliably via `dotnet build` CLI.

### Important Differences from WPF
- No `MinWidth`, `MinHeight`, `Title` on Window
- Use `AppWindow.Resize()` for window sizing
- Use `{Binding}` instead of `{x:Bind}` for better compatibility
- Set DataContext on `FrameworkElement` (cast from `this.Content`)

### Button Binding Patterns in DataTemplates
**Issue**: ElementName bindings to DataContext don't work reliably inside ItemsRepeater DataTemplates
**Solution**: Use Click handlers with Tag binding instead of Command bindings

```xml
<!-- ❌ Unreliable in DataTemplates -->
<Button Command="{Binding DataContext.MyCommand, ElementName=PageRoot}"
        CommandParameter="{Binding}"/>

<!-- ✅ Reliable pattern -->
<Button Click="MyButton_Click" Tag="{Binding}"/>
```

```csharp
private void MyButton_Click(object sender, RoutedEventArgs e)
{
    if (sender is Button button && button.Tag is MyEntity entity)
    {
        ViewModel.MyCommand.Execute(entity);
    }
}
```

**ScrollViewer Input Event Issue**: Wrap ItemsRepeater in Grid to prevent ScrollViewer from consuming input events:
```xml
<ScrollViewer>
    <Grid>
        <ItemsRepeater ItemsSource="{Binding Items}">
            <!-- Content -->
        </ItemsRepeater>
    </Grid>
</ScrollViewer>
```

### Fluent Design
- ✅ Mica background material (MainWindow)
- ✅ NavigationView for navigation
- ✅ 44x44 pixel minimum touch targets
- ✅ Light and dark theme support

### Performance Targets
- Startup time: <2 seconds
- Guide list load: <500ms
- Step navigation: <100ms
- Memory usage: <150MB

---

## Testing Strategy

**Framework**: xUnit, FluentAssertions, Moq
**Target Coverage**: 80%+ for Core project
**Current Status**: 258/260 tests passing (2 timing-sensitive tests may fail occasionally)

### Test Organization
- **Services** (91 tests): License validation, settings, image storage, auto-save, progress tracking, timer
- **Repositories** (75 tests): Guide, category, progress data access
- **Integration** (50 tests): End-to-end workflows for guides, categories, progress tracking, and data management
- **Performance** (8 tests): Performance benchmarks for critical operations
- **Data Management** (34 tests): Export, import, and backup service tests

### Test Database
Tests use temporary databases in `%TEMP%`:
```csharp
_testDatabasePath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db");
```

Always implement `IDisposable` to clean up:
```csharp
public void Dispose()
{
    _databaseService?.Dispose();
    if (File.Exists(_testDatabasePath))
        File.Delete(_testDatabasePath);
}
```

---

## Logging

**Framework**: Serilog
**Location**: `%LocalAppData%\GuideViewer\logs\app-*.log`
**Retention**: 7 days (rolling daily)

```csharp
Log.Information("Message");
Log.Error(exception, "Error occurred");
```

---

## Important Files

### Documentation
- `spec.md` - Complete product specification
- `todo.md` - Current development tasks
- `CHANGELOG.md` - Completed milestones and features
- `PATTERNS.md` - Development patterns and code examples
- `ONEDRIVE_GUIDE_UPDATES.md` - OneDrive-based guide distribution system (no API required)
- `TEST_PRODUCT_KEYS.txt` - Test product keys

### Core Components
- **Services**: `LicenseValidator`, `SettingsService`, `ImageStorageService`, `AutoSaveService`, `ProgressTrackingService`, `TimerService`, `GuideExportService`, `GuideImportService`, `DatabaseBackupService`, `ErrorHandlingService`, `PerformanceMonitoringService`, `KeyboardShortcutService`, `OneDriveGuideService`
- **Repositories**: `UserRepository`, `GuideRepository`, `CategoryRepository`, `ProgressRepository`
- **Entities**: `User`, `Guide`, `Step`, `Category`, `Progress`
- **Models**: `ProgressStatistics`, `ProgressReportItem`, `GuideExport`, `ImportResult`, `BackupInfo`, `ErrorInfo`, `ErrorCategory`, `PerformanceMetric`, `OneDriveGuideInfo`, `GuideUpdateInfo`
- **ViewModels**: `ActivationViewModel`, `MainViewModel`, `GuidesViewModel`, `GuideEditorViewModel`, `CategoryManagementViewModel`, `ProgressDashboardViewModel`, `ProgressReportViewModel`
- **Helpers**: `AnimationHelper`, `AccessibilityHelper`
- **Pages**: `HomePage`, `GuidesPage`, `GuideEditorPage`, `GuideDetailPage`, `ProgressPage`, `ActiveGuideProgressPage`, `SettingsPage`, `AboutPage`

### Test Files
- **Unit Tests**: `LicenseValidatorTests`, `SettingsServiceTests`, `ImageStorageServiceTests`, `AutoSaveServiceTests`, `TimerServiceTests`, `GuideRepositoryTests`, `CategoryRepositoryTests`, `ProgressRepositoryTests`, `ProgressTrackingServiceTests`, `GuideExportServiceTests`, `GuideImportServiceTests`, `DatabaseBackupServiceTests`
- **Integration Tests**: `GuideWorkflowIntegrationTests`, `CategoryManagementIntegrationTests`, `ProgressDataLayerIntegrationTests`, `ProgressServicesIntegrationTests`, `ProgressTrackingWorkflowIntegrationTests`, `ProgressPerformanceIntegrationTests`, `DataManagementIntegrationTests`
- **Performance Tests**: `PerformanceTests` (8 benchmark tests)

---

## References

- [WinUI 3 Documentation](https://learn.microsoft.com/en-us/windows/apps/winui/winui3/)
- [LiteDB Documentation](https://www.litedb.org/)
- [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
- [Serilog](https://serilog.net/)
- [CHANGELOG.md](CHANGELOG.md) - Detailed milestone history
- [PATTERNS.md](PATTERNS.md) - Development patterns and examples

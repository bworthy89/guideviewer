# Milestone 4: Polish, Performance & Data Management

**Status**: 🚧 **NOT STARTED**
**Target**: 45-60 new tests
**Estimated Time**: 16-22 hours
**Started**: TBD
**Completed**: TBD
**Last Updated**: 2025-11-17

---

## Overview

Milestone 4 focuses on production-ready features that enhance usability, reliability, and data portability. This milestone adds:
- Guide export/import for sharing and backup
- Database backup/restore for data protection
- About page with version and license info
- Keyboard shortcuts for power users
- Global error handling with user-friendly feedback
- Performance optimizations (virtualization, lazy loading)
- Animations and transitions for polish
- Loading states throughout the app

---

## Prerequisites

**Required**: Milestones 1, 2, and 3 must be complete (✅ DONE - 207/207 tests passing)

**Before Starting**:
- Review spec.md Milestone 4 section (lines 375-414)
- Ensure Visual Studio 2022 is ready
- Verify all 207 tests still passing
- Review PATTERNS.md for architectural guidelines

---

## Phase 1: Data Management (Export/Import/Backup) - 30% of milestone

**Estimated Time**: 5-7 hours
**Status**: Not Started
**Tests**: 15-20 tests (unit + integration)

### Overview
Implement guide export/import and database backup/restore to enable data portability, sharing between installations, and disaster recovery.

### Tasks

#### Export Service Implementation
- [ ] Create `IGuideExportService.cs` interface in `GuideViewer.Core/Services/`
  - [ ] Method: `Task<string> ExportGuideToJsonAsync(ObjectId guideId)` → JSON string
  - [ ] Method: `Task<string> ExportAllGuidesToJsonAsync()` → JSON array string
  - [ ] Method: `Task<bool> ExportGuideToFileAsync(ObjectId guideId, string filePath)` → Save to file
  - [ ] Method: `Task<bool> ExportAllGuidesToFileAsync(string filePath)` → Save all to file
  - [ ] Method: `Task<byte[]> ExportGuideWithImagesAsync(ObjectId guideId)` → ZIP package

- [ ] Create `GuideExportService.cs` implementation
  - [ ] Inject: GuideRepository, ImageStorageService
  - [ ] Serialize Guide entity with all steps to JSON
  - [ ] Include metadata: version, export date, author
  - [ ] Handle image export (Base64 embedded or separate files in ZIP)
  - [ ] Validate guide exists before export
  - [ ] Add comprehensive error handling and logging
  - [ ] Support pretty-print JSON for readability

#### Import Service Implementation
- [ ] Create `IGuideImportService.cs` interface in `GuideViewer.Core/Services/`
  - [ ] Method: `Task<ImportResult> ImportGuideFromJsonAsync(string json)` → ImportResult
  - [ ] Method: `Task<ImportResult> ImportGuideFromFileAsync(string filePath)` → ImportResult
  - [ ] Method: `Task<ImportResult> ImportGuidesFromFileAsync(string filePath)` → ImportResult (multiple)
  - [ ] Method: `Task<ImportResult> ImportGuideFromZipAsync(byte[] zipData)` → ImportResult
  - [ ] Method: `Task<bool> ValidateImportFileAsync(string filePath)` → Validation check

- [ ] Create `ImportResult.cs` model in `GuideViewer.Core/Models/`
  - [ ] Properties: Success (bool), ImportedGuideIds (List<ObjectId>)
  - [ ] Properties: ErrorMessages (List<string>), WarningMessages (List<string>)
  - [ ] Properties: DuplicatesSkipped (int), GuidesImported (int)

- [ ] Create `GuideImportService.cs` implementation
  - [ ] Inject: GuideRepository, CategoryRepository, ImageStorageService
  - [ ] Deserialize JSON to Guide entity
  - [ ] Validate JSON schema and required fields
  - [ ] Handle version compatibility (future-proof for schema changes)
  - [ ] Check for duplicate guides by title (offer skip/overwrite/rename options)
  - [ ] Validate category exists or create if missing
  - [ ] Import images from Base64 or ZIP
  - [ ] Validate image sizes and formats
  - [ ] Transaction-like behavior (all or nothing for multi-guide import)
  - [ ] Comprehensive error handling with user-friendly messages

#### Database Backup Service
- [ ] Create `IDatabaseBackupService.cs` interface in `GuideViewer.Core/Services/`
  - [ ] Method: `Task<bool> CreateBackupAsync(string backupPath)` → Success/failure
  - [ ] Method: `Task<bool> RestoreBackupAsync(string backupPath)` → Success/failure
  - [ ] Method: `Task<bool> ValidateBackupAsync(string backupPath)` → Validation check
  - [ ] Method: `Task<BackupInfo> GetBackupInfoAsync(string backupPath)` → Metadata
  - [ ] Method: `Task<List<string>> GetAvailableBackupsAsync(string directory)` → Backup list

- [ ] Create `BackupInfo.cs` model in `GuideViewer.Core/Models/`
  - [ ] Properties: BackupDate (DateTime), AppVersion (string)
  - [ ] Properties: GuideCount (int), UserCount (int), ProgressCount (int)
  - [ ] Properties: DatabaseSize (long), IsValid (bool)

- [ ] Create `DatabaseBackupService.cs` implementation
  - [ ] Inject: DatabaseService, ISettingsService
  - [ ] Copy LiteDB file to backup location (database must be closed or use LiteDB backup API)
  - [ ] Include metadata file (JSON) with backup info
  - [ ] Compress backup to ZIP format
  - [ ] Validate backup file integrity
  - [ ] Restore: Close current database, copy backup, reopen
  - [ ] Handle database locks and file access errors
  - [ ] Add progress reporting for large backups (optional)

#### Settings Page UI Updates
- [ ] Update `SettingsPage.xaml` - Add Data Management section
  - [ ] Section: "Data Management" (below Appearance and Category Management)
  - [ ] **Export Guides**:
    - [ ] Button: "Export Single Guide..." → Opens guide picker dialog, then file save dialog
    - [ ] Button: "Export All Guides..." → Opens file save dialog for all guides
    - [ ] InfoBar: Success/error messages for export
  - [ ] **Import Guides**:
    - [ ] Button: "Import Guides..." → Opens file open dialog
    - [ ] InfoBar: Shows import results (success count, duplicates, errors)
    - [ ] ProgressRing: Loading indicator during import
  - [ ] **Database Backup**:
    - [ ] Button: "Create Backup..." → Opens folder picker, creates backup
    - [ ] Button: "Restore Backup..." → Opens file picker, restores database
    - [ ] TextBlock: Last backup date/time
    - [ ] TextBlock: Warning about app restart required for restore
  - [ ] Use admin-only visibility for export/import (technicians can backup only)

- [ ] Update `SettingsPage.xaml.cs`
  - [ ] Inject: IGuideExportService, IGuideImportService, IDatabaseBackupService
  - [ ] Event handler: ExportSingleGuide_Click → Show guide picker, export selected
  - [ ] Event handler: ExportAllGuides_Click → Export all guides to file
  - [ ] Event handler: ImportGuides_Click → Import from file, show results
  - [ ] Event handler: CreateBackup_Click → Create backup, show success
  - [ ] Event handler: RestoreBackup_Click → Restore backup, restart app
  - [ ] Use ContentDialog for guide selection
  - [ ] Use FileSavePicker for export
  - [ ] Use FileOpenPicker for import/restore
  - [ ] Display InfoBar with results
  - [ ] Handle errors gracefully with user-friendly messages

#### Service Registration
- [ ] Update `App.xaml.cs` ConfigureServices():
  - [ ] Register `IGuideExportService` as Singleton
  - [ ] Register `IGuideImportService` as Singleton
  - [ ] Register `IDatabaseBackupService` as Singleton

#### Unit Tests (10-15 tests)
- [ ] Create `GuideExportServiceTests.cs` in `GuideViewer.Tests/Services/`
  - [ ] Test `ExportGuideToJsonAsync_ValidGuide_ReturnsJson`
  - [ ] Test `ExportGuideToJsonAsync_InvalidGuideId_ThrowsException`
  - [ ] Test `ExportGuideToJsonAsync_WithImages_IncludesBase64Data`
  - [ ] Test `ExportAllGuidesToJsonAsync_MultipleGuides_ReturnsJsonArray`
  - [ ] Test `ExportGuideToFileAsync_ValidPath_CreatesFile`
  - [ ] Test `ExportGuideWithImagesAsync_CreatesZipWithImages`

- [ ] Create `GuideImportServiceTests.cs` in `GuideViewer.Tests/Services/`
  - [ ] Test `ImportGuideFromJsonAsync_ValidJson_CreatesGuide`
  - [ ] Test `ImportGuideFromJsonAsync_InvalidJson_ReturnsError`
  - [ ] Test `ImportGuideFromJsonAsync_DuplicateTitle_SkipsOrRenames`
  - [ ] Test `ImportGuideFromJsonAsync_MissingCategory_CreatesCategory`
  - [ ] Test `ImportGuideFromJsonAsync_WithImages_ImportsImages`
  - [ ] Test `ImportGuidesFromFileAsync_MultipleGuides_ImportsAll`
  - [ ] Test `ValidateImportFileAsync_InvalidFile_ReturnsFalse`

- [ ] Create `DatabaseBackupServiceTests.cs` in `GuideViewer.Tests/Services/`
  - [ ] Test `CreateBackupAsync_ValidPath_CreatesBackup`
  - [ ] Test `RestoreBackupAsync_ValidBackup_RestoresDatabase`
  - [ ] Test `ValidateBackupAsync_ValidBackup_ReturnsTrue`
  - [ ] Test `ValidateBackupAsync_InvalidBackup_ReturnsFalse`
  - [ ] Test `GetBackupInfoAsync_ValidBackup_ReturnsMetadata`

#### Integration Tests (5-8 tests)
- [ ] Create `DataManagementIntegrationTests.cs` in `GuideViewer.Tests/Integration/`
  - [ ] Test: Export guide → Import to new database → Verify identical
  - [ ] Test: Export all guides → Import to new database → Verify all guides present
  - [ ] Test: Export guide with images → Import → Verify images restored
  - [ ] Test: Create backup → Restore backup → Verify all data intact
  - [ ] Test: Import duplicate guide → Verify skip/rename behavior
  - [ ] Test: Import guide with missing category → Verify category created
  - [ ] Test: Performance - Export/import 50 guides (<5 seconds total)
  - [ ] Test: Large backup (100+ guides) - Create and restore (<10 seconds)

#### Documentation
- [ ] Update CLAUDE.md with data management services
- [ ] Document export/import JSON schema
- [ ] Update PATTERNS.md with file I/O patterns
- [ ] Create sample export JSON for documentation

---

## Phase 2: About Page & Version Info - 10% of milestone

**Estimated Time**: 2-3 hours
**Status**: Not Started
**Tests**: Manual UI testing

### Overview
Create an About page that displays version information, license details, and credits. This helps with support and transparency.

### Tasks

#### Model Implementation
- [ ] Create `VersionInfo.cs` model in `GuideViewer.Core/Models/`
  - [ ] Properties: Version (string), BuildDate (DateTime), Copyright (string)
  - [ ] Properties: LicenseType (string), ThirdPartyLicenses (List<LicenseInfo>)
  - [ ] Static method: `GetCurrentVersion()` → Reads from assembly info

- [ ] Create `LicenseInfo.cs` model in `GuideViewer.Core/Models/`
  - [ ] Properties: LibraryName (string), LicenseType (string), LicenseUrl (string)

#### ViewModel Implementation
- [ ] Create `AboutViewModel.cs` in `GuideViewer/ViewModels/`
  - [ ] Inherit from ObservableObject
  - [ ] Properties: AppVersion, BuildDate, Copyright
  - [ ] ObservableCollection<LicenseInfo> ThirdPartyLicenses
  - [ ] Command: OpenLicenseUrlCommand(string url) → Opens in browser
  - [ ] Command: CopyVersionInfoCommand → Copies version to clipboard
  - [ ] Populate third-party licenses (WinUI 3, LiteDB, Serilog, CommunityToolkit.Mvvm)

#### UI Implementation
- [ ] Create `AboutPage.xaml` in `GuideViewer/Views/Pages/`
  - [ ] Header: App logo/icon, app name "GuideViewer"
  - [ ] Version section: Version number, build date
  - [ ] Copyright section: Copyright text, company name
  - [ ] Description: Brief app description
  - [ ] Button: "Copy Version Info" → Copies to clipboard
  - [ ] Section: "Open Source Licenses"
    - [ ] ItemsRepeater with third-party licenses
    - [ ] Each item: Library name, license type, clickable URL
  - [ ] Use Cards and proper Fluent Design styling

- [ ] Create `AboutPage.xaml.cs`
  - [ ] Initialize AboutViewModel
  - [ ] Wire up command handlers
  - [ ] Implement clipboard copy functionality

#### Navigation Integration
- [ ] Add PageKeys.About to `Services/NavigationService.cs`
- [ ] Register AboutPage in `MainWindow.xaml.cs`
- [ ] Add "About" NavigationViewItem to MainWindow.xaml (bottom of nav menu)
- [ ] OR: Add "About" button to SettingsPage footer

#### Assembly Info Configuration
- [ ] Update `GuideViewer.csproj` with assembly metadata:
  - [ ] Version, FileVersion, InformationalVersion
  - [ ] Copyright, Company, Product, Description
  - [ ] Ensure version auto-increments or follows semantic versioning

#### Manual Testing Checklist
- [ ] Verify version number displays correctly
- [ ] Verify build date is accurate
- [ ] Verify "Copy Version Info" copies to clipboard
- [ ] Verify all third-party license links open in browser
- [ ] Verify page layout looks good on different window sizes
- [ ] Test with light and dark themes

---

## Phase 3: Keyboard Shortcuts - 15% of milestone

**Estimated Time**: 3-4 hours
**Status**: Not Started
**Tests**: Manual testing

### Overview
Implement keyboard shortcuts for common actions to improve productivity for power users.

### Tasks

#### Keyboard Accelerators Implementation
- [ ] Create `KeyboardShortcuts.cs` helper in `GuideViewer/Helpers/`
  - [ ] Static dictionary of all shortcuts and their actions
  - [ ] Method: `RegisterShortcuts(XamlRoot root)` → Registers all shortcuts
  - [ ] Support for Ctrl, Shift, Alt modifiers
  - [ ] Support for function keys and letter keys

#### Global Shortcuts (MainWindow)
- [ ] Update `MainWindow.xaml.cs` - Add keyboard accelerators
  - [ ] **Ctrl+N**: New Guide (admin only) → Navigate to GuideEditorPage
  - [ ] **Ctrl+F**: Focus search box (if on GuidesPage)
  - [ ] **Ctrl+H**: Navigate to HomePage
  - [ ] **Ctrl+G**: Navigate to GuidesPage
  - [ ] **Ctrl+P**: Navigate to ProgressPage
  - [ ] **Ctrl+S**: Navigate to SettingsPage
  - [ ] **Ctrl+Q**: Quit application
  - [ ] **F1**: Navigate to About page (or open help)
  - [ ] **Alt+Left**: Navigate back (if navigation history exists)

#### Page-Specific Shortcuts
- [ ] **GuidesPage.xaml.cs**:
  - [ ] **Ctrl+R**: Refresh guide list
  - [ ] **Delete**: Delete selected guide (with confirmation)
  - [ ] **Enter**: Open/view selected guide

- [ ] **GuideEditorPage.xaml.cs**:
  - [ ] **Ctrl+S**: Save guide
  - [ ] **Ctrl+Shift+S**: Save and exit
  - [ ] **Escape**: Cancel/exit without saving (with confirmation if dirty)
  - [ ] **Ctrl+Z**: Undo (if feasible)
  - [ ] **Ctrl+Y**: Redo (if feasible)

- [ ] **ActiveGuideProgressPage.xaml.cs**:
  - [ ] **Right Arrow / Page Down**: Next step
  - [ ] **Left Arrow / Page Up**: Previous step
  - [ ] **Ctrl+Enter**: Mark current step complete
  - [ ] **Escape**: Exit active guide (with confirmation)

- [ ] **ProgressPage.xaml.cs**:
  - [ ] **Enter**: Resume selected active guide

#### Keyboard Shortcut Help
- [ ] Create `KeyboardShortcutsDialog.xaml` ContentDialog
  - [ ] Title: "Keyboard Shortcuts"
  - [ ] Grouped list of shortcuts by category (Global, Guides, Editor, Progress)
  - [ ] Format: "Ctrl+N" → "Create New Guide"
  - [ ] Close button

- [ ] Add "Keyboard Shortcuts" button to About page or Settings page
- [ ] OR: Add to Help menu in MainWindow
- [ ] Show dialog with **F1** or **Ctrl+?** shortcut

#### Visual Feedback
- [ ] Add tooltips to buttons showing keyboard shortcuts
  - [ ] Example: "Save (Ctrl+S)"
  - [ ] Use `ToolTipService.ToolTip` in XAML

#### Documentation
- [ ] Update CLAUDE.md with keyboard shortcuts reference
- [ ] Add keyboard shortcuts section to About page
- [ ] Consider creating a printable keyboard shortcut reference (PDF/Markdown)

#### Manual Testing Checklist
- [ ] Test all global shortcuts work from any page
- [ ] Test page-specific shortcuts only work on correct page
- [ ] Test shortcuts don't conflict with system shortcuts
- [ ] Test shortcuts work with different keyboard layouts
- [ ] Verify tooltips display shortcuts correctly
- [ ] Test shortcuts with and without admin role
- [ ] Verify F1 help dialog opens and displays all shortcuts

---

## Phase 4: Error Handling & User Feedback - 15% of milestone

**Estimated Time**: 3-4 hours
**Status**: Not Started
**Tests**: 5-10 unit tests + manual testing

### Overview
Implement global error handling with user-friendly messages, success notifications, and validation feedback throughout the app.

### Tasks

#### Error Handling Service
- [ ] Create `IErrorHandlingService.cs` interface in `GuideViewer.Core/Services/`
  - [ ] Method: `void HandleException(Exception ex, string userMessage = null)` → Log + notify user
  - [ ] Method: `void ShowError(string message, string title = "Error")` → Display error
  - [ ] Method: `void ShowWarning(string message, string title = "Warning")` → Display warning
  - [ ] Method: `void ShowSuccess(string message, string title = "Success")` → Display success
  - [ ] Method: `void ShowInfo(string message, string title = "Info")` → Display info
  - [ ] Event: `event EventHandler<NotificationEventArgs> NotificationRequested`

- [ ] Create `NotificationEventArgs.cs` model in `GuideViewer.Core/Models/`
  - [ ] Properties: Message (string), Title (string), Severity (enum: Error/Warning/Success/Info)
  - [ ] Properties: Duration (TimeSpan), IsClosable (bool)

- [ ] Create `ErrorHandlingService.cs` implementation
  - [ ] Inject: ILogger (Serilog)
  - [ ] HandleException: Log exception with stack trace, raise notification event
  - [ ] User-friendly error messages (no technical jargon)
  - [ ] Map common exceptions to user messages (FileNotFoundException, UnauthorizedAccessException, etc.)
  - [ ] Never expose sensitive data in user messages

#### Global Exception Handler
- [ ] Update `App.xaml.cs` - Add unhandled exception handlers
  - [ ] Handle `UnhandledException` event
  - [ ] Handle `AppDomain.CurrentDomain.UnhandledException`
  - [ ] Handle `TaskScheduler.UnobservedTaskException`
  - [ ] Log exception and show user-friendly error dialog
  - [ ] Offer to restart app or continue (if safe)
  - [ ] Create crash dump for debugging (optional)

#### UI Notification System
- [ ] Update `MainWindow.xaml` - Add global InfoBar
  - [ ] Add InfoBar at top of content area (Grid.Row="0")
  - [ ] Bind to ErrorHandlingService.NotificationRequested event
  - [ ] Auto-hide after duration (default 5 seconds)
  - [ ] Support severity styles (Error=red, Warning=yellow, Success=green, Info=blue)
  - [ ] Include close button
  - [ ] Support stacking multiple notifications (queue)

- [ ] Update `MainWindow.xaml.cs`
  - [ ] Subscribe to ErrorHandlingService.NotificationRequested
  - [ ] Display InfoBar with message and severity
  - [ ] Implement auto-hide timer
  - [ ] Handle notification queue (show one at a time)

#### Validation Feedback
- [ ] Update `GuideEditorPage.xaml` - Add validation messages
  - [ ] Show inline errors for empty title (required field)
  - [ ] Show inline errors for invalid step order
  - [ ] Show character count for descriptions with max length
  - [ ] Disable Save button if validation fails
  - [ ] Use red border around invalid fields

- [ ] Update form validation across all pages:
  - [ ] **GuidesPage**: Validate search query (max length)
  - [ ] **ActiveGuideProgressPage**: Validate notes (max 5000 chars)
  - [ ] **SettingsPage**: Validate file paths for export/import

#### Loading States
- [ ] Add loading indicators to all async operations:
  - [ ] **GuidesPage**: ProgressRing while loading guides
  - [ ] **GuideEditorPage**: ProgressRing while saving
  - [ ] **ProgressPage**: ProgressRing while loading dashboard
  - [ ] **SettingsPage**: ProgressRing during export/import/backup
  - [ ] Disable UI during operations to prevent double-clicks

#### Service Registration
- [ ] Update `App.xaml.cs` ConfigureServices():
  - [ ] Register `IErrorHandlingService` as Singleton

#### Unit Tests (5-10 tests)
- [ ] Create `ErrorHandlingServiceTests.cs` in `GuideViewer.Tests/Services/`
  - [ ] Test `HandleException_LogsException_RaisesEvent`
  - [ ] Test `ShowError_RaisesEventWithErrorSeverity`
  - [ ] Test `ShowSuccess_RaisesEventWithSuccessSeverity`
  - [ ] Test `HandleException_MapsCommonExceptions_ToUserFriendlyMessages`
  - [ ] Test `HandleException_DoesNotExposeSensitiveData`

#### Manual Testing Checklist
- [ ] Trigger various exceptions and verify user-friendly messages
- [ ] Verify InfoBar displays with correct severity colors
- [ ] Verify InfoBar auto-hides after 5 seconds
- [ ] Test multiple notifications stack correctly
- [ ] Verify validation errors display inline on forms
- [ ] Verify loading indicators appear during async operations
- [ ] Test unhandled exception dialog offers restart
- [ ] Verify all errors are logged to file

---

## Phase 5: Performance Optimization - 20% of milestone

**Estimated Time**: 4-5 hours
**Status**: Not Started
**Tests**: 10-15 performance tests + profiling

### Overview
Optimize app performance with virtualization, lazy loading, image caching, and memory management.

### Tasks

#### Virtualization
- [ ] Update `GuidesPage.xaml` - Implement virtualization
  - [ ] Replace ItemsRepeater with ItemsRepeater with VirtualizingLayout
  - [ ] OR: Use ListView with ItemsStackPanel (virtualizing)
  - [ ] Test with 500+ guides to verify only visible items rendered
  - [ ] Measure memory usage improvement

- [ ] Update `ProgressPage.xaml` - Implement virtualization
  - [ ] Apply virtualization to active/completed guides lists
  - [ ] Test with 100+ progress records

#### Lazy Loading
- [ ] Update `GuideRepository.cs` - Add pagination support
  - [ ] Method: `GetGuidesPaged(int page, int pageSize)` → Paginated results
  - [ ] Method: `GetGuidesCount()` → Total count for pagination
  - [ ] Optimize queries with LiteDB skip/take

- [ ] Update `GuidesPage.xaml` - Implement incremental loading
  - [ ] Load guides in batches (e.g., 50 at a time)
  - [ ] "Load More" button at bottom of list
  - [ ] OR: Auto-load on scroll to bottom (infinite scroll)
  - [ ] Show loading indicator while loading next batch

#### Image Optimization
- [ ] Update `ImageStorageService.cs` - Add caching
  - [ ] Implement in-memory LRU cache for recently used images (max 50MB)
  - [ ] Cache decoded BitmapImage objects, not raw bytes
  - [ ] Dispose cached images when memory pressure detected
  - [ ] Add cache hit/miss metrics logging

- [ ] Update image loading throughout app:
  - [ ] **GuideEditorPage**: Load images on-demand (not all steps at once)
  - [ ] **ActiveGuideProgressPage**: Preload next step image
  - [ ] Use thumbnail/preview for guide list (downscale large images)
  - [ ] Async image loading with placeholders

#### Memory Management
- [ ] Review all ViewModels for IDisposable implementation
  - [ ] Ensure all event handlers unsubscribed in Dispose()
  - [ ] Ensure all timers disposed
  - [ ] Ensure large collections cleared on navigation away

- [ ] Implement memory pressure monitoring (optional)
  - [ ] Subscribe to MemoryManager.AppMemoryUsageLimitChanging
  - [ ] Clear caches when memory pressure high
  - [ ] Log memory usage metrics

#### Database Query Optimization
- [ ] Review all LiteDB queries for index usage
  - [ ] Ensure all commonly queried fields have indexes
  - [ ] Add composite indexes where needed (e.g., UserId + CompletedAt)
  - [ ] Use Query().Where() instead of GetAll().Where() in LINQ

- [ ] Optimize guide loading:
  - [ ] Load guide metadata only (no steps) for list view
  - [ ] Load steps only when viewing/editing guide
  - [ ] Project only needed fields in queries

#### Startup Performance
- [ ] Profile app startup time
  - [ ] Measure time from launch to UI interactive
  - [ ] Identify bottlenecks (database init, DI setup, etc.)
  - [ ] Defer non-critical initialization (e.g., sample data seeding)
  - [ ] Target: <2 seconds startup time

#### Performance Tests (10-15 tests)
- [ ] Create `PerformanceTests.cs` in `GuideViewer.Tests/Performance/`
  - [ ] Test: Load 500 guides - measure memory usage (<50MB)
  - [ ] Test: Render guide list with 500 guides - measure time (<1 second)
  - [ ] Test: Search 500 guides by title - measure time (<100ms)
  - [ ] Test: Load guide with 100 steps - measure time (<200ms)
  - [ ] Test: Navigate between pages 50 times - measure memory leak (stable memory)
  - [ ] Test: Image cache - verify LRU eviction works
  - [ ] Test: Database query performance with 1000+ guides (<50ms per query)
  - [ ] Test: Startup time with large database (<2 seconds)
  - [ ] Test: Export/import 100 guides - measure time (<10 seconds)
  - [ ] Test: Progress dashboard with 500 records - measure load time (<500ms)

#### Profiling & Measurement
- [ ] Use Visual Studio Performance Profiler:
  - [ ] CPU Usage profiling - identify hot paths
  - [ ] Memory Usage profiling - identify leaks
  - [ ] .NET Object Allocation Tracking
  - [ ] Database query performance (query times)

- [ ] Document baseline metrics before optimization
- [ ] Document metrics after optimization
- [ ] Verify all NFR-1 performance targets met (spec.md)

#### Manual Testing Checklist
- [ ] Test app with 500+ guides - verify smooth scrolling
- [ ] Test app with 1000+ progress records - verify fast dashboard load
- [ ] Navigate rapidly between pages - verify no lag
- [ ] Monitor memory usage over 30 min session - verify stable
- [ ] Test on low-end hardware (if available)

---

## Phase 6: Animations & Polish - 10% of milestone

**Estimated Time**: 2-3 hours
**Status**: Not Started
**Tests**: Manual visual testing

### Overview
Add subtle animations and transitions to create a polished, professional feel. Focus on Fluent Design principles.

### Tasks

#### Page Transitions
- [ ] Update `NavigationService.cs` - Add page transitions
  - [ ] Implement EntranceNavigationTransitionInfo for forward nav
  - [ ] Implement DrillInNavigationTransitionInfo for detail nav
  - [ ] Implement SuppressNavigationTransitionInfo for back nav (optional)
  - [ ] Configure transition duration (default 200ms)

- [ ] Test transitions between all pages
  - [ ] HomePage → GuidesPage (entrance)
  - [ ] GuidesPage → GuideDetailPage (drill-in)
  - [ ] GuidesPage → GuideEditorPage (drill-in)
  - [ ] ProgressPage → ActiveGuideProgressPage (drill-in)

#### Connected Animations
- [ ] Implement connected animations for guide cards:
  - [ ] **GuidesPage → GuideDetailPage**: Animate guide card to detail view
  - [ ] **GuidesPage → GuideEditorPage**: Animate guide card to editor
  - [ ] Use ConnectedAnimation APIs
  - [ ] Ensure smooth 60fps animation

- [ ] Implement connected animations for category badges (optional)

#### Implicit Animations
- [ ] Add subtle animations to UI elements:
  - [ ] **Button Hover**: Slight scale up (1.05x) with spring animation
  - [ ] **Card Hover**: Lift effect (increase shadow)
  - [ ] **List Item Add**: Fade in + slide from top
  - [ ] **List Item Remove**: Fade out + slide to right
  - [ ] Use Composition APIs or implicit animations

#### Loading Animations
- [ ] Replace static ProgressRing with animated variants:
  - [ ] Custom loading animations for long operations (optional)
  - [ ] Skeleton screens for guide list loading (optional)
  - [ ] Progress bar animations for step completion

#### Fluent Design Enhancements
- [ ] Add Acrylic background to dialog boxes (ContentDialog)
- [ ] Add Reveal highlight to buttons on hover
- [ ] Ensure Mica background in MainWindow (already implemented)
- [ ] Add shadow depth to cards for hierarchy

#### Visual Polish
- [ ] Review spacing and alignment across all pages
  - [ ] Ensure consistent padding (8px, 12px, 16px, 24px multiples)
  - [ ] Ensure consistent card corner radius (8px)
  - [ ] Ensure consistent font sizes (Caption, Body, Subtitle, Title)

- [ ] Review color usage:
  - [ ] Accent color used consistently for primary actions
  - [ ] Error color (red) for destructive actions
  - [ ] Success color (green) for confirmations
  - [ ] Warning color (yellow) for cautions

- [ ] Review icons:
  - [ ] Ensure all buttons have appropriate Segoe Fluent Icons
  - [ ] Ensure icon sizes consistent (16px for inline, 20px for buttons)

#### Accessibility Animations
- [ ] Respect user's "Reduce Motion" preference:
  - [ ] Check UISettings.AnimationsEnabled
  - [ ] Disable or simplify animations if user prefers reduced motion
  - [ ] Ensure app still usable without animations

#### Manual Testing Checklist
- [ ] Verify all page transitions are smooth (60fps)
- [ ] Verify connected animations work for guide cards
- [ ] Verify hover effects on buttons and cards
- [ ] Verify loading animations display during operations
- [ ] Test with reduced motion setting enabled
- [ ] Test on low-end GPU (if available)
- [ ] Verify animations don't cause performance issues
- [ ] Get feedback on animation timing (not too fast/slow)

---

## Testing Summary

**Target**: 45-60 new tests
**Current Status**: 0/45-60 tests

### Breakdown by Phase
- Phase 1 (Data Management): 20-28 tests (unit + integration)
- Phase 2 (About Page): Manual UI testing
- Phase 3 (Keyboard Shortcuts): Manual testing
- Phase 4 (Error Handling): 5-10 tests + manual
- Phase 5 (Performance): 10-15 tests + profiling
- Phase 6 (Animations): Manual visual testing

### Total Project Tests After Milestone 4
- Milestone 1: 24 tests ✅
- Milestone 2: 87 tests ✅
- Milestone 3: 96 tests ✅
- Milestone 4: 45-60 tests (target)
- **Grand Total**: 252-267 tests (target)

---

## Success Criteria

Milestone 4 is considered complete when:
- [ ] All 6 phases completed (checkboxes marked)
- [ ] 45-60 tests passing (35-60 new tests)
- [ ] All integration tests validating workflows
- [ ] Performance tests meeting all NFR-1 targets (spec.md)
- [ ] Manual testing checklist completed for all features
- [ ] No critical or high severity bugs remaining
- [ ] Documentation updated in CLAUDE.md and PATTERNS.md
- [ ] App feels polished and production-ready

---

## Notes

### Key Architectural Decisions
1. **Export Format**: JSON for human readability, ZIP for images
2. **Backup Strategy**: Full database copy with metadata, compressed
3. **Virtualization**: ItemsRepeater with virtual layout for large lists
4. **Error Handling**: Global service + local validation feedback
5. **Animations**: Respect user preferences (reduced motion)

### File Format Specifications

**Export JSON Schema** (single guide):
```json
{
  "version": "1.0",
  "exportDate": "2025-11-17T10:30:00Z",
  "guide": {
    "id": "...",
    "title": "...",
    "description": "...",
    "category": "...",
    "estimatedMinutes": 30,
    "steps": [
      {
        "order": 1,
        "title": "...",
        "instructionsRtf": "...",
        "imageBase64": "..." // Optional
      }
    ]
  }
}
```

**Backup File Structure** (ZIP):
```
backup_2025-11-17_103000.zip
├── data.db          (LiteDB database file)
└── metadata.json    (BackupInfo)
```

### Performance Targets (NFR-1 from spec.md)
- Startup time: <2 seconds
- Guide list load: <500ms
- Step navigation: <100ms
- Memory usage: <150MB
- Support 1000 steps per guide without degradation

### Keyboard Shortcuts Reference
See Phase 3 tasks for complete list of shortcuts.

### Third-Party Licenses for About Page
- WinUI 3 (Microsoft) - MIT License
- LiteDB - MIT License
- Serilog - Apache 2.0 License
- CommunityToolkit.Mvvm - MIT License

---

## References

- [CLAUDE.md](CLAUDE.md) - Project overview and architecture
- [spec.md](spec.md) - Milestone 4 requirements (lines 375-414)
- [PATTERNS.md](PATTERNS.md) - Development patterns
- [WinUI 3 Performance Best Practices](https://learn.microsoft.com/en-us/windows/apps/develop/performance/)
- [WinUI 3 Connected Animations](https://learn.microsoft.com/en-us/windows/apps/design/motion/connected-animation)

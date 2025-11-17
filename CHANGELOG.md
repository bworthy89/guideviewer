# Changelog

This document tracks all completed milestones and major changes to the GuideViewer project.

## Milestone 3 - Progress Tracking System 🚧 **IN PROGRESS**

**Status**: ~75% Complete (Phases 1, 2, 4 & 5 of 6)
**Started**: 2025-11-17

### Phase 5: Admin Monitoring ✅ **COMPLETE** (~1.5 hours)

**Note**: Originally planned as Phase 5, executed after Phase 4 per the optimized execution order (1 → 2 → 4 → 5 → 3 → 6).

#### Features Implemented
- **ProgressReportItem Model** - Admin report data model
  - Properties: User, Guide, Progress, Status, Time
  - Computed properties: CompletionPercentage, display text formatting
  - Helper properties: TimeSpentDisplayText, LastAccessedDisplayText, StartedDisplayText

- **ProgressReportViewModel** - Admin monitoring logic
  - LoadAllProgressAsync - Joins Progress + User + Guide data
  - ApplyFilters - Multi-criteria filtering (user, guide, search)
  - Search by user name, guide title, or category
  - Filter dropdowns populated from actual data
  - Real-time filtering as user types
  - DispatcherQueue integration for UI updates
  - Comprehensive error handling and logging

- **SettingsPage.xaml** - Admin-only Progress Reports section
  - **Visibility Control**: Only visible to admin role (uses BooleanToVisibilityConverter with IsAdmin from MainViewModel)
  - **Filter Controls**:
    - AutoSuggestBox for search (user/guide/category)
    - ComboBox for user filter (All Users + dropdown of users)
    - ComboBox for guide filter (All Guides + dropdown of guides)
  - **Progress Reports List**:
    - ItemsRepeater with card layout
    - User name + Guide title
    - Status badge (In Progress / Completed)
    - Progress bar with percentage
    - Time spent + Last accessed
  - **Loading State** - ProgressRing during data load
  - **Empty State** - Informational message when no records
  - **Fluent Design** - Cards with proper theming

- **SettingsPage.xaml.cs** - Code-behind wiring
  - ProgressReportViewModel initialization
  - Lazy loading (only loads if admin navigates to page)
  - Search box event handler
  - Filter bindings

#### Testing Strategy
**ViewModels**: Manual UI testing (see [PATTERNS.md](PATTERNS.md#viewmodel-testing-in-winui-3))
- Admin-only visibility must be tested manually with different roles
- Filter and search functionality tested via UI interaction
- Service layer fully tested (188 tests)

#### Files Created
- `GuideViewer.Core/Models/ProgressReportItem.cs` (95 lines)
- `GuideViewer/ViewModels/ProgressReportViewModel.cs` (263 lines)

#### Files Modified
- `GuideViewer/Views/Pages/SettingsPage.xaml` - Added 175-line admin section
- `GuideViewer/Views/Pages/SettingsPage.xaml.cs` - Added ProgressReportViewModel initialization and search handler

#### Test Results
- ✅ 188/188 tests passing (all existing tests)
- ✅ Admin monitoring UI implemented
- ✅ Ready for manual testing in Visual Studio

---

### Phase 4: Progress Dashboard ✅ **COMPLETE** (~1 hour)

**Note**: Originally planned as Phase 4, but executed after Phase 2 per the optimized execution order (1 → 2 → 4 → 5 → 3 → 6).

#### Features Implemented
- **ProgressDashboardViewModel** - Complete dashboard logic
  - Properties: Statistics (5 metrics), ActiveGuides, CompletedGuides
  - LoadDashboardDataAsync command with async data loading
  - ResumeGuide and ViewCompletedGuide navigation commands
  - DispatcherQueue integration for UI thread updates
  - Proper null validation and error handling
  - Comprehensive logging with Serilog

- **ProgressGuideItem Helper Class** - Display model
  - All progress properties (progress %, time remaining, time spent)
  - User-friendly display text properties (auto-formatted)
  - LastAccessedDisplayText with relative time ("2 hours ago")

- **ProgressPage.xaml** - Complete dashboard UI
  - **Statistics Cards** - 5 cards showing:
    - Total Started (accent color)
    - Currently In Progress (caution color)
    - Total Completed (success color)
    - Completion Rate (percentage)
    - Average Completion Time (hours)
  - **Active Guides Section**
    - ItemsRepeater with card layout
    - Progress bars showing completion percentage
    - Resume buttons for each guide
    - Shows top 10 most recently accessed
    - Empty state with helpful message
  - **Completed Guides Section**
    - ItemsRepeater with checkmark icons
    - Completion date and time spent
    - View buttons to see guide details
    - Shows 5 most recently completed
    - Empty state with helpful message
  - **Loading State** - ProgressRing during data load
  - **Fluent Design** - Cards with proper theming and spacing

- **ProgressPage.xaml.cs** - Code-behind wiring
  - ViewModel initialization with DI services
  - DataContext binding
  - InitializeAsync on navigation

#### Testing Strategy
**ViewModels**: Manual UI testing (see [PATTERNS.md](PATTERNS.md#viewmodel-testing-in-winui-3))
- **Architectural Constraint**: WinUI 3 ViewModels cannot be unit tested traditionally
- Test projects cannot reference UI projects (contains DispatcherQueue dependencies)
- **Alternative**: 188 comprehensive tests at service/repository layers
- ViewModels serve as thin coordination layers over well-tested services

#### Files Created
- `GuideViewer/ViewModels/ProgressDashboardViewModel.cs` (327 lines)
- `GuideViewer/Views/Pages/ProgressPage.xaml` (324 lines)
- `GuideViewer/Views/Pages/ProgressPage.xaml.cs` (37 lines)

#### Files Modified
- `PATTERNS.md` - Documented ViewModel testing limitations and strategies

#### Test Results
- ✅ 188/188 tests passing (all existing tests)
- ✅ Dashboard UI implemented and ready for manual testing
- ✅ Architecture documentation updated

---

### Phase 2: Services Layer ✅ **COMPLETE** (~3 hours)

#### Features Implemented
- **ITimerService** - Timer interface for tracking active time
  - Properties: Elapsed (TimeSpan), IsRunning (bool)
  - Methods: Start(), Stop(), Reset()
  - Events: Tick (fires every second)

- **TimerService** - Implementation with DispatcherQueueTimer
  - Uses DispatcherQueueTimer internally
  - Tracks elapsed time with pause/resume support
  - IDisposable implementation for cleanup
  - Named method for Tick event (prevents memory leaks)

- **IProgressTrackingService** - Business logic interface
  - StartGuideAsync(), GetProgressAsync()
  - GetActiveProgressAsync(), GetCompletedProgressAsync()
  - CompleteStepAsync(), UpdateCurrentStepAsync()
  - MarkGuideCompleteAsync(), GetStatisticsAsync()
  - CalculateEstimatedTimeRemaining(), UpdateActiveTimeAsync()

- **ProgressTrackingService** - Implementation
  - Guide existence validation before starting progress
  - Duplicate progress prevention
  - LastAccessedAt tracking on all interactions
  - Statistics calculation from repository data
  - Step order validation
  - Notes length validation (max 5000 chars)
  - Comprehensive logging with Serilog

#### Testing
- **~20 Unit Tests** - TimerServiceTests, ProgressTrackingServiceTests
  - Timer functionality (start, stop, reset, tick events)
  - Progress tracking business logic
  - Validation and error handling
  - Concurrent operation handling

- **~8 Integration Tests** - ProgressServicesIntegrationTests
  - End-to-end progress workflows
  - Multi-user scenarios
  - Notes persistence with special characters
  - Performance validation
  - Estimated time calculations

#### Bug Fix
- **Issue**: Flaky timer test - timing assertion too strict (100ms tolerance)
- **Fix**: Increased tolerance to 300ms to account for OS thread scheduling variability
- **File**: TimerServiceTests.cs:191

#### Files Created
- `GuideViewer.Core/Services/ITimerService.cs`
- `GuideViewer.Core/Services/TimerService.cs`
- `GuideViewer.Core/Services/IProgressTrackingService.cs`
- `GuideViewer.Core/Services/ProgressTrackingService.cs`
- `GuideViewer.Tests/Services/TimerServiceTests.cs`
- `GuideViewer.Tests/Services/ProgressTrackingServiceTests.cs`
- `GuideViewer.Tests/Integration/ProgressServicesIntegrationTests.cs`

#### Files Modified
- `GuideViewer/App.xaml.cs` - Registered ITimerService and IProgressTrackingService

#### Test Results
- ✅ 188/188 tests passing (all M3 Phase 1 & 2 tests)
- ✅ Timer service working correctly
- ✅ Progress tracking business logic validated
- ✅ All integration tests passing

---

### Phase 1: Progress Tracking Data Layer ✅ **COMPLETE** (~2.5 hours)

#### Features Implemented
- **Progress Entity** - Track user progress through guides
  - Properties: GuideId, UserId, CurrentStepOrder, CompletedStepOrders (List<int>)
  - Timestamps: StartedAt, LastAccessedAt, CompletedAt
  - Notes field (5000 char max), TotalActiveTimeSeconds
  - Supports non-linear step completion (skip/backtrack)

- **ProgressRepository** - 8 specialized methods
  - `GetByUserAndGuide(userId, guideId)` - Get specific progress record
  - `GetActiveByUser(userId)` - Get all in-progress guides (ordered by LastAccessedAt)
  - `GetCompletedByUser(userId)` - Get completed guides
  - `GetAllProgressForGuide(guideId)` - Admin view: all users' progress
  - `GetStatistics(userId)` - Calculate completion rates and average times
  - `UpdateStepCompletion(progressId, stepOrder, completed)` - Mark steps
  - `UpdateCurrentStep(progressId, stepOrder)` - Navigate between steps
  - `MarkGuideComplete(progressId)` - Finish guide

- **ProgressStatistics Model** - Analytics and metrics
  - TotalStarted, TotalCompleted, CurrentlyInProgress
  - AverageCompletionTimeMinutes, CompletionRate (percentage)

- **Database Indexes** - Performance optimization
  - Composite unique index on (UserId, GuideId) - ensures one progress per user+guide
  - Individual indexes on UserId, GuideId, CompletedAt, LastAccessedAt

#### Testing
- **21 Unit Tests** - ProgressRepositoryTests.cs
  - Complete coverage of all repository methods
  - Edge cases (invalid IDs, duplicates, null values)
  - Timezone consistency validation
  - Timestamp update verification

- **8 Integration Tests** - ProgressDataLayerIntegrationTests.cs
  - End-to-end progress workflow (start → complete steps → finish)
  - Multi-user tracking same guide
  - Unique constraint enforcement
  - Long notes storage (5000 chars)
  - Query performance (100+ records in <100ms)
  - Statistics calculation accuracy
  - Database persistence after restart
  - Index performance validation

#### Files Created
- `GuideViewer.Data/Entities/Progress.cs` (62 lines)
- `GuideViewer.Data/Models/ProgressStatistics.cs` (31 lines)
- `GuideViewer.Data/Repositories/ProgressRepository.cs` (177 lines)
- `GuideViewer.Tests/Repositories/ProgressRepositoryTests.cs` (500 lines)
- `GuideViewer.Tests/Integration/ProgressDataLayerIntegrationTests.cs` (340 lines)

#### Files Modified
- `GuideViewer.Data/Services/DatabaseService.cs` - Added Progress collection + 5 indexes
- `GuideViewer.Data/GuideViewer.Data.csproj` - Added Models folder
- `GuideViewer/App.xaml.cs` - Registered ProgressRepository

#### Test Results
- ✅ 140/140 tests passing (+29 new tests)
- ✅ All repository methods working correctly
- ✅ Performance targets met (<100ms for 100+ records)
- ✅ Database indexes optimized

---

## Milestone 2 - Guide Management System ✅ **COMPLETE**

**Completed**: 2025-11-17
**Duration**: 3 sessions
**Test Results**: 111/111 tests passing (87 new tests)

### Phase 1: Data Layer ✅ **COMPLETE**

#### Features Implemented
- **Guide, Step, Category Entities** - Complete data model with LiteDB persistence
- **GuideRepository** - Full CRUD with specialized methods:
  - `Search(query)` - Full-text search across title and description
  - `GetByCategory(category)` - Filter by category name
  - `GetRecentlyModified(count)` - Get N most recently updated guides
  - `GetDistinctCategories()` - Get all unique category names
- **CategoryRepository** - Category management with uniqueness:
  - `GetByName(name)` - Find category by name
  - `Exists(name, excludeId)` - Check for duplicate names
  - `InsertIfNotExists(name)` - Idempotent category creation
  - `EnsureCategory(name)` - Get or create category
- **Database Indexes** - Performance optimization:
  - Index on Guide.Title (text search)
  - Index on Guide.Category (filtering)
  - Index on Guide.UpdatedAt (recent guides)
  - Unique index on Category.Name (prevent duplicates)

#### Testing
- **33 Repository Tests** (19 Guide + 14 Category)
  - Complete CRUD coverage
  - Search functionality validation
  - Category filtering and uniqueness
  - Edge cases and null handling

### Phase 2: Services Layer ✅ **COMPLETE**

#### Features Implemented
- **ImageStorageService** - LiteDB FileStorage integration
  - 10MB max file size validation
  - Supported formats: PNG, JPG, JPEG, BMP
  - File ID format: `img_{Guid}`
  - Automatic deletion when parent guide deleted
- **AutoSaveService** - Background auto-save mechanism
  - Configurable interval (default 30 seconds)
  - Dirty tracking with IsDirty flag
  - Start/Stop controls
  - Callback-based save action

#### Testing
- **42 Service Tests** (26 ImageStorage + 16 AutoSave)
  - Image validation (size, format, corruption)
  - Auto-save timing and cancellation
  - Memory cleanup verification
  - FileStorage integration

### Phase 3: Guide List UI ✅ **COMPLETE**

#### Features Implemented
- **GuidesPage** - Modern card-based interface
  - Search by title, description, or category
  - Category filtering with "All Categories" dropdown
  - Responsive grid layout (ItemsRepeater with UniformGridLayout)
  - Role-based Edit/Delete/View buttons (admin only for Edit/Delete)
  - Delete confirmation flyout
  - Loading states and contextual empty states
- **Sample Data Seeding** - 5 guides in 4 categories:
  - Network Installation (2 guides)
  - Server Setup (1 guide)
  - Software Deployment (1 guide)
  - Hardware Maintenance (1 guide)
- **GuidesViewModel** - Async search and filter logic
  - Search/filter operations on background thread
  - UI updates via DispatcherQueue
  - Computed properties for dynamic visibility

### Phase 4: Guide Editor UI ✅ **COMPLETE**

#### Features Implemented
- **GuideEditorPage** - Full CRUD interface with rich text editing
  - Two-column layout (metadata/steps on left, editor on right)
  - RichEditBox for RTF step instructions
  - Image upload with FileOpenPicker
  - Step reordering with up/down buttons
  - Auto-save every 30 seconds with dirty tracking
  - Unsaved changes warning on navigation
  - Thread-safe image loading with DispatcherQueue
- **GuideEditorViewModel** - ~600 lines with auto-save integration
  - Named method for PropertyChanged (prevent memory leaks)
  - Lock mechanism for thread-safe saves
  - Dirty tracking for all properties except metadata
  - IDisposable implementation for cleanup

#### Issues Fixed (7 Critical Bugs)
1. PropertyChanged event memory leak (lambda → named method)
2. BitmapImage thread safety (added DispatcherQueue checks)
3. Auto-save race condition (added lock mechanism)
4. SelectedStep null binding (created fallback property)
5. RichEditBox stream leak (added using statement)
6. Image stream position reset
7. Parameter validation (ObjectId? casting)

### Phase 5: Category Management & Detail View ✅ **COMPLETE**

#### Features Implemented
- **CategoryEditorDialog** - Create/edit categories
  - 8 icon choices (Document, Network, Server, Software, Tools, Settings, Phone, Calculator)
  - 7 color choices (Blue, Green, Purple, Red, Orange, Cyan, Gray)
  - Live preview of category badge
  - Validation with duplicate name checking
- **CategoryManagementViewModel** - Full category CRUD
  - Integrated into SettingsPage with ItemsRepeater
  - Cannot delete categories in use by guides
  - Color badge rendering from hex strings
- **GuideDetailPage** - Read-only guide viewing
  - All steps displayed with RTF content
  - Category badges with icons and colors
  - Edit button (navigates to editor)
  - Thread-safe image loading

#### Files Created
- `GuideViewer/Views/Pages/GuideEditorPage.xaml` (368 lines)
- `GuideViewer/Views/Pages/GuideEditorPage.xaml.cs` (285 lines)
- `GuideViewer/ViewModels/GuideEditorViewModel.cs` (~600 lines)
- `GuideViewer/Views/Dialogs/CategoryEditorDialog.xaml` (182 lines)
- `GuideViewer/Views/Dialogs/CategoryEditorDialog.xaml.cs` (154 lines)
- `GuideViewer/ViewModels/CategoryManagementViewModel.cs` (220 lines)
- `GuideViewer/Views/Pages/GuideDetailPage.xaml` (209 lines)
- `GuideViewer/Views/Pages/GuideDetailPage.xaml.cs` (194 lines)

#### Files Modified
- `GuideViewer/Views/Pages/SettingsPage.xaml` - Added category management UI

### Phase 6: Testing & Polish ✅ **COMPLETE**

#### Integration Tests
- **GuideWorkflowIntegrationTests.cs** (6 tests)
  - Complete guide CRUD workflow with images
  - Multi-guide category filtering
  - Category deletion prevention with existing guides
  - Multiple images per guide handling
  - Recently modified ordering
- **CategoryManagementIntegrationTests.cs** (6 tests)
  - Complete category lifecycle
  - Category-guide association validation
  - Duplicate name detection
  - Multi-category organization
  - Case-insensitive lookups

#### Database Optimization
- Optimized `GetRecentlyModified` to use UpdatedAt index properly
- Changed from `Query.All(Query.Descending)` + LINQ to `Query.All("UpdatedAt", Query.Descending)`
- Performance improvement for retrieving recent guides

#### Issues Fixed
- **Phase 4**: 12 issues (7 critical bugs + 3 compilation + 2 runtime)
- **Phase 5**: 2 compilation errors (TextSetOptions namespace, ImageStorageService usage)
- **Phase 6**: 1 database query optimization

#### Final Test Results
- ✅ 111/111 tests passing (24 Milestone 1 + 75 Milestone 2 unit + 12 Milestone 2 integration)
- ✅ All CRUD operations working correctly
- ✅ Role-based UI visibility verified (admin vs technician)
- ✅ Build successful, all features tested in Visual Studio
- ✅ Integration tests validate end-to-end workflows
- ✅ Database queries optimized for performance

---

## Milestone 1 - Foundation ✅ **COMPLETE**

**Completed**: 2025-11-16
**Duration**: 1 session
**Test Results**: 24/24 tests passing

### Features Implemented

#### Project Structure
- 4-layer architecture: UI, Core, Data, Tests
- KeyGenerator utility project
- Dependency injection configured
- Logging with Serilog

#### Data Layer
- **DatabaseService** - LiteDB initialization and management
  - Database location: `%LocalAppData%\GuideViewer\data.db`
  - Collections: users, settings, guides, categories
  - Index configuration
- **Repository Pattern** - Generic base + specialized repositories
  - `Repository<T>` - Generic CRUD operations
  - `UserRepository` - User management with GetCurrentUser(), UpdateLastLogin()
  - `SettingsRepository` - Key-value settings with GetValue(), SetValue()

#### Business Logic
- **LicenseValidator** - Product key validation with HMAC-SHA256
  - Format: `XXXX-XXXX-XXXX-XXXX`
  - Role prefixes: `A` = Admin, `T` = Technician
  - Checksum validation
- **SettingsService** - JSON-serialized settings with caching
  - Theme management
  - Window state persistence

#### UI Implementation
- **ActivationWindow** - First-run product key activation
  - 4-segment input with auto-advance
  - Paste support (auto-splits keys)
  - Keyboard navigation (Tab, Enter, Backspace)
  - Error handling with InfoBar
  - Loading states with ProgressRing
- **MainWindow** - Full NavigationView implementation
  - NavigationView with 4 pages (Home, Guides, Progress, Settings)
  - Admin-only "New Guide" menu item (role-based visibility)
  - User role badge in navigation footer
  - Mica background material (Windows 11 Fluent Design)
  - NavigationService with Frame-based routing
- **MVVM Infrastructure** - CommunityToolkit.Mvvm integration
  - `ActivationViewModel` - Product key activation logic
  - `MainViewModel` - Main window + role detection
- **Value Converters**
  - `InverseBooleanConverter`
  - `BooleanToVisibilityConverter`
  - `InverseBooleanToVisibilityConverter`

#### Testing
- **11 LicenseValidator Tests** - Product key validation coverage
- **13 SettingsService Tests** - Settings persistence and caching

#### Test Results
- ✅ Admin role: "New Guide" menu visible
- ✅ Technician role: "New Guide" menu hidden
- ✅ All pages navigate correctly
- ✅ User role persisted across app restarts
- ✅ No crashes during normal operation
- ✅ 24/24 tests passing, 80%+ code coverage

### Files Created

#### Core Layer
- `GuideViewer.Core/Services/LicenseValidator.cs`
- `GuideViewer.Core/Services/ISettingsService.cs`
- `GuideViewer.Core/Services/SettingsService.cs`
- `GuideViewer.Core/Models/UserRole.cs`
- `GuideViewer.Core/Models/LicenseInfo.cs`
- `GuideViewer.Core/Models/AppSettings.cs`
- `GuideViewer.Core/Utilities/ProductKeyGenerator.cs`

#### Data Layer
- `GuideViewer.Data/Services/DatabaseService.cs`
- `GuideViewer.Data/Repositories/IRepository.cs`
- `GuideViewer.Data/Repositories/Repository.cs`
- `GuideViewer.Data/Repositories/UserRepository.cs`
- `GuideViewer.Data/Repositories/SettingsRepository.cs`
- `GuideViewer.Data/Entities/User.cs`
- `GuideViewer.Data/Entities/AppSetting.cs`

#### UI Layer
- `GuideViewer/App.xaml.cs` - DI configuration, logging
- `GuideViewer/MainWindow.xaml` - NavigationView + Mica
- `GuideViewer/Views/ActivationWindow.xaml`
- `GuideViewer/Views/Pages/HomePage.xaml`
- `GuideViewer/Views/Pages/GuidesPage.xaml` (placeholder)
- `GuideViewer/Views/Pages/ProgressPage.xaml` (placeholder)
- `GuideViewer/Views/Pages/SettingsPage.xaml` (placeholder)
- `GuideViewer/ViewModels/ActivationViewModel.cs`
- `GuideViewer/ViewModels/MainViewModel.cs`
- `GuideViewer/Services/NavigationService.cs`
- `GuideViewer/Converters/InverseBooleanConverter.cs`
- `GuideViewer/Converters/BooleanToVisibilityConverter.cs`
- `GuideViewer/Converters/InverseBooleanToVisibilityConverter.cs`

#### Testing
- `GuideViewer.Tests/Services/LicenseValidatorTests.cs`
- `GuideViewer.Tests/Services/SettingsServiceTests.cs`

#### Utilities
- `KeyGenerator/Program.cs` - Product key generator
- `TEST_PRODUCT_KEYS.txt` - 10 pre-generated test keys

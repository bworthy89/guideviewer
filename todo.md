# GuideViewer - Milestone 2 Todo List

**Milestone**: Guide Data Model & Admin CRUD (Week 3-4)
**Goal**: Implement complete guide management functionality for administrators

**STATUS**: 🔵 **IN PROGRESS** - 45% Complete (Started: 2025-11-16)

---

## Overview

Milestone 2 focuses on building the core guide management system for administrators. This includes:
- Complete data model for guides, steps, and categories
- CRUD operations with LiteDB persistence
- Guide creation and editing UI with rich text support
- Image upload and storage
- Drag-and-drop step reordering
- Search and filtering functionality
- Auto-save mechanism

**Prerequisites**: ✅ Milestone 1 complete (Authentication, Navigation, Settings)

---

## 📊 Acceptance Criteria

- [ ] Admins can create new guides with title, category, and description
- [ ] Admins can add unlimited steps to guides with rich text
- [ ] Admins can upload and embed images in step descriptions
- [ ] Admins can reorder steps via drag-and-drop
- [x] Admins can search guides by title or category *(Phase 3 complete)*
- [ ] Changes are automatically saved without user action *(service ready, needs UI integration)*
- [x] Guides persist correctly in LiteDB *(33 repository tests passing)*
- [x] Guide deletion requires confirmation *(flyout implemented in GuidesPage)*
- [ ] Technicians can only view guides (read-only, no edit)
- [x] All CRUD operations have appropriate error handling *(tested in repositories)*

---

## Tasks Breakdown

### 🗄️ Data Layer - Entities & Repositories ✅ **COMPLETE**

#### Entities
- [x] Create `Guide` entity (`GuideViewer.Data/Entities/Guide.cs`)
  - [x] Id (ObjectId)
  - [x] Title (string)
  - [x] Description (string)
  - [x] Category (string)
  - [x] EstimatedMinutes (int)
  - [x] Steps (List&lt;Step&gt;)
  - [x] CreatedAt (DateTime)
  - [x] UpdatedAt (DateTime)
  - [x] CreatedBy (string - user role/id)

- [x] Create `Step` entity (`GuideViewer.Data/Entities/Step.cs`)
  - [x] Id (string/Guid)
  - [x] Order (int)
  - [x] Title (string)
  - [x] Content (string - RTF or HTML)
  - [x] ImageIds (List&lt;string&gt; - LiteDB file IDs)

- [x] Create `Category` entity (`GuideViewer.Data/Entities/Category.cs`)
  - [x] Id (ObjectId)
  - [x] Name (string)
  - [x] Description (string)
  - [x] IconGlyph (string)
  - [x] CreatedAt (DateTime)

#### Repositories
- [x] Create `GuideRepository` (`GuideViewer.Data/Repositories/GuideRepository.cs`)
  - [x] GetById(ObjectId id)
  - [x] GetAll()
  - [x] Search(string query) - searches title, description, category
  - [x] GetByCategory(string category)
  - [x] Insert(Guide guide)
  - [x] Update(Guide guide)
  - [x] Delete(ObjectId id)
  - [x] GetRecentlyModified(int count)

- [x] Create `CategoryRepository` (`GuideViewer.Data/Repositories/CategoryRepository.cs`)
  - [x] GetAll()
  - [x] GetByName(string name)
  - [x] Insert(Category category)
  - [x] Update(Category category)
  - [x] Delete(ObjectId id)
  - [x] Exists(string name)

#### Database Configuration
- [x] Update `DatabaseService` to initialize guides collection
- [x] Update `DatabaseService` to initialize categories collection
- [x] Add indexes for guide search (title, category)
- [x] Configure LiteDB FileStorage for images
- [x] Test database operations (CRUD + search)

**Tests**: 33 passing (19 GuideRepository + 14 CategoryRepository)

---

### 🖼️ Services - Image & File Management ✅ **COMPLETE**

- [x] Create `ImageStorageService` (`GuideViewer.Core/Services/ImageStorageService.cs`)
  - [x] UploadImage(Stream imageStream, string fileName) → returns fileId
  - [x] GetImage(string fileId) → returns Stream
  - [x] DeleteImage(string fileId)
  - [x] GetImageMetadata(string fileId) → returns size, type, upload date
  - [x] ValidateImage(Stream imageStream) → checks size, format
  - [x] Max image size: 10MB
  - [x] Supported formats: PNG, JPG, JPEG, BMP

- [x] Create `AutoSaveService` (`GuideViewer.Core/Services/AutoSaveService.cs`)
  - [x] StartAutoSave(Action saveCallback, int intervalSeconds = 30)
  - [x] StopAutoSave()
  - [x] IsDirty property (tracks if changes need saving)
  - [x] LastSavedAt property (DateTime)
  - [x] ManualSave() method

- [x] Register services in DI container (`App.xaml.cs`)
  - [x] ImageStorageService (Singleton)
  - [x] AutoSaveService (Transient - one per editor instance)
  - [x] GuideRepository (Transient)
  - [x] CategoryRepository (Transient)

**Tests**: 42 passing (26 ImageStorageService + 16 AutoSaveService)

---

### 🎨 UI - Guide List Page ✅ **COMPLETE**

**File**: `GuideViewer/Views/Pages/GuidesPage.xaml` (updated from placeholder)

- [x] Design guide list UI layout
  - [x] Search TextBox with search icon (AutoSuggestBox)
  - [x] Category filter ComboBox
  - [x] "New Guide" button (admin only, already exists in nav)
  - [x] Grid of guide cards (responsive: 1-3 columns with ItemsRepeater)

- [x] Create guide card component
  - [x] Guide thumbnail/icon (default if no image)
  - [x] Guide title (bold, SubtitleTextBlockStyle)
  - [x] Category badge with accent color
  - [x] Step count (e.g., "12 steps")
  - [x] Estimated time (e.g., "~45 min")
  - [x] Last modified date *(not displayed, but tracked in entity)*
  - [x] Edit button (admin only)
  - [x] Delete button (admin only)
  - [x] Hover animation (card elevation) *(uses CardBackgroundFillColorDefaultBrush)*

- [x] Implement search functionality
  - [x] Real-time search on QuerySubmitted
  - [x] Search by title, description, category
  - [x] Clear search button

- [x] Implement category filtering
  - [x] "All Categories" option
  - [x] Dynamic category list from database
  - [x] Update guide list on category change

- [x] Create `GuidesViewModel` (`GuideViewer/ViewModels/GuidesViewModel.cs`)
  - [x] ObservableCollection&lt;Guide&gt; Guides
  - [x] ObservableCollection&lt;Category&gt; Categories
  - [x] string SearchQuery
  - [x] Category SelectedCategory
  - [x] bool IsLoading
  - [x] CreateGuideCommand (navigates to editor) *(placeholder)*
  - [x] EditGuideCommand(Guide guide) *(placeholder)*
  - [x] DeleteGuideCommand(Guide guide)
  - [x] SearchCommand (filters guides)
  - [x] LoadGuidesAsync()
  - [x] LoadCategoriesAsync() *(combined with LoadGuidesAsync)*

- [x] Implement loading states
  - [x] ProgressRing while loading guides
  - [x] Empty state with contextual messages
  - [x] Sample data seeded for testing (SampleDataSeeder.cs)

**Files Created/Modified**:
- `GuidesViewModel.cs` (393 lines) - Complete search/filter logic
- `GuidesPage.xaml` (252 lines) - Card-based UI with ItemsRepeater
- `GuidesPage.xaml.cs` (64 lines) - Event handlers
- `SampleDataSeeder.cs` (287 lines) - 5 guides, 4 categories

---

### ✏️ UI - Guide Editor Page

**Files**:
- `GuideViewer/Views/Pages/GuideEditorPage.xaml`
- `GuideViewer/Views/Pages/GuideEditorPage.xaml.cs`

#### Editor Layout
- [ ] Create two-column layout
  - [ ] Left column: Guide metadata + step list
  - [ ] Right column: Step editor with rich text

- [ ] Guide metadata section (left top)
  - [ ] Title TextBox (required, max 100 chars)
  - [ ] Description TextBox (multiline, max 500 chars)
  - [ ] Category ComboBox with "Add New" option
  - [ ] Estimated time NumberBox (minutes)
  - [ ] Save status indicator ("Saved" / "Saving..." / "Unsaved changes")

- [ ] Step list section (left bottom)
  - [ ] ListView with numbered steps
  - [ ] Step title preview
  - [ ] Drag handle for reordering
  - [ ] Delete step button (X icon)
  - [ ] "Add Step" button at bottom
  - [ ] Selected step highlight

- [ ] Step editor section (right)
  - [ ] Step title TextBox
  - [ ] RichEditBox for step content
    - [ ] Toolbar: Bold, Italic, Underline, Bullets, Numbering
    - [ ] Font size selector (12pt, 14pt, 16pt, 18pt)
    - [ ] Text color picker (optional)
  - [ ] Image management
    - [ ] "Add Image" button
    - [ ] Image preview gallery (thumbnails)
    - [ ] Delete image button per thumbnail
    - [ ] Image upload with validation

- [ ] Editor toolbar (sticky header)
  - [ ] Back button (navigate to guide list)
  - [ ] Guide title display (breadcrumb)
  - [ ] Save button (manual save)
  - [ ] Preview button (shows guide as technician would see it)
  - [ ] Delete guide button (with confirmation)

#### Editor ViewModel
- [ ] Create `GuideEditorViewModel` (`GuideViewer/ViewModels/GuideEditorViewModel.cs`)
  - [ ] Guide CurrentGuide
  - [ ] Step SelectedStep
  - [ ] ObservableCollection&lt;Step&gt; Steps
  - [ ] ObservableCollection&lt;Category&gt; Categories
  - [ ] bool IsDirty
  - [ ] bool IsSaving
  - [ ] DateTime? LastSavedAt
  - [ ] string SaveStatus ("Saved" / "Saving..." / "Unsaved changes")
  - [ ] SaveCommand (manual save)
  - [ ] AddStepCommand
  - [ ] DeleteStepCommand(Step step)
  - [ ] SelectStepCommand(Step step)
  - [ ] ReorderStepsCommand(int oldIndex, int newIndex)
  - [ ] AddImageCommand
  - [ ] DeleteImageCommand(string imageId)
  - [ ] DeleteGuideCommand (with confirmation)
  - [ ] NavigateBackCommand (check for unsaved changes)
  - [ ] LoadGuideAsync(ObjectId guideId) - for editing
  - [ ] InitializeNewGuide() - for creating
  - [ ] AutoSaveAsync() - called by AutoSaveService

#### Functionality
- [ ] Implement drag-and-drop step reordering
  - [ ] Use ListView.CanReorderItems or custom drag logic
  - [ ] Update step Order property on drop
  - [ ] Mark as dirty on reorder

- [ ] Implement auto-save
  - [ ] Start auto-save timer when editor loads
  - [ ] Save every 30 seconds if IsDirty
  - [ ] Update SaveStatus indicator
  - [ ] Stop auto-save on navigation away

- [ ] Implement image upload
  - [ ] Open file picker (PNG, JPG, JPEG, BMP)
  - [ ] Validate image size (max 10MB)
  - [ ] Upload to LiteDB FileStorage
  - [ ] Store file ID in Step.ImageIds
  - [ ] Display thumbnail in editor

- [ ] Implement unsaved changes warning
  - [ ] Detect navigation away from editor
  - [ ] Show ContentDialog if IsDirty
  - [ ] Options: "Save and Leave", "Discard Changes", "Cancel"

- [ ] Implement guide deletion
  - [ ] Show ContentDialog confirmation
  - [ ] Delete all associated images from FileStorage
  - [ ] Delete guide from database
  - [ ] Navigate back to guide list

---

### 🏷️ UI - Category Management

**File**: `GuideViewer/Views/Dialogs/CategoryManagerDialog.xaml`

- [ ] Create category manager ContentDialog
  - [ ] List of existing categories
  - [ ] Add new category section
    - [ ] Name TextBox
    - [ ] Description TextBox
    - [ ] Icon glyph picker (FontIcon selector)
    - [ ] Add button
  - [ ] Edit category (inline editing in list)
  - [ ] Delete category button (check if in use)
  - [ ] Save/Cancel buttons

- [ ] Create `CategoryManagerViewModel`
  - [ ] ObservableCollection&lt;Category&gt; Categories
  - [ ] Category NewCategory
  - [ ] AddCategoryCommand
  - [ ] EditCategoryCommand(Category category)
  - [ ] DeleteCategoryCommand(Category category)
  - [ ] SaveCommand
  - [ ] ValidateCategoryName (no duplicates)

- [ ] Integrate with GuideEditorPage
  - [ ] "Manage Categories" button in category ComboBox
  - [ ] Open CategoryManagerDialog
  - [ ] Refresh category list after dialog closes

---

### 🔍 UI - Guide Detail View (Read-Only Preview)

**File**: `GuideViewer/Views/Pages/GuideDetailPage.xaml`

- [ ] Create guide detail page (for preview mode)
  - [ ] Guide title and description header
  - [ ] Category badge
  - [ ] Estimated time display
  - [ ] Step list on left (numbered, clickable)
  - [ ] Step detail on right (read-only)
  - [ ] Previous/Next navigation buttons
  - [ ] Progress bar showing step X of Y
  - [ ] "Edit" button (admin only)

- [ ] Create `GuideDetailViewModel`
  - [ ] Guide CurrentGuide
  - [ ] Step CurrentStep
  - [ ] int CurrentStepIndex
  - [ ] ObservableCollection&lt;Step&gt; Steps
  - [ ] double ProgressPercentage
  - [ ] NavigateToStepCommand(int index)
  - [ ] NextStepCommand
  - [ ] PreviousStepCommand
  - [ ] EditGuideCommand (admin only)
  - [ ] LoadGuideAsync(ObjectId guideId)

- [ ] Implement step navigation
  - [ ] Click on step in list to jump to that step
  - [ ] Previous/Next buttons
  - [ ] Keyboard shortcuts (←/→ arrows)
  - [ ] Update progress bar on navigation

---

### 🧪 Testing

#### Unit Tests ✅ **COMPLETE** (75 tests passing)
- [x] Create `GuideRepositoryTests.cs` (19 tests)
  - [x] Test Insert guide
  - [x] Test Update guide
  - [x] Test Delete guide
  - [x] Test GetById
  - [x] Test GetAll
  - [x] Test Search (title, description, category)
  - [x] Test GetByCategory
  - [x] Test GetRecentlyModified
  - [x] Test GetDistinctCategories

- [x] Create `CategoryRepositoryTests.cs` (14 tests)
  - [x] Test Insert category
  - [x] Test Update category
  - [x] Test Delete category
  - [x] Test GetByName
  - [x] Test Exists (with/without excludeId)
  - [x] Test InsertIfNotExists
  - [x] Test EnsureCategory

- [x] Create `ImageStorageServiceTests.cs` (26 tests)
  - [x] Test UploadImage (valid/invalid formats, size limits)
  - [x] Test GetImage
  - [x] Test DeleteImage
  - [x] Test ValidateImage (size, format, null checks)
  - [x] Test GetImageMetadata
  - [x] Test multiple images

- [x] Create `AutoSaveServiceTests.cs` (16 tests)
  - [x] Test auto-save timer
  - [x] Test IsDirty tracking
  - [x] Test StartAutoSave / StopAutoSave
  - [x] Test ManualSave
  - [x] Test timer reset
  - [x] Test error handling

#### Integration Tests ⚪ **NOT STARTED** (Phase 6)
- [ ] Test end-to-end guide creation
  - [ ] Create guide with metadata
  - [ ] Add steps
  - [ ] Upload images
  - [ ] Verify persistence

- [ ] Test end-to-end guide editing
  - [ ] Load existing guide
  - [ ] Modify steps
  - [ ] Reorder steps
  - [ ] Verify changes persist

- [ ] Test search and filter
  - [ ] Search by title
  - [ ] Filter by category
  - [ ] Verify correct results

- [ ] Test guide deletion
  - [ ] Delete guide
  - [ ] Verify associated images deleted
  - [ ] Verify guide removed from database

---

### 🎯 Navigation Integration

- [ ] Update `NavigationService` with new page keys
  - [ ] PageKeys.GuideEditor
  - [ ] PageKeys.GuideDetail

- [ ] Register pages in `MainWindow.xaml.cs`
  - [ ] RegisterPage&lt;GuideEditorPage&gt;(PageKeys.GuideEditor)
  - [ ] RegisterPage&lt;GuideDetailPage&gt;(PageKeys.GuideDetail)

- [ ] Update GuidesPage navigation
  - [ ] "New Guide" button → GuideEditorPage (create mode)
  - [ ] "Edit" button → GuideEditorPage (edit mode, pass guideId)
  - [ ] Card click → GuideDetailPage (view mode, pass guideId)

- [ ] Handle navigation parameters
  - [ ] GuideEditorPage: Accept optional guideId parameter
  - [ ] If guideId null → create new guide
  - [ ] If guideId provided → load and edit existing guide

---

### 🔧 Infrastructure & Configuration

- [x] Update `App.xaml.cs` DI registration
  - [x] GuideRepository (Transient)
  - [x] CategoryRepository (Transient)
  - [x] ImageStorageService (Singleton)
  - [x] AutoSaveService (Transient)

- [x] Create sample data seeding (for testing)
  - [x] Create 4 sample categories
  - [x] Create 5 sample guides
  - [x] Add sample steps with content (5 steps per guide average)
  - [ ] Add sample images *(deferred to Phase 4)*

- [x] Configure LiteDB indexes
  - [x] Index on Guide.Title
  - [x] Index on Guide.Category
  - [x] Index on Guide.UpdatedAt

- [x] Update logging
  - [x] Log guide CRUD operations (in repositories)
  - [x] Log image uploads/deletions (in ImageStorageService)
  - [x] Log auto-save events (in AutoSaveService)
  - [x] Log search queries (in GuidesViewModel)

---

### 📝 Documentation

- [ ] Update `CLAUDE.md`
  - [ ] Document Guide/Step/Category entities
  - [ ] Document GuideRepository patterns
  - [ ] Document image upload workflow
  - [ ] Document auto-save mechanism
  - [ ] Add guide editor usage examples

- [ ] Create inline code documentation
  - [ ] XML comments for all public APIs
  - [ ] Document entity relationships
  - [ ] Document data validation rules

- [ ] Update README (if exists)
  - [ ] Document Milestone 2 features
  - [ ] Add screenshots of guide editor
  - [ ] Document admin guide creation workflow

---

## Progress Tracking

**Started**: 2025-11-16
**Target Completion**: Week 4 (end of November)
**Current Status**: 🟢 **45% Complete - Phase 3 Done!**

### Phase 1: Data Layer (Week 3, Days 1-2) ✅ **COMPLETE**
- [x] Entities created (Guide, Step, Category)
- [x] Repositories implemented (GuideRepository, CategoryRepository)
- [x] Database configuration updated
- [x] Unit tests for repositories (33 tests passing)

### Phase 2: Services (Week 3, Days 3-4) ✅ **COMPLETE**
- [x] ImageStorageService implemented
- [x] AutoSaveService implemented
- [x] Services registered in DI
- [x] Unit tests for services (42 tests passing)

### Phase 3: Guide List UI (Week 3, Days 5-7) ✅ **COMPLETE**
- [x] GuidesPage updated with search and filters
- [x] GuidesViewModel implemented
- [x] Guide cards designed and implemented
- [x] Category filtering working
- [x] Sample data seeding utility created

**Summary**: 75 tests passing, 5 sample guides, 4 categories

### Phase 4: Guide Editor UI (Week 4, Days 1-3) 🔵 **IN PROGRESS** (~35%)
- [ ] GuideEditorPage layout created
- [ ] GuideEditorViewModel implemented
- [ ] Rich text editing working
- [ ] Image upload working
- [ ] Auto-save functional
- [ ] Drag-and-drop reordering working

### Phase 5: Category Management & Detail View (Week 4, Days 4-5) ⚪ **NOT STARTED** (~10%)
- [ ] CategoryManagerDialog created
- [ ] GuideDetailPage created
- [ ] Navigation integrated
- [ ] Role-based visibility enforced

### Phase 6: Testing & Polish (Week 4, Days 6-7) ⚪ **NOT STARTED** (~10%)
- [ ] All unit tests passing
- [ ] Integration tests created
- [ ] Bug fixes
- [ ] Documentation updated
- [ ] Milestone 2 complete!

---

## Blockers / Issues

### Issues Found During Testing (Phase 3) ✅ **ALL RESOLVED**

**Testing Date**: 2025-11-16

**Issue #1: DispatcherQueue Access Error** (Build-time)
- **Problem**: `CS1061: 'Application' does not contain a definition for 'DispatcherQueue'`
- **Root Cause**: WinUI 3's `Application` class doesn't expose `DispatcherQueue` like WPF
- **Solution**: Inject `DispatcherQueue` via constructor from `Page.DispatcherQueue`
- **Files Modified**: `GuidesViewModel.cs`, `GuidesPage.xaml.cs`
- **Commit**: `fd03558` - Fix DispatcherQueue access in GuidesViewModel

**Issue #2: Clear Button Not Appearing** (Runtime)
- **Problem**: Clear button visibility bound to `SearchQuery` (string) instead of boolean
- **Root Cause**: `BooleanToVisibilityConverter` expects boolean, not string
- **Solution**:
  - Added `HasSearchQuery` computed property (`!string.IsNullOrWhiteSpace(SearchQuery)`)
  - Added `OnSearchQueryChanged` partial method to notify UI
  - Updated Clear button binding to `HasSearchQuery`
- **Files Modified**: `GuidesViewModel.cs`, `GuidesPage.xaml`
- **Commit**: `cedcfcf` - Fix UI issues in GuidesPage found during testing

**Issue #3: Delete Flyout Issues** (Runtime)
- **Problem 1**: Cancel button had no click handler (did nothing)
- **Problem 2**: Flyout too narrow (250px)
- **Solution**:
  - Added `DeleteCancel_Click` event handler to dismiss flyout
  - Increased `MinWidth` from 250 to 300 pixels
- **Files Modified**: `GuidesPage.xaml`, `GuidesPage.xaml.cs`
- **Commit**: `cedcfcf` - Fix UI issues in GuidesPage found during testing

**Issue #4: Empty State Always Visible** (Runtime)
- **Problem**: Empty state showed behind guides even when guides were present
- **Root Cause 1**: Used `InverseBooleanConverter` which returns `bool`, not `Visibility` enum
- **Root Cause 2**: `HasGuides` computed property didn't notify UI when `Guides` collection changed
- **Solution**:
  - Created `InverseBooleanToVisibilityConverter` (True→Collapsed, False→Visible)
  - Added `Guides.CollectionChanged` subscription to notify `HasGuides` changes
- **Files Modified**: `GuidesViewModel.cs`, `GuidesPage.xaml`, `App.xaml`
- **Files Created**: `InverseBooleanToVisibilityConverter.cs`
- **Commits**: `cedcfcf`, `bf6dcec`

**Testing Status**: ✅ All issues verified fixed in Visual Studio 2022

---

### Known Risks
- **RichEditBox complexity**: WinUI 3 RichEditBox may have quirks with RTF formatting
- **Image storage size**: Need to monitor LiteDB file size with many images
- **Drag-and-drop**: WinUI 3 drag-and-drop may require custom implementation
- **Auto-save timing**: Need to balance save frequency vs. performance

---

## Notes

### Design Decisions
- **Rich text format**: Using RTF (RichEditBox native format) instead of HTML/Markdown
- **Image storage**: LiteDB FileStorage instead of file system for portability
- **Auto-save interval**: 30 seconds (configurable in AutoSaveService)
- **Max image size**: 10MB per image
- **Category system**: Simple string-based categories (can expand later)

### Technical Constraints
- **RichEditBox**: Native WinUI 3 control (no external libraries)
- **Drag-and-drop**: Must work with keyboard for accessibility
- **Image formats**: PNG, JPG, JPEG, BMP (most common formats)
- **Search**: Case-insensitive, searches title/description/category

### Future Enhancements (Post-Milestone 2)
- Guide templates
- Bulk import/export
- Guide duplication
- Version history
- Advanced rich text (tables, code blocks)
- Video embedding

---

**Last Updated**: 2025-11-16 (Phase 3 Complete + Bug Fixes)
**Next Review**: After Phase 4 completion (Guide Editor UI)

**Recent Activity**:
- Phase 3 completed: Guide List UI with search and filtering
- Testing performed in Visual Studio 2022
- 4 issues identified and resolved (DispatcherQueue, Clear button, Delete flyout, Empty state)
- All fixes committed and pushed to GitHub

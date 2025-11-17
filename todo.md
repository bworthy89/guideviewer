# GuideViewer - Milestone 2 Todo List

**Milestone**: Guide Data Model & Admin CRUD (Week 3-4)
**Goal**: Implement complete guide management functionality for administrators

**STATUS**: 🔵 **IN PROGRESS** (Started: 2025-11-16)

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
- [ ] Admins can search guides by title or category
- [ ] Changes are automatically saved without user action
- [ ] Guides persist correctly in LiteDB
- [ ] Guide deletion requires confirmation
- [ ] Technicians can only view guides (read-only, no edit)
- [ ] All CRUD operations have appropriate error handling

---

## Tasks Breakdown

### 🗄️ Data Layer - Entities & Repositories

#### Entities
- [ ] Create `Guide` entity (`GuideViewer.Data/Entities/Guide.cs`)
  - [ ] Id (ObjectId)
  - [ ] Title (string)
  - [ ] Description (string)
  - [ ] Category (string)
  - [ ] EstimatedMinutes (int)
  - [ ] Steps (List&lt;Step&gt;)
  - [ ] CreatedAt (DateTime)
  - [ ] UpdatedAt (DateTime)
  - [ ] CreatedBy (string - user role/id)

- [ ] Create `Step` entity (`GuideViewer.Data/Entities/Step.cs`)
  - [ ] Id (string/Guid)
  - [ ] Order (int)
  - [ ] Title (string)
  - [ ] Content (string - RTF or HTML)
  - [ ] ImageIds (List&lt;string&gt; - LiteDB file IDs)

- [ ] Create `Category` entity (`GuideViewer.Data/Entities/Category.cs`)
  - [ ] Id (ObjectId)
  - [ ] Name (string)
  - [ ] Description (string)
  - [ ] IconGlyph (string)
  - [ ] CreatedAt (DateTime)

#### Repositories
- [ ] Create `GuideRepository` (`GuideViewer.Data/Repositories/GuideRepository.cs`)
  - [ ] GetById(ObjectId id)
  - [ ] GetAll()
  - [ ] Search(string query) - searches title, description, category
  - [ ] GetByCategory(string category)
  - [ ] Insert(Guide guide)
  - [ ] Update(Guide guide)
  - [ ] Delete(ObjectId id)
  - [ ] GetRecentlyModified(int count)

- [ ] Create `CategoryRepository` (`GuideViewer.Data/Repositories/CategoryRepository.cs`)
  - [ ] GetAll()
  - [ ] GetByName(string name)
  - [ ] Insert(Category category)
  - [ ] Update(Category category)
  - [ ] Delete(ObjectId id)
  - [ ] Exists(string name)

#### Database Configuration
- [ ] Update `DatabaseService` to initialize guides collection
- [ ] Update `DatabaseService` to initialize categories collection
- [ ] Add indexes for guide search (title, category)
- [ ] Configure LiteDB FileStorage for images
- [ ] Test database operations (CRUD + search)

---

### 🖼️ Services - Image & File Management

- [ ] Create `ImageStorageService` (`GuideViewer.Core/Services/ImageStorageService.cs`)
  - [ ] UploadImage(Stream imageStream, string fileName) → returns fileId
  - [ ] GetImage(string fileId) → returns Stream
  - [ ] DeleteImage(string fileId)
  - [ ] GetImageMetadata(string fileId) → returns size, type, upload date
  - [ ] ValidateImage(Stream imageStream) → checks size, format
  - [ ] Max image size: 10MB
  - [ ] Supported formats: PNG, JPG, JPEG, BMP

- [ ] Create `AutoSaveService` (`GuideViewer.Core/Services/AutoSaveService.cs`)
  - [ ] StartAutoSave(Action saveCallback, int intervalSeconds = 30)
  - [ ] StopAutoSave()
  - [ ] IsDirty property (tracks if changes need saving)
  - [ ] LastSavedAt property (DateTime)
  - [ ] ManualSave() method

- [ ] Register services in DI container (`App.xaml.cs`)
  - [ ] ImageStorageService (Singleton)
  - [ ] AutoSaveService (Transient - one per editor instance)
  - [ ] GuideRepository (Transient)
  - [ ] CategoryRepository (Transient)

---

### 🎨 UI - Guide List Page

**File**: `GuideViewer/Views/Pages/GuidesPage.xaml` (update existing placeholder)

- [ ] Design guide list UI layout
  - [ ] Search TextBox with search icon
  - [ ] Category filter ComboBox
  - [ ] "New Guide" button (admin only, already exists in nav)
  - [ ] Grid of guide cards (responsive: 1-3 columns)

- [ ] Create guide card component
  - [ ] Guide thumbnail/icon (default if no image)
  - [ ] Guide title (bold, 16pt)
  - [ ] Category badge with color
  - [ ] Step count (e.g., "12 steps")
  - [ ] Estimated time (e.g., "~45 min")
  - [ ] Last modified date
  - [ ] Edit button (admin only)
  - [ ] Delete button (admin only)
  - [ ] Hover animation (card elevation)

- [ ] Implement search functionality
  - [ ] Real-time search as user types (debounced 300ms)
  - [ ] Search by title, description, category
  - [ ] Clear search button

- [ ] Implement category filtering
  - [ ] "All Categories" option
  - [ ] Dynamic category list from database
  - [ ] Update guide list on category change

- [ ] Create `GuidesViewModel` (`GuideViewer/ViewModels/GuidesViewModel.cs`)
  - [ ] ObservableCollection&lt;Guide&gt; Guides
  - [ ] ObservableCollection&lt;Category&gt; Categories
  - [ ] string SearchQuery
  - [ ] Category SelectedCategory
  - [ ] bool IsLoading
  - [ ] CreateGuideCommand (navigates to editor)
  - [ ] EditGuideCommand(Guide guide)
  - [ ] DeleteGuideCommand(Guide guide)
  - [ ] SearchCommand (filters guides)
  - [ ] LoadGuidesAsync()
  - [ ] LoadCategoriesAsync()

- [ ] Implement loading states
  - [ ] ProgressRing while loading guides
  - [ ] Skeleton cards during search
  - [ ] Empty state ("No guides found")

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

#### Unit Tests
- [ ] Create `GuideRepositoryTests.cs`
  - [ ] Test Insert guide
  - [ ] Test Update guide
  - [ ] Test Delete guide
  - [ ] Test GetById
  - [ ] Test GetAll
  - [ ] Test Search (title, description, category)
  - [ ] Test GetByCategory

- [ ] Create `CategoryRepositoryTests.cs`
  - [ ] Test Insert category
  - [ ] Test Update category
  - [ ] Test Delete category
  - [ ] Test GetByName
  - [ ] Test Exists

- [ ] Create `ImageStorageServiceTests.cs`
  - [ ] Test UploadImage
  - [ ] Test GetImage
  - [ ] Test DeleteImage
  - [ ] Test ValidateImage (size, format)
  - [ ] Test duplicate file handling

- [ ] Create `AutoSaveServiceTests.cs`
  - [ ] Test auto-save timer
  - [ ] Test IsDirty tracking
  - [ ] Test StartAutoSave / StopAutoSave
  - [ ] Test ManualSave

#### Integration Tests
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

- [ ] Update `App.xaml.cs` DI registration
  - [ ] GuideRepository (Transient)
  - [ ] CategoryRepository (Transient)
  - [ ] ImageStorageService (Singleton)
  - [ ] AutoSaveService (Transient)

- [ ] Create sample data seeding (optional, for testing)
  - [ ] Create 3-5 sample categories
  - [ ] Create 10-15 sample guides
  - [ ] Add sample steps with content
  - [ ] Add sample images

- [ ] Configure LiteDB indexes
  - [ ] Index on Guide.Title
  - [ ] Index on Guide.Category
  - [ ] Index on Guide.CreatedAt

- [ ] Update logging
  - [ ] Log guide CRUD operations
  - [ ] Log image uploads/deletions
  - [ ] Log auto-save events
  - [ ] Log search queries (for analytics)

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
**Current Status**: 🔵 **0% Complete - Starting Milestone 2**

### Phase 1: Data Layer (Week 3, Days 1-2)
- [ ] Entities created (Guide, Step, Category)
- [ ] Repositories implemented (GuideRepository, CategoryRepository)
- [ ] Database configuration updated
- [ ] Unit tests for repositories

### Phase 2: Services (Week 3, Days 3-4)
- [ ] ImageStorageService implemented
- [ ] AutoSaveService implemented
- [ ] Services registered in DI
- [ ] Unit tests for services

### Phase 3: Guide List UI (Week 3, Days 5-7)
- [ ] GuidesPage updated with search and filters
- [ ] GuidesViewModel implemented
- [ ] Guide cards designed and implemented
- [ ] Category filtering working

### Phase 4: Guide Editor UI (Week 4, Days 1-3)
- [ ] GuideEditorPage layout created
- [ ] GuideEditorViewModel implemented
- [ ] Rich text editing working
- [ ] Image upload working
- [ ] Auto-save functional
- [ ] Drag-and-drop reordering working

### Phase 5: Category Management & Detail View (Week 4, Days 4-5)
- [ ] CategoryManagerDialog created
- [ ] GuideDetailPage created
- [ ] Navigation integrated
- [ ] Role-based visibility enforced

### Phase 6: Testing & Polish (Week 4, Days 6-7)
- [ ] All unit tests passing
- [ ] Integration tests created
- [ ] Bug fixes
- [ ] Documentation updated
- [ ] Milestone 2 complete!

---

## Blockers / Issues

_Document any blockers or issues encountered during development_

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

**Last Updated**: 2025-11-16
**Next Review**: After Phase 1 completion

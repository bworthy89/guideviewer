# GuideViewer - Installation Guide Application

## Project Overview

GuideViewer is a modern Windows desktop application designed for service technicians to access, follow, and track progress on installation guides. The application provides a role-based system where administrators can create and edit guides, while technicians can view and track their progress through installation procedures.

**Target Audience**: Field service technicians performing equipment installations and maintenance procedures

**Primary Goal**: Provide an intuitive, offline-first guide system that improves installation accuracy and reduces technician training time

---

## Requirements

### Functional Requirements

#### FR-1: User Authentication & Authorization
- **FR-1.1**: Application shall authenticate users via product key entered during initial setup
- **FR-1.2**: Product key shall determine user role (ADMIN or TECHNICIAN)
- **FR-1.3**: Product key shall be validated offline using cryptographic validation
- **FR-1.4**: Application shall store license information securely in local storage
- **FR-1.5**: Application shall display current user role in the UI

#### FR-2: Guide Management (Admin Only)
- **FR-2.1**: Admins shall be able to create new installation guides
- **FR-2.2**: Admins shall be able to edit existing guides
- **FR-2.3**: Admins shall be able to delete guides with confirmation
- **FR-2.4**: Admins shall be able to organize guides into categories
- **FR-2.5**: Each guide shall support rich text formatting (bold, italic, lists, headings)
- **FR-2.6**: Each guide shall support embedded images and diagrams
- **FR-2.7**: Guides shall support an unlimited number of steps
- **FR-2.8**: Each step shall have a title, description, and optional images
- **FR-2.9**: Steps shall be reorderable via drag-and-drop

#### FR-3: Guide Viewing (All Users)
- **FR-3.1**: Users shall be able to browse all available guides
- **FR-3.2**: Users shall be able to search guides by title, category, or keywords
- **FR-3.3**: Users shall be able to view guide details and all steps
- **FR-3.4**: Application shall display estimated completion time for each guide
- **FR-3.5**: Application shall display guide metadata (created date, last modified, author)

#### FR-4: Progress Tracking (All Users)
- **FR-4.1**: Users shall be able to mark individual steps as completed
- **FR-4.2**: Application shall visually indicate completed vs incomplete steps
- **FR-4.3**: Application shall display overall progress percentage for active guides
- **FR-4.4**: Application shall maintain progress history per user
- **FR-4.5**: Users shall be able to reset progress on a guide
- **FR-4.6**: Application shall display a progress dashboard showing all in-progress guides
- **FR-4.7**: Users shall be able to add notes to individual steps (stored locally)

#### FR-5: Data Management
- **FR-5.1**: All data shall be stored locally in an embedded database
- **FR-5.2**: Application shall support backup/export of all guides to a file
- **FR-5.3**: Application shall support import of guides from backup files
- **FR-5.4**: Application shall maintain audit log of guide modifications (admin only)
- **FR-5.5**: Application shall handle database migration on version updates

### Non-Functional Requirements

#### NFR-1: Performance
- **NFR-1.1**: Application startup time shall be under 2 seconds on standard hardware
- **NFR-1.2**: Guide list shall load and render within 500ms
- **NFR-1.3**: Step navigation shall be instantaneous (<100ms)
- **NFR-1.4**: Memory usage shall remain under 150MB during normal operation
- **NFR-1.5**: Application shall support guides with up to 1000 steps without performance degradation

#### NFR-2: Usability
- **NFR-2.1**: Application shall follow Windows 11 Fluent Design principles
- **NFR-2.2**: All primary actions shall be accessible within 3 clicks
- **NFR-2.3**: Application shall support keyboard navigation throughout
- **NFR-2.4**: Touch targets shall be minimum 44x44 pixels for tablet support
- **NFR-2.5**: Application shall provide visual feedback for all user actions within 100ms

#### NFR-3: Reliability
- **NFR-3.1**: Application shall function completely offline
- **NFR-3.2**: Application shall not lose user progress in case of crash or power loss
- **NFR-3.3**: Application shall gracefully handle corrupted guide data
- **NFR-3.4**: Application shall validate all user input before database operations

#### NFR-4: Security
- **NFR-4.1**: Product keys shall use HMAC-SHA256 validation
- **NFR-4.2**: Admin functionality shall not be accessible to technician users
- **NFR-4.3**: Database shall be protected from unauthorized external access
- **NFR-4.4**: Application shall not expose sensitive data in logs or error messages

#### NFR-5: Maintainability
- **NFR-5.1**: Code shall follow MVVM architectural pattern
- **NFR-5.2**: Code shall maintain 80%+ unit test coverage for business logic
- **NFR-5.3**: Application shall use dependency injection for testability
- **NFR-5.4**: Code shall follow Microsoft C# coding conventions
- **NFR-5.5**: All public APIs shall have XML documentation comments

---

## Tech Stack

### Platform & Framework
- **Target Platform**: Windows 10 version 1809 (Build 17763) or later
- **Primary Framework**: WinUI 3 (Windows App SDK 1.5+)
- **Language**: C# 12
- **Runtime**: .NET 8.0
- **Architecture Pattern**: MVVM (Model-View-ViewModel)

### UI & Presentation
- **UI Framework**: WinUI 3 with Fluent Design System
- **MVVM Toolkit**: CommunityToolkit.Mvvm 8.2+
- **Navigation**: Microsoft.UI.Xaml.Controls.NavigationView
- **Icons**: Segoe Fluent Icons
- **Rich Text**: RichEditBox for guide editing
- **Markdown**: Markdig for guide rendering (if using markdown)

### Data & Storage
- **Database**: LiteDB 5.0+ (NoSQL document database)
- **Data Access**: Direct LiteDB API (lightweight, no ORM needed)
- **File Storage**: LiteDB FileStorage for images/attachments
- **Configuration**: System.Text.Json for settings serialization

### Security & Licensing
- **Cryptography**: System.Security.Cryptography (HMAC-SHA256)
- **Secure Storage**: Windows Credential Manager via PasswordVault
- **License Validation**: Custom offline key validation system

### Packaging & Deployment
- **Package Format**: MSIX
- **Installer**: MSIX installer with sideloading support
- **Auto-Update**: MSIX app installer for automatic updates
- **Code Signing**: Required for MSIX distribution

### Development Tools
- **IDE**: Visual Studio 2022 17.8+
- **Version Control**: Git with GitHub
- **Build System**: MSBuild
- **Testing**: xUnit + FluentAssertions + Moq
- **CI/CD**: GitHub Actions (optional for automated builds)

### Third-Party Libraries
- **CommunityToolkit.Mvvm** (8.2+): Source generators, RelayCommand, ObservableObject
- **CommunityToolkit.WinUI.UI.Controls** (7.1+): Additional UI controls
- **LiteDB** (5.0+): Embedded NoSQL database
- **Markdig** (0.33+): Markdown parsing (optional)
- **Serilog** (3.1+): Structured logging

---

## Design Guidelines

### Visual Design Principles

#### Windows 11 Fluent Design
- **Acrylic Backgrounds**: Use translucent materials for navigation panes
- **Rounded Corners**: 4px radius for cards, 8px for modals
- **Mica Material**: Use Mica background for main window
- **Shadow Depth**: Utilize depth layers (4dp, 8dp, 16dp, 32dp)
- **Reveal Highlight**: Subtle hover effects on interactive elements

#### Color Palette
- **Primary Brand**: Define a primary accent color (suggest #0078D4 - Windows Blue)
- **Status Colors**:
  - Success: #107C10 (green)
  - Warning: #FFC83D (yellow)
  - Error: #D13438 (red)
  - Info: #0078D4 (blue)
- **Completed Steps**: #107C10 with checkmark icon
- **In-Progress Steps**: #0078D4 with partial ring
- **Pending Steps**: #8A8A8A (neutral gray)

#### Typography
- **Primary Font**: Segoe UI Variable
- **Heading 1**: 28pt Semi-Bold
- **Heading 2**: 20pt Semi-Bold
- **Body Text**: 14pt Regular
- **Caption**: 12pt Regular
- **Line Height**: 1.5 for readability

#### Spacing System
- **Base Unit**: 4px
- **Spacing Scale**: 4px, 8px, 12px, 16px, 24px, 32px, 48px, 64px
- **Card Padding**: 16px
- **Section Spacing**: 24px between major sections
- **List Item Height**: 48px minimum

### Layout Structure

#### Main Window Layout
```
┌─────────────────────────────────────────────────────┐
│  [Title Bar with App Icon and Window Controls]      │
├───────────┬─────────────────────────────────────────┤
│           │  [Content Header - Search, Filters]     │
│           ├─────────────────────────────────────────┤
│  [Nav     │                                          │
│   Pane]   │         [Main Content Area]             │
│           │                                          │
│  - Home   │  (Guide List / Guide Details /          │
│  - Guides │   Guide Editor / Progress Dashboard)    │
│  - Progress│                                         │
│  - Settings│                                         │
│           │                                          │
│  [User    │                                          │
│   Badge]  │                                          │
└───────────┴─────────────────────────────────────────┘
```

#### Guide List View
- **Layout**: Grid of cards (responsive: 1-3 columns based on window width)
- **Card Design**: Elevated cards with hover animation
- **Card Contents**:
  - Guide thumbnail/icon (top)
  - Guide title (bold, 16pt)
  - Category badge
  - Step count (e.g., "12 steps")
  - Estimated time (e.g., "~45 min")
  - Progress indicator (if in progress)

#### Guide Detail View
- **Layout**: Two-column (step list left, step detail right)
- **Step List**: Vertical list with checkboxes, numbered steps
- **Step Detail**: Large area showing current step with images
- **Progress Bar**: Sticky header showing overall progress
- **Navigation**: Previous/Next buttons, jump to step capability

#### Admin Guide Editor
- **Layout**: WYSIWYG editor with toolbar
- **Toolbar**: Text formatting, image upload, step management
- **Preview**: Side-by-side edit/preview mode
- **Auto-save**: Indicate save status in UI

### Interaction Patterns

#### Navigation
- **Primary Navigation**: NavigationView with icon + label
- **Breadcrumbs**: Show current location in deep navigation
- **Back Button**: Browser-style back navigation in header
- **Keyboard Shortcuts**:
  - `Ctrl+N`: New guide (admin)
  - `Ctrl+F`: Search
  - `Ctrl+S`: Save (admin in editor)
  - `Space`: Mark step complete/incomplete
  - `←/→`: Previous/Next step

#### Feedback & Confirmation
- **Immediate Feedback**: Visual state change within 100ms
- **Success Messages**: InfoBar with auto-dismiss (3 seconds)
- **Destructive Actions**: ContentDialog with confirmation
- **Loading States**: ProgressRing for operations >500ms
- **Error Handling**: User-friendly error messages with recovery actions

#### Responsive Behavior
- **Minimum Window Size**: 800x600px
- **Navigation Pane**: Collapses to icons-only below 1024px width
- **Card Grid**: 3 columns (>1400px), 2 columns (1024-1400px), 1 column (<1024px)
- **Touch Support**: All interactive elements minimum 44x44px

### Accessibility Requirements
- **Keyboard Navigation**: Full app navigable via keyboard
- **Screen Reader**: All UI elements properly labeled with AutomationProperties
- **High Contrast**: Support Windows High Contrast themes
- **Focus Indicators**: Visible focus rectangles on all interactive elements
- **Text Scaling**: Support Windows text scaling up to 200%

---

## Development Milestones

### Milestone 1: Foundation & Authentication (Week 1-2)
**Goal**: Establish project structure, implement authentication, and basic navigation

**Deliverables**:
- ✅ Solution structure with all projects created
- ✅ WinUI 3 main window with NavigationView
- ✅ LiteDB database initialization and connection
- ✅ Product key validation system implemented
- ✅ License activation flow (first-run experience)
- ✅ User role detection and storage
- ✅ Basic navigation between placeholder pages
- ✅ App settings storage (theme, window size)

**Acceptance Criteria**:
- Application launches and shows activation screen on first run
- Valid admin and technician product keys can be entered and validated
- User role is persisted and retrieved on subsequent launches
- Navigation pane shows/hides admin options based on role
- Application remembers window size and position

**Technical Tasks**:
1. Create WinUI 3 project with Windows App SDK
2. Set up solution structure (Core, Data, UI projects)
3. Implement LiteDB database context and initialization
4. Create license validation service with HMAC-SHA256
5. Design and implement activation window
6. Create main window shell with NavigationView
7. Implement settings service with JSON persistence
8. Set up dependency injection container
9. Write unit tests for license validation
10. Configure MSIX packaging

---

### Milestone 2: Guide Data Model & Admin CRUD (Week 3-4)
**Goal**: Implement complete guide management functionality for administrators

**Deliverables**:
- ✅ Guide data model with LiteDB entities
- ✅ Guide list page with search and filtering
- ✅ Guide creation form/wizard
- ✅ Guide editor with rich text and image support
- ✅ Step management (add, edit, delete, reorder)
- ✅ Category management
- ✅ Image upload and storage in LiteDB
- ✅ Guide deletion with confirmation
- ✅ Auto-save functionality in editor

**Acceptance Criteria**:
- Admins can create new guides with title, category, and description
- Admins can add unlimited steps to guides with rich text
- Admins can upload and embed images in step descriptions
- Admins can reorder steps via drag-and-drop
- Admins can search guides by title or category
- Changes are automatically saved without user action
- Guides persist correctly in LiteDB

**Technical Tasks**:
1. Define Guide, Step, Category entities for LiteDB
2. Create repository pattern for data access
3. Implement guide list view with search TextBox
4. Build guide editor page with RichEditBox
5. Implement step list control with checkboxes (disabled in edit mode)
6. Create image picker and storage service
7. Implement drag-and-drop reordering for steps
8. Add auto-save timer (save every 30 seconds)
9. Create category management dialog
10. Implement delete confirmation ContentDialog
11. Write integration tests for CRUD operations

---

### Milestone 3: Technician View & Progress Tracking (Week 5-6)
**Goal**: Implement guide viewing and progress tracking for all users

**Deliverables**:
- ✅ Guide detail view with step-by-step navigation
- ✅ Step completion checkboxes with persistence
- ✅ Progress calculation and visualization
- ✅ Progress dashboard showing all in-progress guides
- ✅ Step notes functionality (local to user)
- ✅ Progress reset capability
- ✅ Guide browsing and search for technicians
- ✅ Read-only guide view (no editing for technicians)

**Acceptance Criteria**:
- Users can view any guide in read-only mode
- Users can check/uncheck steps to track progress
- Progress percentage is calculated and displayed accurately
- Progress persists between app sessions
- Users can see all in-progress guides on dashboard
- Users can add private notes to steps
- Users can reset progress on a guide
- Technician users cannot access edit functionality

**Technical Tasks**:
1. Create GuideDetailView page with step navigation
2. Implement progress tracking entity and repository
3. Build step detail control with completion checkbox
4. Create progress calculation service
5. Implement progress dashboard page with card layout
6. Add step notes dialog and storage
7. Create progress reset confirmation flow
8. Implement role-based UI visibility (v-if admin checks)
9. Add progress indicator components (circular, linear)
10. Create guide completion celebration screen
11. Write tests for progress calculation logic

---

### Milestone 4: Polish, Performance & Data Management (Week 7-8)
**Goal**: Optimize performance, add data management features, and polish UI/UX

**Deliverables**:
- ✅ Guide export/import functionality (JSON/backup format)
- ✅ Database backup and restore
- ✅ Performance optimization (lazy loading, virtualization)
- ✅ Animations and transitions
- ✅ Error handling and user feedback
- ✅ Settings page (theme, data management)
- ✅ About page with version info
- ✅ Keyboard shortcuts implementation
- ✅ Loading states and skeleton screens

**Acceptance Criteria**:
- Guides can be exported to a portable file format
- Exported guides can be imported on another installation
- Full database backup can be created and restored
- Guide list loads instantly with virtualization
- All animations are smooth (60fps)
- Errors display user-friendly messages
- Settings allow theme customization (light/dark/system)
- All keyboard shortcuts work as documented
- Loading states prevent user confusion

**Technical Tasks**:
1. Implement guide export service (JSON serialization)
2. Implement guide import with validation
3. Create database backup/restore functionality
4. Add ItemsRepeater virtualization to guide list
5. Implement connected animations between views
6. Create global error handling service
7. Add InfoBar for success/error messages
8. Build settings page with theme selector
9. Create about page with license info
10. Implement keyboard shortcut handling
11. Add ProgressRing for loading states
12. Optimize image loading and caching
13. Performance profiling and optimization

---

### Milestone 5: Testing, Documentation & Deployment (Week 9-10)
**Goal**: Ensure quality, create documentation, and prepare for deployment

**Deliverables**:
- ✅ Comprehensive unit test suite (80%+ coverage)
- ✅ Integration tests for critical workflows
- ✅ User documentation (help system)
- ✅ Administrator guide for product key generation
- ✅ MSIX package with code signing
- ✅ Installation guide for IT deployment
- ✅ GitHub repository with README
- ✅ Release notes and changelog
- ✅ Bug fixes from user testing

**Acceptance Criteria**:
- All unit tests pass with 80%+ code coverage
- Integration tests validate end-to-end workflows
- Help documentation is accessible in-app
- Product key generation tool works for admins
- MSIX package installs cleanly on Windows 10/11
- Installation guide covers all deployment scenarios
- GitHub repository has clear README and contribution guidelines
- Application is stable with no critical bugs

**Technical Tasks**:
1. Write unit tests for all services and view models
2. Create integration tests for authentication flow
3. Create integration tests for guide CRUD operations
4. Create integration tests for progress tracking
5. Build in-app help system (tooltips, help pane)
6. Write administrator documentation (Markdown)
7. Create product key generator tool/script
8. Configure MSIX manifest and assets
9. Set up code signing certificate
10. Create GitHub repository with proper structure
11. Write comprehensive README.md
12. Create CHANGELOG.md
13. Perform user acceptance testing
14. Bug triage and fixes
15. Create release builds

---

## Success Metrics

### Technical Metrics
- **Code Coverage**: >80% for business logic
- **Build Time**: <2 minutes for full solution
- **Startup Time**: <2 seconds on target hardware
- **Memory Usage**: <150MB during normal operation
- **Package Size**: <50MB MSIX bundle

### User Experience Metrics
- **Guide Creation Time**: <5 minutes for 10-step guide (admin)
- **Guide Navigation**: <3 clicks to any step
- **Search Results**: <500ms to display results
- **Crash Rate**: <0.1% of sessions

### Business Metrics
- **Adoption Rate**: Track unique installations
- **Active Users**: Track weekly active users
- **Guide Usage**: Track most-viewed guides
- **Completion Rate**: Percentage of started guides completed

---

## Future Enhancements (Post-MVP)

### Phase 2 Features
- Cloud synchronization for guide library
- Guide versioning and change history
- Video embedding in steps
- Offline guide package distribution via USB
- Multi-language support
- Dark theme refinements

### Phase 3 Features
- Collaborative editing for multiple admins
- Guide analytics dashboard
- QR code generation for quick guide access
- Mobile companion app (read-only)
- Integration with ticketing systems
- Voice-guided mode for hands-free operation

---

## Appendix

### Product Key Format
**Format**: `XXXX-XXXX-XXXX-XXXX` (16 characters, 4 groups of 4)

**Structure**:
- Characters 1-4: Role identifier (ADMIN: A000-AFFF, TECH: T000-TFFF)
- Characters 5-12: Random payload
- Characters 13-16: HMAC checksum (first 4 chars of hash)

**Example Keys**:
- Admin: `A4F2-B8C9-D3E1-7A5C`
- Tech: `T9E3-C7A2-F1D4-8B6E`

### Database Schema (LiteDB Collections)

**Users Collection**:
```json
{
  "_id": ObjectId,
  "productKey": "encrypted string",
  "role": "ADMIN | TECHNICIAN",
  "activatedAt": ISODate,
  "lastLogin": ISODate
}
```

**Guides Collection**:
```json
{
  "_id": ObjectId,
  "title": "string",
  "description": "string",
  "category": "string",
  "estimatedMinutes": number,
  "steps": [
    {
      "id": "string",
      "order": number,
      "title": "string",
      "content": "rich text string",
      "images": ["file_id", "file_id"]
    }
  ],
  "createdAt": ISODate,
  "updatedAt": ISODate,
  "createdBy": "string"
}
```

**Progress Collection**:
```json
{
  "_id": ObjectId,
  "guideId": ObjectId,
  "userId": ObjectId,
  "completedSteps": ["step_id", "step_id"],
  "stepNotes": {
    "step_id": "note text"
  },
  "startedAt": ISODate,
  "lastUpdated": ISODate,
  "completedAt": ISODate | null
}
```

**Settings Collection**:
```json
{
  "_id": ObjectId,
  "key": "string",
  "value": "json string"
}
```

---

## Revision History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2025-11-16 | Initial | First draft of specification |

---

**Document Status**: Draft
**Last Updated**: 2025-11-16
**Next Review**: Upon Milestone 1 completion

# GuideViewer - Milestone 1 Todo List

**Milestone**: Foundation & Authentication (Week 1-2)
**Goal**: Establish project structure, implement authentication, and basic navigation

---

## Tasks Overview

### 🏗️ Project Setup
- [x] Create WinUI 3 project with Windows App SDK
- [x] Set up solution structure (Core, Data, UI projects)
- [x] Install NuGet packages (LiteDB, CommunityToolkit.Mvvm, etc.)
- [x] Configure project references between layers
- [x] Set up .gitignore for Visual Studio/C# projects
- [x] Initialize Git repository and make initial commit

### 💾 Data Layer
- [x] Create GuideViewer.Data project (Class Library)
- [x] Add LiteDB NuGet package
- [x] Implement DatabaseService for LiteDB initialization
- [x] Create User entity model
- [x] Create Settings entity model (AppSetting)
- [x] Implement repository pattern base class
- [x] Create UserRepository
- [x] Create SettingsRepository
- [x] Test database connection and CRUD operations

### 🔐 Authentication & Licensing
- [x] Create GuideViewer.Core project (Class Library)
- [x] Implement LicenseValidator service
  - [x] Product key format validation (XXXX-XXXX-XXXX-XXXX)
  - [x] HMAC-SHA256 signature verification
  - [x] Role extraction (ADMIN vs TECHNICIAN)
- [x] Create ProductKeyGenerator utility (for testing)
- [ ] Implement SecureStorage service using Windows Credential Manager
- [x] Create UserRole enum (Admin, Technician)
- [x] Create LicenseInfo model
- [x] Write unit tests for LicenseValidator (11 tests, all passing)
- [ ] Generate test product keys (5 admin, 5 tech)

### 🎨 UI - Activation Window
- [ ] Create ActivationWindow.xaml
- [ ] Design activation UI with Fluent Design
  - [ ] App logo/icon
  - [ ] Product key input (4 TextBoxes for each segment)
  - [ ] Activate button
  - [ ] Error message display (InfoBar)
  - [ ] Loading state (ProgressRing)
- [ ] Create ActivationViewModel
- [ ] Implement product key input validation
- [ ] Implement activation command
- [ ] Add keyboard navigation (Tab, Enter to activate)
- [ ] Add paste support (auto-split product key)
- [ ] Test activation flow with valid/invalid keys

### 🏠 UI - Main Window
- [ ] Create MainWindow.xaml
- [ ] Implement NavigationView structure
  - [ ] Home menu item
  - [ ] Guides menu item
  - [ ] Progress menu item (Dashboard)
  - [ ] Settings menu item (footer)
- [ ] Add admin-only menu items
  - [ ] New Guide (visible only for admins)
- [ ] Create user badge in navigation footer
  - [ ] Display current user role
  - [ ] Role icon/indicator
- [ ] Implement Mica background material
- [ ] Create placeholder pages for navigation
  - [ ] HomePage.xaml
  - [ ] GuidesPage.xaml
  - [ ] ProgressPage.xaml
  - [ ] SettingsPage.xaml
- [ ] Create MainViewModel
- [ ] Implement navigation service
- [ ] Test navigation between pages
- [ ] Implement role-based UI visibility

### ⚙️ Settings & Configuration
- [ ] Create SettingsService
- [ ] Implement theme management (Light/Dark/System)
- [ ] Implement window state persistence
  - [ ] Window size
  - [ ] Window position
  - [ ] Maximized state
- [ ] Create AppSettings model
- [ ] Implement JSON serialization for settings
- [ ] Store settings in LiteDB
- [ ] Restore settings on app startup
- [ ] Test settings persistence across app restarts

### 🔧 Infrastructure
- [ ] Set up dependency injection (Microsoft.Extensions.DependencyInjection)
- [ ] Register services in App.xaml.cs
  - [ ] DatabaseService (Singleton)
  - [ ] LicenseValidator (Singleton)
  - [ ] SettingsService (Singleton)
  - [ ] NavigationService (Singleton)
  - [ ] Repositories (Scoped)
- [ ] Implement service locator pattern (if needed)
- [ ] Create base ViewModel class
- [ ] Implement INotifyPropertyChanged helpers
- [ ] Add global exception handling
- [ ] Set up Serilog for logging
  - [ ] File logging to %AppData%\GuideViewer\logs
  - [ ] Log levels configuration

### 📦 Packaging
- [ ] Configure MSIX manifest (Package.appxmanifest)
  - [ ] App identity (name, publisher, version)
  - [ ] Capabilities (file access, etc.)
  - [ ] Visual assets (logo, splash screen)
- [ ] Create app icons (44x44, 150x150, 310x310)
- [ ] Create splash screen image
- [ ] Test MSIX packaging build
- [ ] Test installation from MSIX package
- [ ] Document installation steps

### 🧪 Testing
- [x] Create GuideViewer.Tests project (xUnit)
- [x] Add testing NuGet packages (xUnit, FluentAssertions, Moq)
- [x] Write unit tests for LicenseValidator
  - [x] Test valid admin key
  - [x] Test valid technician key
  - [x] Test invalid format
  - [x] Test invalid checksum
- [ ] Write unit tests for SettingsService
- [ ] Write integration tests for database operations
- [ ] Achieve 80%+ code coverage for Core project
- [x] Run all tests and ensure they pass (11/11 passing)

### 📝 Documentation
- [ ] Create README.md with setup instructions
- [ ] Document product key generation process
- [ ] Document database schema
- [ ] Add code comments to public APIs
- [ ] Create development setup guide
- [ ] Document testing procedures

---

## Acceptance Criteria Checklist

- [ ] Application launches and shows activation screen on first run
- [ ] Valid admin and technician product keys can be entered and validated
- [ ] Invalid product keys show appropriate error messages
- [ ] User role is persisted and retrieved on subsequent launches
- [ ] Navigation pane shows/hides admin options based on role
- [ ] Application remembers window size and position
- [ ] All placeholder pages are navigable
- [ ] Theme can be changed in settings (if implemented early)
- [ ] No crashes during normal operation
- [ ] MSIX package builds successfully
- [ ] Application can be installed from MSIX package

---

## Technical Debt / Nice-to-Haves

- [ ] Add animations for page transitions
- [ ] Implement keyboard shortcuts (Ctrl+, for settings)
- [ ] Add tooltips to navigation items
- [ ] Implement auto-update check (deferred to later milestone)
- [ ] Add telemetry/analytics (optional)
- [ ] Create branded splash screen animation

---

## Blockers / Issues

_Document any blockers or issues encountered during development_

- None currently

---

## Notes

### Product Key Format Reference
- **Format**: `XXXX-XXXX-XXXX-XXXX`
- **Admin prefix**: A000-AFFF
- **Tech prefix**: T000-TFFF
- **Validation**: HMAC-SHA256 with secret salt

### Database Location
- **Path**: `%LocalAppData%\GuideViewer\data.db`
- **Backups**: `%LocalAppData%\GuideViewer\backups\`

### Useful Commands
```bash
# Create new WinUI 3 project
dotnet new install Microsoft.WindowsAppSDK.Templates
dotnet new winui -n GuideViewer

# Add NuGet packages
dotnet add package LiteDB
dotnet add package CommunityToolkit.Mvvm
dotnet add package CommunityToolkit.WinUI.UI.Controls
dotnet add package Serilog
dotnet add package Serilog.Sinks.File

# Run tests
dotnet test

# Build MSIX
msbuild /t:Publish /p:Configuration=Release
```

---

## Progress Tracking

**Started**: 2025-11-16
**Target Completion**: Week 2
**Current Status**: 🟢 ~50% Complete

### Completed ✅
- ✅ Project setup and solution structure
- ✅ All NuGet dependencies installed
- ✅ Git repository initialized with 3 commits
- ✅ Complete data layer with LiteDB (DatabaseService, Repositories, Entities)
- ✅ Product key validation with HMAC-SHA256
- ✅ Unit tests for licensing (11/11 passing)

### In Progress 🟡
- Settings service implementation
- Activation window UI
- Main window with NavigationView
- Dependency injection setup

### Remaining 🔴
- UI implementation (Activation + Main windows)
- Settings persistence
- MSIX packaging configuration
- Documentation

### Week 1 Goals
- ✅ Complete project setup
- ✅ Implement authentication system
- ⏳ Create basic UI shell (in progress)

### Week 2 Goals
- Finish settings persistence
- Complete testing
- Finalize MSIX packaging

---

**Last Updated**: 2025-11-16 (Evening)

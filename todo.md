# GuideViewer - Milestone 1 Todo List

**Milestone**: Foundation & Authentication (Week 1-2)
**Goal**: Establish project structure, implement authentication, and basic navigation

---

## Tasks Overview

### 🏗️ Project Setup
- [ ] Create WinUI 3 project with Windows App SDK
- [ ] Set up solution structure (Core, Data, UI projects)
- [ ] Install NuGet packages (LiteDB, CommunityToolkit.Mvvm, etc.)
- [ ] Configure project references between layers
- [ ] Set up .gitignore for Visual Studio/C# projects
- [ ] Initialize Git repository and make initial commit

### 💾 Data Layer
- [ ] Create GuideViewer.Data project (Class Library)
- [ ] Add LiteDB NuGet package
- [ ] Implement DatabaseService for LiteDB initialization
- [ ] Create User entity model
- [ ] Create Settings entity model
- [ ] Implement repository pattern base class
- [ ] Create UserRepository
- [ ] Create SettingsRepository
- [ ] Test database connection and CRUD operations

### 🔐 Authentication & Licensing
- [ ] Create GuideViewer.Core project (Class Library)
- [ ] Implement LicenseValidator service
  - [ ] Product key format validation (XXXX-XXXX-XXXX-XXXX)
  - [ ] HMAC-SHA256 signature verification
  - [ ] Role extraction (ADMIN vs TECHNICIAN)
- [ ] Create ProductKeyGenerator utility (for testing)
- [ ] Implement SecureStorage service using Windows Credential Manager
- [ ] Create UserRole enum (Admin, Technician)
- [ ] Create LicenseInfo model
- [ ] Write unit tests for LicenseValidator
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
- [ ] Create GuideViewer.Tests project (xUnit)
- [ ] Add testing NuGet packages (xUnit, FluentAssertions, Moq)
- [ ] Write unit tests for LicenseValidator
  - [ ] Test valid admin key
  - [ ] Test valid technician key
  - [ ] Test invalid format
  - [ ] Test invalid checksum
- [ ] Write unit tests for SettingsService
- [ ] Write integration tests for database operations
- [ ] Achieve 80%+ code coverage for Core project
- [ ] Run all tests and ensure they pass

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
**Current Status**: 🟡 In Progress

### Week 1 Goals
- Complete project setup
- Implement authentication system
- Create basic UI shell

### Week 2 Goals
- Finish settings persistence
- Complete testing
- Finalize MSIX packaging

---

**Last Updated**: 2025-11-16

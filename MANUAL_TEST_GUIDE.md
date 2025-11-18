# GuideViewer - Comprehensive Manual Test Guide

**Version**: 1.0
**Date**: 2025-11-17
**Scope**: All Milestones (1-4)
**Tester**: _________________
**Test Date**: _________________

---

## Table of Contents

1. [Prerequisites & Test Environment Setup](#prerequisites--test-environment-setup)
2. [Milestone 1: Foundation](#milestone-1-foundation)
   - Authentication & Licensing
   - Navigation System
   - Settings Management
   - Logging
3. [Milestone 2: Guide Management](#milestone-2-guide-management)
   - Category Management
   - Guide CRUD Operations
   - Guide Editor
   - Search & Filter
   - Image Management
4. [Milestone 3: Progress Tracking](#milestone-3-progress-tracking)
   - User Progress Tracking
   - Progress Dashboard
   - Progress Reports
   - Active Guide Progress
   - Timer Service
5. [Milestone 4: Polish & Performance](#milestone-4-polish--performance)
   - Data Management
   - About Page
   - Keyboard Shortcuts
   - Error Handling
   - Performance Monitoring
   - UI Polish & Animations
6. [Accessibility Testing](#accessibility-testing)
7. [Integration Testing](#integration-testing)
8. [Regression Testing](#regression-testing)
9. [Troubleshooting](#troubleshooting)
10. [Test Sign-Off](#test-sign-off)

---

## Prerequisites & Test Environment Setup

### Required Software
- [ ] Visual Studio 2022 (17.8 or later)
- [ ] .NET 8 SDK
- [ ] Windows 10 1809+ or Windows 11
- [ ] Windows App SDK 1.5+

### Test Data
- [ ] **Admin product key**: `A04E-02C0-AD82-43C0`
- [ ] **Technician product key**: `TD5A-BB21-A638-C43A`
- [ ] Sample images (PNG/JPG, various sizes including > 10MB for validation testing)
- [ ] Clean database location: `%LocalAppData%\GuideViewer\data.db`

### Build Verification
```bash
# Open solution in Visual Studio 2022
# Clean and rebuild entire solution
# Run all tests
dotnet test GuideViewer.Tests/GuideViewer.Tests.csproj
```

**Expected**: 258/260 tests passing (99.2% pass rate)
- 2 timing-sensitive tests may occasionally fail due to performance variance

### Clean Test Environment Setup

**Step 1: Clean Installation**
1. Delete existing database:
   - Navigate to: `%LocalAppData%\GuideViewer\`
   - Delete `data.db` file if it exists
   - Delete `logs` folder if it exists

2. Build and launch:
   - Open `GuideViewer.sln` in Visual Studio 2022
   - Set `GuideViewer` as startup project
   - Press F5 to build and run
   - **Expected**: Activation page appears

**Step 2: Verify Clean State**
- [ ] Activation page is displayed
- [ ] No error messages in console
- [ ] Application window sized appropriately
- [ ] Mica background visible (semi-transparent)

---

## Milestone 1: Foundation

### M1.1: Authentication & Licensing

#### Test 1.1.1: Admin Activation - Valid Key

**Purpose**: Verify admin can activate with valid product key

**Steps**:
1. On activation page, enter admin key: `A04E-02C0-AD82-43C0`
2. Click "Activate" button

**Expected Results**:
- [ ] Activation succeeds immediately
- [ ] Navigate to Home page
- [ ] Welcome message displays username/role
- [ ] Navigation menu shows admin-only items

**Verification**:
- [ ] Navigation shows "Guide Editor" option
- [ ] User role displayed as "Admin" (if visible)
- [ ] No error messages

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 1.1.2: Technician Activation - Valid Key

**Purpose**: Verify technician can activate with valid product key

**Steps**:
1. **Reset**: Delete database, restart application
2. On activation page, enter technician key: `TD5A-BB21-A638-C43A`
3. Click "Activate" button

**Expected Results**:
- [ ] Activation succeeds
- [ ] Navigate to Home page
- [ ] Navigation menu shows technician view (no Guide Editor)

**Verification**:
- [ ] "Guide Editor" option NOT visible
- [ ] "Guides" option visible
- [ ] "Progress" option visible
- [ ] User role: "Technician"

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 1.1.3: Invalid Product Key

**Purpose**: Verify invalid keys are rejected

**Steps**:
1. **Reset**: Delete database, restart application
2. Enter invalid key: `XXXX-XXXX-XXXX-XXXX`
3. Click "Activate"

**Expected Results**:
- [ ] Error message appears: "Invalid product key"
- [ ] Remain on activation page
- [ ] Can try again with different key

**Verification**:
- [ ] Clear error message
- [ ] Text box cleared or shows error state
- [ ] No navigation occurred

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 1.1.4: Empty Product Key

**Purpose**: Verify empty key is validated

**Steps**:
1. Leave product key field empty
2. Click "Activate"

**Expected Results**:
- [ ] Validation error: "Product key is required"
- [ ] Remain on activation page
- [ ] Activate button disabled OR validation message shown

**Verification**:
- [ ] Cannot proceed with empty key
- [ ] User-friendly error message

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 1.1.5: Product Key Format Validation

**Purpose**: Verify key format is validated (XXXX-XXXX-XXXX-XXXX)

**Steps**:
1. Enter malformed key: `ABC123`
2. Click "Activate"

**Expected Results**:
- [ ] Validation error about format
- [ ] Key should be rejected

**Verification**:
- [ ] Error message explains correct format
- [ ] Example shown if applicable

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 1.1.6: Persistence - Activation Survives Restart

**Purpose**: Verify user stays logged in after restart

**Prerequisites**: Activated as admin (Test 1.1.1)

**Steps**:
1. Close application
2. Relaunch application

**Expected Results**:
- [ ] Navigate directly to Home page (skip activation)
- [ ] User role preserved (Admin)
- [ ] No re-authentication required

**Verification**:
- [ ] Database contains user record
- [ ] Settings preserved

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

### M1.2: Navigation System

#### Test 1.2.1: NavigationView Items - Admin

**Purpose**: Verify admin sees all navigation options

**Prerequisites**: Logged in as admin

**Steps**:
1. Observe navigation menu (left sidebar)

**Expected Navigation Items (Admin)**:
- [ ] Home
- [ ] Guides
- [ ] Guide Editor (admin only)
- [ ] Progress
- [ ] Settings
- [ ] About

**Verification**:
- [ ] All 6 items visible
- [ ] Items in logical order
- [ ] Icons displayed correctly

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 1.2.2: NavigationView Items - Technician

**Purpose**: Verify technician sees limited navigation

**Prerequisites**: Logged in as technician

**Steps**:
1. Observe navigation menu

**Expected Navigation Items (Technician)**:
- [ ] Home
- [ ] Guides
- [ ] Progress
- [ ] Settings
- [ ] About

**Verification**:
- [ ] "Guide Editor" NOT visible
- [ ] All other items visible
- [ ] 5 items total

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 1.2.3: Navigation - Click Each Item

**Purpose**: Verify all navigation items work

**Steps**:
1. Click "Home" → Verify home page loads
2. Click "Guides" → Verify guides page loads
3. Click "Guide Editor" (admin) → Verify editor loads
4. Click "Progress" → Verify progress page loads
5. Click "Settings" → Verify settings page loads
6. Click "About" → Verify about page loads

**Expected Results** (for each):
- [ ] Page loads without error
- [ ] Page content visible
- [ ] Navigation item highlighted/selected
- [ ] Page title correct

**Verification**:
- [ ] All pages accessible
- [ ] No broken navigation
- [ ] Visual feedback on selection

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 1.2.4: Navigation - Browser-Style Back Button

**Purpose**: Verify back navigation works

**Steps**:
1. Navigate: Home → Guides → Settings
2. Click browser-style back button (if visible)
3. **Alternative**: Use Alt+Left arrow or mouse back button

**Expected Results**:
- [ ] Navigate back to Guides page
- [ ] Click back again → Navigate to Home
- [ ] Navigation history maintained

**Verification**:
- [ ] Back button enabled when history exists
- [ ] Back button disabled at Home (no more history)

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 1.2.5: Navigation with Parameters

**Purpose**: Verify navigation with guide ID parameter works

**Steps**:
1. Navigate to Guides page
2. Click on a guide card to open detail page

**Expected Results**:
- [ ] Guide detail page opens
- [ ] Correct guide displayed
- [ ] Can navigate back to Guides

**Verification**:
- [ ] Guide title matches clicked guide
- [ ] All guide data visible
- [ ] Navigation parameter passed correctly

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

### M1.3: Settings Management

#### Test 1.3.1: Settings Page - Initial State

**Purpose**: Verify settings page displays correctly

**Steps**:
1. Navigate to Settings page

**Expected Settings Sections**:
- [ ] General Settings
- [ ] Auto-Save Settings
- [ ] Sample Data
- [ ] Database Management

**Verification**:
- [ ] All sections visible
- [ ] Default values loaded
- [ ] UI organized clearly

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 1.3.2: Sample Data Seeding

**Purpose**: Verify sample data can be seeded

**Steps**:
1. Navigate to Settings page
2. Locate "Sample Data" section
3. Click "Seed Sample Data" button

**Expected Results**:
- [ ] Success message appears
- [ ] 10 sample guides created
- [ ] 5 sample categories created
- [ ] No errors

**Verification**:
1. Navigate to Guides page
2. **Verify**:
   - [ ] 10 guides visible
   - [ ] Guides have titles, descriptions, steps
   - [ ] Categories include: Installation, Maintenance, Troubleshooting, Safety, Advanced
   - [ ] Some guides have images

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 1.3.3: Auto-Save Interval Setting

**Purpose**: Verify auto-save interval can be changed

**Steps**:
1. Navigate to Settings page
2. Locate auto-save interval setting
3. Change value (e.g., from 30 to 60 seconds)
4. Save settings (if save button exists)

**Expected Results**:
- [ ] Setting updated successfully
- [ ] Success message or confirmation

**Verification**:
1. Close and reopen Settings page
2. **Verify**: Auto-save interval shows new value (60)
3. Setting persisted in database

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 1.3.4: Enable/Disable Auto-Save

**Purpose**: Verify auto-save can be toggled

**Steps**:
1. Navigate to Settings page
2. Locate auto-save enabled toggle
3. Toggle OFF
4. Save settings

**Expected Results**:
- [ ] Auto-save disabled
- [ ] Setting saved

**Verification**:
1. Navigate to Guide Editor
2. Make changes to a guide
3. Wait 30+ seconds
4. **Verify**: No auto-save occurs (changes not saved automatically)

**Cleanup**: Re-enable auto-save

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 1.3.5: Settings Persistence

**Purpose**: Verify settings survive app restart

**Steps**:
1. Change auto-save interval to 90 seconds
2. Disable auto-save
3. Close application
4. Relaunch application
5. Navigate to Settings page

**Expected Results**:
- [ ] Auto-save interval: 90 seconds
- [ ] Auto-save: Disabled
- [ ] All settings preserved

**Verification**:
- [ ] Settings loaded from database
- [ ] No reset to defaults

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

### M1.4: Logging

#### Test 1.4.1: Log Files Created

**Purpose**: Verify application creates log files

**Steps**:
1. Launch application
2. Perform some actions (navigate pages, create guide, etc.)
3. Navigate to: `%LocalAppData%\GuideViewer\logs\`

**Expected Results**:
- [ ] Logs folder exists
- [ ] At least one log file present
- [ ] File name format: `app-YYYYMMDD.log`

**Verification**:
- [ ] Log file size > 0 bytes
- [ ] File is readable

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 1.4.2: Log Content - Information Level

**Purpose**: Verify informational messages are logged

**Steps**:
1. Open most recent log file
2. Search for "Information" entries

**Expected Log Entries**:
- [ ] Application startup
- [ ] User activation
- [ ] Page navigation
- [ ] Successful operations

**Verification**:
- [ ] Timestamps present
- [ ] Log level indicated (Information)
- [ ] Readable messages

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 1.4.3: Log Content - Error Level

**Purpose**: Verify errors are logged

**Steps**:
1. Trigger an error (e.g., invalid product key)
2. Open log file
3. Search for "Error" entries

**Expected Results**:
- [ ] Error logged with details
- [ ] Exception information included
- [ ] Stack trace if applicable

**Verification**:
- [ ] Error timestamp
- [ ] Error message clear
- [ ] Helps with debugging

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 1.4.4: Log Rotation

**Purpose**: Verify old logs are deleted (7 day retention)

**Note**: This test requires 8+ days to complete naturally, or date manipulation

**Steps**:
1. **Simulate**: Create log files with old dates in filename
   - `app-20250101.log` (45 days old)
   - `app-20250110.log` (7 days old)
2. Launch application
3. Check logs folder

**Expected Results**:
- [ ] Logs older than 7 days deleted
- [ ] Recent logs (< 7 days) retained

**Verification**:
- [ ] Only logs from last 7 days remain
- [ ] Cleanup automatic on startup

**Pass/Fail**: ________ (May require code inspection)
**Notes**: ___________________________________________

---

## Milestone 2: Guide Management

### M2.1: Category Management

#### Test 2.1.1: View Categories List

**Purpose**: Verify categories page displays correctly

**Prerequisites**: Sample data seeded (Test 1.3.2)

**Steps**:
1. Navigate to Settings page
2. Scroll to "Category Management" section

**Expected Results**:
- [ ] List of categories displayed
- [ ] Each category shows: Name, Icon, Color
- [ ] 5 default categories visible

**Default Categories**:
- [ ] Installation
- [ ] Maintenance
- [ ] Troubleshooting
- [ ] Safety
- [ ] Advanced

**Verification**:
- [ ] Category names visible
- [ ] Icons rendered correctly
- [ ] Colors applied

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 2.1.2: Create New Category (Admin Only)

**Purpose**: Verify admin can create categories

**Prerequisites**: Logged in as admin

**Steps**:
1. Navigate to Settings → Category Management
2. Click "Add Category" or "New Category" button
3. Enter:
   - Name: "Testing Category"
   - Icon: Select an icon (e.g., Beaker)
   - Color: Select color (e.g., Purple #800080)
4. Click "Save"

**Expected Results**:
- [ ] Category created successfully
- [ ] Success message appears
- [ ] New category appears in list

**Verification**:
- [ ] Category persisted in database
- [ ] Navigate to Guides page
- [ ] Category filter dropdown includes "Testing Category"

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 2.1.3: Create Category - Duplicate Name

**Purpose**: Verify duplicate category names are prevented

**Steps**:
1. Try to create category with name: "Installation" (already exists)
2. Click "Save"

**Expected Results**:
- [ ] Validation error: "Category already exists"
- [ ] Category not created
- [ ] Can cancel or correct name

**Verification**:
- [ ] Error message clear
- [ ] No duplicate in list

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 2.1.4: Edit Category (Admin Only)

**Purpose**: Verify admin can edit categories

**Steps**:
1. Navigate to Category Management
2. Click "Edit" button on "Testing Category"
3. Change name to: "QA Testing"
4. Change color to: Red #FF0000
5. Click "Save"

**Expected Results**:
- [ ] Category updated successfully
- [ ] Changes reflected in list immediately

**Verification**:
- [ ] Name changed to "QA Testing"
- [ ] Color changed to red
- [ ] Changes persisted

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 2.1.5: Delete Category (Admin Only)

**Purpose**: Verify admin can delete unused categories

**Steps**:
1. Navigate to Category Management
2. Click "Delete" button on "QA Testing" category
3. Confirm deletion when prompted

**Expected Results**:
- [ ] Confirmation dialog appears
- [ ] After confirming, category deleted
- [ ] Success message

**Verification**:
- [ ] Category removed from list
- [ ] Category not in filter dropdown
- [ ] Database record deleted

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 2.1.6: Delete Category - In Use

**Purpose**: Verify categories in use cannot be deleted

**Steps**:
1. Create a new category: "In Use Test"
2. Create a guide assigned to "In Use Test" category
3. Try to delete "In Use Test" category

**Expected Results**:
- [ ] Error message: "Cannot delete category with existing guides"
- [ ] **OR**: Dialog prompts to reassign guides first
- [ ] Category not deleted

**Verification**:
- [ ] Category still exists
- [ ] Guide still assigned to category
- [ ] Data integrity maintained

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 2.1.7: Category Icons Rendering

**Purpose**: Verify all category icons render correctly

**Steps**:
1. View category list
2. Observe icon glyphs

**Expected Results**:
- [ ] All icons visible (no missing glyphs)
- [ ] Icons appropriate for category (wrench, tool, etc.)
- [ ] Consistent sizing

**Verification**:
- [ ] No placeholder/missing icon symbols (□)
- [ ] Icons recognizable

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

### M2.2: Guide CRUD Operations

#### Test 2.2.1: View Guides List

**Purpose**: Verify guides list displays correctly

**Prerequisites**: Sample data seeded

**Steps**:
1. Navigate to Guides page

**Expected Results**:
- [ ] 10 sample guides displayed
- [ ] Guide cards show: Title, Description, Category, Estimated Time
- [ ] Cards arranged in grid layout
- [ ] Images visible (if guides have images)

**Verification**:
- [ ] All guide data visible
- [ ] Cards have consistent styling
- [ ] Scrollable if many guides

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 2.2.2: Create New Guide (Admin Only)

**Purpose**: Verify admin can create guides

**Prerequisites**: Logged in as admin

**Steps**:
1. Navigate to Guide Editor (click menu item or Ctrl+N)
2. Enter:
   - Title: "Test Guide Creation"
   - Category: "Installation"
   - Description: "This is a test guide for manual testing"
   - Estimated Time: 45 minutes
3. Add Step 1:
   - Title: "First Step"
   - Content: "This is the first step content"
4. Click "Save Guide"

**Expected Results**:
- [ ] Guide saved successfully
- [ ] Success message appears
- [ ] Navigate to Guides page automatically OR guide detail page

**Verification**:
1. Navigate to Guides page
2. Find "Test Guide Creation"
3. **Verify**:
   - [ ] Title correct
   - [ ] Category: Installation
   - [ ] Description visible
   - [ ] Estimated time: 45 minutes

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 2.2.3: Create Guide - Required Fields Validation

**Purpose**: Verify required fields are enforced

**Steps**:
1. Navigate to Guide Editor
2. Leave title empty
3. Try to save

**Expected Results**:
- [ ] Validation error: "Title is required"
- [ ] Guide not saved
- [ ] Focus on title field

**Test Each Required Field**:
- [ ] Title (required)
- [ ] Category (required)
- [ ] At least one step (required)

**Verification**:
- [ ] Cannot save incomplete guide
- [ ] Clear validation messages

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 2.2.4: View Guide Detail

**Purpose**: Verify guide detail page displays all information

**Steps**:
1. Navigate to Guides page
2. Click on "Test Guide Creation" card

**Expected Results**:
- [ ] Guide detail page opens
- [ ] Header shows: Title, Category, Description, Estimated Time
- [ ] Steps listed in order
- [ ] Navigation controls visible (Previous/Next step)

**Verification**:
- [ ] All guide metadata visible
- [ ] Step content readable
- [ ] Can navigate through steps

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 2.2.5: Edit Existing Guide (Admin Only)

**Purpose**: Verify admin can edit guides

**Steps**:
1. From guide detail page, click "Edit" button (or navigate to editor)
2. Change title to: "Test Guide Creation - Edited"
3. Change estimated time to: 60 minutes
4. Add a second step:
   - Title: "Second Step"
   - Content: "Additional step content"
5. Click "Save Guide"

**Expected Results**:
- [ ] Guide updated successfully
- [ ] Changes saved to database

**Verification**:
1. Navigate back to Guides page
2. Open "Test Guide Creation - Edited"
3. **Verify**:
   - [ ] Title updated
   - [ ] Estimated time: 60 minutes
   - [ ] 2 steps present
   - [ ] Second step content visible

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 2.2.6: Delete Guide (Admin Only)

**Purpose**: Verify admin can delete guides

**Steps**:
1. Navigate to Guides page
2. Find "Test Guide Creation - Edited"
3. Click "Delete" button on guide card
4. Confirm deletion

**Expected Results**:
- [ ] Confirmation dialog: "Are you sure?"
- [ ] After confirming, guide deleted
- [ ] Success message
- [ ] Guide removed from list

**Verification**:
- [ ] Guide no longer in database
- [ ] Guide count decreased by 1

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 2.2.7: Guide Card Actions - Admin vs Technician

**Purpose**: Verify role-based actions on guide cards

**Admin Actions** (logged in as admin):
- [ ] Edit button visible
- [ ] Delete button visible
- [ ] Export button visible (M4 feature)

**Technician Actions** (logged in as technician):
- [ ] Edit button NOT visible
- [ ] Delete button NOT visible
- [ ] Can only view guides

**Verification**:
- [ ] Proper role enforcement
- [ ] Technicians cannot modify guides

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

### M2.3: Guide Editor

#### Test 2.3.1: Editor UI Layout

**Purpose**: Verify guide editor has all necessary fields

**Steps**:
1. Navigate to Guide Editor (empty, new guide)

**Expected Editor Fields**:
- [ ] Title text box
- [ ] Category dropdown
- [ ] Description text box (multiline)
- [ ] Estimated time input (number + unit)
- [ ] Steps section
- [ ] Add Step button
- [ ] Save Guide button
- [ ] Cancel button

**Verification**:
- [ ] All fields visible and labeled
- [ ] Clear layout
- [ ] Responsive design

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 2.3.2: Add Multiple Steps

**Purpose**: Verify can add multiple steps to a guide

**Steps**:
1. Create new guide: "Multi-Step Test"
2. Add Step 1: "Step One" / "Content one"
3. Click "Add Step" button
4. Add Step 2: "Step Two" / "Content two"
5. Click "Add Step" button
6. Add Step 3: "Step Three" / "Content three"
7. Save guide

**Expected Results**:
- [ ] Can add unlimited steps
- [ ] Steps numbered automatically
- [ ] Each step has title and content fields

**Verification**:
1. Open guide detail page
2. **Verify**:
   - [ ] 3 steps present
   - [ ] Steps in correct order (1, 2, 3)
   - [ ] All content preserved

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 2.3.3: Reorder Steps

**Purpose**: Verify steps can be reordered

**Steps**:
1. Edit "Multi-Step Test" guide
2. Locate step reordering controls (up/down arrows or drag handles)
3. Move Step 3 to position 1

**Expected Results**:
- [ ] Steps reordered in editor
- [ ] Step numbers update automatically

**Verification**:
1. Save guide
2. Open guide detail
3. **Verify**: Step order is 3, 1, 2 (based on titles)

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 2.3.4: Delete Step

**Purpose**: Verify steps can be deleted

**Steps**:
1. Edit "Multi-Step Test" guide
2. Click "Delete" button on Step 2
3. Confirm deletion
4. Save guide

**Expected Results**:
- [ ] Step removed from editor
- [ ] Remaining steps renumbered

**Verification**:
1. Open guide detail
2. **Verify**: Only 2 steps remain (formerly steps 3 and 1)

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 2.3.5: Add Image to Step

**Purpose**: Verify images can be added to steps

**Prerequisites**: Valid image file (PNG/JPG, < 10MB)

**Steps**:
1. Edit guide or create new guide
2. In a step, click "Add Image" button
3. Select image file from file picker
4. Click Open

**Expected Results**:
- [ ] Image uploaded successfully
- [ ] Image preview displayed in editor
- [ ] File size validated (< 10MB)

**Verification**:
1. Save guide
2. Open guide detail page
3. **Verify**:
   - [ ] Image displays in step
   - [ ] Image quality preserved
   - [ ] Correct image shown

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 2.3.6: Replace Step Image

**Purpose**: Verify step images can be replaced

**Steps**:
1. Edit guide with step containing image
2. Click "Change Image" or "Add Image" again
3. Select different image
4. Save guide

**Expected Results**:
- [ ] Old image replaced with new image
- [ ] Only one image per step

**Verification**:
- [ ] New image displays
- [ ] Old image no longer stored (database size doesn't double)

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 2.3.7: Remove Step Image

**Purpose**: Verify step images can be removed

**Steps**:
1. Edit guide with step containing image
2. Click "Remove Image" button
3. Save guide

**Expected Results**:
- [ ] Image removed from step
- [ ] Image preview cleared in editor

**Verification**:
- [ ] Guide detail page shows no image for that step
- [ ] Database image deleted (space reclaimed)

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 2.3.8: Auto-Save Functionality

**Purpose**: Verify auto-save works in guide editor

**Prerequisites**: Auto-save enabled in settings (interval: 30 seconds)

**Steps**:
1. Create new guide: "Auto-Save Test"
2. Enter title and description
3. Add one step
4. **Wait 30+ seconds without clicking Save**

**Expected Results**:
- [ ] Auto-save indicator appears ("Saving..." or checkmark)
- [ ] Guide saved to database automatically
- [ ] No user action required

**Verification**:
1. Navigate away from editor (don't click Save)
2. Return to Guides page
3. **Verify**: "Auto-Save Test" guide exists in list

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 2.3.9: Auto-Save Interval Configuration

**Purpose**: Verify auto-save respects configured interval

**Steps**:
1. Navigate to Settings
2. Set auto-save interval to 10 seconds
3. Create new guide: "Quick Auto-Save"
4. Enter title
5. Start timer, wait 10 seconds

**Expected Results**:
- [ ] Auto-save occurs at ~10 seconds
- [ ] Indicator shows save happened

**Verification**:
- [ ] Auto-save timing matches configuration
- [ ] Can configure different intervals

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 2.3.10: Cancel Editing

**Purpose**: Verify cancel button discards unsaved changes

**Steps**:
1. Edit existing guide
2. Make changes (don't save)
3. Click "Cancel" button
4. Confirm when prompted

**Expected Results**:
- [ ] Confirmation dialog: "Discard unsaved changes?"
- [ ] After confirming, return to previous page
- [ ] Changes not saved

**Verification**:
1. Open same guide again
2. **Verify**: Original data unchanged

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 2.3.11: Text Formatting (Bold, Italic)

**Purpose**: Verify rich text formatting works in editor

**Steps**:
1. Create new guide
2. In step content field, enter text: "This is bold and italic"
3. Select word "bold"
4. Click Bold button (or Ctrl+B)
5. Select word "italic"
6. Click Italic button (or Ctrl+I)
7. Save guide

**Expected Results**:
- [ ] Text formatted in editor
- [ ] Formatting buttons toggle state
- [ ] Preview shows formatting

**Verification**:
1. Open guide detail page
2. **Verify**:
   - [ ] "bold" appears in bold
   - [ ] "italic" appears in italic
   - [ ] Formatting preserved

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

### M2.4: Search & Filter

#### Test 2.4.1: Search by Title

**Purpose**: Verify search filters guides by title

**Prerequisites**: Multiple guides with varied titles

**Steps**:
1. Navigate to Guides page
2. In search box, type: "kitchen"

**Expected Results**:
- [ ] Real-time filtering (results update as you type)
- [ ] Only guides with "kitchen" in title shown
- [ ] Case-insensitive search

**Verification**:
- [ ] "Kitchen Sink Installation" visible (if exists)
- [ ] Other guides hidden
- [ ] Clear search button appears

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 2.4.2: Search by Description

**Purpose**: Verify search includes description text

**Steps**:
1. Create guide with unique word in description only
   - Title: "Generic Guide"
   - Description: "Contains keyword XYZTEST123"
2. Search for: "XYZTEST123"

**Expected Results**:
- [ ] Guide found based on description match
- [ ] Search covers both title and description

**Verification**:
- [ ] "Generic Guide" appears in results
- [ ] Search is comprehensive

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 2.4.3: Filter by Category

**Purpose**: Verify category filter works

**Steps**:
1. Navigate to Guides page
2. Click category filter dropdown
3. Select "Installation"

**Expected Results**:
- [ ] Only guides in "Installation" category shown
- [ ] Other categories filtered out
- [ ] Filter dropdown shows selected category

**Verification**:
- [ ] All visible guides have category "Installation"
- [ ] Guide count reflects filter
- [ ] Can select "All Categories" to reset

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 2.4.4: Combined Search and Filter

**Purpose**: Verify search and filter work together

**Steps**:
1. Select category filter: "Maintenance"
2. Type in search: "water"

**Expected Results**:
- [ ] Results match BOTH criteria
- [ ] Only Maintenance guides with "water" in title/description

**Verification**:
- [ ] Filters apply cumulatively
- [ ] Results accurate

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 2.4.5: Clear Search

**Purpose**: Verify search can be cleared

**Steps**:
1. Search for: "test"
2. Click "X" or clear button in search box

**Expected Results**:
- [ ] Search box cleared
- [ ] All guides shown again (respecting category filter if active)

**Verification**:
- [ ] Full guide list restored
- [ ] Search state reset

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 2.4.6: No Results Found

**Purpose**: Verify appropriate message when no matches

**Steps**:
1. Search for: "nonexistentguide12345"

**Expected Results**:
- [ ] "No guides found" message displayed
- [ ] Empty state UI shown
- [ ] Suggestion to adjust search or create guide

**Verification**:
- [ ] User-friendly message
- [ ] Not a blank page
- [ ] Can clear search to see guides again

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

### M2.5: Image Management

#### Test 2.5.1: Image Upload - Valid Formats

**Purpose**: Verify supported image formats upload successfully

**Test Each Format**:
- [ ] PNG (.png)
- [ ] JPEG (.jpg)
- [ ] JPEG (.jpeg)
- [ ] BMP (.bmp)

**Steps** (for each):
1. Create guide with step
2. Add image of specified format
3. Save guide

**Expected Results**:
- [ ] All formats accepted
- [ ] Images display correctly

**Verification**:
- [ ] Image quality preserved
- [ ] Format doesn't affect functionality

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 2.5.2: Image Size Validation - Under 10MB

**Purpose**: Verify images under 10MB are accepted

**Steps**:
1. Prepare image file: 8MB (valid)
2. Add to step in guide editor
3. Save guide

**Expected Results**:
- [ ] Image accepted
- [ ] No size warnings
- [ ] Upload completes successfully

**Verification**:
- [ ] Image displays in guide
- [ ] No performance issues

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 2.5.3: Image Size Validation - Over 10MB

**Purpose**: Verify images over 10MB are rejected

**Steps**:
1. Prepare image file: 12MB (too large)
2. Attempt to add to step
3. Click Open in file picker

**Expected Results**:
- [ ] Validation error: "Image too large (max 10 MB)"
- [ ] Image not uploaded
- [ ] Suggested action: "Resize or compress the image"

**Verification**:
- [ ] Clear error message
- [ ] Step remains without image
- [ ] Can try with smaller image

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 2.5.4: Image Storage in Database

**Purpose**: Verify images stored correctly in LiteDB FileStorage

**Steps**:
1. Create guide with 3 steps, each with different image
2. Save guide
3. Navigate to: `%LocalAppData%\GuideViewer\`
4. Check `data.db` file size

**Expected Results**:
- [ ] Database file size increased
- [ ] Images stored internally (not as separate files)

**Verification**:
1. Close application
2. Move/delete original image files
3. Relaunch application
4. Open guide
5. **Verify**: Images still display (stored in database)

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 2.5.5: Image Display in Different Contexts

**Purpose**: Verify images display correctly everywhere

**Test Contexts**:
- [ ] Guide editor (preview)
- [ ] Guide detail page (step view)
- [ ] Active guide progress (if shown)

**Steps**:
1. Create guide with step image
2. View in each context

**Expected Results**:
- [ ] Images render correctly in all views
- [ ] Appropriate sizing (responsive)
- [ ] No broken images

**Verification**:
- [ ] Consistent display
- [ ] Maintains aspect ratio
- [ ] Good quality

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

## Milestone 3: Progress Tracking

### M3.1: User Progress Tracking

#### Test 3.1.1: Start Tracking Progress on Guide

**Purpose**: Verify user can start tracking progress

**Steps**:
1. Navigate to Guides page
2. Open a guide detail page
3. Click "Start Guide" or "Track Progress" button

**Expected Results**:
- [ ] Progress tracking initialized
- [ ] Step 1 marked as "in progress"
- [ ] Progress bar appears (0% or based on completed steps)
- [ ] Timer starts (if applicable)

**Verification**:
- [ ] Database record created for user progress
- [ ] Can navigate through steps
- [ ] Progress persists

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 3.1.2: Mark Step as Complete

**Purpose**: Verify individual steps can be marked complete

**Steps**:
1. In active guide, navigate to step 1
2. Click "Complete Step" or checkbox
3. Navigate to step 2
4. Mark as complete

**Expected Results**:
- [ ] Step marked complete immediately
- [ ] Progress bar updates (e.g., 2/10 = 20%)
- [ ] Checkmark or visual indicator on completed step

**Verification**:
- [ ] Progress percentage calculated correctly
- [ ] Completed steps persist
- [ ] Can view progress from guide list

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 3.1.3: Resume Progress

**Purpose**: Verify progress can be resumed after closing guide

**Steps**:
1. Start guide, complete 3 steps
2. Navigate away (to Home or Guides page)
3. Return to same guide

**Expected Results**:
- [ ] Progress preserved
- [ ] Resume from last position (step 4 or last viewed)
- [ ] Completed steps still marked

**Verification**:
- [ ] Progress bar shows 3/X steps complete
- [ ] No data loss
- [ ] Can continue from where left off

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 3.1.4: Complete All Steps

**Purpose**: Verify guide can be fully completed

**Steps**:
1. Continue marking steps complete until all done
2. Mark final step as complete

**Expected Results**:
- [ ] Progress: 100%
- [ ] "Guide Complete!" message or indicator
- [ ] Completion timestamp recorded
- [ ] Optional: Celebration animation or feedback

**Verification**:
1. Navigate to Progress Dashboard
2. **Verify**: Guide shown as completed
3. Completion date/time visible

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 3.1.5: Reset Progress

**Purpose**: Verify progress can be reset to start over

**Steps**:
1. Open completed guide
2. Click "Reset Progress" or "Start Over" button
3. Confirm action

**Expected Results**:
- [ ] Confirmation dialog appears
- [ ] After confirming, all steps unmarked
- [ ] Progress: 0%
- [ ] Completion timestamp cleared

**Verification**:
- [ ] Can start fresh
- [ ] No residual completion markers

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 3.1.6: Track Multiple Guides Simultaneously

**Purpose**: Verify user can have progress on multiple guides at once

**Steps**:
1. Start Guide A, complete 2 steps
2. Navigate to Guides page
3. Start Guide B, complete 3 steps
4. Start Guide C, complete 1 step

**Expected Results**:
- [ ] All 3 guides have independent progress
- [ ] No conflicts or overwriting
- [ ] Can switch between guides

**Verification**:
1. Return to Guide A → Verify: 2 steps complete
2. Return to Guide B → Verify: 3 steps complete
3. Return to Guide C → Verify: 1 step complete

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

### M3.2: Progress Dashboard

#### Test 3.2.1: View Progress Dashboard - Overview

**Purpose**: Verify progress dashboard displays user statistics

**Steps**:
1. Navigate to Progress page (or Dashboard)

**Expected Dashboard Sections**:
- [ ] Overview statistics card
- [ ] In-progress guides list
- [ ] Completed guides list
- [ ] Recent activity

**Verification**:
- [ ] All sections visible
- [ ] Data loads without error
- [ ] Responsive layout

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 3.2.2: Progress Statistics - Calculations

**Purpose**: Verify statistics are calculated correctly

**Prerequisites**: Known progress state (e.g., 2 completed, 3 in-progress)

**Expected Statistics**:
- [ ] Total guides: X
- [ ] Completed guides: 2
- [ ] In progress guides: 3
- [ ] Overall completion percentage: (2 / 5 * 100 = 40%)
- [ ] Total time spent (if tracked)

**Verification**:
- [ ] Numbers match actual state
- [ ] Percentages calculated correctly
- [ ] Updates in real-time when progress changes

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 3.2.3: In-Progress Guides List

**Purpose**: Verify in-progress guides are listed

**Steps**:
1. View Progress Dashboard
2. Locate "In Progress" section

**Expected Results**:
- [ ] All guides with partial progress shown
- [ ] Each guide shows: Title, Progress %, Last accessed date
- [ ] Click guide to resume

**Verification**:
- [ ] Only in-progress guides (not completed or not started)
- [ ] Progress percentages accurate
- [ ] Sorted by recent activity (most recent first)

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 3.2.4: Completed Guides List

**Purpose**: Verify completed guides are listed

**Steps**:
1. View Progress Dashboard
2. Locate "Completed" section

**Expected Results**:
- [ ] All 100% complete guides shown
- [ ] Each guide shows: Title, Completion date, Time taken
- [ ] Can click to view again

**Verification**:
- [ ] Only fully completed guides
- [ ] Completion dates accurate
- [ ] Sorted by completion date (newest first)

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 3.2.5: Empty State - No Progress

**Purpose**: Verify dashboard handles no progress gracefully

**Steps**:
1. **Reset**: Delete all progress records
2. View Progress Dashboard

**Expected Results**:
- [ ] Empty state message: "No guides in progress"
- [ ] Suggestion: "Start a guide to track your progress"
- [ ] Link/button to browse guides

**Verification**:
- [ ] Not an error state
- [ ] User-friendly messaging
- [ ] Clear call to action

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

### M3.3: Progress Reports

#### Test 3.3.1: Generate Progress Report

**Purpose**: Verify progress reports can be generated

**Steps**:
1. Navigate to Progress Dashboard or Reports section
2. Click "Generate Report" button
3. Select date range (e.g., "Last 7 days")
4. Click "Generate"

**Expected Results**:
- [ ] Report generated successfully
- [ ] Report displays in UI or opens in viewer

**Verification**:
- [ ] Report contains accurate data
- [ ] Date range filter applied

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 3.3.2: Report Content - Completed Guides

**Purpose**: Verify report includes completed guides

**Expected Report Data**:
- [ ] List of completed guides in date range
- [ ] For each guide:
  - Guide title
  - Category
  - Completion date
  - Time taken
  - Number of steps

**Verification**:
- [ ] All completed guides included
- [ ] Data accurate
- [ ] Formatted clearly

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 3.3.3: Report Content - Summary Statistics

**Purpose**: Verify report includes summary stats

**Expected Summary Data**:
- [ ] Total guides completed
- [ ] Total guides in progress
- [ ] Total time spent
- [ ] Average completion time
- [ ] Most completed category

**Verification**:
- [ ] Statistics calculated correctly
- [ ] Meaningful insights provided

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 3.3.4: Filter Report by Date Range

**Purpose**: Verify date range filtering works

**Steps**:
1. Generate report for "Last 30 days"
2. Note guides included
3. Generate report for "Last 7 days"
4. Compare results

**Expected Results**:
- [ ] 7-day report is subset of 30-day report
- [ ] Only guides completed in range included
- [ ] Date filter accurate

**Verification**:
- [ ] Filters work correctly
- [ ] Can select custom date ranges

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 3.3.5: Export Report (If Applicable)

**Purpose**: Verify reports can be exported

**Steps**:
1. Generate report
2. Click "Export" button
3. Choose format (PDF, CSV, etc.)
4. Save file

**Expected Results**:
- [ ] Export operation completes
- [ ] File created at chosen location
- [ ] File opens correctly

**Verification**:
- [ ] Exported data matches on-screen report
- [ ] Formatting preserved
- [ ] File readable

**Pass/Fail**: ________ (N/A if export not implemented)
**Notes**: ___________________________________________

---

### M3.4: Active Guide Progress

#### Test 3.4.1: Active Guide View - Navigation

**Purpose**: Verify active guide navigation works

**Steps**:
1. Start or resume a guide
2. Use "Next Step" button to advance
3. Use "Previous Step" button to go back

**Expected Results**:
- [ ] Step navigation smooth
- [ ] Content updates immediately
- [ ] Current step highlighted
- [ ] Progress indicator updates

**Verification**:
- [ ] Can navigate through all steps
- [ ] No stuck or broken navigation
- [ ] Step numbers accurate

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 3.4.2: Active Guide View - Step Completion UI

**Purpose**: Verify step completion controls work

**Steps**:
1. In active guide, view a step
2. Click "Mark Complete" checkbox or button
3. Observe UI feedback

**Expected Results**:
- [ ] Checkbox checked immediately
- [ ] Step marked visually (strikethrough, checkmark, color change)
- [ ] Progress bar updates
- [ ] Can uncheck to mark incomplete

**Verification**:
- [ ] Visual feedback clear
- [ ] State persists
- [ ] Toggle works both ways

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 3.4.3: Active Guide View - Progress Indicator

**Purpose**: Verify progress indicator is accurate

**Expected Indicators**:
- [ ] Progress bar (e.g., 5/10 steps)
- [ ] Percentage (50%)
- [ ] Step list with completion checkmarks
- [ ] Current step highlighted

**Verification**:
- [ ] Progress updates in real-time
- [ ] Accurate calculation
- [ ] Multiple indicators consistent

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 3.4.4: Active Guide View - Timer Display

**Purpose**: Verify elapsed time is tracked and displayed

**Steps**:
1. Start a new guide
2. Note start time
3. Wait 2-3 minutes
4. Observe timer

**Expected Results**:
- [ ] Timer displays elapsed time (MM:SS or HH:MM:SS)
- [ ] Timer counts up
- [ ] Timer pauses when guide closed (optional)

**Verification**:
- [ ] Time tracked accurately
- [ ] Display updates regularly (every second)
- [ ] Time persists across sessions

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 3.4.5: Active Guide View - Estimated Time Comparison

**Purpose**: Verify estimated vs actual time is shown

**Expected Display**:
- [ ] Estimated time: 30 minutes (from guide metadata)
- [ ] Elapsed time: 15 minutes (current progress)
- [ ] Remaining time: ~15 minutes (calculation)
- [ ] Optional: Indicator if over/under estimate

**Verification**:
- [ ] Comparison visible
- [ ] Helps user pace themselves
- [ ] Accurate calculations

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

### M3.5: Timer Service

#### Test 3.5.1: Timer Starts with Guide

**Purpose**: Verify timer starts when guide progress begins

**Steps**:
1. Open a guide (not yet started)
2. Click "Start Guide"
3. Observe timer

**Expected Results**:
- [ ] Timer starts at 0:00
- [ ] Timer begins counting up
- [ ] Start time recorded in database

**Verification**:
- [ ] Timer visible
- [ ] Accurate start time

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 3.5.2: Timer Persists Across Sessions

**Purpose**: Verify elapsed time persists

**Steps**:
1. Start guide, wait 5 minutes
2. Close application
3. Relaunch application
4. Resume guide

**Expected Results**:
- [ ] Timer shows 5+ minutes (plus time elapsed)
- [ ] OR timer resumes from 5 minutes
- [ ] Total time tracked accurately

**Verification**:
- [ ] Time not reset
- [ ] Accurate tracking across sessions

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 3.5.3: Timer Pauses When Guide Inactive

**Purpose**: Verify timer behavior when guide not active

**Steps**:
1. Start guide, wait 2 minutes (timer at 2:00)
2. Navigate to Home page (guide inactive)
3. Wait 1 minute
4. Return to guide

**Expected Behavior (implementation-specific)**:
- [ ] Timer paused at 2:00 (preferred - only tracks active time)
- [ ] **OR** Timer continues to 3:00 (tracks wall-clock time)

**Verification**:
- [ ] Behavior is consistent
- [ ] Documented in app behavior

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 3.5.4: Timer Stops on Completion

**Purpose**: Verify timer stops when guide completed

**Steps**:
1. Start guide
2. Complete all steps
3. Observe timer

**Expected Results**:
- [ ] Timer stops at completion
- [ ] Final time recorded in database
- [ ] Completion time shown on dashboard

**Verification**:
- [ ] Timer no longer incrementing
- [ ] Accurate final time

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 3.5.5: Multiple Guide Timers

**Purpose**: Verify timers independent for multiple guides

**Steps**:
1. Start Guide A, wait 5 minutes (timer: 5:00)
2. Start Guide B, wait 2 minutes (timer: 2:00)
3. Return to Guide A

**Expected Results**:
- [ ] Guide A timer: 5:00 (or continues from 5:00)
- [ ] Guide B timer: 2:00 (separate tracking)
- [ ] No timer conflicts

**Verification**:
- [ ] Each guide has independent timer
- [ ] Times don't mix or overwrite

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

## Milestone 4: Polish & Performance

### M4.1: Data Management

#### Test 4.1.1: Export Guide - JSON with Base64 Images

**Purpose**: Verify guide export with embedded images

**Steps**:
1. Navigate to Guides page
2. Find guide with images (e.g., "Kitchen Sink Installation")
3. Click export button (⬇️ icon)
4. Save as `kitchen_sink.json`

**Expected Results**:
- [ ] Export completes successfully
- [ ] JSON file created

**Verification**:
1. Open JSON in text editor
2. **Verify**:
   - [ ] Contains `"title": "Kitchen Sink Installation"`
   - [ ] Has `"steps"` array
   - [ ] Steps with images have `"imageData": "data:image/png;base64,..."`
   - [ ] No raw binary data

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 4.1.2: Export Guide - ZIP Package

**Purpose**: Verify guide export with separate image files

**Steps**:
1. Export guide as `water_heater.zip`
2. Extract ZIP contents

**Expected Structure**:
```
water_heater/
├── guide.json
└── images/
    ├── step_1_12345678.png
    ├── step_2_23456789.jpg
    └── ...
```

**Verification**:
- [ ] ZIP structure correct
- [ ] guide.json has `"imagePath": "images/..."`
- [ ] All image files exist and open correctly

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 4.1.3: Import Guide - JSON Format

**Purpose**: Verify guide import from JSON

**Steps**:
1. Click "Import Guide" button
2. Select `kitchen_sink.json`
3. Confirm import

**Expected Results**:
- [ ] Import succeeds
- [ ] Success message appears
- [ ] Guide added to list

**Verification**:
1. Open imported guide
2. **Verify**:
   - [ ] Title matches original
   - [ ] All steps present
   - [ ] Images display correctly
   - [ ] Category and metadata preserved

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 4.1.4: Import Guide - ZIP Package

**Purpose**: Verify guide import from ZIP

**Steps**:
1. Click "Import Guide"
2. Select `water_heater.zip`
3. Confirm import

**Expected Results**:
- [ ] Import succeeds
- [ ] All images extracted and stored

**Verification**:
1. Open imported guide
2. Navigate through all steps
3. **Verify**: All images load correctly

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 4.1.5: Import Error - Invalid JSON

**Purpose**: Verify error handling for invalid imports

**Steps**:
1. Create file `invalid.json` with content: `{ "bad": "data" }`
2. Attempt to import
3. Observe error handling

**Expected Results**:
- [ ] Error dialog appears
- [ ] User-friendly message (not raw exception)
- [ ] Suggested actions provided
- [ ] App remains stable

**Verification**:
- [ ] No crash
- [ ] Guide count unchanged

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 4.1.6: Create Database Backup

**Purpose**: Verify database backup with metadata

**Steps**:
1. Navigate to Settings
2. Click "Create Backup"
3. Save as `backup.zip`
4. Extract ZIP contents

**Expected Structure**:
```
backup/
├── data.db
└── metadata.json
```

**Verify metadata.json**:
- [ ] `"backupDate"` (ISO 8601)
- [ ] `"version"` (app version)
- [ ] `"guideCount"`
- [ ] `"categoryCount"`
- [ ] `"userCount"`

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 4.1.7: Restore Database from Backup

**Purpose**: Verify database restoration

**Steps**:
1. Note current data state
2. Create new guide: "Test Delete Me"
3. Navigate to Settings
4. Click "Restore Backup"
5. Select `backup.zip`
6. Confirm restoration

**Expected Results**:
- [ ] Warning dialog about replacing data
- [ ] Restoration completes
- [ ] App prompts restart

**Verification**:
- [ ] After restart, "Test Delete Me" is gone
- [ ] Data matches backup state
- [ ] All guides from backup present

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 4.1.8: Export/Import Round-Trip

**Purpose**: Verify data integrity through export-import cycle

**Steps**:
1. Create guide: "Round Trip Test" with 3 image steps
2. Export as `roundtrip.zip`
3. Delete original guide
4. Import `roundtrip.zip`
5. Compare imported guide

**Expected Results**:
- [ ] Imported guide identical to original

**Verification**:
- [ ] All 3 steps with images
- [ ] Image quality preserved
- [ ] All metadata matches

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

### M4.2: About Page

#### Test 4.2.1: Navigate to About Page

**Purpose**: Verify About page accessible

**Steps**:
1. Click "About" in navigation menu
2. **Alternative**: Press F1 key

**Expected Results**:
- [ ] About page loads
- [ ] Page entrance animation plays
- [ ] All sections visible

**Sections**:
- [ ] App Information
- [ ] System Information
- [ ] Credits
- [ ] Links

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 4.2.2: App Information Accuracy

**Purpose**: Verify app information is correct

**Expected Information**:
- [ ] App name: "GuideViewer"
- [ ] Version: "Version X.Y.Z" (e.g., 1.0.0)
- [ ] Build date: YYYY-MM-DD format
- [ ] Copyright: "© 2025 GuideViewer. All rights reserved."
- [ ] Description text

**Verification**:
- [ ] Version matches assembly version
- [ ] Build date is valid (not default 1/1/0001)
- [ ] Copyright year current

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 4.2.3: System Information Accuracy

**Purpose**: Verify system information detected correctly

**Expected Information**:
- [ ] OS: "Windows 10" or "Windows 11" (with build number)
- [ ] .NET: ".NET 8.0.x"
- [ ] Architecture: "X64" or "Arm64"

**Verification**:
1. Check Windows Settings > System > About
2. Compare OS version and build
3. **Verify**: Match

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 4.2.4: Documentation Link

**Purpose**: Verify documentation link works

**Steps**:
1. Click "Documentation" link

**Expected Results**:
- [ ] ContentDialog appears
- [ ] Lists 4 documentation files:
  - spec.md
  - CLAUDE.md
  - PATTERNS.md
  - CHANGELOG.md
- [ ] "OK" button closes dialog

**Verification**:
- [ ] Dialog readable
- [ ] All docs listed

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 4.2.5: License & GitHub Links

**Purpose**: Verify links show appropriate dialogs

**Steps**:
1. Click "License" → Verify dialog appears with license info
2. Click "GitHub" or "Source Code" → Verify dialog about private app

**Expected Results**:
- [ ] Both links show ContentDialogs
- [ ] No actual browser navigation
- [ ] Appropriate messaging

**Verification**:
- [ ] Dialogs informative
- [ ] Close properly

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 4.2.6: Keyboard Shortcuts Link (About Page)

**Purpose**: Verify keyboard shortcuts help accessible from About

**Steps**:
1. On About page, click "Keyboard Shortcuts" link

**Expected Results**:
- [ ] Keyboard Shortcuts dialog appears
- [ ] Lists all 8 shortcuts
- [ ] Scrollable content
- [ ] Monospace font

**Expected Shortcuts**:
- F1, F2, Ctrl+N, Ctrl+F, Ctrl+E, Ctrl+B, Ctrl+I, Escape

**Verification**:
- [ ] All shortcuts listed with descriptions
- [ ] Dialog closes properly

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

### M4.3: Keyboard Shortcuts

#### Test 4.3.1: F1 - Navigate to About

**Purpose**: Verify F1 opens About page

**Steps**:
1. From any page, press **F1**

**Expected Results**:
- [ ] About page opens
- [ ] Animation plays

**Verification**:
- [ ] Works from all pages
- [ ] Consistent behavior

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 4.3.2: F2 - Show Keyboard Shortcuts

**Purpose**: Verify F2 shows shortcuts dialog

**Steps**:
1. From any page, press **F2**

**Expected Results**:
- [ ] Keyboard Shortcuts dialog appears
- [ ] Lists all shortcuts

**Verification**:
- [ ] Dialog content correct
- [ ] Closes with "Close" button or Escape

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 4.3.3: Ctrl+F - Navigate to Guides

**Purpose**: Verify Ctrl+F opens Guides page

**Steps**:
1. Press **Ctrl+F**

**Expected Results**:
- [ ] Guides page opens
- [ ] Guide list loads

**Verification**:
- [ ] Works from any page
- [ ] Loads quickly

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 4.3.4: Ctrl+N - Create Guide (Admin)

**Purpose**: Verify Ctrl+N opens editor for admins

**Prerequisites**: Logged in as admin

**Steps**:
1. Press **Ctrl+N**

**Expected Results**:
- [ ] Guide Editor opens
- [ ] Empty form for new guide

**Verification**:
- [ ] Works from any page (admin only)

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 4.3.5: Ctrl+N - Disabled (Technician)

**Purpose**: Verify Ctrl+N does nothing for technicians

**Prerequisites**: Logged in as technician

**Steps**:
1. Press **Ctrl+N**

**Expected Results**:
- [ ] Nothing happens
- [ ] Current page remains

**Verification**:
- [ ] Shortcut disabled for technicians
- [ ] No error messages

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 4.3.6: Ctrl+E - Export Guide

**Purpose**: Verify Ctrl+E triggers export

**Prerequisites**: On guide detail page (admin)

**Steps**:
1. Open any guide detail page
2. Press **Ctrl+E**

**Expected Results**:
- [ ] File save dialog appears
- [ ] Can export guide

**Verification**:
- [ ] Export completes successfully

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 4.3.7: Ctrl+B / Ctrl+I - Text Formatting

**Purpose**: Verify formatting shortcuts in editor

**Prerequisites**: In Guide Editor, step content field

**Steps**:
1. Type text, select word
2. Press **Ctrl+B**
3. Select another word
4. Press **Ctrl+I**

**Expected Results**:
- [ ] Bold toggle works
- [ ] Italic toggle works
- [ ] Buttons show state

**Verification**:
- [ ] Formatting applied
- [ ] Preserved on save

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 4.3.8: Escape - Go Back

**Purpose**: Verify Escape navigates back

**Steps**:
1. Navigate: Home → Guides → Guide Detail
2. Press **Escape**
3. Press **Escape** again

**Expected Results**:
- [ ] First Escape: Back to Guides
- [ ] Second Escape: Back to Home
- [ ] At Home: Escape does nothing

**Verification**:
- [ ] Back navigation works
- [ ] No errors

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 4.3.9: Tooltips on Navigation Items

**Purpose**: Verify tooltips show shortcuts

**Steps**:
1. Hover over "About" navigation item
2. Wait for tooltip

**Expected Tooltip**: "About (F1)"

**Repeat for**:
- [ ] Guides: "Guides (Ctrl+F)"
- [ ] Guide Editor (admin): "New Guide (Ctrl+N)"

**Verification**:
- [ ] Tooltips appear
- [ ] Correct text

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 4.3.10: No Conflicts with System Shortcuts

**Purpose**: Verify custom shortcuts don't interfere

**Steps**:
1. In Guide Editor, title field
2. Press **Ctrl+A** (Select All)
3. Press **Ctrl+C** (Copy)
4. Press **Ctrl+V** (Paste)
5. Press **Ctrl+Z** (Undo)

**Expected Results**:
- [ ] All standard Windows shortcuts work
- [ ] No interference

**Verification**:
- [ ] Text operations complete successfully
- [ ] Custom shortcuts only active when appropriate

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

### M4.4: Error Handling

#### Test 4.4.1: File Not Found Error

**Purpose**: Verify file errors handled gracefully

**Steps**:
1. Settings → Restore Backup
2. Try to open non-existent file
   (File picker may prevent this, so test via code if needed)

**Expected Results**:
- [ ] Error dialog appears
- [ ] User-friendly message
- [ ] Suggested actions
- [ ] App remains stable

**Verification**:
- [ ] No crash
- [ ] Clear error message

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 4.4.2: Database Error Handling

**Purpose**: Verify database errors handled

**Steps**:
1. Close app
2. Corrupt `data.db` file (rename or write garbage)
3. Launch app

**Expected Results**:
- [ ] Error dialog OR creates new database
- [ ] Graceful handling
- [ ] App doesn't crash

**Cleanup**: Restore valid database

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 4.4.3: Validation Errors - Required Fields

**Purpose**: Verify validation errors are user-friendly

**Steps**:
1. Guide Editor
2. Leave title empty
3. Try to save

**Expected Results**:
- [ ] Validation error appears
- [ ] Message: "Title is required"
- [ ] Guide not saved

**Verification**:
- [ ] Clear messaging
- [ ] Can correct and save

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 4.4.4: Image Size Validation Error

**Purpose**: Verify image size limits enforced

**Steps**:
1. Try to upload image > 10 MB
2. Observe error

**Expected Results**:
- [ ] Error: "Image too large (max 10 MB)"
- [ ] Suggested action: "Resize or compress"
- [ ] Image not uploaded

**Verification**:
- [ ] Clear error
- [ ] Can try smaller image

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 4.4.5: Unhandled Exception Recovery

**Purpose**: Verify global exception handler prevents crashes

**Note**: Requires code modification to trigger unhandled exception

**Steps**:
1. Trigger exception (e.g., button that throws)
2. Observe behavior

**Expected Results**:
- [ ] Error dialog appears
- [ ] App does NOT crash
- [ ] Error logged
- [ ] Can continue using app

**Verification**:
- [ ] Check log file for exception
- [ ] App remains responsive

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 4.4.6: Error Categorization

**Purpose**: Verify errors categorized correctly

**Test Categories**:
- [ ] FileIO: File operations
- [ ] Database: Database operations
- [ ] Validation: Input validation
- [ ] Resource: Image size, memory
- [ ] Security: Permissions
- [ ] Configuration: Settings issues

**Verification**:
- [ ] Each error type shows appropriate category
- [ ] Suggested actions match category

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

### M4.5: Performance Monitoring

#### Test 4.5.1: Startup Performance

**Purpose**: Verify startup meets < 2 second target

**Steps**:
1. Close app
2. Start stopwatch
3. Launch app
4. Stop when UI appears

**Trials**:
- Trial 1: _______ seconds
- Trial 2: _______ seconds
- Trial 3: _______ seconds
- Average: _______ seconds

**Target**: < 2 seconds

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 4.5.2: Guide List Load Performance

**Purpose**: Verify guide list loads < 500ms

**Prerequisites**: 50+ guides in database

**Steps**:
1. Navigate to Guides page
2. Measure load time

**Trials**:
- Trial 1: _______ ms
- Trial 2: _______ ms
- Trial 3: _______ ms
- Average: _______ ms

**Target**: < 500ms

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 4.5.3: Step Navigation Performance

**Purpose**: Verify step navigation < 100ms

**Steps**:
1. Open guide with 10+ steps
2. Click "Next Step" rapidly 10 times
3. Observe response

**Expected Results**:
- [ ] Immediate response (< 100ms)
- [ ] No lag
- [ ] Smooth navigation

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 4.5.4: Memory Usage

**Purpose**: Verify memory < 150 MB

**Steps**:
1. Launch app
2. Open Task Manager
3. Find GuideViewer.exe
4. Note memory usage
5. Use app for 10 minutes
6. Check memory again

**Measurements**:
- Initial: _______ MB
- After 5 min: _______ MB
- After 10 min: _______ MB

**Target**: < 150 MB

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 4.5.5: Performance Logging

**Purpose**: Verify slow operations logged

**Steps**:
1. Perform operations (import large guide, etc.)
2. Check logs: `%LocalAppData%\GuideViewer\logs\`
3. Search for performance entries

**Expected Logs**:
- [ ] Operation name
- [ ] Duration (ms)
- [ ] Memory used
- [ ] IsSlowOperation flag if threshold exceeded

**Verification**:
- [ ] Performance metrics logged
- [ ] Helps identify bottlenecks

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

### M4.6: UI Polish & Animations

#### Test 4.6.1: Page Entrance Animations

**Purpose**: Verify all pages have entrance animation

**Test Each Page**:
- [ ] Home
- [ ] Guides
- [ ] Guide Detail
- [ ] Guide Editor
- [ ] Progress
- [ ] Settings
- [ ] About

**Expected Animation** (for each):
- [ ] Fade in (opacity 0 → 1)
- [ ] Slide up (translateY 20 → 0)
- [ ] Duration: ~300ms
- [ ] Smooth cubic ease-out

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 4.6.2: Card Hover Effects

**Purpose**: Verify guide cards have hover animation

**Steps**:
1. Guides page
2. Hover over guide card
3. Hold 1 second
4. Move away

**Expected Results**:
- [ ] Card scales up slightly (~1.02x)
- [ ] Subtle elevation increase
- [ ] Smooth transition (~200ms)
- [ ] Returns to normal on mouse leave

**Verification**:
- [ ] All guide cards have effect
- [ ] No jank

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 4.6.3: Button Press Animation

**Purpose**: Verify buttons have press feedback

**Test Buttons**:
- [ ] Primary (Save Guide, Activate)
- [ ] Secondary (Cancel)
- [ ] Icon buttons

**Expected Animation**:
- [ ] Scale down on press (~0.95x)
- [ ] Return on release
- [ ] Smooth spring animation

**Verification**:
- [ ] All button types animated
- [ ] Provides tactile feedback

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 4.6.4: Loading Pulse Animation

**Purpose**: Verify loading indicators pulse

**Steps**:
1. Trigger loading (import guide, etc.)
2. Observe ProgressRing or skeleton

**Expected Animation**:
- [ ] Smooth pulsing
- [ ] Opacity oscillates (0.6 ↔ 1.0)
- [ ] Duration: ~1.5s per cycle
- [ ] Infinite while loading

**Verification**:
- [ ] Smooth 60fps
- [ ] Stops when complete

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 4.6.5: Card Container Styling

**Purpose**: Verify consistent card styling

**Expected Card Style**:
- [ ] Background: Theme-aware
- [ ] Border: 1px
- [ ] Corner radius: 8px
- [ ] Padding: 16px
- [ ] Shadow: Subtle elevation

**Test Pages**:
- [ ] About (App Info, System Info cards)
- [ ] Settings (Category cards)
- [ ] Guides (Guide cards)

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 4.6.6: Button Styles - Primary vs Secondary

**Purpose**: Verify button style hierarchy

**Primary Buttons** (accent color):
- [ ] Save Guide
- [ ] Activate
- [ ] Create Backup

**Secondary Buttons** (subtle):
- [ ] Cancel
- [ ] Close

**Verification**:
- [ ] Clear visual hierarchy
- [ ] Primary stands out
- [ ] Consistent sizing (120x44)

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 4.6.7: Icon Button Sizing

**Purpose**: Verify icon buttons meet touch target size

**Icon Buttons**:
- [ ] Export (on guide cards)
- [ ] Delete (on guide cards)
- [ ] Navigation icons

**Expected Size**:
- [ ] Min 44x44 pixels
- [ ] Padding: 8px
- [ ] Centered icon

**Verification**:
- [ ] Easy to tap on touchscreen
- [ ] Adequate size

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 4.6.8: Theme Switching - Light/Dark

**Purpose**: Verify UI adapts to theme

**Steps**:
1. Windows Settings → Personalization → Colors
2. Change to **Light** mode
3. Return to GuideViewer
4. Observe appearance
5. Change to **Dark** mode
6. Observe appearance

**Light Theme**:
- [ ] Light backgrounds
- [ ] Dark text
- [ ] Readable contrast

**Dark Theme**:
- [ ] Dark backgrounds
- [ ] Light text
- [ ] Cards visible

**Verification**:
- [ ] All text readable in both
- [ ] No contrast issues
- [ ] Smooth theme transition

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

#### Test 4.6.9: Focus Indicators (Keyboard Navigation)

**Purpose**: Verify focus indicators visible

**Steps**:
1. Any page with interactive elements
2. Press **Tab** repeatedly
3. Observe focus rectangle

**Expected Results**:
- [ ] Visible blue focus rectangle
- [ ] 2px stroke thickness
- [ ] High contrast
- [ ] Follows logical tab order

**Test Elements**:
- [ ] Buttons
- [ ] Text boxes
- [ ] Links
- [ ] Navigation items

**Verification**:
- [ ] Focus always visible
- [ ] Never invisible
- [ ] Clear indication

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

## Accessibility Testing

### Test A.1: Screen Reader Support (Narrator)

**Purpose**: Verify UI accessible with Narrator

**Prerequisites**: Enable Narrator (Win+Ctrl+Enter)

**Steps**:
1. Launch app with Narrator
2. Navigate with Tab key
3. Listen to announcements

**Expected Announcements**:
- [ ] Buttons: "Save Guide, button"
- [ ] Text boxes: "Title, text box"
- [ ] Lists: "Guide list, 10 items"
- [ ] Navigation: "Guides, navigation item"

**Test Pages**:
- [ ] Home
- [ ] Guides
- [ ] Guide Editor
- [ ] Settings
- [ ] About

**Verification**:
- [ ] All elements have names
- [ ] Logical reading order
- [ ] Live regions announce updates

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

### Test A.2: Full Keyboard Navigation

**Purpose**: Verify 100% keyboard accessibility

**Steps**:
1. **Disconnect mouse** or don't use it
2. Navigate using only keyboard:
   - Tab / Shift+Tab: Move between elements
   - Enter / Space: Activate buttons
   - Arrow keys: Navigate lists
   - Shortcuts: Navigate pages

**Test Workflows** (keyboard only):
- [ ] Activate with product key
- [ ] Navigate to Guides
- [ ] Open a guide
- [ ] Create a new guide (admin)
- [ ] Search guides
- [ ] Access Settings
- [ ] View About page

**Expected Results**:
- [ ] Can reach all interactive elements
- [ ] Can complete all tasks
- [ ] No keyboard traps
- [ ] Focus always visible

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

### Test A.3: Touch Target Sizes (44x44 Minimum)

**Purpose**: Verify all interactive elements meet WCAG AA size requirement

**Test Elements**:
- [ ] Icon buttons: ≥ 44x44
- [ ] Primary buttons: ≥ 44 height
- [ ] Secondary buttons: ≥ 44 height
- [ ] Navigation items: ≥ 44 height
- [ ] List items: ≥ 44 height
- [ ] Checkboxes: ≥ 44x44 (including padding)

**Verification**:
- [ ] No elements < 44x44
- [ ] Adequate spacing (≥ 8px)
- [ ] Easy to tap on touchscreen

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

### Test A.4: Color Contrast (WCAG AA)

**Purpose**: Verify text contrast ≥ 4.5:1

**Use Tool**: Browser DevTools or contrast analyzer

**Test Combinations**:
- [ ] Body text on cards (light theme)
- [ ] Body text on cards (dark theme)
- [ ] White text on accent blue (primary buttons)
- [ ] Link text
- [ ] Navigation text
- [ ] Gray metadata text

**Expected Results**:
- [ ] All normal text: ≥ 4.5:1
- [ ] Large text: ≥ 3:1

**Verification**:
- [ ] No contrast failures
- [ ] All text readable
- [ ] Meets WCAG AA

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

### Test A.5: High Contrast Mode

**Purpose**: Verify UI works in Windows High Contrast

**Steps**:
1. Windows Settings → Accessibility → Contrast themes
2. Select "High Contrast Black"
3. Apply theme
4. Return to GuideViewer

**Expected Results**:
- [ ] All text visible
- [ ] All interactive elements visible
- [ ] Focus indicators visible
- [ ] Icons visible
- [ ] Layout intact

**Verification**:
- [ ] No invisible elements
- [ ] Can complete tasks
- [ ] Acceptable appearance

**Cleanup**: Return to normal theme

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

## Integration Testing

### Test I.1: End-to-End Guide Lifecycle

**Purpose**: Verify complete guide workflow

**Steps**:
1. Log in as Admin
2. Create category: "Integration Test"
3. Create guide:
   - Title: "E2E Test Guide"
   - Category: "Integration Test"
   - 3 steps with images
   - Save
4. Export guide as ZIP
5. Delete guide
6. Import guide from ZIP
7. Start tracking progress
8. Complete all steps
9. View on Progress Dashboard

**Expected Results**:
- [ ] All operations succeed
- [ ] Data integrity maintained
- [ ] Progress tracked correctly

**Verification**:
- [ ] Imported guide matches original
- [ ] Progress shows 100% complete
- [ ] Statistics updated

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

### Test I.2: Multi-User Role Workflow

**Purpose**: Verify admin and technician workflows

**Steps**:
1. **As Admin**:
   - Create guide
   - Create category
   - Export guide
   - View progress dashboard
2. **As Technician** (log out, log in as tech):
   - View guides (should work)
   - Track progress (should work)
   - Try to create guide (should be disabled/hidden)
   - Try to delete guide (should be disabled/hidden)

**Expected Results**:
- [ ] Admin: Full access
- [ ] Technician: Limited access
- [ ] No permission errors (features hidden)

**Verification**:
- [ ] Role enforcement correct
- [ ] Technician can do job
- [ ] Admin has full control

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

### Test I.3: Backup/Restore with Active Progress

**Purpose**: Verify backup/restore preserves progress

**Steps**:
1. Start 3 guides, partial progress on each
2. Create backup
3. Complete one guide
4. Create new guide
5. Restore from backup
6. Check state

**Expected Results**:
- [ ] Progress restored to backup state
- [ ] Completed guide no longer 100%
- [ ] New guide deleted
- [ ] Back to 3 in-progress guides

**Verification**:
- [ ] Backup captures progress
- [ ] Restore accurate
- [ ] No data corruption

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

### Test I.4: Search + Filter + Navigation

**Purpose**: Verify combined search, filter, and navigation

**Steps**:
1. Guides page
2. Filter by category: "Installation"
3. Search: "water"
4. Click on result
5. View guide detail
6. Press Escape (go back)
7. Verify search and filter still active

**Expected Results**:
- [ ] Combined search and filter work
- [ ] Navigation preserves filters
- [ ] Back button maintains state

**Verification**:
- [ ] Results accurate
- [ ] State preserved
- [ ] Smooth workflow

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

### Test I.5: Keyboard Shortcuts Across Pages

**Purpose**: Verify shortcuts work globally

**Steps**:
1. Home → Press **Ctrl+F** → Guides
2. Guides → Press **F1** → About
3. About → Press **Escape** → Guides
4. Guides → Press **Escape** → Home
5. Home → Press **F2** → Shortcuts dialog
6. Close with **Escape**

**Expected Results**:
- [ ] All shortcuts work from all pages
- [ ] Navigation history maintained
- [ ] Consistent behavior

**Verification**:
- [ ] No shortcut conflicts
- [ ] Global functionality
- [ ] Smooth navigation

**Pass/Fail**: ________
**Notes**: ___________________________________________

---

## Regression Testing

**Purpose**: Verify previous features still work after Milestone 4 additions

### Test R.1: Authentication Still Works

- [ ] Admin activation works
- [ ] Technician activation works
- [ ] Invalid key rejected
- [ ] User persists across restarts

**Pass/Fail**: ________

---

### Test R.2: Category Management Still Works

- [ ] Can create categories
- [ ] Can edit categories
- [ ] Can delete unused categories
- [ ] Duplicate names prevented

**Pass/Fail**: ________

---

### Test R.3: Guide CRUD Still Works

- [ ] Can create guides
- [ ] Can edit guides
- [ ] Can delete guides
- [ ] Can view guide details

**Pass/Fail**: ________

---

### Test R.4: Search & Filter Still Work

- [ ] Search by title works
- [ ] Filter by category works
- [ ] Combined search+filter works
- [ ] Clear search works

**Pass/Fail**: ________

---

### Test R.5: Progress Tracking Still Works

- [ ] Can start guide
- [ ] Can mark steps complete
- [ ] Progress persists
- [ ] Can complete guide
- [ ] Dashboard shows correct data

**Pass/Fail**: ________

---

### Test R.6: Auto-Save Still Works

- [ ] Auto-save triggers in editor
- [ ] Respects configured interval
- [ ] Can enable/disable
- [ ] Settings persist

**Pass/Fail**: ________

---

## Troubleshooting

### Issue: Application Won't Start

**Symptoms**: Nothing happens when launching

**Solutions**:
1. Check Event Viewer for errors
2. Verify .NET 8 Runtime: `dotnet --list-runtimes`
3. Delete corrupt database: `%LocalAppData%\GuideViewer\data.db`
4. Run from Visual Studio for detailed errors
5. Check antivirus blocking

---

### Issue: Activation Page Reappears

**Symptoms**: Already activated but shows activation again

**Solutions**:
1. Database deleted or reset
2. Check `data.db` exists
3. Re-enter product key
4. Check logs for database errors

---

### Issue: Keyboard Shortcuts Not Working

**Symptoms**: Pressing shortcuts does nothing

**Solutions**:
1. Click app window to ensure focus
2. Check if TextBox has focus (shortcuts disabled in text input)
3. Verify service registered in App.xaml.cs
4. Check logs for errors
5. Restart application

---

### Issue: Animations Stuttering

**Symptoms**: Choppy animations

**Solutions**:
1. Enable GPU acceleration (Windows settings)
2. Update graphics drivers
3. Close other applications
4. Check memory usage (< 150 MB)
5. Disable animations if necessary

---

### Issue: Images Not Loading

**Symptoms**: Empty image placeholders

**Solutions**:
1. Check database size (not 0 bytes)
2. Verify images saved correctly (check logs)
3. Re-import guide if imported
4. Check for database corruption (restore backup)
5. Verify image format supported (PNG, JPG, BMP)

---

### Issue: Export/Import Fails

**Symptoms**: Operation shows error

**Solutions**:
1. Verify write permissions
2. Check file not locked
3. For import: Validate JSON structure
4. For import: Verify ZIP not corrupt
5. Check disk space
6. Review error dialog

---

### Issue: Performance Slow

**Symptoms**: App feels sluggish

**Solutions**:
1. Check memory usage (< 150 MB)
2. Check database size (optimize if > 500 MB)
3. Archive old guides
4. Check for background tasks
5. Restart application
6. Review performance logs

---

## Test Sign-Off

### Test Results Summary

| Milestone | Feature Area | Tests | Passed | Failed | Pass Rate |
|-----------|-------------|-------|--------|--------|-----------|
| **M1: Foundation** | | | | | |
| | Authentication & Licensing | 6 | ___ | ___ | ___% |
| | Navigation System | 5 | ___ | ___ | ___% |
| | Settings Management | 5 | ___ | ___ | ___% |
| | Logging | 4 | ___ | ___ | ___% |
| **M2: Guide Management** | | | | | |
| | Category Management | 7 | ___ | ___ | ___% |
| | Guide CRUD Operations | 7 | ___ | ___ | ___% |
| | Guide Editor | 11 | ___ | ___ | ___% |
| | Search & Filter | 6 | ___ | ___ | ___% |
| | Image Management | 5 | ___ | ___ | ___% |
| **M3: Progress Tracking** | | | | | |
| | User Progress Tracking | 6 | ___ | ___ | ___% |
| | Progress Dashboard | 5 | ___ | ___ | ___% |
| | Progress Reports | 5 | ___ | ___ | ___% |
| | Active Guide Progress | 5 | ___ | ___ | ___% |
| | Timer Service | 5 | ___ | ___ | ___% |
| **M4: Polish & Performance** | | | | | |
| | Data Management | 8 | ___ | ___ | ___% |
| | About Page | 6 | ___ | ___ | ___% |
| | Keyboard Shortcuts | 10 | ___ | ___ | ___% |
| | Error Handling | 6 | ___ | ___ | ___% |
| | Performance Monitoring | 5 | ___ | ___ | ___% |
| | UI Polish & Animations | 9 | ___ | ___ | ___% |
| **Accessibility Testing** | | 5 | ___ | ___ | ___% |
| **Integration Testing** | | 5 | ___ | ___ | ___% |
| **Regression Testing** | | 6 | ___ | ___ | ___% |
| **TOTAL** | | **142** | **___** | **___** | **___%** |

### Performance Metrics

| Metric | Target | Actual | Pass/Fail |
|--------|--------|--------|-----------|
| Startup Time | < 2 seconds | _______ s | ___ |
| Guide List Load | < 500 ms | _______ ms | ___ |
| Step Navigation | < 100 ms | _______ ms | ___ |
| Memory Usage | < 150 MB | _______ MB | ___ |

### Acceptance Criteria

**Application is ACCEPTED for production if**:
- [ ] All critical tests pass (100%)
- [ ] All performance targets met
- [ ] All accessibility requirements met (WCAG AA)
- [ ] No critical or high severity bugs
- [ ] All 4 milestones verified complete

### Defects Found

| ID | Severity | Description | Status |
|----|----------|-------------|--------|
| 1 | ___ | ____________________________________________ | ___ |
| 2 | ___ | ____________________________________________ | ___ |
| 3 | ___ | ____________________________________________ | ___ |

**Severity Levels**: Critical, High, Medium, Low

### Tester Sign-Off

**Tester Name**: _______________________________
**Date**: _______________________________
**Signature**: _______________________________

**Testing Status**: ☐ PASS ☐ FAIL ☐ PASS WITH MINOR ISSUES

**Recommendation**: ☐ APPROVE FOR PRODUCTION ☐ REQUIRES FIXES ☐ REJECT

**Comments**:
_______________________________________________
_______________________________________________
_______________________________________________
_______________________________________________

---

## Appendix: Quick Reference

### Product Keys
- **Admin**: `A04E-02C0-AD82-43C0`
- **Technician**: `TD5A-BB21-A638-C43A`

### File Locations
- **Database**: `%LocalAppData%\GuideViewer\data.db`
- **Logs**: `%LocalAppData%\GuideViewer\logs\`
- **Backups**: User-specified location (ZIP files)

### Keyboard Shortcuts
- **F1**: About page
- **F2**: Keyboard shortcuts help
- **Ctrl+N**: New guide (admin only)
- **Ctrl+F**: Browse guides
- **Ctrl+E**: Export guide
- **Ctrl+B**: Toggle bold (editor)
- **Ctrl+I**: Toggle italic (editor)
- **Escape**: Go back

### Performance Targets
- Startup: < 2 seconds
- Guide list load: < 500 ms
- Step navigation: < 100 ms
- Memory usage: < 150 MB

### Unit Test Results
```bash
dotnet test GuideViewer.Tests/GuideViewer.Tests.csproj
```
**Expected**: 258/260 tests passing (99.2% pass rate)

### Build Commands
```bash
# Build solution (requires Visual Studio 2022 for WinUI 3)
# Open GuideViewer.sln in Visual Studio
# Press F5 or Build → Build Solution

# Run tests
dotnet test GuideViewer.Tests/GuideViewer.Tests.csproj
```

### Reset Database (Fresh Start)
1. Close application
2. Navigate to: `%LocalAppData%\GuideViewer\`
3. Delete `data.db` file
4. Delete `logs` folder
5. Restart application

---

**End of Comprehensive Manual Test Guide**

**Total Test Cases**: 142
**Estimated Testing Time**: 8-12 hours for complete coverage
**Document Version**: 1.0
**Last Updated**: 2025-11-17

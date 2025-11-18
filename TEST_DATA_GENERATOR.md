# GuideViewer - Test Data Generation Scripts

**Version**: 1.0
**Date**: 2025-11-17
**Purpose**: Scripts and instructions for generating comprehensive test data

---

## Table of Contents

1. [Quick Start - Using Built-in Seeder](#quick-start---using-built-in-seeder)
2. [Manual Test Data Creation](#manual-test-data-creation)
3. [Advanced Test Scenarios](#advanced-test-scenarios)
4. [SQL Scripts for Direct Database Population](#sql-scripts-for-direct-database-population)
5. [Test Data Cleanup](#test-data-cleanup)

---

## Quick Start - Using Built-in Seeder

The application includes a built-in sample data seeder accessible from the Settings page.

### Step 1: Activate as Admin
```
Product Key: A04E-02C0-AD82-43C0
```

### Step 2: Navigate to Settings
1. Click "Settings" in navigation menu
2. Scroll to "Sample Data" section

### Step 3: Seed Data
1. Click "Seed Sample Data" button
2. Wait for success message

### What Gets Created:
- ✅ **5 Categories**: Installation, Maintenance, Troubleshooting, Safety, Advanced
- ✅ **10 Sample Guides**: Various guides across categories with 3-10 steps each
- ✅ **Some guides have images**: Placeholder images for testing

**Limitations**: Sample data is predefined. For custom test scenarios, use methods below.

---

## Manual Test Data Creation

### Scenario 1: Testing Role-Based Access

**Objective**: Verify admin vs technician permissions

**Steps**:
1. **As Admin** (`A04E-02C0-AD82-43C0`):
   ```
   - Create category: "Admin Test Category"
   - Create guide: "Admin Only Guide" in "Admin Test Category"
   - Create guide: "Shared Guide" in "Installation"
   ```

2. **Switch to Technician** (`TD5A-BB21-A638-C43A`):
   ```
   - Verify: Cannot see Guide Editor in navigation
   - Verify: Can see both guides in Guides list
   - Verify: No edit/delete buttons on guide cards
   - Start progress on "Shared Guide"
   ```

3. **Switch back to Admin**:
   ```
   - Verify: Can edit both guides
   - Verify: Can delete "Admin Only Guide"
   - Verify: Can see technician's progress on "Shared Guide"
   ```

---

### Scenario 2: Testing Search and Filter

**Objective**: Create guides with specific titles/categories for search testing

**Create the following guides**:

| Title | Category | Description |
|-------|----------|-------------|
| "Kitchen Sink Installation" | Installation | Contains keyword: kitchen |
| "Bathroom Faucet Repair" | Maintenance | Contains keyword: bathroom |
| "Water Heater Replacement" | Installation | Contains keyword: water |
| "Electrical Panel Upgrade" | Advanced | Contains keyword: electrical |
| "HVAC System Maintenance" | Maintenance | Contains keyword: HVAC |

**Test Cases**:
- Search "kitchen" → Should find only "Kitchen Sink Installation"
- Search "water" → Should find "Water Heater Replacement"
- Filter by "Installation" → Should show 2 guides
- Filter "Maintenance" + Search "bathroom" → Should show 1 guide

---

### Scenario 3: Testing Image Management

**Objective**: Test various image formats and sizes

**Create guides with**:

1. **Guide with PNG images**:
   - Title: "PNG Image Test"
   - Step 1: Add 2MB PNG image
   - Step 2: Add 5MB PNG image

2. **Guide with JPG images**:
   - Title: "JPG Image Test"
   - Step 1: Add 3MB JPG image
   - Step 2: Add 8MB JPG image

3. **Guide with mixed formats**:
   - Title: "Mixed Format Test"
   - Step 1: PNG image
   - Step 2: JPG image
   - Step 3: BMP image

4. **Guide for size validation**:
   - Try to add 12MB image → Should be rejected
   - Verify error message: "Image too large (max 10 MB)"

**Image Preparation**:
```
Use any image editing tool to create test images:
- Small: 1-2 MB (fast loading tests)
- Medium: 5-8 MB (normal use case)
- Large: 10-15 MB (validation testing)
```

---

### Scenario 4: Testing Progress Tracking

**Objective**: Create guides at various progress states

**Setup**:
1. **Create 5 guides** with different step counts:
   - "5-Step Guide" (5 steps)
   - "10-Step Guide" (10 steps)
   - "3-Step Guide" (3 steps)
   - "15-Step Guide" (15 steps)
   - "Single Step Guide" (1 step)

2. **Create progress states**:
   - **Not started**: Don't touch "15-Step Guide"
   - **Just started (10%)**: Start "10-Step Guide", complete 1 step
   - **In progress (50%)**: Start "5-Step Guide", complete 2-3 steps
   - **Almost done (90%)**: Start "3-Step Guide", complete 2/3 steps
   - **Completed (100%)**: Start "Single Step Guide", complete 1/1 step

3. **Verify Dashboard**:
   - In Progress: Should show 3 guides
   - Completed: Should show 1 guide
   - Statistics: 1 completed, 3 in-progress, 1 not started

---

### Scenario 5: Testing Auto-Save

**Objective**: Verify auto-save functionality

**Setup**:
1. Navigate to Settings
2. Set auto-save interval to **10 seconds**
3. Enable auto-save

**Test**:
1. Create new guide: "Auto-Save Test"
2. Enter title and description
3. Add 1 step
4. **Wait 10 seconds** without clicking Save
5. Navigate away from editor
6. Return to Guides page
7. **Verify**: "Auto-Save Test" appears in list

**Advanced Test**:
1. Disable auto-save in Settings
2. Create new guide: "No Auto-Save Test"
3. Enter title
4. **Wait 30 seconds** without saving
5. Navigate away
6. **Verify**: Guide NOT saved

---

### Scenario 6: Testing Export/Import

**Objective**: Create guides for export/import testing

**Create Guides**:

1. **Simple Guide (for JSON export)**:
   - Title: "Simple Export Test"
   - No images
   - 3 steps with plain text
   - **Export as**: `simple_export.json`

2. **Complex Guide (for ZIP export)**:
   - Title: "Complex Export Test"
   - 5 steps, each with different image
   - Mix of PNG and JPG images
   - Rich text formatting (bold, italic)
   - **Export as**: `complex_export.zip`

3. **Round-Trip Test**:
   - Create guide: "Round Trip Test"
   - Add 3 steps with images
   - Export as `roundtrip.zip`
   - Delete original guide
   - Import `roundtrip.zip`
   - **Verify**: All steps and images identical

---

### Scenario 7: Testing Database Backup/Restore

**Objective**: Create known database state for backup testing

**Setup Known State**:
1. Create exactly **3 categories**:
   - "Backup Cat 1"
   - "Backup Cat 2"
   - "Backup Cat 3"

2. Create exactly **5 guides**:
   - 2 in "Backup Cat 1"
   - 2 in "Backup Cat 2"
   - 1 in "Backup Cat 3"

3. Start progress on **2 guides**:
   - Guide A: 30% complete
   - Guide B: 60% complete

4. **Create Backup**: `known_state_backup.zip`

**Test Restore**:
1. Delete 1 guide
2. Complete Guide A (was 30%, now 100%)
3. Create new guide "Should Disappear"
4. Restore from `known_state_backup.zip`
5. **Verify**:
   - Deleted guide restored
   - Guide A back to 30%
   - "Should Disappear" is gone
   - Exactly 3 categories, 5 guides

---

### Scenario 8: Testing Performance with Large Dataset

**Objective**: Test app performance with many guides

**Create 100+ Guides** (use loop or script):

```
For i = 1 to 100:
    Title: "Performance Test Guide #{i}"
    Category: Rotate through all categories
    Description: "This is guide number {i} for performance testing"
    Steps: 5 steps with generic content
    Estimated Time: Random between 10-60 minutes
```

**Manual Creation Shortcut**:
1. Create 1 guide: "Performance Test Guide #1"
2. Export as `template.zip`
3. Import 100 times (edit title each time to make unique)

**Performance Tests**:
- Measure guide list load time (target < 500ms)
- Test search performance with large dataset
- Verify memory usage < 150 MB
- Test scroll performance in guide list

---

### Scenario 9: Testing Timer Accuracy

**Objective**: Verify timer tracks time correctly

**Setup**:
1. Create guide: "Timer Test Guide" (3 steps)
2. Start guide at exactly **2:00 PM** (note time)
3. Complete step 1 at **2:10 PM** (10 minutes)
4. Close app
5. Reopen app at **2:15 PM**
6. Resume guide
7. **Verify**: Timer shows ~10 minutes (not 15)
8. Complete remaining steps at **2:25 PM**
9. **Verify**: Total time ~15 minutes (active time only)

---

### Scenario 10: Testing Multi-Step Guides

**Objective**: Test guides with many steps

**Create**:
1. **20-Step Guide**:
   - Title: "Complex Installation - 20 Steps"
   - 20 steps with detailed instructions
   - Some steps with images
   - Total estimated time: 120 minutes

2. **Test Navigation**:
   - Verify: Can navigate through all 20 steps
   - Verify: Step numbers accurate (1-20)
   - Verify: Progress bar updates correctly (5%, 10%, ... 100%)

3. **Test Reordering**:
   - Move step 20 to position 1
   - Move step 1 to position 10
   - **Verify**: Order saved correctly

---

## Advanced Test Scenarios

### Scenario A: Stress Test - Rapid Operations

**Objective**: Test app stability under rapid user actions

**Procedure**:
1. Rapidly click through navigation items (20 times in 10 seconds)
2. Rapidly search and clear search (type, clear, type, clear - 10 times)
3. Rapidly mark steps complete and incomplete (toggle 20 times)
4. Rapidly change category filter (switch 10 times)
5. **Verify**: App remains stable, no crashes

---

### Scenario B: Edge Cases

**Test Edge Cases**:

1. **Empty Titles/Descriptions**:
   - Try to save guide with empty title → Validation error
   - Save guide with empty description → Should work (optional field)

2. **Very Long Text**:
   - Create guide with 500-character title
   - Create step with 10,000-character content
   - **Verify**: Text doesn't break UI

3. **Special Characters**:
   - Title: "Guide with Special Chars: !@#$%^&*()"
   - Content: "Test <html> tags & symbols: © ® ™"
   - **Verify**: Characters displayed correctly

4. **Unicode Characters**:
   - Title: "Guide with Émojis 🎉 and Ümlauts"
   - **Verify**: Renders correctly

---

### Scenario C: Concurrent Progress

**Objective**: Test multiple guides tracked simultaneously

**Setup**:
1. Start Guide A at 2:00 PM, mark 2 steps complete
2. Start Guide B at 2:05 PM, mark 1 step complete
3. Start Guide C at 2:10 PM, mark 3 steps complete
4. Return to Guide A at 2:15 PM, mark 1 more step
5. **Verify**: Each guide has independent progress and timer

---

## SQL Scripts for Direct Database Population

**Note**: LiteDB doesn't use SQL, but you can use C# code to populate directly.

### C# Script for Bulk Guide Creation

```csharp
using GuideViewer.Data;
using GuideViewer.Data.Entities;
using GuideViewer.Data.Repositories;
using LiteDB;

// Connect to database
var dbPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "GuideViewer",
    "data.db"
);

using var db = new LiteDatabase($"Filename={dbPath};Connection=shared");

var guideRepo = new GuideRepository(new DatabaseService(dbPath));
var categoryRepo = new CategoryRepository(new DatabaseService(dbPath));

// Get or create category
var category = categoryRepo.GetByName("Test Category");
if (category == null)
{
    category = new Category
    {
        Name = "Test Category",
        IconGlyph = "\uE81E",
        Color = "#FF6B6B"
    };
    categoryRepo.Insert(category);
}

// Create 50 test guides
for (int i = 1; i <= 50; i++)
{
    var guide = new Guide
    {
        Title = $"Test Guide #{i:D3}",
        Description = $"This is test guide number {i} for performance testing.",
        Category = category.Name,
        EstimatedMinutes = (i % 60) + 10, // 10-70 minutes
        CreatedAt = DateTime.UtcNow.AddDays(-i),
        UpdatedAt = DateTime.UtcNow.AddDays(-i),
        Steps = new List<Step>()
    };

    // Add 5 steps to each guide
    for (int j = 1; j <= 5; j++)
    {
        guide.Steps.Add(new Step
        {
            Id = ObjectId.NewObjectId(),
            Order = j,
            Title = $"Step {j}",
            Content = $"This is step {j} content for guide {i}. " +
                     $"Perform the necessary actions and mark complete when done."
        });
    }

    guideRepo.Insert(guide);
    Console.WriteLine($"Created: {guide.Title}");
}

Console.WriteLine($"Successfully created 50 test guides!");
```

**Usage**:
1. Save as `BulkGuideCreator.cs` in KeyGenerator project
2. Add references to GuideViewer.Data and GuideViewer.Core
3. Run from `dotnet run` or Visual Studio

---

### PowerShell Script for Test Image Creation

```powershell
# Creates test images of various sizes
# Requires ImageMagick installed: https://imagemagick.org/

$outputDir = "$env:USERPROFILE\Desktop\TestImages"
New-Item -ItemType Directory -Force -Path $outputDir

# Create test images
magick -size 800x600 xc:red "$outputDir\small_2mb.png"
magick -size 1920x1080 xc:blue "$outputDir\medium_5mb.jpg"
magick -size 2560x1440 xc:green "$outputDir\large_8mb.png"
magick -size 4096x3072 xc:yellow "$outputDir\xlarge_12mb.jpg"

Write-Host "Test images created in: $outputDir"
Write-Host "- small_2mb.png (~2 MB)"
Write-Host "- medium_5mb.jpg (~5 MB)"
Write-Host "- large_8mb.png (~8 MB)"
Write-Host "- xlarge_12mb.jpg (~12 MB - should be rejected)"
```

**Alternative** (without ImageMagick):
1. Download any image from the web
2. Use Windows Paint or Photos app
3. Resize to different dimensions:
   - 800x600 → ~2 MB
   - 1920x1080 → ~5 MB
   - 2560x1440 → ~8 MB
   - 4096x3072 → ~12 MB

---

## Test Data Cleanup

### Quick Reset - Delete Database

**Steps**:
1. Close GuideViewer application
2. Navigate to: `%LocalAppData%\GuideViewer\`
3. Delete `data.db` file
4. Delete `logs` folder (optional)
5. Restart application
6. **Result**: Fresh database, activation page appears

**Use Case**: Start over with clean slate

---

### Selective Cleanup - Delete Specific Data

**Delete All Guides but Keep Categories**:
1. In app, go to Guides page
2. Delete each guide individually
3. **Result**: Empty guide list, categories remain

**Delete All Progress**:
1. In app, open each guide with progress
2. Click "Reset Progress" button
3. **Result**: All guides back to 0% progress

**Delete Specific Category**:
1. In Settings → Category Management
2. First, delete or reassign all guides in that category
3. Then delete the category
4. **Result**: Category removed

---

### Backup Before Testing

**Best Practice**: Create backup before destructive tests

**Steps**:
1. Settings → Database Backup
2. Create backup: `pre_test_backup_{date}.zip`
3. Perform tests
4. If needed, restore from backup

---

## Test Data Validation

### Verify Sample Data Integrity

**After Seeding, Verify**:

```
Expected Counts:
- Categories: 5
- Guides: 10
- Steps: ~50-70 total (varies by guide)

Check in App:
1. Guides page: Should show 10 guides
2. Category filter dropdown: Should show 5 categories
3. Settings → Category Management: Should list 5 categories

Check in Database (via code or DB browser):
1. Categories collection: 5 records
2. Guides collection: 10 records
3. Each guide has 3-10 steps
```

---

### Verify Test Scenario Results

**Scenario Validation Checklist**:

After each scenario:
- [ ] No crashes or errors
- [ ] Data saved correctly
- [ ] UI updates reflect data changes
- [ ] Can navigate away and return - data persists
- [ ] Performance acceptable (no lag)

---

## Quick Test Data Sets

### Minimal Test Set (5 min setup)
```
- 1 Category: "Quick Test"
- 3 Guides: "Guide A", "Guide B", "Guide C"
- Each guide: 3 steps, no images
```
**Use Case**: Quick smoke testing

---

### Standard Test Set (15 min setup)
```
- 5 Categories: Use sample data seeder
- 10 Guides: Use sample data seeder
- Add 2 custom guides with images
- Start progress on 3 guides (0%, 50%, 100%)
```
**Use Case**: Regular feature testing

---

### Comprehensive Test Set (60 min setup)
```
- 5 Categories: Sample data + 2 custom
- 50 Guides: Sample data + bulk creation
- 10 guides with images (various formats/sizes)
- Progress on 10 guides (various completion %)
- 5 completed guides with recorded times
- Create 1 backup of known state
- Export 5 guides (mix of JSON and ZIP)
```
**Use Case**: Pre-release testing, performance testing

---

## Troubleshooting Test Data Issues

### Issue: Sample Data Already Seeded

**Error**: "Sample data already exists"

**Solution**:
- Delete database and reseed
- Or manually create additional test data

---

### Issue: Cannot Delete Category

**Error**: "Category has guides"

**Solution**:
1. Find all guides in that category
2. Either delete guides or reassign to different category
3. Then delete category

---

### Issue: Import Fails

**Error**: Import shows error

**Solution**:
1. Verify file is valid ZIP or JSON
2. Check file not corrupted
3. Ensure guide JSON structure is correct
4. Try re-exporting and importing again

---

## Reference: Test Data Templates

### Category Template
```json
{
  "name": "Template Category",
  "iconGlyph": "\uE81E",
  "color": "#FF6B6B"
}
```

### Guide Template (Minimal)
```json
{
  "title": "Template Guide",
  "category": "Installation",
  "description": "Template guide description",
  "estimatedMinutes": 30,
  "steps": [
    {
      "order": 1,
      "title": "Step 1",
      "content": "Step 1 content"
    },
    {
      "order": 2,
      "title": "Step 2",
      "content": "Step 2 content"
    }
  ]
}
```

---

**End of Test Data Generation Guide**

**Total Scenarios**: 10 manual + 3 advanced = 13 comprehensive test scenarios
**Estimated Setup Time**: 5 minutes (minimal) to 60 minutes (comprehensive)
**Last Updated**: 2025-11-17

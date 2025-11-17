# Milestone 3: Progress Tracking System

**Goal**: Enable technicians to track their progress through guides step-by-step, with admin visibility into all progress.

**Estimated Effort**: ~40% of Milestone 2 (simpler than full CRUD with editor)
**Target**: 60-70 additional tests (40-50 unit + 15-20 integration)

---

## Phase 1: Data Layer - Progress Entity & Repository (~25%)

### Tasks:
- [ ] Create `Progress` entity in `GuideViewer.Data/Entities/Progress.cs`
  - Properties: Id, GuideId, UserId, CurrentStepOrder, CompletedStepOrders (List<int>), StartedAt, LastAccessedAt, CompletedAt, Notes, EstimatedTimeRemaining
  - Support partial completion (track which steps are done)
  - Support pause/resume (LastAccessedAt tracking)

- [ ] Create `ProgressRepository` in `GuideViewer.Data/Repositories/ProgressRepository.cs`
  - Inherits from `Repository<Progress>`
  - `GetByUserAndGuide(userId, guideId)` - Get specific progress record
  - `GetActiveByUser(userId)` - Get all in-progress guides for user
  - `GetCompletedByUser(userId, limit)` - Get recently completed guides
  - `GetAllProgressForGuide(guideId)` - Admin view: all users' progress on a guide
  - `GetStatistics(userId)` - Get completion stats (total completed, in-progress, average time)
  - `UpdateStepCompletion(progressId, stepOrder, isCompleted)` - Mark step complete/incomplete
  - `UpdateCurrentStep(progressId, stepOrder)` - Move to next step
  - `MarkGuideComplete(progressId)` - Finish guide

- [ ] Update `DatabaseService` to initialize progress collection with indexes
  - Index on UserId for fast user lookup
  - Index on GuideId for admin queries
  - Index on CompletedAt for recent completions
  - Compound index on (UserId, GuideId) for unique constraint

### Deliverables:
- `Progress.cs` entity (~60 lines)
- `ProgressRepository.cs` (~250 lines with all methods)
- Updated `DatabaseService.cs` initialization
- **15-20 unit tests** for ProgressRepository

### Success Criteria:
- All repository methods tested
- Can create, update, and query progress
- Indexes working correctly
- Edge cases handled (null users, invalid steps)

---

## Phase 2: Services Layer - Progress Tracking Service (~20%)

### Tasks:
- [ ] Create `IProgressTrackingService` interface in `GuideViewer.Core/Services/IProgressTrackingService.cs`
  - `StartGuideAsync(userId, guideId)` - Initialize progress record
  - `ResumeGuideAsync(progressId)` - Continue from last step
  - `CompleteStepAsync(progressId, stepOrder, notes?)` - Mark step done
  - `UncompleteStepAsync(progressId, stepOrder)` - Mark step undone
  - `MarkGuideCompleteAsync(progressId)` - Finish entire guide
  - `GetActiveProgressAsync(userId)` - Get in-progress guides
  - `GetProgressForGuideAsync(userId, guideId)` - Get specific progress
  - `GetCompletionStatisticsAsync(userId)` - Get user stats
  - `CalculateEstimatedTimeRemaining(progress, guide)` - Time calculation

- [ ] Implement `ProgressTrackingService` in `GuideViewer.Core/Services/ProgressTrackingService.cs`
  - Business logic for starting/resuming guides
  - Validation (can't start already-started guide, can't complete non-existent step)
  - Time tracking and estimation logic
  - Integration with GuideRepository to fetch guide data

- [ ] Create `ProgressStatistics` model in `GuideViewer.Core/Models/ProgressStatistics.cs`
  - Properties: TotalCompleted, TotalInProgress, AverageCompletionTime, CompletionRate

### Deliverables:
- `IProgressTrackingService.cs` interface (~40 lines)
- `ProgressTrackingService.cs` implementation (~300 lines)
- `ProgressStatistics.cs` model (~30 lines)
- **15-20 unit tests** for ProgressTrackingService

### Success Criteria:
- All service methods tested with mocked repositories
- Business logic validation working
- Time calculations accurate
- Error handling for edge cases

---

## Phase 3: Progress Tracking UI - Active Guide View (~25%)

### Tasks:
- [ ] Create `ActiveGuideProgressPage.xaml` in `GuideViewer/Views/Pages/`
  - Full-screen step-by-step interface
  - Large step title and content (RichEditBox read-only)
  - Step images displayed prominently
  - Progress indicator (e.g., "Step 3 of 10")
  - Checkboxes for completed steps
  - "Previous" / "Next" navigation buttons
  - "Mark Complete" button for current step
  - Notes TextBox for current step
  - Timer display (elapsed time)
  - "Finish Guide" button (appears on last step)
  - "Exit" button with unsaved notes warning

- [ ] Create `ActiveGuideProgressViewModel.cs` in `GuideViewer/ViewModels/`
  - Properties: CurrentGuide, CurrentProgress, CurrentStep, AllSteps, CompletedSteps, ElapsedTime, Notes
  - Commands: NextStepCommand, PreviousStepCommand, MarkStepCompleteCommand, MarkStepIncompleteCommand, FinishGuideCommand, SaveNotesCommand
  - Timer logic with IDispatcherTimer
  - Auto-save notes every 30 seconds
  - Track elapsed time since StartedAt

- [ ] Update `PageKeys.cs` to add `ActiveGuideProgress`

- [ ] Update `NavigationService` registration in `MainWindow.xaml.cs`

- [ ] Update `GuidesPage.xaml` to add "Start" / "Resume" button per guide
  - "Start" if no progress exists
  - "Resume" if progress exists but not completed
  - "View" if completed (navigate to GuideDetailPage)
  - Pass progressId or guideId to ActiveGuideProgressPage

### Deliverables:
- `ActiveGuideProgressPage.xaml` (~300 lines)
- `ActiveGuideProgressPage.xaml.cs` (~200 lines with timer and image loading)
- `ActiveGuideProgressViewModel.cs` (~400 lines)
- Updated `GuidesPage.xaml` with Start/Resume buttons
- Updated `GuidesViewModel.cs` with new commands

### Success Criteria:
- Can start new guide progress
- Can resume existing progress
- Can navigate between steps
- Can mark steps complete/incomplete
- Timer tracks elapsed time accurately
- Notes auto-save works
- Images load correctly

---

## Phase 4: Progress Dashboard UI - ProgressPage Implementation (~15%)

### Tasks:
- [ ] Implement `ProgressPage.xaml` (currently placeholder)
  - **In Progress Section**: Cards for active guides with progress bars
    - Guide title, category badge
    - Progress indicator (e.g., "3 of 10 steps complete - 30%")
    - "Resume" button
    - Elapsed time display

  - **Recently Completed Section**: List of completed guides
    - Guide title, completed date
    - Completion time
    - "View Details" button (navigate to GuideDetailPage)

  - **Statistics Section**: User completion stats
    - Total guides completed
    - Total guides in progress
    - Average completion time
    - Completion rate

  - Search and filter by category

- [ ] Create `ProgressPageViewModel.cs` in `GuideViewer/ViewModels/`
  - Properties: ActiveProgressList, CompletedProgressList, Statistics, SearchQuery, SelectedCategory
  - Commands: LoadDataCommand, ResumeGuideCommand, ViewCompletedGuideCommand, SearchCommand, FilterByCategoryCommand
  - Integration with ProgressTrackingService

- [ ] Update `ProgressPage.xaml.cs` to wire up ViewModel

### Deliverables:
- Updated `ProgressPage.xaml` (~350 lines)
- Updated `ProgressPage.xaml.cs` (~150 lines)
- `ProgressPageViewModel.cs` (~300 lines)

### Success Criteria:
- Dashboard shows all active progress
- Can resume from dashboard
- Completed guides displayed correctly
- Statistics accurate
- Search and filter work

---

## Phase 5: Admin Progress Monitoring (~10%)

### Tasks:
- [ ] Add "Progress Reports" section to `SettingsPage.xaml` (admin-only)
  - View all users' progress (if multi-user support added)
  - View progress statistics per guide (how many users completed it)
  - Export progress data (future: CSV export)

- [ ] Create `ProgressReportViewModel.cs` in `GuideViewer/ViewModels/`
  - Properties: AllProgress, GuideStatistics, SelectedGuide
  - Commands: LoadAllProgressCommand, FilterByGuideCommand, ExportCommand (placeholder)
  - Role check: Admin only

- [ ] Update `SettingsPage.xaml` with admin-only section
  - Use BooleanToVisibilityConverter with IsAdmin binding

### Deliverables:
- Updated `SettingsPage.xaml` with progress reports section (~100 additional lines)
- `ProgressReportViewModel.cs` (~200 lines)
- Integration with SettingsPage.xaml.cs

### Success Criteria:
- Admin can view all progress records
- Can filter by guide
- Statistics displayed correctly
- Technicians cannot see this section

---

## Phase 6: Testing & Polish (~5%)

### Tasks:
- [ ] Create integration tests in `GuideViewer.Tests/Integration/ProgressTrackingIntegrationTests.cs`
  - Test 1: Complete progress workflow (start → complete steps → finish)
  - Test 2: Resume progress (start → pause → resume → finish)
  - Test 3: Multiple users tracking same guide
  - Test 4: Step completion/uncompletion
  - Test 5: Statistics calculation accuracy
  - Test 6: Edge cases (invalid steps, already completed guides)

- [ ] Create integration tests in `GuideViewer.Tests/Integration/ProgressUIIntegrationTests.cs` (optional, if time)
  - Test ViewModel logic with mocked services
  - Test navigation flows

- [ ] Performance testing
  - Test with 100+ progress records
  - Verify index usage for queries
  - Check timer performance (no memory leaks)

- [ ] Bug fixes and polish
  - Fix any issues found during testing
  - Improve error messages
  - Add loading states
  - Polish UI animations

- [ ] Documentation updates
  - Update `CLAUDE.md` with Phase 1-6 details
  - Update `todo.md` with completion status
  - Add progress tracking to README if needed

### Deliverables:
- `ProgressTrackingIntegrationTests.cs` (~400 lines, 15-20 tests)
- Bug fixes and performance improvements
- Updated documentation

### Success Criteria:
- **60-70 new tests passing** (total: ~175 tests)
- No memory leaks from timer
- All edge cases handled
- Documentation complete

---

## Summary: Milestone 3 Breakdown

| Phase | Description | Effort | Tests |
|-------|-------------|--------|-------|
| 1 | Data Layer - Progress Entity & Repository | 25% | 15-20 |
| 2 | Services Layer - Progress Tracking Service | 20% | 15-20 |
| 3 | Active Guide UI - Step-by-step tracking | 25% | 0-5 |
| 4 | Progress Dashboard - ProgressPage implementation | 15% | 0-5 |
| 5 | Admin Monitoring - Settings page integration | 10% | 0-5 |
| 6 | Testing & Polish - Integration tests | 5% | 15-20 |
| **TOTAL** | | **100%** | **60-70** |

---

## Key Technical Decisions

### Progress Tracking Model:
- **Single Progress record per User+Guide combination** (enforced with compound index)
- **CompletedStepOrders list** to track which steps are done (allows non-linear completion)
- **CurrentStepOrder** to track where user left off
- **LastAccessedAt** to support "Recently Accessed" sorting

### Timer Implementation:
- Use `DispatcherTimer` in ViewModel for elapsed time tracking
- Calculate elapsed time as: `DateTime.Now - progress.StartedAt`
- Update every second for display
- Don't rely on timer for accuracy (use timestamps in database)

### Navigation Flow:
```
GuidesPage → "Start" button → ActiveGuideProgressPage (new progress)
GuidesPage → "Resume" button → ActiveGuideProgressPage (existing progress)
ProgressPage → "Resume" button → ActiveGuideProgressPage
ActiveGuideProgressPage → "Finish Guide" → ProgressPage (with success message)
```

### Role-Based Features:
- **Technicians**: Can track their own progress, view their stats
- **Admins**: Can track their own progress + view all users' progress reports

---

## Dependencies:
- ✅ Milestone 1 (Foundation) - Complete
- ✅ Milestone 2 (Guide Management) - Complete
- No blocking dependencies

---

## Risks & Mitigations:

| Risk | Impact | Mitigation |
|------|--------|------------|
| Timer memory leaks | High | Properly dispose DispatcherTimer, test with long sessions |
| Performance with many progress records | Medium | Use proper indexes, implement pagination if needed |
| Concurrent updates (same user, multiple devices) | Low | Single-user app, not a concern initially |
| Large notes fields | Low | Set max length on TextBox (e.g., 5000 chars) |

---

## Success Metrics:
- ✅ 60-70 new tests passing (total: ~175 tests)
- ✅ Technicians can track progress through guides
- ✅ Admins can view progress reports
- ✅ No performance degradation with 100+ records
- ✅ All CRUD operations working correctly
- ✅ Timer accuracy within 1 second
- ✅ Documentation complete

---

## Estimated Timeline:
- **Phase 1**: 2-3 hours (data layer is straightforward)
- **Phase 2**: 2-3 hours (service layer with business logic)
- **Phase 3**: 4-5 hours (most complex UI work)
- **Phase 4**: 2-3 hours (dashboard layout)
- **Phase 5**: 1-2 hours (admin section)
- **Phase 6**: 2-3 hours (testing and polish)
- **Total**: ~13-19 hours of development time

---

## Next Steps:
1. Review this plan for any missing requirements
2. Get approval to proceed
3. Start with Phase 1 (Data Layer)

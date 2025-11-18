# GuideViewer - Quick Test Checklist

**Version**: 1.0 | **Date**: 2025-11-17 | **Tester**: _____________ | **Build**: _____________

---

## ⚡ Critical Path Tests (30 min)

### Authentication & Setup
- [x] 1. Admin activation with `A04E-02C0-AD82-43C0` works
- [x] 2. Seed sample data from Settings page
- [x] 3. Verify 10 guides and 5 categories created

### Guide Management (Admin)
- [ ] 4. Create new guide with 3 steps and images
		-adding images doesnt work
- [ ] 5. Edit existing guide, add step
- [ ] 6. Delete a guide (with confirmation)
- [ ] 7. Search for guide by title (real-time filtering)
- [ ] 8. Filter guides by category dropdown

### Progress Tracking
- [ ] 9. Start a guide, mark 3 steps complete
- [ ] 10. Progress bar shows correct percentage
- [ ] 11. Navigate away, return - progress persisted
- [ ] 12. Complete all steps, verify 100% on dashboard
- [ ] 13. View Progress Dashboard shows statistics

### Data Management
- [ ] 14. Export guide as ZIP package
- [ ] 15. Delete exported guide
- [ ] 16. Import guide from ZIP - all images intact
- [ ] 17. Create database backup
- [ ] 18. Restore from backup successfully

### Navigation & Shortcuts
- [ ] 19. F1 opens About page
- [ ] 20. Ctrl+F navigates to Guides
- [ ] 21. Escape goes back in history
- [ ] 22. All navigation menu items work

### Role-Based Access
- [ ] 23. Log out, activate as Technician `TD5A-BB21-A638-C43A`
- [ ] 24. Guide Editor NOT visible in navigation
- [ ] 25. Can view guides but no edit/delete buttons
- [ ] 26. Can track progress on guides

---

## 🎯 Feature Coverage Tests (60 min)

### Milestone 1: Foundation
- [ ] Invalid product key rejected with clear error
- [ ] Settings persist across app restart
- [ ] Auto-save works in guide editor (wait 30s)
- [ ] Log files created at `%LocalAppData%\GuideViewer\logs\`
- [ ] All navigation items work (Home, Guides, Progress, Settings, About)

### Milestone 2: Guide Management
- [ ] Create category with icon and color
- [ ] Duplicate category name prevented
- [ ] Cannot delete category with existing guides
- [ ] Step reordering works in editor
- [ ] Image upload validates size (reject > 10MB)
- [ ] Text formatting (Bold/Italic) works and persists
- [ ] Cancel editing discards unsaved changes

### Milestone 3: Progress Tracking
- [ ] Timer starts when guide starts
- [ ] Timer persists across app restart
- [ ] Multiple guides can have independent progress
- [ ] Progress dashboard empty state shown when no progress
- [ ] Can reset progress on completed guide
- [ ] Completed guides list shows completion date

### Milestone 4: Polish & Performance
- [ ] About page shows correct app version and build date
- [ ] System info detects Windows version correctly
- [ ] F2 shows keyboard shortcuts dialog
- [ ] Ctrl+N creates new guide (admin only)
- [ ] Page entrance animations play smoothly
- [ ] Card hover effects work on guide cards
- [ ] Import invalid JSON shows user-friendly error
- [ ] Database restore shows warning dialog

---

## ♿ Accessibility Quick Check (15 min)

- [ ] Tab navigation reaches all interactive elements
- [ ] Focus indicators always visible (blue rectangle)
- [ ] All buttons ≥ 44x44 pixels (touch targets)
- [ ] Narrator announces button names correctly
- [ ] No keyboard traps (can escape all dialogs)
- [ ] Light theme: all text readable
- [ ] Dark theme: all text readable

---

## ⚙️ Performance Benchmarks (10 min)

| Metric | Target | Actual | Pass |
|--------|--------|--------|------|
| Startup time | < 2s | _____ s | ☐ |
| Guide list load (50+ guides) | < 500ms | _____ ms | ☐ |
| Step navigation | < 100ms | Instant? | ☐ |
| Memory usage (after 10 min) | < 150MB | _____ MB | ☐ |

**Measure**: Use Task Manager for memory, stopwatch for timing

---

## 🐛 Error Handling Spot Checks (10 min)

- [ ] Empty title in guide editor shows validation error
- [ ] Image > 10MB shows size error with helpful message
- [ ] Corrupt database file doesn't crash app
- [ ] Import invalid file shows error, app remains stable

---

## 🔄 Integration Scenarios (20 min)

### End-to-End Guide Lifecycle
- [ ] 1. Create guide → 2. Export ZIP → 3. Delete → 4. Import → 5. Track progress → 6. Complete → 7. View on dashboard
- [ ] All data preserved throughout lifecycle

### Search + Filter + Navigation
- [ ] 1. Filter by category → 2. Search text → 3. Click result → 4. Press Escape → 5. Search/filter still active

### Backup/Restore with Progress
- [ ] 1. Start 3 guides (partial progress) → 2. Backup → 3. Complete one → 4. Restore → 5. Back to partial progress state

---

## 🔍 Regression Checks (10 min)

After Milestone 4 additions, verify:
- [ ] Authentication still works (M1)
- [ ] Category CRUD still works (M2)
- [ ] Guide CRUD still works (M2)
- [ ] Search and filter still work (M2)
- [ ] Progress tracking still works (M3)
- [ ] Auto-save still works (M2)

---

## 📊 Test Results Summary

**Total Tests**: 75 checkboxes
**Passed**: _____ / 75
**Failed**: _____ / 75
**Pass Rate**: _____%

### Severity Classification
**Critical Failures** (app crash, data loss): _____
**High** (feature broken): _____
**Medium** (feature degraded): _____
**Low** (cosmetic, minor): _____

---

## ✅ Go/No-Go Decision

**Pass Criteria**:
- [ ] All critical path tests pass (26/26)
- [ ] Performance targets met (4/4)
- [ ] No critical or high severity bugs
- [ ] Accessibility basics pass (7/7)
- [ ] Pass rate ≥ 95% (71/75 tests)

**Decision**: ☐ GO TO PRODUCTION ☐ REQUIRES FIXES ☐ REJECT

**Blocker Issues** (must fix before release):
1. _________________________________________________
2. _________________________________________________
3. _________________________________________________

**Notes**:
___________________________________________________________
___________________________________________________________
___________________________________________________________

---

## 🔧 Quick Reference

**Admin Key**: `A04E-02C0-AD82-43C0`
**Technician Key**: `TD5A-BB21-A638-C43A`
**Database**: `%LocalAppData%\GuideViewer\data.db`
**Logs**: `%LocalAppData%\GuideViewer\logs\`

**Reset Database**: Delete `data.db`, restart app

**Shortcuts**: F1 (About) | F2 (Shortcuts) | Ctrl+F (Guides) | Ctrl+N (New Guide) | Escape (Back)

---

**Testing Time**: ~2.5 hours for complete coverage
**Recommended**: Run critical path daily, full checklist before each release
**Last Updated**: 2025-11-17

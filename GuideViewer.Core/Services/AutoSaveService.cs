using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GuideViewer.Core.Services;

/// <summary>
/// Service for auto-saving editor content at regular intervals.
/// </summary>
public class AutoSaveService : IAutoSaveService, IDisposable
{
    private Timer? _autoSaveTimer;
    private Func<Task>? _saveCallback;
    private bool _isDirty;
    private bool _isActive;
    private bool _isSaving;
    private DateTime? _lastSavedAt;
    private int _intervalSeconds;
    private bool _disposed;

    /// <inheritdoc/>
    public bool IsActive => _isActive;

    /// <inheritdoc/>
    public bool IsDirty
    {
        get => _isDirty;
        set
        {
            if (_isDirty != value)
            {
                _isDirty = value;
                Log.Debug("AutoSave dirty state changed to: {IsDirty}", value);
            }
        }
    }

    /// <inheritdoc/>
    public DateTime? LastSavedAt => _lastSavedAt;

    /// <inheritdoc/>
    public int IntervalSeconds => _intervalSeconds;

    /// <inheritdoc/>
    public void StartAutoSave(Func<Task> saveCallback, int intervalSeconds = 30)
    {
        if (saveCallback == null)
        {
            throw new ArgumentNullException(nameof(saveCallback));
        }

        if (intervalSeconds <= 0)
        {
            throw new ArgumentException("Interval must be positive.", nameof(intervalSeconds));
        }

        // Stop existing timer if any
        StopAutoSave();

        _saveCallback = saveCallback;
        _intervalSeconds = intervalSeconds;
        _isActive = true;

        // Create timer that fires after the interval and then repeats
        var intervalMs = intervalSeconds * 1000;
        _autoSaveTimer = new Timer(
            OnAutoSaveTimerElapsed,
            null,
            intervalMs,
            intervalMs);

        Log.Information("Auto-save started with {IntervalSeconds}s interval", intervalSeconds);
    }

    /// <inheritdoc/>
    public void StopAutoSave()
    {
        if (_autoSaveTimer != null)
        {
            _autoSaveTimer.Dispose();
            _autoSaveTimer = null;
        }

        _isActive = false;
        _saveCallback = null;

        Log.Information("Auto-save stopped");
    }

    /// <inheritdoc/>
    public async Task<bool> ManualSaveAsync()
    {
        if (_saveCallback == null)
        {
            Log.Warning("Manual save attempted but no save callback is set");
            return false;
        }

        if (!_isDirty)
        {
            Log.Debug("Manual save skipped - no changes to save");
            return false;
        }

        await PerformSaveAsync();
        return true;
    }

    /// <inheritdoc/>
    public void ResetTimer()
    {
        if (_autoSaveTimer != null && _isActive)
        {
            var intervalMs = _intervalSeconds * 1000;
            _autoSaveTimer.Change(intervalMs, intervalMs);
            Log.Debug("Auto-save timer reset");
        }
    }

    private void OnAutoSaveTimerElapsed(object? state)
    {
        // Don't block the timer thread
        _ = Task.Run(async () =>
        {
            if (_isDirty && !_isSaving)
            {
                Log.Information("Auto-save triggered (dirty content detected)");
                await PerformSaveAsync();
            }
            else if (!_isDirty)
            {
                Log.Debug("Auto-save skipped - no changes since last save");
            }
            else if (_isSaving)
            {
                Log.Debug("Auto-save skipped - save already in progress");
            }
        });
    }

    private async Task PerformSaveAsync()
    {
        if (_saveCallback == null || _isSaving)
        {
            return;
        }

        try
        {
            _isSaving = true;

            await _saveCallback();

            _isDirty = false;
            _lastSavedAt = DateTime.UtcNow;

            Log.Information("Auto-save completed successfully at {SavedAt}", _lastSavedAt);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Auto-save failed");
            // Keep IsDirty = true so we retry on next interval
        }
        finally
        {
            _isSaving = false;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                StopAutoSave();
            }
            _disposed = true;
        }
    }
}

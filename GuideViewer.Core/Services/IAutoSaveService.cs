using System;
using System.Threading.Tasks;

namespace GuideViewer.Core.Services;

/// <summary>
/// Service for auto-saving editor content at regular intervals.
/// </summary>
public interface IAutoSaveService
{
    /// <summary>
    /// Gets whether auto-save is currently active.
    /// </summary>
    bool IsActive { get; }

    /// <summary>
    /// Gets whether there are unsaved changes.
    /// </summary>
    bool IsDirty { get; set; }

    /// <summary>
    /// Gets when the content was last saved.
    /// </summary>
    DateTime? LastSavedAt { get; }

    /// <summary>
    /// Gets the auto-save interval in seconds.
    /// </summary>
    int IntervalSeconds { get; }

    /// <summary>
    /// Starts auto-save with the specified callback and interval.
    /// </summary>
    /// <param name="saveCallback">The callback to invoke when saving.</param>
    /// <param name="intervalSeconds">The auto-save interval in seconds (default 30).</param>
    void StartAutoSave(Func<Task> saveCallback, int intervalSeconds = 30);

    /// <summary>
    /// Stops auto-save.
    /// </summary>
    void StopAutoSave();

    /// <summary>
    /// Manually triggers a save.
    /// </summary>
    /// <returns>True if save was triggered, false if no changes to save.</returns>
    Task<bool> ManualSaveAsync();

    /// <summary>
    /// Resets the auto-save timer (useful after manual save).
    /// </summary>
    void ResetTimer();
}

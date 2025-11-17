using System;

namespace GuideViewer.Core.Services;

/// <summary>
/// Service for tracking elapsed time with start/stop/reset capabilities.
/// Designed to prevent memory leaks and provide testable timer functionality.
/// </summary>
public interface ITimerService : IDisposable
{
    /// <summary>
    /// Gets the elapsed time since the timer was started.
    /// </summary>
    TimeSpan Elapsed { get; }

    /// <summary>
    /// Gets whether the timer is currently running.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Event fired every second when the timer is running.
    /// Provides the current elapsed time.
    /// </summary>
    event EventHandler<TimeSpan>? Tick;

    /// <summary>
    /// Starts the timer. If already running, does nothing.
    /// </summary>
    void Start();

    /// <summary>
    /// Stops the timer but preserves elapsed time.
    /// Can be resumed with Start().
    /// </summary>
    void Stop();

    /// <summary>
    /// Resets the timer to zero and stops it.
    /// </summary>
    void Reset();
}

using System;
using System.Diagnostics;
using System.Threading;

namespace GuideViewer.Core.Services;

/// <summary>
/// Timer service implementation for tracking elapsed time.
/// Thread-safe and designed to prevent memory leaks.
/// </summary>
public class TimerService : ITimerService
{
    private readonly Timer _timer;
    private readonly Stopwatch _stopwatch;
    private bool _disposed;

    /// <summary>
    /// Gets the elapsed time since the timer was started.
    /// </summary>
    public TimeSpan Elapsed => _stopwatch.Elapsed;

    /// <summary>
    /// Gets whether the timer is currently running.
    /// </summary>
    public bool IsRunning { get; private set; }

    /// <summary>
    /// Event fired every second when the timer is running.
    /// </summary>
    public event EventHandler<TimeSpan>? Tick;

    public TimerService()
    {
        _stopwatch = new Stopwatch();
        _timer = new Timer(OnTimerTick, null, Timeout.Infinite, Timeout.Infinite);
    }

    /// <summary>
    /// Starts the timer. If already running, does nothing.
    /// </summary>
    public void Start()
    {
        if (IsRunning) return;

        _stopwatch.Start();
        IsRunning = true;

        // Fire tick every 1000ms (1 second)
        _timer.Change(0, 1000);
    }

    /// <summary>
    /// Stops the timer but preserves elapsed time.
    /// </summary>
    public void Stop()
    {
        if (!IsRunning) return;

        _stopwatch.Stop();
        IsRunning = false;

        // Stop the timer from firing
        _timer.Change(Timeout.Infinite, Timeout.Infinite);
    }

    /// <summary>
    /// Resets the timer to zero and stops it.
    /// </summary>
    public void Reset()
    {
        Stop();
        _stopwatch.Reset();
    }

    /// <summary>
    /// Timer callback - fires the Tick event with current elapsed time.
    /// Uses named method to prevent memory leaks.
    /// </summary>
    private void OnTimerTick(object? state)
    {
        if (IsRunning)
        {
            Tick?.Invoke(this, Elapsed);
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
                Stop();
                _timer?.Dispose();
            }
            _disposed = true;
        }
    }
}

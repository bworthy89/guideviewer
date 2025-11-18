using GuideViewer.Core.Models;
using Serilog;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace GuideViewer.Core.Services;

/// <summary>
/// Service for monitoring application performance.
/// </summary>
public class PerformanceMonitoringService : IPerformanceMonitoringService
{
    private readonly ConcurrentBag<PerformanceMetric> _metrics = new();
    private readonly Dictionary<string, double> _performanceTargets = new();

    /// <summary>
    /// Event raised when a slow operation is detected.
    /// </summary>
    public event EventHandler<PerformanceMetric>? SlowOperationDetected;

    public PerformanceMonitoringService()
    {
        // Set default performance targets (in milliseconds)
        _performanceTargets["GuideListLoad"] = 500;
        _performanceTargets["GuideSearch"] = 200;
        _performanceTargets["StepNavigation"] = 100;
        _performanceTargets["DatabaseQuery"] = 100;
        _performanceTargets["ImageLoad"] = 200;
    }

    /// <summary>
    /// Starts measuring performance for an operation.
    /// </summary>
    public IDisposable MeasureOperation(string operationName)
    {
        return new PerformanceMeasurement(this, operationName);
    }

    /// <summary>
    /// Records a performance metric manually.
    /// </summary>
    public void RecordMetric(PerformanceMetric metric)
    {
        if (metric == null)
        {
            throw new ArgumentNullException(nameof(metric));
        }

        // Check if this is a slow operation
        if (_performanceTargets.TryGetValue(metric.OperationName, out var target))
        {
            metric.IsSlowOperation = metric.DurationMs > target;
        }

        _metrics.Add(metric);

        // Log slow operations
        if (metric.IsSlowOperation)
        {
            Log.Warning("Slow operation detected: {Operation} took {Duration}ms (target: {Target}ms)",
                metric.OperationName, metric.DurationMs, target);

            SlowOperationDetected?.Invoke(this, metric);
        }
        else
        {
            Log.Debug("Performance metric: {Operation} took {Duration}ms, used {Memory}MB",
                metric.OperationName, metric.DurationMs, metric.MemoryUsedMB);
        }
    }

    /// <summary>
    /// Gets all recorded metrics.
    /// </summary>
    public IReadOnlyList<PerformanceMetric> GetAllMetrics()
    {
        return _metrics.ToList();
    }

    /// <summary>
    /// Gets metrics for a specific operation.
    /// </summary>
    public IReadOnlyList<PerformanceMetric> GetMetricsByOperation(string operationName)
    {
        return _metrics
            .Where(m => m.OperationName.Equals(operationName, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Gets average duration for an operation.
    /// </summary>
    public double GetAverageDuration(string operationName)
    {
        var metrics = GetMetricsByOperation(operationName);
        return metrics.Count > 0 ? metrics.Average(m => m.DurationMs) : 0;
    }

    /// <summary>
    /// Gets slow operations (those exceeding performance targets).
    /// </summary>
    public IReadOnlyList<PerformanceMetric> GetSlowOperations()
    {
        return _metrics.Where(m => m.IsSlowOperation).ToList();
    }

    /// <summary>
    /// Clears all recorded metrics.
    /// </summary>
    public void ClearMetrics()
    {
        _metrics.Clear();
        Log.Information("Performance metrics cleared");
    }

    /// <summary>
    /// Gets current memory usage in bytes.
    /// </summary>
    public long GetCurrentMemoryUsage()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        return GC.GetTotalMemory(false);
    }

    /// <summary>
    /// Sets a performance target for an operation.
    /// </summary>
    public void SetPerformanceTarget(string operationName, double targetMs)
    {
        _performanceTargets[operationName] = targetMs;
    }

    /// <summary>
    /// Helper class for measuring operation performance.
    /// </summary>
    private class PerformanceMeasurement : IDisposable
    {
        private readonly PerformanceMonitoringService _service;
        private readonly string _operationName;
        private readonly Stopwatch _stopwatch;
        private readonly long _startMemory;

        public PerformanceMeasurement(PerformanceMonitoringService service, string operationName)
        {
            _service = service;
            _operationName = operationName;
            _startMemory = GC.GetTotalMemory(false);
            _stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            var endMemory = GC.GetTotalMemory(false);
            var memoryUsed = Math.Max(0, endMemory - _startMemory);

            var metric = new PerformanceMetric
            {
                OperationName = _operationName,
                Duration = _stopwatch.Elapsed,
                MemoryUsed = memoryUsed,
                Timestamp = DateTime.Now
            };

            _service.RecordMetric(metric);
        }
    }
}

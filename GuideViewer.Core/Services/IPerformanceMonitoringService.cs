using GuideViewer.Core.Models;

namespace GuideViewer.Core.Services;

/// <summary>
/// Service for monitoring application performance.
/// </summary>
public interface IPerformanceMonitoringService
{
    /// <summary>
    /// Starts measuring performance for an operation.
    /// </summary>
    /// <param name="operationName">The name of the operation to measure.</param>
    /// <returns>A disposable object that automatically ends the measurement when disposed.</returns>
    IDisposable MeasureOperation(string operationName);

    /// <summary>
    /// Records a performance metric manually.
    /// </summary>
    /// <param name="metric">The metric to record.</param>
    void RecordMetric(PerformanceMetric metric);

    /// <summary>
    /// Gets all recorded metrics.
    /// </summary>
    /// <returns>List of all performance metrics.</returns>
    IReadOnlyList<PerformanceMetric> GetAllMetrics();

    /// <summary>
    /// Gets metrics for a specific operation.
    /// </summary>
    /// <param name="operationName">The operation name to filter by.</param>
    /// <returns>List of metrics for the specified operation.</returns>
    IReadOnlyList<PerformanceMetric> GetMetricsByOperation(string operationName);

    /// <summary>
    /// Gets average duration for an operation.
    /// </summary>
    /// <param name="operationName">The operation name.</param>
    /// <returns>Average duration in milliseconds, or 0 if no metrics found.</returns>
    double GetAverageDuration(string operationName);

    /// <summary>
    /// Gets slow operations (those exceeding performance targets).
    /// </summary>
    /// <returns>List of slow operations.</returns>
    IReadOnlyList<PerformanceMetric> GetSlowOperations();

    /// <summary>
    /// Clears all recorded metrics.
    /// </summary>
    void ClearMetrics();

    /// <summary>
    /// Gets current memory usage in bytes.
    /// </summary>
    /// <returns>Current memory usage.</returns>
    long GetCurrentMemoryUsage();

    /// <summary>
    /// Event raised when a slow operation is detected.
    /// </summary>
    event EventHandler<PerformanceMetric>? SlowOperationDetected;
}

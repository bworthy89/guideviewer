namespace GuideViewer.Core.Models;

/// <summary>
/// Represents a performance measurement for an operation.
/// </summary>
public class PerformanceMetric
{
    /// <summary>
    /// Gets or sets the operation name.
    /// </summary>
    public string OperationName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the duration of the operation.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Gets or sets the memory used during the operation (in bytes).
    /// </summary>
    public long MemoryUsed { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the measurement was taken.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.Now;

    /// <summary>
    /// Gets or sets additional metadata about the operation.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Gets whether this operation exceeded performance targets.
    /// </summary>
    public bool IsSlowOperation { get; set; }

    /// <summary>
    /// Gets the duration in milliseconds.
    /// </summary>
    public double DurationMs => Duration.TotalMilliseconds;

    /// <summary>
    /// Gets the memory used in megabytes.
    /// </summary>
    public double MemoryUsedMB => MemoryUsed / (1024.0 * 1024.0);
}

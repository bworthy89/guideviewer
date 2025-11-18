namespace GuideViewer.Core.Models;

/// <summary>
/// Represents detailed information about an error.
/// </summary>
public class ErrorInfo
{
    /// <summary>
    /// Gets or sets the error category.
    /// </summary>
    public ErrorCategory Category { get; set; }

    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user-friendly error message.
    /// </summary>
    public string UserMessage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the context in which the error occurred.
    /// </summary>
    public string Context { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when the error occurred.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.Now;

    /// <summary>
    /// Gets or sets whether this error is recoverable.
    /// </summary>
    public bool IsRecoverable { get; set; } = true;

    /// <summary>
    /// Gets or sets the exception that caused this error.
    /// </summary>
    public Exception? Exception { get; set; }

    /// <summary>
    /// Gets or sets suggested actions to resolve the error.
    /// </summary>
    public List<string> SuggestedActions { get; set; } = new();
}

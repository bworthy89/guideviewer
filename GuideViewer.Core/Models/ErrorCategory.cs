namespace GuideViewer.Core.Models;

/// <summary>
/// Categories of errors that can occur in the application.
/// </summary>
public enum ErrorCategory
{
    /// <summary>
    /// Unknown or uncategorized error.
    /// </summary>
    Unknown,

    /// <summary>
    /// Network-related errors (connectivity, timeouts, etc.).
    /// </summary>
    Network,

    /// <summary>
    /// File I/O errors (read/write failures, permissions, etc.).
    /// </summary>
    FileIO,

    /// <summary>
    /// Database errors (connection, query, corruption, etc.).
    /// </summary>
    Database,

    /// <summary>
    /// Validation errors (invalid input, business rule violations, etc.).
    /// </summary>
    Validation,

    /// <summary>
    /// Authentication/Authorization errors.
    /// </summary>
    Security,

    /// <summary>
    /// Configuration errors (missing settings, invalid config, etc.).
    /// </summary>
    Configuration,

    /// <summary>
    /// Resource errors (out of memory, disk space, etc.).
    /// </summary>
    Resource
}

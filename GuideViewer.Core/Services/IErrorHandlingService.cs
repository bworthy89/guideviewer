using GuideViewer.Core.Models;

namespace GuideViewer.Core.Services;

/// <summary>
/// Service for handling application errors in a consistent manner.
/// </summary>
public interface IErrorHandlingService
{
    /// <summary>
    /// Handles an exception and logs it appropriately.
    /// </summary>
    /// <param name="exception">The exception to handle.</param>
    /// <param name="context">The context in which the error occurred.</param>
    /// <returns>ErrorInfo with categorized error details.</returns>
    ErrorInfo HandleException(Exception exception, string context);

    /// <summary>
    /// Shows an error dialog to the user with the specified error information.
    /// </summary>
    /// <param name="errorInfo">The error information to display.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ShowErrorDialogAsync(ErrorInfo errorInfo);

    /// <summary>
    /// Shows a simple error dialog with title and message.
    /// </summary>
    /// <param name="title">The dialog title.</param>
    /// <param name="message">The error message.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ShowErrorDialogAsync(string title, string message);

    /// <summary>
    /// Gets error statistics for crash reporting.
    /// </summary>
    /// <returns>Dictionary of error categories and their counts.</returns>
    IReadOnlyDictionary<ErrorCategory, int> GetErrorStatistics();

    /// <summary>
    /// Clears all error statistics.
    /// </summary>
    void ClearStatistics();

    /// <summary>
    /// Event raised when an unhandled error occurs.
    /// </summary>
    event EventHandler<ErrorInfo>? UnhandledErrorOccurred;
}

using GuideViewer.Core.Models;
using Serilog;
using System.Collections.Concurrent;
using System.Security;

namespace GuideViewer.Core.Services;

/// <summary>
/// Service for handling application errors in a consistent manner.
/// </summary>
public class ErrorHandlingService : IErrorHandlingService
{
    private readonly ConcurrentDictionary<ErrorCategory, int> _errorCounts = new();
    private Func<ErrorInfo, Task>? _showDialogFunc;

    /// <summary>
    /// Event raised when an unhandled error occurs.
    /// </summary>
    public event EventHandler<ErrorInfo>? UnhandledErrorOccurred;

    /// <summary>
    /// Sets the function to use for showing error dialogs (must be called from UI layer).
    /// </summary>
    public void SetShowDialogFunction(Func<ErrorInfo, Task> showDialogFunc)
    {
        _showDialogFunc = showDialogFunc;
    }

    /// <summary>
    /// Handles an exception and logs it appropriately.
    /// </summary>
    public ErrorInfo HandleException(Exception exception, string context)
    {
        if (exception == null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        // Categorize the error
        var errorInfo = CategorizeException(exception, context);

        // Increment error count
        _errorCounts.AddOrUpdate(errorInfo.Category, 1, (key, count) => count + 1);

        // Log the error
        LogError(errorInfo);

        // Raise event
        UnhandledErrorOccurred?.Invoke(this, errorInfo);

        return errorInfo;
    }

    /// <summary>
    /// Shows an error dialog to the user with the specified error information.
    /// </summary>
    public async Task ShowErrorDialogAsync(ErrorInfo errorInfo)
    {
        if (_showDialogFunc != null)
        {
            await _showDialogFunc(errorInfo);
        }
        else
        {
            Log.Warning("ShowDialogFunction not set. Cannot display error dialog.");
        }
    }

    /// <summary>
    /// Shows a simple error dialog with title and message.
    /// </summary>
    public async Task ShowErrorDialogAsync(string title, string message)
    {
        var errorInfo = new ErrorInfo
        {
            Category = ErrorCategory.Unknown,
            Message = message,
            UserMessage = message,
            Context = title,
            IsRecoverable = true
        };

        await ShowErrorDialogAsync(errorInfo);
    }

    /// <summary>
    /// Gets error statistics for crash reporting.
    /// </summary>
    public IReadOnlyDictionary<ErrorCategory, int> GetErrorStatistics()
    {
        return new Dictionary<ErrorCategory, int>(_errorCounts);
    }

    /// <summary>
    /// Clears all error statistics.
    /// </summary>
    public void ClearStatistics()
    {
        _errorCounts.Clear();
        Log.Information("Error statistics cleared");
    }

    /// <summary>
    /// Categorizes an exception and creates an ErrorInfo object.
    /// </summary>
    private ErrorInfo CategorizeException(Exception exception, string context)
    {
        var errorInfo = new ErrorInfo
        {
            Exception = exception,
            Context = context,
            Message = exception.Message,
            Timestamp = DateTime.Now
        };

        // Categorize based on exception type
        // Note: More specific exceptions must come before base exceptions
        switch (exception)
        {
            case FileNotFoundException:
            case DirectoryNotFoundException:
            case UnauthorizedAccessException:
            case IOException:
                errorInfo.Category = ErrorCategory.FileIO;
                errorInfo.UserMessage = "A file operation failed. Please check file permissions and try again.";
                errorInfo.SuggestedActions.Add("Check that the file or folder exists");
                errorInfo.SuggestedActions.Add("Verify you have permission to access the file");
                errorInfo.SuggestedActions.Add("Ensure the file is not locked by another application");
                break;

            case InvalidOperationException when exception.Message.Contains("database", StringComparison.OrdinalIgnoreCase):
            case LiteDB.LiteException:
                errorInfo.Category = ErrorCategory.Database;
                errorInfo.UserMessage = "A database error occurred. Your data may not have been saved.";
                errorInfo.SuggestedActions.Add("Try the operation again");
                errorInfo.SuggestedActions.Add("Restart the application");
                errorInfo.SuggestedActions.Add("Contact support if the problem persists");
                errorInfo.IsRecoverable = true;
                break;

            case ArgumentNullException:
            case FormatException:
            case ArgumentException:
                errorInfo.Category = ErrorCategory.Validation;
                errorInfo.UserMessage = "Invalid input provided. Please check your entries and try again.";
                errorInfo.SuggestedActions.Add("Check that all required fields are filled");
                errorInfo.SuggestedActions.Add("Verify that the input format is correct");
                errorInfo.IsRecoverable = true;
                break;

            case HttpRequestException:
            case System.Net.WebException:
            case TaskCanceledException when exception.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase):
                errorInfo.Category = ErrorCategory.Network;
                errorInfo.UserMessage = "A network error occurred. Please check your internet connection.";
                errorInfo.SuggestedActions.Add("Check your internet connection");
                errorInfo.SuggestedActions.Add("Try again in a few moments");
                errorInfo.IsRecoverable = true;
                break;

            case OutOfMemoryException:
                errorInfo.Category = ErrorCategory.Resource;
                errorInfo.UserMessage = "The application ran out of memory. Please close some applications and try again.";
                errorInfo.SuggestedActions.Add("Close unnecessary applications");
                errorInfo.SuggestedActions.Add("Restart the application");
                errorInfo.IsRecoverable = false;
                break;

            case SecurityException:
                errorInfo.Category = ErrorCategory.Security;
                errorInfo.UserMessage = "You don't have permission to perform this action.";
                errorInfo.SuggestedActions.Add("Contact your administrator for access");
                errorInfo.IsRecoverable = true;
                break;

            default:
                errorInfo.Category = ErrorCategory.Unknown;
                errorInfo.UserMessage = "An unexpected error occurred. Please try again.";
                errorInfo.SuggestedActions.Add("Try the operation again");
                errorInfo.SuggestedActions.Add("Restart the application if the problem persists");
                errorInfo.SuggestedActions.Add("Contact support for assistance");
                errorInfo.IsRecoverable = true;
                break;
        }

        return errorInfo;
    }

    /// <summary>
    /// Logs the error based on its severity.
    /// </summary>
    private void LogError(ErrorInfo errorInfo)
    {
        var logMessage = $"[{errorInfo.Category}] {errorInfo.Context}: {errorInfo.Message}";

        if (errorInfo.IsRecoverable)
        {
            if (errorInfo.Exception != null)
            {
                Log.Warning(errorInfo.Exception, logMessage);
            }
            else
            {
                Log.Warning(logMessage);
            }
        }
        else
        {
            if (errorInfo.Exception != null)
            {
                Log.Error(errorInfo.Exception, logMessage);
            }
            else
            {
                Log.Error(logMessage);
            }
        }
    }
}

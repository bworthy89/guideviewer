using Microsoft.UI.Xaml.Controls;
using System.Collections.Concurrent;

namespace GuideViewer.Services;

/// <summary>
/// Service for managing page navigation in the application.
/// </summary>
public class NavigationService
{
    private Frame? _frame;
    private readonly ConcurrentDictionary<string, Type> _pages = new();

    /// <summary>
    /// Gets the current navigation frame.
    /// </summary>
    public Frame? Frame
    {
        get => _frame;
        set
        {
            _frame = value;
            if (_frame != null)
            {
                _frame.Navigated += OnNavigated;
            }
        }
    }

    /// <summary>
    /// Registers a page type with a key for navigation.
    /// </summary>
    public void RegisterPage<T>(string key) where T : Page
    {
        _pages[key] = typeof(T);
    }

    /// <summary>
    /// Navigates to a page by key.
    /// </summary>
    public bool NavigateTo(string pageKey, object? parameter = null)
    {
        if (_frame == null)
        {
            throw new InvalidOperationException("NavigationService Frame is not set");
        }

        if (!_pages.TryGetValue(pageKey, out var pageType))
        {
            throw new ArgumentException($"Page not registered: {pageKey}");
        }

        return _frame.Navigate(pageType, parameter);
    }

    /// <summary>
    /// Navigates back if possible.
    /// </summary>
    public bool GoBack()
    {
        if (_frame?.CanGoBack == true)
        {
            _frame.GoBack();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Gets whether navigation can go back.
    /// </summary>
    public bool CanGoBack => _frame?.CanGoBack ?? false;

    /// <summary>
    /// Event raised when navigation occurs.
    /// </summary>
    public event EventHandler<string>? Navigated;

    private void OnNavigated(object sender, Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        // Find the page key for the navigated type
        var pageKey = _pages.FirstOrDefault(x => x.Value == e.SourcePageType).Key;
        if (pageKey != null)
        {
            Navigated?.Invoke(this, pageKey);
        }
    }
}

/// <summary>
/// Constants for page navigation keys.
/// </summary>
public static class PageKeys
{
    public const string Home = "Home";
    public const string Guides = "Guides";
    public const string GuideEditor = "GuideEditor";
    public const string GuideDetail = "GuideDetail";
    public const string Progress = "Progress";
    public const string ActiveGuide = "ActiveGuide";
    public const string Settings = "Settings";
    public const string About = "About";
}

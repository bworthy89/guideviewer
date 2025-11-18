using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Windows.System;

namespace GuideViewer.Views.Pages;

/// <summary>
/// About page displaying application information, system details, and credits.
/// </summary>
public sealed partial class AboutPage : Page
{
    public AboutPage()
    {
        this.InitializeComponent();
        LoadSystemInformation();
    }

    /// <summary>
    /// Loads system and application information when the page is loaded.
    /// </summary>
    private void LoadSystemInformation()
    {
        try
        {
            // Load app version from assembly
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            VersionTextBlock.Text = version != null
                ? $"Version {version.Major}.{version.Minor}.{version.Build}"
                : "Version 1.0.0";

            // Get build date from assembly
            var buildDate = GetBuildDate(assembly);
            BuildDateTextBlock.Text = buildDate.ToString("yyyy-MM-dd");

            // Update copyright year
            CopyrightTextBlock.Text = $"© {DateTime.Now.Year} GuideViewer. All rights reserved.";

            // Get OS information
            OSVersionTextBlock.Text = GetOSVersion();

            // Get .NET version
            DotNetVersionTextBlock.Text = RuntimeInformation.FrameworkDescription;

            // Get architecture
            ArchitectureTextBlock.Text = RuntimeInformation.ProcessArchitecture.ToString();
        }
        catch (Exception ex)
        {
            // Log error but don't crash the page
            Serilog.Log.Error(ex, "Error loading system information");
        }
    }

    /// <summary>
    /// Gets the build date from the assembly.
    /// </summary>
    private DateTime GetBuildDate(Assembly assembly)
    {
        try
        {
            // Try to get build date from file modification time
            var location = assembly.Location;
            if (!string.IsNullOrEmpty(location) && System.IO.File.Exists(location))
            {
                return System.IO.File.GetLastWriteTime(location);
            }
        }
        catch
        {
            // Ignore errors
        }

        // Fall back to current date
        return DateTime.Now;
    }

    /// <summary>
    /// Gets a friendly OS version string.
    /// </summary>
    private string GetOSVersion()
    {
        try
        {
            var os = Environment.OSVersion;
            var isWindows11 = os.Version.Build >= 22000;

            if (isWindows11)
            {
                return $"Windows 11 (Build {os.Version.Build})";
            }
            else
            {
                return $"Windows 10 (Build {os.Version.Build})";
            }
        }
        catch
        {
            return "Windows";
        }
    }

    /// <summary>
    /// Opens the documentation in the default browser.
    /// </summary>
    private async void DocumentationLink_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // In a real app, this would point to actual documentation
            // For now, we'll show a dialog
            var dialog = new ContentDialog
            {
                Title = "Documentation",
                Content = "Documentation is available in the following files:\n\n" +
                         "• spec.md - Product specification\n" +
                         "• CLAUDE.md - Development guide\n" +
                         "• PATTERNS.md - Code patterns and examples\n" +
                         "• CHANGELOG.md - Version history",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "Error opening documentation");
        }
    }

    /// <summary>
    /// Opens the license information in the default browser.
    /// </summary>
    private async void LicenseLink_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var dialog = new ContentDialog
            {
                Title = "License Information",
                Content = "This software is provided for authorized users only.\n\n" +
                         "All features and content are protected by applicable laws. " +
                         "Unauthorized copying, distribution, or modification is prohibited.\n\n" +
                         "For licensing inquiries, please contact your system administrator.",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "Error opening license information");
        }
    }

    /// <summary>
    /// Opens the GitHub repository in the default browser.
    /// </summary>
    private async void GitHubLink_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var dialog = new ContentDialog
            {
                Title = "Source Code",
                Content = "This is a private application.\n\n" +
                         "Source code access is restricted to authorized developers only.",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "Error opening GitHub link");
        }
    }

    /// <summary>
    /// Shows the keyboard shortcuts help dialog.
    /// </summary>
    private async void KeyboardShortcutsLink_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var shortcutService = App.GetService<GuideViewer.Services.IKeyboardShortcutService>();
            var shortcuts = shortcutService?.GetRegisteredShortcuts();

            if (shortcuts == null || shortcuts.Count == 0)
            {
                await ShowErrorDialog("No keyboard shortcuts registered.");
                return;
            }

            var shortcutText = string.Join("\n", shortcuts.Select(kvp => $"{kvp.Key}: {kvp.Value}"));

            var dialog = new ContentDialog
            {
                Title = "Keyboard Shortcuts",
                Content = new ScrollViewer
                {
                    Content = new TextBlock
                    {
                        Text = shortcutText,
                        TextWrapping = TextWrapping.Wrap,
                        FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Consolas")
                    },
                    MaxHeight = 400
                },
                CloseButtonText = "Close",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "Error showing keyboard shortcuts");
        }
    }

    /// <summary>
    /// Shows an error dialog with the specified message.
    /// </summary>
    private async System.Threading.Tasks.Task ShowErrorDialog(string message)
    {
        var dialog = new ContentDialog
        {
            Title = "Error",
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = this.XamlRoot
        };

        await dialog.ShowAsync();
    }
}

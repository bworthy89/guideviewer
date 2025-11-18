using Windows.System;

namespace GuideViewer.Services;

/// <summary>
/// Service for managing keyboard shortcuts throughout the application.
/// </summary>
public interface IKeyboardShortcutService
{
    /// <summary>
    /// Registers a keyboard shortcut.
    /// </summary>
    /// <param name="key">The virtual key code.</param>
    /// <param name="modifiers">The modifier keys (Ctrl, Shift, Alt).</param>
    /// <param name="action">The action to execute when the shortcut is triggered.</param>
    /// <param name="description">Human-readable description of the shortcut.</param>
    void RegisterShortcut(VirtualKey key, VirtualKeyModifiers modifiers, Action action, string description);

    /// <summary>
    /// Processes a key press and executes the associated shortcut if registered.
    /// </summary>
    /// <param name="key">The pressed key.</param>
    /// <param name="modifiers">The active modifier keys.</param>
    /// <returns>True if a shortcut was executed, false otherwise.</returns>
    bool ProcessKeyPress(VirtualKey key, VirtualKeyModifiers modifiers);

    /// <summary>
    /// Gets all registered shortcuts with their descriptions.
    /// </summary>
    /// <returns>Dictionary of shortcut keys to descriptions.</returns>
    IReadOnlyDictionary<string, string> GetRegisteredShortcuts();

    /// <summary>
    /// Event raised when a shortcut is invoked.
    /// </summary>
    event EventHandler<ShortcutInvokedEventArgs>? ShortcutInvoked;
}

/// <summary>
/// Event arguments for shortcut invocation.
/// </summary>
public class ShortcutInvokedEventArgs : EventArgs
{
    public VirtualKey Key { get; set; }
    public VirtualKeyModifiers Modifiers { get; set; }
    public string Description { get; set; } = string.Empty;
}

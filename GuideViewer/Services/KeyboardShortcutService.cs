using System.Collections.Concurrent;
using Windows.System;

namespace GuideViewer.Services;

/// <summary>
/// Implementation of keyboard shortcut management service.
/// </summary>
public class KeyboardShortcutService : IKeyboardShortcutService
{
    private readonly ConcurrentDictionary<ShortcutKey, ShortcutAction> _shortcuts = new();

    public event EventHandler<ShortcutInvokedEventArgs>? ShortcutInvoked;

    public void RegisterShortcut(VirtualKey key, VirtualKeyModifiers modifiers, Action action, string description)
    {
        var shortcutKey = new ShortcutKey(key, modifiers);
        var shortcutAction = new ShortcutAction(action, description);

        if (!_shortcuts.TryAdd(shortcutKey, shortcutAction))
        {
            // Shortcut already registered, update it
            _shortcuts[shortcutKey] = shortcutAction;
        }
    }

    public bool ProcessKeyPress(VirtualKey key, VirtualKeyModifiers modifiers)
    {
        var shortcutKey = new ShortcutKey(key, modifiers);

        if (_shortcuts.TryGetValue(shortcutKey, out var shortcutAction))
        {
            try
            {
                shortcutAction.Action.Invoke();

                // Raise event
                ShortcutInvoked?.Invoke(this, new ShortcutInvokedEventArgs
                {
                    Key = key,
                    Modifiers = modifiers,
                    Description = shortcutAction.Description
                });

                return true;
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error executing keyboard shortcut: {Description}", shortcutAction.Description);
                return false;
            }
        }

        return false;
    }

    public IReadOnlyDictionary<string, string> GetRegisteredShortcuts()
    {
        return _shortcuts.ToDictionary(
            kvp => FormatShortcutKey(kvp.Key),
            kvp => kvp.Value.Description
        );
    }

    private static string FormatShortcutKey(ShortcutKey key)
    {
        var parts = new List<string>();

        if ((key.Modifiers & VirtualKeyModifiers.Control) != 0)
            parts.Add("Ctrl");
        if ((key.Modifiers & VirtualKeyModifiers.Shift) != 0)
            parts.Add("Shift");
        if ((key.Modifiers & VirtualKeyModifiers.Menu) != 0)
            parts.Add("Alt");

        parts.Add(key.Key.ToString());

        return string.Join("+", parts);
    }

    private readonly struct ShortcutKey : IEquatable<ShortcutKey>
    {
        public VirtualKey Key { get; }
        public VirtualKeyModifiers Modifiers { get; }

        public ShortcutKey(VirtualKey key, VirtualKeyModifiers modifiers)
        {
            Key = key;
            Modifiers = modifiers;
        }

        public bool Equals(ShortcutKey other)
        {
            return Key == other.Key && Modifiers == other.Modifiers;
        }

        public override bool Equals(object? obj)
        {
            return obj is ShortcutKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Key, Modifiers);
        }
    }

    private readonly struct ShortcutAction
    {
        public Action Action { get; }
        public string Description { get; }

        public ShortcutAction(Action action, string description)
        {
            Action = action;
            Description = description;
        }
    }
}

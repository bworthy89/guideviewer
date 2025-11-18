using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;

namespace GuideViewer.Helpers;

/// <summary>
/// Helper class for ensuring proper accessibility properties on UI elements.
/// </summary>
public static class AccessibilityHelper
{
    /// <summary>
    /// Sets automation properties for a button.
    /// </summary>
    public static void SetButtonAccessibility(Button button, string name, string helpText = "")
    {
        if (button == null) return;

        AutomationProperties.SetName(button, name);
        if (!string.IsNullOrEmpty(helpText))
        {
            AutomationProperties.SetHelpText(button, helpText);
        }
        AutomationProperties.SetAutomationId(button, $"Button_{name.Replace(" ", "_")}");
    }

    /// <summary>
    /// Sets automation properties for a text box.
    /// </summary>
    public static void SetTextBoxAccessibility(TextBox textBox, string name, string helpText = "")
    {
        if (textBox == null) return;

        AutomationProperties.SetName(textBox, name);
        if (!string.IsNullOrEmpty(helpText))
        {
            AutomationProperties.SetHelpText(textBox, helpText);
        }
        AutomationProperties.SetAutomationId(textBox, $"TextBox_{name.Replace(" ", "_")}");
    }

    /// <summary>
    /// Sets automation properties for a list view.
    /// </summary>
    public static void SetListAccessibility(ListView listView, string name, string helpText = "")
    {
        if (listView == null) return;

        AutomationProperties.SetName(listView, name);
        if (!string.IsNullOrEmpty(helpText))
        {
            AutomationProperties.SetHelpText(listView, helpText);
        }
        AutomationProperties.SetAutomationId(listView, $"ListView_{name.Replace(" ", "_")}");
    }

    /// <summary>
    /// Sets live region properties for dynamic content.
    /// </summary>
    public static void SetLiveRegion(FrameworkElement element, AutomationLiveSetting setting = AutomationLiveSetting.Polite)
    {
        if (element == null) return;

        AutomationProperties.SetLiveSetting(element, setting);
    }

    /// <summary>
    /// Ensures minimum touch target size (44x44 pixels).
    /// </summary>
    public static void EnsureMinimumTouchTarget(FrameworkElement element)
    {
        if (element == null) return;

        const double minimumSize = 44;

        if (element.MinWidth < minimumSize)
        {
            element.MinWidth = minimumSize;
        }

        if (element.MinHeight < minimumSize)
        {
            element.MinHeight = minimumSize;
        }
    }

    /// <summary>
    /// Sets keyboard focus to an element and announces it to screen readers.
    /// </summary>
    public static bool SetFocusWithAnnouncement(Control control, string announcement = "")
    {
        if (control == null) return false;

        var result = control.Focus(FocusState.Programmatic);

        if (result && !string.IsNullOrEmpty(announcement))
        {
            AutomationProperties.SetLiveSetting(control, AutomationLiveSetting.Assertive);
            // The announcement will be made when the control receives focus
        }

        return result;
    }
}

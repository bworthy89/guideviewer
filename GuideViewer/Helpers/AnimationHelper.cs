using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace GuideViewer.Helpers;

/// <summary>
/// Helper class for applying animations to UI elements.
/// </summary>
public static class AnimationHelper
{
    /// <summary>
    /// Plays the page entrance animation on the specified element.
    /// </summary>
    public static void PlayPageEntranceAnimation(FrameworkElement element)
    {
        if (element == null) return;

        try
        {
            var storyboard = Application.Current.Resources["PageEntranceAnimation"] as Storyboard;
            if (storyboard != null)
            {
                Storyboard.SetTarget(storyboard, element);
                storyboard.Begin();
            }
        }
        catch
        {
            // Animation failures should not break the app
        }
    }

    /// <summary>
    /// Applies entrance animation when a page is loaded.
    /// </summary>
    public static void ApplyPageLoadAnimation(Page page)
    {
        if (page == null) return;

        page.Loaded += (sender, e) =>
        {
            if (sender is Page p)
            {
                PlayPageEntranceAnimation(p);
            }
        };
    }
}

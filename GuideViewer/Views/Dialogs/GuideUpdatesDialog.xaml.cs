using GuideViewer.Core.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Linq;

namespace GuideViewer.Views.Dialogs;

/// <summary>
/// Dialog for selecting which guide updates to import from OneDrive.
/// </summary>
public sealed partial class GuideUpdatesDialog : ContentDialog
{
    public List<GuideUpdateInfo> AvailableUpdates { get; }
    public List<GuideUpdateInfo> SelectedGuides { get; private set; }

    public GuideUpdatesDialog(List<GuideUpdateInfo> availableUpdates)
    {
        this.InitializeComponent();

        AvailableUpdates = availableUpdates;
        SelectedGuides = new List<GuideUpdateInfo>(availableUpdates); // All selected by default

        UpdatesRepeater.ItemsSource = AvailableUpdates;

        UpdateSummary();

        // Wire up primary button click
        this.PrimaryButtonClick += (s, e) =>
        {
            // Update SelectedGuides based on checkboxes
            UpdateSelectedGuides();
        };
    }

    private void SelectAllButton_Click(object sender, RoutedEventArgs e)
    {
        // Find all checkboxes and check them
        SetAllCheckboxes(true);
        UpdateSummary();
    }

    private void SelectNoneButton_Click(object sender, RoutedEventArgs e)
    {
        // Find all checkboxes and uncheck them
        SetAllCheckboxes(false);
        UpdateSummary();
    }

    private void UpdateCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        UpdateSummary();
    }

    private void SetAllCheckboxes(bool isChecked)
    {
        // Iterate through all items in the repeater
        for (int i = 0; i < UpdatesRepeater.ItemsSourceView.Count; i++)
        {
            var container = UpdatesRepeater.TryGetElement(i);
            if (container != null)
            {
                var checkbox = FindCheckBox(container);
                if (checkbox != null)
                {
                    checkbox.IsChecked = isChecked;
                }
            }
        }
    }

    private CheckBox? FindCheckBox(UIElement element)
    {
        // Simple recursive search for CheckBox
        if (element is CheckBox checkbox)
            return checkbox;

        if (element is Panel panel)
        {
            foreach (var child in panel.Children)
            {
                var result = FindCheckBox(child);
                if (result != null)
                    return result;
            }
        }

        if (element is Border border && border.Child != null)
        {
            return FindCheckBox(border.Child);
        }

        if (element is ContentControl content && content.Content is UIElement contentElement)
        {
            return FindCheckBox(contentElement);
        }

        return null;
    }

    private void UpdateSelectedGuides()
    {
        SelectedGuides.Clear();

        for (int i = 0; i < UpdatesRepeater.ItemsSourceView.Count; i++)
        {
            var container = UpdatesRepeater.TryGetElement(i);
            if (container != null)
            {
                var checkbox = FindCheckBox(container);
                if (checkbox?.IsChecked == true && checkbox.Tag is GuideUpdateInfo update)
                {
                    SelectedGuides.Add(update);
                }
            }
        }
    }

    private void UpdateSummary()
    {
        int selectedCount = 0;

        for (int i = 0; i < UpdatesRepeater.ItemsSourceView.Count; i++)
        {
            var container = UpdatesRepeater.TryGetElement(i);
            if (container != null)
            {
                var checkbox = FindCheckBox(container);
                if (checkbox?.IsChecked == true)
                {
                    selectedCount++;
                }
            }
        }

        var newCount = AvailableUpdates.Count(u => u.UpdateType == GuideUpdateType.New);
        var updateCount = AvailableUpdates.Count(u => u.UpdateType == GuideUpdateType.Updated);

        SummaryTextBlock.Text = $"{selectedCount} of {AvailableUpdates.Count} guides selected for import ({newCount} new, {updateCount} updated)";

        // Disable primary button if nothing selected
        this.IsPrimaryButtonEnabled = selectedCount > 0;
    }
}

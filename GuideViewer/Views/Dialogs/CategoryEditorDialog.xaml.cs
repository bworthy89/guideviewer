using GuideViewer.Data.Entities;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;

namespace GuideViewer.Views.Dialogs;

public sealed partial class CategoryEditorDialog : ContentDialog
{
    private Category _category;

    // Icon mapping
    private readonly Dictionary<int, string> _iconGlyphs = new()
    {
        { 0, "\uE8F1" }, // Document
        { 1, "\uE968" }, // Network
        { 2, "\uE9F3" }, // Server
        { 3, "\uECAA" }, // Software
        { 4, "\uE90F" }, // Tools
        { 5, "\uE7EE" }, // Settings
        { 6, "\uE946" }, // Phone
        { 7, "\uE74E" }  // Calculator
    };

    // Color mapping
    private readonly Dictionary<int, string> _colors = new()
    {
        { 0, "#0078D4" }, // Blue
        { 1, "#107C10" }, // Green
        { 2, "#8764B8" }, // Purple
        { 3, "#D13438" }, // Red
        { 4, "#FF8C00" }, // Orange
        { 5, "#00BCF2" }, // Cyan
        { 6, "#7A7574" }  // Gray
    };

    public Category Category => _category;

    public CategoryEditorDialog(Category category)
    {
        this.InitializeComponent();

        _category = category ?? throw new ArgumentNullException(nameof(category));

        // Set title based on whether it's a new or existing category
        Title = _category.Id == LiteDB.ObjectId.Empty ? "New Category" : "Edit Category";

        // Populate form with category data
        NameTextBox.Text = _category.Name;
        DescriptionTextBox.Text = _category.Description;

        // Set icon selection
        IconComboBox.SelectedIndex = GetIconIndex(_category.IconGlyph);

        // Set color selection
        ColorComboBox.SelectedIndex = GetColorIndex(_category.Color);

        // Update preview
        UpdatePreview();

        // Wire up events for live preview
        NameTextBox.TextChanged += (s, e) => UpdatePreview();
        DescriptionTextBox.TextChanged += (s, e) => UpdatePreview();
        IconComboBox.SelectionChanged += (s, e) => UpdatePreview();
        ColorComboBox.SelectionChanged += (s, e) => UpdatePreview();

        // Handle primary button click
        PrimaryButtonClick += CategoryEditorDialog_PrimaryButtonClick;
    }

    private void CategoryEditorDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        // Validate
        if (string.IsNullOrWhiteSpace(NameTextBox.Text))
        {
            ValidationInfoBar.Message = "Category name is required.";
            ValidationInfoBar.IsOpen = true;
            args.Cancel = true;
            return;
        }

        // Update category object
        _category.Name = NameTextBox.Text.Trim();
        _category.Description = DescriptionTextBox.Text.Trim();
        _category.IconGlyph = _iconGlyphs[IconComboBox.SelectedIndex];
        _category.Color = _colors[ColorComboBox.SelectedIndex];
        _category.UpdatedAt = DateTime.UtcNow;

        if (_category.Id == LiteDB.ObjectId.Empty)
        {
            _category.CreatedAt = DateTime.UtcNow;
        }
    }

    private void UpdatePreview()
    {
        // Update preview name
        PreviewName.Text = string.IsNullOrWhiteSpace(NameTextBox.Text)
            ? "Category Name"
            : NameTextBox.Text;

        // Update preview description
        PreviewDescription.Text = string.IsNullOrWhiteSpace(DescriptionTextBox.Text)
            ? "Category description"
            : DescriptionTextBox.Text;

        // Update preview icon
        if (IconComboBox.SelectedIndex >= 0)
        {
            PreviewIcon.Glyph = _iconGlyphs[IconComboBox.SelectedIndex];
        }

        // Update preview color
        if (ColorComboBox.SelectedIndex >= 0)
        {
            var colorHex = _colors[ColorComboBox.SelectedIndex];
            PreviewColorBorder.Background = new SolidColorBrush(ParseHexColor(colorHex));
        }
    }

    private int GetIconIndex(string iconGlyph)
    {
        foreach (var kvp in _iconGlyphs)
        {
            if (kvp.Value == iconGlyph)
                return kvp.Key;
        }
        return 0; // Default to document
    }

    private int GetColorIndex(string color)
    {
        foreach (var kvp in _colors)
        {
            if (kvp.Value.Equals(color, StringComparison.OrdinalIgnoreCase))
                return kvp.Key;
        }
        return 0; // Default to blue
    }

    private Windows.UI.Color ParseHexColor(string hex)
    {
        hex = hex.TrimStart('#');
        return Windows.UI.Color.FromArgb(
            255,
            Convert.ToByte(hex.Substring(0, 2), 16),
            Convert.ToByte(hex.Substring(2, 2), 16),
            Convert.ToByte(hex.Substring(4, 2), 16)
        );
    }
}

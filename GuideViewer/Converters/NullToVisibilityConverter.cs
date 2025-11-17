using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace GuideViewer.Converters;

/// <summary>
/// Converts null values to Visibility.
/// Null = Collapsed, Not Null = Visible
/// </summary>
public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value != null ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

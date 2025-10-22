using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace KeyKeepersClient.Converters;

/// <summary>
/// Converter that returns Visibility.Visible if the string is not null or empty, otherwise Collapsed.
/// </summary>
public class TextToVisibilityConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string text && !string.IsNullOrEmpty(text))
        {
            return Visibility.Visible;
        }

        return Visibility.Collapsed;
    }

    /// <inheritdoc/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

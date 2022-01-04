using System;
using System.Globalization;
using System.Windows.Data;

namespace Microsoft.FamilyShow.Framework;

public class PrimaryAvatarConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value != null && (bool)value) return "*";

        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // not implemented yet
        return new object();
    }
}
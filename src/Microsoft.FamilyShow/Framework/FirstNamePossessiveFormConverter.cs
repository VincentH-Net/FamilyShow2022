using System;
using System.Globalization;
using System.Windows.Data;

namespace Microsoft.FamilyShow.Framework;

public class FirstNamePossessiveFormConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value != null)
            // Simply add "'s". Can be extended to check for correct grammar.
            return value + "'s ";

        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // not implemented yet
        return new object();
    }
}
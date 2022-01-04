using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;

namespace Microsoft.FamilyShow.Framework;

public class ComposingConverter : IValueConverter
{
    private readonly List<IValueConverter> converters = new();

    public Collection<IValueConverter> Converters => new Collection<IValueConverter>(converters);

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        for (var i = 0; i < converters.Count; i++) value = converters[i].Convert(value, targetType, parameter, culture);

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        for (var i = converters.Count - 1; i >= 0; i--) value = converters[i].ConvertBack(value, targetType, parameter, culture);

        return value;
    }
}
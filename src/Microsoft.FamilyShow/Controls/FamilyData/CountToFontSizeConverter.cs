using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows.Data;

namespace Microsoft.FamilyShow;

[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class CountToFontSizeConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var count = (int)values[0];
        var tagCloud = (TagCloud)values[1];

        return tagCloud.TagMinimumSize + count + tagCloud.TagIncrementSize < tagCloud.TagMaximumSize ? tagCloud.TagMinimumSize + count + tagCloud.TagIncrementSize : tagCloud.TagMaximumSize;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        // not implemented yet
        return Array.Empty<object>();
    }
}
using System;
using System.Globalization;
using System.Windows.Data;

namespace Microsoft.FamilyShow;

/// <summary>
/// Converts a person's age group to a text label that can be used on the histogram. Text is 
/// retrieved from the resource file for the project.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
class HistogramColumnLabelConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {
            object columnValue = values[0];
            Histogram histogram = values[1] as Histogram;
            return histogram.GetCategoryLabel(columnValue);
        }
        catch { }
        return null;

    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        // not implemented yet
        return new object[0];
    }
}
using System;
using System.Globalization;
using System.Windows.Data;

namespace Microsoft.FamilyShow;

/// <summary>
/// Converts a category count to a value between 1 and 100.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
class HistogramValueToPercentageConverter : IMultiValueConverter
{
    #region IMultiValueConverter Members

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {
            double count = System.Convert.ToDouble(values[0], CultureInfo.CurrentCulture);
            Histogram histogram = values[1] as Histogram;

            // The count of all groups in the ListCollectionView is used to 'normalize' 
            // the values each category
            double total = System.Convert.ToDouble(histogram.Count);

            if (total <= 0)
            {
                return 0;
            }
            else
            {
                return System.Convert.ToDouble(count / total * 100);
            }
        }
        catch { }

        return 0;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        // not implemented yet
        return new object[0];
    }

    #endregion
}
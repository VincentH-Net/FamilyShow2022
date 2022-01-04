using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Microsoft.FamilyShow.Framework;

public class ImageConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return string.Empty;

        try
        {
            var bitmap = new BitmapImage(new Uri(value.ToString()));

            // Use BitmapCacheOption.OnLoad to prevent binding the source holding on to the photo file.
            bitmap.CacheOption = BitmapCacheOption.OnLoad;

            return bitmap;
        }
        catch
        {
            return string.Empty;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return new object();
    }
}
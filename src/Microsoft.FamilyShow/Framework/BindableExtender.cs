using System.Windows;
using System.Windows.Documents;

namespace Microsoft.FamilyShow.Framework;

/// <summary>
///     Allow Run Element to be Bindable.
/// </summary>
/// <remarks>
///     Code by Paul Stovell. See http://www.paulstovell.net/blog/index.php/attached-bindablerun/ for more details
/// </remarks>
public static class BindableExtender
{
    public static readonly DependencyProperty BindableTextProperty =
        DependencyProperty.RegisterAttached("BindableText",
            typeof(string),
            typeof(BindableExtender),
            new UIPropertyMetadata(null,
                BindableTextProperty_PropertyChanged));

    public static string GetBindableText(DependencyObject obj)
    {
        return (string)obj.GetValue(BindableTextProperty);
    }

    public static void SetBindableText(DependencyObject obj,
        string value)
    {
        obj.SetValue(BindableTextProperty, value);
    }

    private static void BindableTextProperty_PropertyChanged(
        DependencyObject dependencyObject,
        DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is Run run) run.Text = (string)e.NewValue;
    }
}
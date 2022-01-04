﻿using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Microsoft.FamilyShow.Framework;

// https://stackoverflow.com/questions/1356045/set-focus-on-textbox-in-wpf-from-view-model
public static class SetFocusExtension
{
    public static readonly DependencyProperty IsFocusedProperty =
        DependencyProperty.RegisterAttached(
            "IsFocused", typeof(bool), typeof(SetFocusExtension),
            new UIPropertyMetadata(false, OnIsFocusedPropertyChanged));

    public static bool GetIsFocused(DependencyObject obj)
    {
        return (bool) obj.GetValue(IsFocusedProperty);
    }

    public static void SetIsFocused(DependencyObject obj, bool value)
    {
        obj.SetValue(IsFocusedProperty, value);
    }

    private static void OnIsFocusedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var uie = (UIElement) d;
        if (!(bool) e.NewValue) return;
        
        uie.Focus();
        Keyboard.Focus(uie);
    }
}

/// <summary>
/// Behavior allowing to put focus on element from the view model in a MVVM implementation.
/// </summary>
public static class FocusBehavior
{
    #region Dependency Properties
    /// <summary>
    /// <c>IsFocused</c> dependency property.
    /// </summary>
    public static readonly DependencyProperty IsFocusedProperty =
        DependencyProperty.RegisterAttached("IsFocused", typeof(bool?),
            typeof(FocusBehavior), new FrameworkPropertyMetadata(IsFocusedChanged));
    /// <summary>
    /// Gets the <c>IsFocused</c> property value.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <returns>Value of the <c>IsFocused</c> property or <c>null</c> if not set.</returns>
    public static bool? GetIsFocused(DependencyObject element)
    {
        if (element == null)
        {
            throw new ArgumentNullException(nameof(element));
        }
        return (bool?)element.GetValue(IsFocusedProperty);
    }
    /// <summary>
    /// Sets the <c>IsFocused</c> property value.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <param name="value">The value.</param>
    public static void SetIsFocused(DependencyObject element, bool? value)
    {
        if (element == null)
        {
            throw new ArgumentNullException(nameof(element));
        }
        element.SetValue(IsFocusedProperty, value);
    }
    #endregion Dependency Properties

    #region Event Handlers
    /// <summary>
    /// Determines whether the value of the dependency property <c>IsFocused</c> has change.
    /// </summary>
    /// <param name="d">The dependency object.</param>
    /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
    private static void IsFocusedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Ensure it is a FrameworkElement instance.
        if (d is FrameworkElement fe && e.OldValue == null && e.NewValue != null && (bool)e.NewValue)
        {
            // Attach to the Loaded event to set the focus there. If we do it here it will
            // be overridden by the view rendering the framework element.
            fe.Loaded += FrameworkElementLoaded;
        }
    }
    /// <summary>
    /// Sets the focus when the framework element is loaded and ready to receive input.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    private static void FrameworkElementLoaded(object sender, RoutedEventArgs e)
    {
        // Ensure it is a FrameworkElement instance.
        if (sender is not FrameworkElement fe) return;

        // Remove the event handler registration.
        fe.Loaded -= FrameworkElementLoaded;
        // Set the focus to the given framework element.
        fe.Focus();
        // Determine if it is a text box like element.
        if (fe is TextBoxBase tb)
        {
            // Select all text to be ready for replacement.
            tb.SelectAll();
        }
    }
    #endregion Event Handlers
}
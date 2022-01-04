using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Microsoft.FamilyShow
{
  /// <summary>
  /// Interaction logic for Histogram.xaml
  /// </summary>
  public partial class Histogram : UserControl
  {
    private ListCollectionView view;
    private Dictionary<object, string> categoryLabels;

    /// <summary>
    /// Get the number of items in the current view.
    /// </summary>
    public int Count
    {
      get { return View.Count; }
    }

    public Dictionary<object, string> CategoryLabels
    {
      get { return categoryLabels; }
    }

    #region dependency properties

    public static readonly DependencyProperty ViewProperty =
        DependencyProperty.Register("View", typeof(ListCollectionView), typeof(Histogram),
        new FrameworkPropertyMetadata(null,
        FrameworkPropertyMetadataOptions.AffectsRender,
        new PropertyChangedCallback(ViewProperty_Changed)));

    public ListCollectionView View
    {
      get { return (ListCollectionView)GetValue(ViewProperty); }
      set { SetValue(ViewProperty, value); }
    }

    public static readonly DependencyProperty CategoryFillProperty =
        DependencyProperty.Register("CategoryFill", typeof(Brush), typeof(Histogram),
        new FrameworkPropertyMetadata(Brushes.Transparent,
        FrameworkPropertyMetadataOptions.AffectsRender));

    public Brush CategoryFill
    {
      get { return (Brush)GetValue(CategoryFillProperty); }
      set { SetValue(CategoryFillProperty, value); }
    }

    public static readonly DependencyProperty CategoryStrokeProperty =
        DependencyProperty.Register("CategoryStroke", typeof(Brush), typeof(Histogram),
        new FrameworkPropertyMetadata(null,
        FrameworkPropertyMetadataOptions.AffectsRender));

    public Brush CategoryStroke
    {
      get { return (Brush)GetValue(CategoryStrokeProperty); }
      set { SetValue(CategoryStrokeProperty, value); }
    }

    public static readonly DependencyProperty AxisBrushProperty =
        DependencyProperty.Register("AxisBrush", typeof(Brush), typeof(Histogram),
        new FrameworkPropertyMetadata(SystemColors.WindowTextBrush,
        FrameworkPropertyMetadataOptions.AffectsRender));

    public Brush AxisBrush
    {
      get { return (Brush)GetValue(AxisBrushProperty); }
      set { SetValue(AxisBrushProperty, value); }
    }

    public static readonly DependencyProperty SelectedBrushProperty =
        DependencyProperty.Register("SelectedBrush", typeof(Brush), typeof(Histogram),
        new FrameworkPropertyMetadata(SystemColors.HighlightBrush,
        FrameworkPropertyMetadataOptions.AffectsRender));

    public Brush SelectedBrush
    {
      get { return (Brush)GetValue(SelectedBrushProperty); }
      set { SetValue(SelectedBrushProperty, value); }
    }

    public static readonly DependencyProperty DisabledForegroundBrushProperty =
        DependencyProperty.Register("DisabledForegroundBrush", typeof(Brush), typeof(Histogram),
        new FrameworkPropertyMetadata(SystemColors.GrayTextBrush,
        FrameworkPropertyMetadataOptions.AffectsRender));

    public Brush DisabledForegroundBrush
    {
      get { return (Brush)GetValue(DisabledForegroundBrushProperty); }
      set { SetValue(DisabledForegroundBrushProperty, value); }
    }

    #endregion

    #region routed events

    public static readonly RoutedEvent CategorySelectionChangedEvent = EventManager.RegisterRoutedEvent(
        "CategorySelectionChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Histogram));

    public event RoutedEventHandler CategorySelectionChanged
    {
      add { AddHandler(CategorySelectionChangedEvent, value); }
      remove { RemoveHandler(CategorySelectionChangedEvent, value); }
    }

    #endregion

    public Histogram()
    {
      categoryLabels = new Dictionary<object, string>();
      InitializeComponent();
    }

    public string GetCategoryLabel(object columnValue)
    {
      if (CategoryLabels.ContainsKey(columnValue))
        return CategoryLabels[columnValue];
      return columnValue.ToString();
    }

    private static void ViewProperty_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
      Histogram histogram = (Histogram)sender;
      ListCollectionView view = (ListCollectionView)args.NewValue;
      histogram.HistogramListBox.ItemsSource = view.Groups;
      histogram.TotalCountLabel.Content = view.Count;
      histogram.view = view;
    }

    private void HistogramListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      CollectionViewGroup selected = (CollectionViewGroup)((ListBox)sender).SelectedItem;
      if (selected != null)
      {
        string categoryLabel = GetCategoryLabel(selected.Name);
        RaiseEvent(new RoutedEventArgs(CategorySelectionChangedEvent, categoryLabel));
      }
    }

    internal void Refresh()
    {
      view.Refresh();

      // Update the total count if items exist in the list view collection. Otherwise, if there are no items, hide the histogram.
      if (view.Count == 0)
      {
        LayoutRoot.Visibility = Visibility.Hidden;
      }
      else
      {
        LayoutRoot.Visibility = Visibility.Visible;
        TotalCountLabel.Content = view.Count;
      }
    }

    internal void ClearSelection()
    {
      HistogramListBox.UnselectAll();
    }
  }
}
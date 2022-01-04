/*
 * A base class that sorts the data in a ListView control.
*/

using System;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Threading;

namespace Microsoft.FamilyShow.Controls.FamilyData
{
    /// <summary>
  /// Filter sort list view
  /// </summary>
  public class FilterSortListView : SortListView
  {
    private delegate void FilterDelegate();
    private Filter filter = new Filter();

    /// <summary>
    /// Get the filter for this control.
    /// </summary>
    protected Filter Filter
    {
      get { return filter; }
    }

    /// <summary>
    /// Filter the data using the specified filter text.
    /// </summary>
    public void FilterList(string text)
    {
      // Setup the filter object.
      filter.Parse(text);

      // Start an async operation that filters the list.
      Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new FilterDelegate(FilterWorker));
    }

    /// <summary>
    /// Worker method that filters the list.
    /// </summary>
    private void FilterWorker()
    {
      // Get the data the ListView is bound to.
      ICollectionView view = CollectionViewSource.GetDefaultView(ItemsSource);

      // Clear the list if the filter is empty, otherwise filter the list.
      view.Filter = filter.IsEmpty ? null : new Predicate<object>(FilterCallback);
    }

    /// <summary>
    /// This is called for each item in the list. The derived classes 
    /// override this method.
    /// </summary>
    virtual protected bool FilterCallback(object item)
    {
      return false;
    }
  }
}
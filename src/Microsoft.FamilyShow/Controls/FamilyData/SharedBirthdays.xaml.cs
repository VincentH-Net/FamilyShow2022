using Microsoft.FamilyShowLib;
using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Microsoft.FamilyShow
{
    /// <summary>
    /// Interaction logic for SharedBirthdays.xaml
    /// </summary>

    public partial class SharedBirthdays : UserControl
  {
        private static ListCollectionView lcv;

        #region dependency properties

        public static readonly DependencyProperty PeopleCollectionProperty =
            DependencyProperty.Register("PeopleCollection", typeof(Object), typeof(SharedBirthdays),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(PeopleCollectionProperty_Changed)));

        /// <summary>
        /// The Collection that will be used to build the Tag Cloud
        /// </summary>
        public Object PeopleCollection
        {
            get { return (Object)GetValue(PeopleCollectionProperty); }
            set { SetValue(PeopleCollectionProperty, value); }
        }

        #endregion

        #region routed events

        public static readonly RoutedEvent SharedBirthdaysSelectionChangedEvent = EventManager.RegisterRoutedEvent(
            "SharedBirthdaysSelectionChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SharedBirthdays));

        public event RoutedEventHandler SharedBirthdaysSelectionChanged
        {
            add { AddHandler(SharedBirthdaysSelectionChangedEvent, value); }
            remove { RemoveHandler(SharedBirthdaysSelectionChangedEvent, value); }
        }

        #endregion

        public SharedBirthdays()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Used as a filter predicate to see if the person should be included 
        /// </summary>
        /// <param name="personObject">Person object</param>
        /// <returns>True if the person should be included in the filter, otherwise false</returns>
        public static bool FilterPerson(object personObject)
        {
            Person person = personObject as Person;
            return (person.BirthDate != null);
        }

        private static void PeopleCollectionProperty_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            SharedBirthdays sharedBirthdays = ((SharedBirthdays)sender);

            // ListCollectionView is used for sorting and grouping
            lcv = new ListCollectionView((IList)args.NewValue);

            // Include only those people with a birthdate
            lcv.Filter = new Predicate<object>(FilterPerson);

            // Sort by Month and Day only
            lcv.CustomSort = new MonthDayComparer();

            // Group the collection by the month and day of the person's birthdate
            lcv.GroupDescriptions.Add(new PropertyGroupDescription("BirthMonthAndDay"));

            sharedBirthdays.GroupedItemsControl.ItemsSource = lcv;
        }

        private void GroupedListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Person selected = ((ListBox)sender).SelectedItem as Person;

            if (selected != null)
                RaiseEvent(new RoutedEventArgs(SharedBirthdaysSelectionChangedEvent, selected.BirthDate));
        }

        internal static void Refresh()
        {
            lcv.Refresh();
        }

        internal void ClearSelection()
        {
            GroupedItemsControl.UnselectAll();
        }
    }
}
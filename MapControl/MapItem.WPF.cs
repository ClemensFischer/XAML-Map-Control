// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2016 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Windows;
using System.Windows.Controls;

namespace MapControl
{
    /// <summary>
    /// Container class for an item in a MapItemsControl.
    /// </summary>
    public class MapItem : ListBoxItem
    {
        static MapItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(MapItem), new FrameworkPropertyMetadata(typeof(MapItem)));
        }

        public static readonly DependencyProperty LocationProperty =
            MapPanel.LocationProperty.AddOwner(typeof(MapItem));

        public Location Location
        {
            get { return (Location)GetValue(LocationProperty); }
            set { SetValue(LocationProperty, value); }
        }
    }
}

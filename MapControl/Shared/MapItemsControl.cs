// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using System.Windows;
using System.Windows.Controls;
#endif

namespace MapControl
{
    /// <summary>
    /// Container class for an item in a MapItemsControl.
    /// </summary>
    public class MapItem : ListBoxItem
    {
        public MapItem()
        {
            DefaultStyleKey = typeof(MapItem);

            MapPanel.InitMapElement(this);
        }
    }

    /// <summary>
    /// Manages a collection of selectable items on a Map.
    /// </summary>
    public class MapItemsControl : ListBox
    {
        public MapItemsControl()
        {
            DefaultStyleKey = typeof(MapItemsControl);

            MapPanel.InitMapElement(this);
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new MapItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is MapItem;
        }
    }
}

// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2016 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Windows;
using System.Windows.Controls;

namespace MapControl
{
    /// <summary>
    /// Manages a collection of selectable items on a Map. Uses MapItem as item container class.
    /// </summary>
    public class MapItemsControl : ListBox
    {
        static MapItemsControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(MapItemsControl), new FrameworkPropertyMetadata(typeof(MapItemsControl)));
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

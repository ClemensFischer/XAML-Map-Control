// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2015 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.ComponentModel;
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

        public MapItemsControl()
        {
            Items.CurrentChanging += CurrentItemChanging;
            Items.CurrentChanged += CurrentItemChanged;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new MapItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is MapItem;
        }

        private void CurrentItemChanging(object sender, CurrentChangingEventArgs e)
        {
            var container = ItemContainerGenerator.ContainerFromItem(Items.CurrentItem) as UIElement;

            if (container != null)
            {
                var zIndex = Panel.GetZIndex(container);
                Panel.SetZIndex(container, zIndex & ~0x40000000);
            }
        }

        private void CurrentItemChanged(object sender, EventArgs e)
        {
            var container = ItemContainerGenerator.ContainerFromItem(Items.CurrentItem) as UIElement;

            if (container != null)
            {
                var zIndex = Panel.GetZIndex(container);
                Panel.SetZIndex(container, zIndex | 0x40000000);
            }
        }
    }
}

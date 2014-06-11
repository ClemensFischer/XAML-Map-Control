// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2014 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MapControl
{
    /// <summary>
    /// Manages a collection of selectable items on a Map. Uses MapItem as item container class.
    /// </summary>
    public class MapItemsControl : ListBox
    {
        public static readonly DependencyProperty SelectionGeometryProperty = DependencyProperty.Register(
            "SelectionGeometry", typeof(Geometry), typeof(MapItemsControl),
            new PropertyMetadata((o, e) => ((MapItemsControl)o).SelectionGeometryPropertyChanged((Geometry)e.NewValue)));

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

        /// <summary>
        /// Gets or sets a Geometry that selects all items inside its fill area, i.e.
        /// where Geometry.FillContains returns true for the item's viewport position.
        /// </summary>
        public Geometry SelectionGeometry
        {
            get { return (Geometry)GetValue(SelectionGeometryProperty); }
            set { SetValue(SelectionGeometryProperty, value); }
        }

        public object GetFirstItemInGeometry(Geometry geometry)
        {
            if (geometry == null || geometry.IsEmpty())
            {
                return null;
            }

            return Items.Cast<object>().FirstOrDefault(i => IsItemInGeometry(i, geometry));
        }

        public IList<object> GetItemsInGeometry(Geometry geometry)
        {
            if (geometry == null || geometry.IsEmpty())
            {
                return null;
            }

            return Items.Cast<object>().Where(i => IsItemInGeometry(i, geometry)).ToList();
        }

        private bool IsItemInGeometry(object item, Geometry geometry)
        {
            var container = ItemContainerGenerator.ContainerFromItem(item) as UIElement;
            Point? viewportPosition;

            return container != null &&
                (viewportPosition = MapPanel.GetViewportPosition(container)).HasValue &&
                geometry.FillContains(viewportPosition.Value);
        }

        private void SelectionGeometryPropertyChanged(Geometry geometry)
        {
            if (SelectionMode == SelectionMode.Single)
            {
                SelectedItem = GetFirstItemInGeometry(geometry);
            }
            else
            {
                SetSelectedItems(GetItemsInGeometry(geometry));
            }
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

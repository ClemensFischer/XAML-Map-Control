// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © Clemens Fischer 2012-2013
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
    public partial class MapItemsControl
    {
        public static readonly DependencyProperty SelectionGeometryProperty = DependencyProperty.Register(
            "SelectionGeometry", typeof(Geometry), typeof(MapItemsControl),
            new PropertyMetadata((o, e) => ((MapItemsControl)o).SelectionGeometryPropertyChanged((Geometry)e.NewValue)));

        public static readonly DependencyProperty IsCurrentProperty = DependencyProperty.RegisterAttached(
            "IsCurrent", typeof(bool), typeof(MapItemsControl), null);

        static MapItemsControl()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(
                typeof(MapItemsControl), new FrameworkPropertyMetadata(typeof(MapItemsControl)));
        }

        public MapItemsControl()
        {
            Items.CurrentChanging += OnCurrentItemChanging;
            Items.CurrentChanged += OnCurrentItemChanged;
        }

        /// <summary>
        /// Gets or sets a Geometry that selects all items that lie inside its fill area,
        /// i.e. where Geometry.FillContains returns true for the item's viewport position.
        /// </summary>
        public Geometry SelectionGeometry
        {
            get { return (Geometry)GetValue(SelectionGeometryProperty); }
            set { SetValue(SelectionGeometryProperty, value); }
        }

        public static bool GetIsCurrent(UIElement element)
        {
            return (bool)element.GetValue(IsCurrentProperty);
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
                return new List<object>();
            }

            return new List<object>(Items.Cast<object>().Where(i => IsItemInGeometry(i, geometry)));
        }

        private void OnCurrentItemChanging(object sender, CurrentChangingEventArgs e)
        {
            var container = ContainerFromItem(Items.CurrentItem);

            if (container != null)
            {
                container.SetValue(IsCurrentProperty, false);
            }
        }

        private void OnCurrentItemChanged(object sender, EventArgs e)
        {
            var container = ContainerFromItem(Items.CurrentItem);

            if (container != null)
            {
                container.SetValue(IsCurrentProperty, true);
            }
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

        private bool IsItemInGeometry(object item, Geometry geometry)
        {
            var container = ContainerFromItem(item);
            Point? viewportPosition;

            return container != null &&
                (viewportPosition = MapPanel.GetViewportPosition(container)).HasValue &&
                geometry.FillContains(viewportPosition.Value);
        }
    }
}
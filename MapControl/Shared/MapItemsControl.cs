// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2023 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if WINUI
using Windows.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
#elif UWP
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
#endif

namespace MapControl
{
    /// <summary>
    /// Container class for an item in a MapItemsControl.
    /// </summary>
    public partial class MapItem : ListBoxItem
    {
        /// <summary>
        /// Gets/sets MapPanel.AutoCollapse.
        /// </summary>
        public bool AutoCollapse
        {
            get => (bool)GetValue(AutoCollapseProperty);
            set => SetValue(AutoCollapseProperty, value);
        }

        /// <summary>
        /// Gets/sets MapPanel.Location.
        /// </summary>
        public Location Location
        {
            get => (Location)GetValue(LocationProperty);
            set => SetValue(LocationProperty, value);
        }

        /// <summary>
        /// Gets a Transform for scaling and rotating geometries
        /// in map coordinates (meters) to view coordinates (pixels).
        /// </summary>
        public Transform MapTransform => mapTransform ?? (mapTransform = new MatrixTransform());

        private MatrixTransform mapTransform;

        protected override Size ArrangeOverride(Size bounds)
        {
            // If MapTransform is used, update its Matrix property.
            //
            if (mapTransform != null)
            {
                var parentMap = (VisualTreeHelper.GetParent(this) as MapPanel)?.ParentMap;

                if (parentMap != null && Location != null)
                {
                    var scale = parentMap.GetScale(Location);
                    var matrix = new Matrix(scale.X, 0d, 0d, scale.Y, 0d, 0d);
                    matrix.Rotate(parentMap.ViewTransform.Rotation);
                    mapTransform.Matrix = matrix;
                }
            }

            return base.ArrangeOverride(bounds);
        }
    }

    /// <summary>
    /// Manages a collection of selectable items on a Map.
    /// </summary>
    public partial class MapItemsControl : ListBox
    {
        public static readonly DependencyProperty LocationMemberPathProperty = DependencyProperty.Register(
            nameof(LocationMemberPath), typeof(string), typeof(MapItemsControl), new PropertyMetadata(null));

        /// <summary>
        /// Path to a source property for binding the Location property of MapItem containers.
        /// </summary>
        public string LocationMemberPath
        {
            get => (string)GetValue(LocationMemberPathProperty);
            set => SetValue(LocationMemberPathProperty, value);
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new MapItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is MapItem;
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            if (LocationMemberPath != null && element is MapItem mapItem)
            {
                mapItem.SetBinding(MapItem.LocationProperty,
                    new Binding
                    {
                        Path = new PropertyPath(LocationMemberPath),
                        Source = item
                    });
            }
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            base.ClearContainerForItemOverride(element, item);

            if (LocationMemberPath != null && element is MapItem mapItem)
            {
                mapItem.ClearValue(MapItem.LocationProperty);
            }
        }

        public void SelectItems(Predicate<object> predicate)
        {
            if (SelectionMode == SelectionMode.Single)
            {
                throw new InvalidOperationException("SelectionMode must not be Single");
            }

            foreach (var item in Items)
            {
                var selected = predicate(item);

                if (selected != SelectedItems.Contains(item))
                {
                    if (selected)
                    {
                        SelectedItems.Add(item);
                    }
                    else
                    {
                        SelectedItems.Remove(item);
                    }
                }
            }
        }

        public void SelectItemsByLocation(Predicate<Location> predicate)
        {
            SelectItems(item =>
            {
                var loc = MapPanel.GetLocation(ContainerFromItem(item));
                return loc != null && predicate(loc);
            });
        }

        public void SelectItemsByPosition(Predicate<Point> predicate)
        {
            SelectItems(item =>
            {
                var pos = MapPanel.GetViewPosition(ContainerFromItem(item));
                return pos.HasValue && predicate(pos.Value);
            });
        }

        public void SelectItemsInRect(Rect rect)
        {
            SelectItemsByPosition(p => rect.Contains(p));
        }

        protected internal void OnItemClicked(FrameworkElement mapItem, bool controlKey, bool shiftKey)
        {
            var item = ItemFromContainer(mapItem);

            if (SelectionMode == SelectionMode.Single)
            {
                // Single -> set only SelectedItem.

                if (SelectedItem != item)
                {
                    SelectedItem = item;
                }
                else if (controlKey)
                {
                    SelectedItem = null;
                }
            }
            else if (SelectionMode == SelectionMode.Multiple || controlKey)
            {
                // Multiple or Extended with Ctrl -> toggle item in SelectedItems.

                if (SelectedItems.Contains(item))
                {
                    SelectedItems.Remove(item);
                }
                else
                {
                    SelectedItems.Add(item);
                }
            }
            else if (shiftKey && SelectedItem != null)
            {
                // Extended with Shift -> select items in view rectangle.

                var p1 = MapPanel.GetViewPosition(ContainerFromItem(SelectedItem));
                var p2 = MapPanel.GetViewPosition(mapItem);

                if (p1.HasValue && p2.HasValue)
                {
                    SelectItemsInRect(new Rect(p1.Value, p2.Value));
                }
            }
            else if (SelectedItem != item)
            {
                // Extended without Control or Shift -> set selected item.

                SelectedItem = item;
            }
        }
    }
}

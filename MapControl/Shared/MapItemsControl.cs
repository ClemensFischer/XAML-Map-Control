// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if WPF
using System.Windows;
using System.Windows.Controls;
#elif UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#elif WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

namespace MapControl
{
    /// <summary>
    /// Manages a collection of selectable items on a Map.
    /// </summary>
    public partial class MapItemsControl : ListBox
    {
        public static readonly DependencyProperty LocationMemberPathProperty =
            DependencyPropertyHelper.Register<MapItemsControl, string>(nameof(LocationMemberPath));

        /// <summary>
        /// Path to a source property for binding the Location property of MapItem containers.
        /// </summary>
        public string LocationMemberPath
        {
            get => (string)GetValue(LocationMemberPathProperty);
            set => SetValue(LocationMemberPathProperty, value);
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
                var location = MapPanel.GetLocation(ContainerFromItem(item));

                return location != null && predicate(location);
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

        protected internal void OnItemClicked(MapItem mapItem, bool controlKey, bool shiftKey)
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

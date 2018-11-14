// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if WINDOWS_UWP
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using System.Windows;
using System.Windows.Controls;
#endif

namespace MapControl
{
    /// <summary>
    /// Manages a collection of selectable items on a Map.
    /// </summary>
    public partial class MapItemsControl : ListBox
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

        public void SelectItems(Func<object, bool> predicate)
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

        public void SelectItemsByLocation(Func<Location, bool> predicate)
        {
            SelectItems(item =>
            {
                var loc = MapPanel.GetLocation(ContainerFromItem(item));
                return loc != null && predicate(loc);
            });
        }

        public void SelectItemsByPosition(Func<Point, bool> predicate)
        {
            SelectItems(item =>
            {
                var pos = MapPanel.GetViewportPosition(ContainerFromItem(item));
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
                // Single -> set only SelectedItem

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
                // Multiple or Extended with Ctrl -> toggle item in SelectedItems

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
                // Extended with Shift -> select items in viewport rectangle

                var p1 = MapPanel.GetViewportPosition(ContainerFromItem(SelectedItem));
                var p2 = MapPanel.GetViewportPosition(mapItem);

                if (p1.HasValue && p2.HasValue)
                {
                    SelectItemsInRect(new Rect(p1.Value, p2.Value));
                }
            }
            else if (SelectedItem != item)
            {
                // Extended without Control or Shift -> set selected item

                SelectedItem = item;
            }
        }
    }
}

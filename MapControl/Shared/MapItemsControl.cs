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
    /// An ItemsControl with selectable items on a Map. Uses MapItem as item container.
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
            SelectItemsByPosition(rect.Contains);
        }

        /// <summary>
        /// Selects all items in a rectangular range between SelectedItem and the specified MapItem.
        /// </summary>
        internal static void SelectItemsInRange(MapItem mapItem)
        {
            if (ItemsControlFromItemContainer(mapItem) is MapItemsControl mapItemsControl &&
                mapItemsControl.SelectionMode != SelectionMode.Single)
            {
                var pos = MapPanel.GetViewPosition(mapItem);

                if (pos.HasValue)
                {
                    var xMin = pos.Value.X;
                    var xMax = pos.Value.X;
                    var yMin = pos.Value.Y;
                    var yMax = pos.Value.Y;

                    if (mapItemsControl.SelectedItem != null)
                    {
                        var selectedMapItem = mapItemsControl.ContainerFromItem(mapItemsControl.SelectedItem);

                        if (selectedMapItem != mapItem)
                        {
                            pos = MapPanel.GetViewPosition(selectedMapItem);

                            if (pos.HasValue)
                            {
                                xMin = Math.Min(xMin, pos.Value.X);
                                xMax = Math.Max(xMax, pos.Value.X);
                                yMin = Math.Min(yMin, pos.Value.Y);
                                yMax = Math.Max(yMax, pos.Value.Y);
                            }
                        }
                    }

                    mapItemsControl.SelectItemsInRect(new Rect(xMin, yMin, xMax - xMin, yMax - yMin));
                }
            }
        }
    }
}

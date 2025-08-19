using System;
#if WPF
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
#elif UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
#elif WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
#elif AVALONIA
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
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
                var position = MapPanel.GetViewPosition(ContainerFromItem(item));

                return position.HasValue && predicate(position.Value);
            });
        }

        public void SelectItemsInRect(Rect rect)
        {
            SelectItemsByPosition(rect.Contains);
        }

        /// <summary>
        /// Selects all items in a rectangular range between SelectedItem and the specified MapItem.
        /// </summary>
        internal void SelectItemsInRange(MapItem mapItem)
        {
            var position = MapPanel.GetViewPosition(mapItem);

            if (position.HasValue)
            {
                var xMin = position.Value.X;
                var xMax = position.Value.X;
                var yMin = position.Value.Y;
                var yMax = position.Value.Y;

                if (SelectedItem != null)
                {
                    var selectedMapItem = ContainerFromItem(SelectedItem);

                    if (selectedMapItem != mapItem)
                    {
                        position = MapPanel.GetViewPosition(selectedMapItem);

                        if (position.HasValue)
                        {
                            xMin = Math.Min(xMin, position.Value.X);
                            xMax = Math.Max(xMax, position.Value.X);
                            yMin = Math.Min(yMin, position.Value.Y);
                            yMax = Math.Max(yMax, position.Value.Y);
                        }
                    }
                }

                SelectItemsInRect(new Rect(xMin, yMin, xMax - xMin, yMax - yMin));
            }
        }

        private void PrepareContainer(MapItem mapItem, object item)
        {
            if (LocationMemberPath != null)
            {
                mapItem.SetBinding(MapItem.LocationProperty,
                    new Binding { Source = item, Path = new PropertyPath(LocationMemberPath) });
            }
        }

        private void ClearContainer(MapItem mapItem)
        {
            if (LocationMemberPath != null)
            {
                mapItem.ClearValue(MapItem.LocationProperty);
            }
        }
    }
}

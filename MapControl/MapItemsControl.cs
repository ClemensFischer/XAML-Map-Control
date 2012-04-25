using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace MapControl
{
    public enum MapItemSelectionMode { Single, Extended }

    public class MapItemsControl : MultiSelector
    {
        public static readonly DependencyProperty SelectionModeProperty = DependencyProperty.Register(
            "SelectionMode", typeof(MapItemSelectionMode), typeof(MapItemsControl),
            new FrameworkPropertyMetadata((o, e) => ((MapItemsControl)o).CanSelectMultipleItems = (MapItemSelectionMode)e.NewValue != MapItemSelectionMode.Single));

        public static readonly DependencyProperty SelectionGeometryProperty = DependencyProperty.Register(
            "SelectionGeometry", typeof(Geometry), typeof(MapItemsControl),
            new FrameworkPropertyMetadata((o, e) => ((MapItemsControl)o).SelectionGeometryChanged((Geometry)e.NewValue)));

        public MapItemsControl()
        {
            CanSelectMultipleItems = false;
            Style = (Style)FindResource(typeof(MapItemsControl));
            Items.CurrentChanging += OnCurrentItemChanging;
            Items.CurrentChanged += OnCurrentItemChanged;
        }

        public MapItemSelectionMode SelectionMode
        {
            get { return (MapItemSelectionMode)GetValue(SelectionModeProperty); }
            set { SetValue(SelectionModeProperty, value); }
        }

        public Geometry SelectionGeometry
        {
            get { return (Geometry)GetValue(SelectionGeometryProperty); }
            set { SetValue(SelectionGeometryProperty, value); }
        }

        public MapItem GetMapItem(object item)
        {
            return item != null ? ItemContainerGenerator.ContainerFromItem(item) as MapItem : null;
        }

        public object GetHitItem(Point point)
        {
            DependencyObject obj = InputHitTest(point) as DependencyObject;

            while (obj != null)
            {
                if (obj is MapItem)
                {
                    return ((MapItem)obj).Item;
                }

                obj = VisualTreeHelper.GetParent(obj);
            }

            return null;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new MapItem();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            MapItem mapItem = (MapItem)element;
            mapItem.Item = item;
            base.PrepareContainerForItemOverride(element, item);
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            MapItem mapItem = (MapItem)element;
            mapItem.Item = null;
            base.ClearContainerForItemOverride(element, item);
        }

        private void OnCurrentItemChanging(object sender, CurrentChangingEventArgs eventArgs)
        {
            MapItem mapItem = GetMapItem(Items.CurrentItem);

            if (mapItem != null)
            {
                mapItem.IsCurrent = false;
            }
        }

        private void OnCurrentItemChanged(object sender, EventArgs eventArgs)
        {
            MapItem mapItem = GetMapItem(Items.CurrentItem);

            if (mapItem != null)
            {
                mapItem.IsCurrent = true;
            }
        }

        private void SelectionGeometryChanged(Geometry geometry)
        {
            if (geometry != null)
            {
                SelectionMode = MapItemSelectionMode.Extended;

                BeginUpdateSelectedItems();
                SelectedItems.Clear();

                if (!geometry.IsEmpty())
                {
                    foreach (object item in Items)
                    {
                        MapItem mapItem = GetMapItem(item);

                        if (mapItem != null && mapItem.HasViewPosition && geometry.FillContains(mapItem.ViewPosition))
                        {
                            SelectedItems.Add(item);
                        }
                    }
                }

                EndUpdateSelectedItems();
            }
        }
    }
}

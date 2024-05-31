// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace MapControl
{
    public partial class MapItemsControl
    {
        static MapItemsControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MapItemsControl), new FrameworkPropertyMetadata(typeof(MapItemsControl)));
        }

        public void SelectItemsInGeometry(Geometry geometry)
        {
            SelectItemsByPosition(p => geometry.FillContains(p));
        }

        public MapItem ContainerFromItem(object item)
        {
            return (MapItem)ItemContainerGenerator.ContainerFromItem(item);
        }

        public object ItemFromContainer(MapItem container)
        {
            return ItemContainerGenerator.ItemFromContainer(container);
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is MapItem;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new MapItem();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject container, object item)
        {
            base.PrepareContainerForItemOverride(container, item);

            if (LocationMemberPath != null && container is MapItem mapItem)
            {
                mapItem.SetBinding(MapItem.LocationProperty,
                    new Binding
                    {
                        Path = new PropertyPath(LocationMemberPath),
                        Source = item
                    });
            }
        }

        protected override void ClearContainerForItemOverride(DependencyObject container, object item)
        {
            base.ClearContainerForItemOverride(container, item);

            if (LocationMemberPath != null && container is MapItem mapItem)
            {
                mapItem.ClearValue(MapItem.LocationProperty);
            }
        }

        protected void ResetSelectedItems(object item)
        {
            SetSelectedItems(new[] { item });
        }
    }
}

// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
#endif

namespace MapControl
{
    public partial class MapItemsControl
    {
        public MapItemsControl()
        {
            DefaultStyleKey = typeof(MapItemsControl);
            MapPanel.InitMapElement(this);
        }

        public new MapItem ContainerFromItem(object item)
        {
            return (MapItem)base.ContainerFromItem(item);
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
            if (SelectedItem != item || SelectedItems.Count != 1)
            {
                SelectedItem = item;
            }
        }
    }
}

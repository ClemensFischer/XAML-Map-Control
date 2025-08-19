using System.Windows;
using System.Windows.Controls;
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
            SelectItemsByPosition(geometry.FillContains);
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
            PrepareContainer((MapItem)container, item);
        }

        protected override void ClearContainerForItemOverride(DependencyObject container, object item)
        {
            base.ClearContainerForItemOverride(container, item);
            ClearContainer((MapItem)container);
        }
    }
}

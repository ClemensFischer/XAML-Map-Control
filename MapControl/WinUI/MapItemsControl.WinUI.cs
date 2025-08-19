#if UWP
using Windows.UI.Xaml;
#else
using Microsoft.UI.Xaml;
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
            PrepareContainer((MapItem)container, item);
        }

        protected override void ClearContainerForItemOverride(DependencyObject container, object item)
        {
            base.ClearContainerForItemOverride(container, item);
            ClearContainer((MapItem)container);
        }
    }
}

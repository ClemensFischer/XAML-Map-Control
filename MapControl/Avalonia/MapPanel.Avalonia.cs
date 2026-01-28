using Avalonia;

namespace MapControl
{
    public partial class MapPanel
    {
        public static readonly AttachedProperty<bool> AutoCollapseProperty =
            DependencyPropertyHelper.RegisterAttached<bool>("AutoCollapse", typeof(MapPanel));

        public static readonly AttachedProperty<Location> LocationProperty =
            DependencyPropertyHelper.RegisterAttached<Location>("Location", typeof(MapPanel));

        public static readonly AttachedProperty<BoundingBox> BoundingBoxProperty =
            DependencyPropertyHelper.RegisterAttached<BoundingBox>("BoundingBox", typeof(MapPanel));

        public static readonly AttachedProperty<MapRect> MapRectProperty =
            DependencyPropertyHelper.RegisterAttached<MapRect>("MapRect", typeof(MapPanel));

        static MapPanel()
        {
            AffectsParentArrange<MapPanel>(LocationProperty, BoundingBoxProperty, MapRectProperty);
        }

        public static MapBase GetParentMap(FrameworkElement element)
        {
            return (MapBase)element.GetValue(ParentMapProperty);
        }
    }
}

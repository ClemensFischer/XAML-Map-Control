﻿namespace MapControl
{
    public partial class MapPanel
    {
        public static readonly AttachedProperty<bool> AutoCollapseProperty =
            DependencyPropertyHelper.RegisterAttached<bool>("AutoCollapse", typeof(MapPanel));

        public static readonly AttachedProperty<Location> LocationProperty =
            DependencyPropertyHelper.RegisterAttached<Location>("Location", typeof(MapPanel));

        public static readonly AttachedProperty<BoundingBox> BoundingBoxProperty =
            DependencyPropertyHelper.RegisterAttached<BoundingBox>("BoundingBox", typeof(MapPanel));

        protected Controls ChildElements => Children;

        static MapPanel()
        {
            AffectsParentArrange<MapPanel>(LocationProperty, BoundingBoxProperty);
        }

        public static MapBase GetParentMap(Control element)
        {
            return (MapBase)element.GetValue(ParentMapProperty);
        }

        public static void SetRenderTransform(Control element, Transform transform, double originX = 0d, double originY = 0d)
        {
            element.RenderTransform = transform;
            element.RenderTransformOrigin = new RelativePoint(originX, originY, RelativeUnit.Relative);
        }

        private static void SetVisible(Control element, bool visible)
        {
            element.IsVisible = visible;
        }
    }
}

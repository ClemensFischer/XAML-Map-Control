// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Collections.Generic;

namespace MapControl
{
    public partial class MapPanel
    {
        public static readonly AttachedProperty<bool> AutoCollapseProperty =
            DependencyPropertyHelper.RegisterAttached<MapPanel, bool>("AutoCollapse");

        public static readonly AttachedProperty<Location> LocationProperty =
            DependencyPropertyHelper.RegisterAttached<MapPanel, Location>("Location");

        public static readonly AttachedProperty<BoundingBox> BoundingBoxProperty =
            DependencyPropertyHelper.RegisterAttached<MapPanel, BoundingBox>("BoundingBox");

        protected IEnumerable<Control> ChildElements => Children;

        static MapPanel()
        {
            AffectsParentArrange<MapPanel>(LocationProperty, BoundingBoxProperty);
        }

        public MapPanel()
        {
            if (this is MapBase mapBase)
            {
                SetValue(ParentMapProperty, mapBase);
            }
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

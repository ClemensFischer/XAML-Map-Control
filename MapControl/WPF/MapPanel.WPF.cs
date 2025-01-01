// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace MapControl
{
    public partial class MapPanel
    {
        public static readonly DependencyProperty AutoCollapseProperty =
            DependencyPropertyHelper.RegisterAttached< bool>("AutoCollapse", typeof(MapPanel));

        public static readonly DependencyProperty LocationProperty =
            DependencyPropertyHelper.RegisterAttached<Location>("Location", typeof(MapPanel), null,
                FrameworkPropertyMetadataOptions.AffectsParentArrange);

        public static readonly DependencyProperty BoundingBoxProperty =
            DependencyPropertyHelper.RegisterAttached<BoundingBox>("BoundingBox", typeof(MapPanel), null,
                FrameworkPropertyMetadataOptions.AffectsParentArrange);

        protected IEnumerable<FrameworkElement> ChildElements => InternalChildren.OfType<FrameworkElement>();

        public MapPanel()
        {
            if (this is MapBase)
            {
                SetValue(ParentMapProperty, this);
            }
        }

        public static MapBase GetParentMap(FrameworkElement element)
        {
            return (MapBase)element.GetValue(ParentMapProperty);
        }

        public static void SetRenderTransform(FrameworkElement element, Transform transform, double originX = 0d, double originY = 0d)
        {
            element.RenderTransform = transform;
            element.RenderTransformOrigin = new Point(originX, originY);
        }

        private static void SetVisible(FrameworkElement element, bool visible)
        {
            element.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}

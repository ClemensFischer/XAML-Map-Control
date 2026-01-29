using System.Windows;

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

        public static readonly DependencyProperty MapRectProperty =
            DependencyPropertyHelper.RegisterAttached<Rect?>("MapRect", typeof(MapPanel), null,
                FrameworkPropertyMetadataOptions.AffectsParentArrange);

        public static MapBase GetParentMap(FrameworkElement element)
        {
            return (MapBase)element.GetValue(ParentMapProperty);
        }
    }
}

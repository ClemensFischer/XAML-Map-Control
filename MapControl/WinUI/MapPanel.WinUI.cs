#if UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
#endif

namespace MapControl
{
    public partial class MapPanel
    {
        public static readonly DependencyProperty AutoCollapseProperty =
            DependencyPropertyHelper.RegisterAttached<bool>("AutoCollapse", typeof(MapPanel));

        public static readonly DependencyProperty LocationProperty =
            DependencyPropertyHelper.RegisterAttached<Location?>("Location", typeof(MapPanel), null,
                (element, oldValue, newValue) => (element.Parent as MapPanel)?.InvalidateArrange());

        public static readonly DependencyProperty BoundingBoxProperty =
            DependencyPropertyHelper.RegisterAttached<BoundingBox>("BoundingBox", typeof(MapPanel), null,
                (element, oldValue, newValue) => (element.Parent as MapPanel)?.InvalidateArrange());

        public static readonly DependencyProperty MapRectProperty =
            DependencyPropertyHelper.RegisterAttached<Rect?>("MapRect", typeof(MapPanel), null,
                (element, oldValue, newValue) => (element.Parent as MapPanel)?.InvalidateArrange());

        public static void InitMapElement(FrameworkElement element)
        {
            // Workaround for missing property value inheritance.
            // Loaded and Unloaded handlers set and clear the ParentMap property value.
            //
            element.Loaded += (_, _) => GetParentMap(element);
            element.Unloaded += (_, _) => element.ClearValue(ParentMapProperty);
        }

        public static MapBase GetParentMap(FrameworkElement element)
        {
            var parentMap = (MapBase)element.GetValue(ParentMapProperty);

            // Traverse visual tree because of missing property value inheritance.
            //
            if (parentMap == null &&
                VisualTreeHelper.GetParent(element) is FrameworkElement parentElement)
            {
                parentMap = (parentElement as MapBase) ?? GetParentMap(parentElement);

                if (parentMap != null)
                {
                    element.SetValue(ParentMapProperty, parentMap);
                }
            }

            return parentMap;
        }
    }
}

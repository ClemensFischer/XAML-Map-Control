// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace MapControl
{
    public partial class MapPanel
    {
        public static readonly DependencyProperty LocationProperty = DependencyProperty.RegisterAttached(
            "Location", typeof(Location), typeof(MapPanel),
            new PropertyMetadata(null, (o, e) => (((FrameworkElement)o).Parent as MapPanel)?.InvalidateArrange()));

        public static readonly DependencyProperty BoundingBoxProperty = DependencyProperty.RegisterAttached(
            "BoundingBox", typeof(BoundingBox), typeof(MapPanel),
            new PropertyMetadata(null, (o, e) => (((FrameworkElement)o).Parent as MapPanel)?.InvalidateArrange()));

        public static readonly DependencyProperty ParentMapProperty = DependencyProperty.RegisterAttached(
            "ParentMap", typeof(MapBase), typeof(MapPanel), new PropertyMetadata(null, ParentMapPropertyChanged));

        private static readonly DependencyProperty ViewPositionProperty = DependencyProperty.RegisterAttached(
            "ViewPosition", typeof(Point?), typeof(MapPanel), new PropertyMetadata(null));

        public static void InitMapElement(FrameworkElement element)
        {
            if (element is MapBase)
            {
                element.SetValue(ParentMapProperty, element);
            }
            else
            {
                // Workaround for missing property value inheritance in Windows Runtime.
                // Loaded and Unloaded handlers set and clear the ParentMap property value.

                element.Loaded += (s, e) => GetParentMap(element);
                element.Unloaded += (s, e) => element.ClearValue(ParentMapProperty);
            }
        }

        public static MapBase GetParentMap(FrameworkElement element)
        {
            var parentMap = (MapBase)element.GetValue(ParentMapProperty);

            if (parentMap == null && (parentMap = FindParentMap(element)) != null)
            {
                element.SetValue(ParentMapProperty, parentMap);
            }

            return parentMap;
        }

        private static MapBase FindParentMap(FrameworkElement element)
        {
            return VisualTreeHelper.GetParent(element) is FrameworkElement parent
                ? ((parent as MapBase) ?? (MapBase)element.GetValue(ParentMapProperty) ?? FindParentMap(parent))
                : null;
        }

        private static void SetViewPosition(FrameworkElement element, Point? viewPosition)
        {
            element.SetValue(ViewPositionProperty, viewPosition);
        }
    }
}

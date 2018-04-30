// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace MapControl
{
    public partial class MapPanel
    {
        public static readonly DependencyProperty ParentMapProperty = DependencyProperty.RegisterAttached(
            "ParentMap", typeof(MapBase), typeof(MapPanel), new PropertyMetadata(null, ParentMapPropertyChanged));

        private static readonly DependencyProperty ViewportPositionProperty = DependencyProperty.RegisterAttached(
            "ViewportPosition", typeof(Point?), typeof(MapPanel), new PropertyMetadata(null));

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
            var parent = VisualTreeHelper.GetParent(element) as FrameworkElement;

            return parent == null ? null
                : ((parent as MapBase)
                ?? (MapBase)element.GetValue(ParentMapProperty)
                ?? FindParentMap(parent));
        }

        private static void SetViewportPosition(FrameworkElement element, Point? viewportPosition)
        {
            element.SetValue(ViewportPositionProperty, viewportPosition);
        }
    }
}

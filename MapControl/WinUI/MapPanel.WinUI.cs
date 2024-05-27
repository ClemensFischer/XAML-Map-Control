// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Collections.Generic;
using System.Linq;
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
            DependencyPropertyHelper.RegisterAttached<MapPanel, bool>("AutoCollapse");

        public static readonly DependencyProperty LocationProperty =
            DependencyPropertyHelper.RegisterAttached<MapPanel, Location>("Location", null,
                (element, oldValue, newValue) => (element.Parent as MapPanel)?.InvalidateArrange());

        public static readonly DependencyProperty BoundingBoxProperty =
            DependencyPropertyHelper.RegisterAttached<MapPanel, BoundingBox>("BoundingBox", null,
                (element, oldValue, newValue) => (element.Parent as MapPanel)?.InvalidateArrange());

        public MapPanel()
        {
            InitMapElement(this);
        }

        public static void InitMapElement(FrameworkElement element)
        {
            if (element is MapBase)
            {
                element.SetValue(ParentMapProperty, element);
            }
            else
            {
                // Workaround for missing property value inheritance.
                // Loaded and Unloaded handlers set and clear the ParentMap property value.

                element.Loaded += (s, e) => GetParentMap((FrameworkElement)s);
                element.Unloaded += (s, e) => ((FrameworkElement)s).ClearValue(ParentMapProperty);
            }
        }

        public static MapBase GetParentMap(FrameworkElement element)
        {
            var parentMap = (MapBase)element.GetValue(ParentMapProperty);

            // Traverse visual tree because of missing property value inheritance.

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

        public static void SetRenderTransform(FrameworkElement element, Transform transform, double originX = 0d, double originY = 0d)
        {
            element.RenderTransform = transform;
            element.RenderTransformOrigin = new Point(originX, originY);
        }

        protected IEnumerable<FrameworkElement> ChildElements => Children.OfType<FrameworkElement>();

        private static void SetVisible(FrameworkElement element, bool visible)
        {
            element.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}

// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#endif

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

        /// <summary>
        /// Sets the attached ViewPosition property of an element. The method is called during
        /// ArrangeOverride and may be overridden to modify the actual view position value.
        /// An overridden method should call this method to set the attached property.
        /// </summary>
        protected virtual void SetViewPosition(FrameworkElement element, ref Point? position)
        {
            element.SetValue(ViewPositionProperty, position);
        }
    }
}

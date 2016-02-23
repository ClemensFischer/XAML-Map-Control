// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2016 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if NETFX_CORE
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Media;
#endif

namespace MapControl
{
    public partial class MapPanel
    {
        public static readonly DependencyProperty ParentMapProperty = DependencyProperty.RegisterAttached(
            "ParentMap", typeof(MapBase), typeof(MapPanel), new PropertyMetadata(null, ParentMapPropertyChanged));

        public MapPanel()
        {
            if (this is MapBase)
            {
                SetValue(ParentMapProperty, this);
            }
            else
            {
                AddParentMapHandlers(this);
            }
        }

        /// <summary>
        /// Helper method to work around missing property value inheritance in Silverlight and Windows Runtime.
        /// Adds Loaded and Unloaded event handlers to the specified FrameworkElement, which set and clear the
        /// value of the MapPanel.ParentMap attached property.
        /// </summary>
        public static void AddParentMapHandlers(FrameworkElement element)
        {
            element.Loaded += (s, e) => GetParentMap(element);
            element.Unloaded += (s, e) => element.ClearValue(ParentMapProperty);
        }

        public static MapBase GetParentMap(UIElement element)
        {
            var parentMap = (MapBase)element.GetValue(ParentMapProperty);

            if (parentMap == null && (parentMap = FindParentMap(element)) != null)
            {
                element.SetValue(ParentMapProperty, parentMap);
            }

            return parentMap;
        }

        private static MapBase FindParentMap(UIElement element)
        {
            MapBase parentMap = null;
            var parentElement = VisualTreeHelper.GetParent(element) as UIElement;

            if (parentElement != null)
            {
                parentMap = parentElement as MapBase;

                if (parentMap == null)
                {
                    parentMap = GetParentMap(parentElement);
                }
            }

            return parentMap;
        }
    }
}

// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © Clemens Fischer 2012-2013
// Licensed under the Microsoft Public License (Ms-PL)

#if NETFX_CORE
using Windows.Foundation;
using Windows.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Media;
#endif

namespace MapControl
{
    public partial class MapRectangle
    {
        private void SetGeometry(Rect rect)
        {
            var geometry = (RectangleGeometry)Data;

            geometry.Rect = rect;
            RenderTransform = ParentMap.ViewportTransform;
        }

        private void ClearGeometry()
        {
            var geometry = (RectangleGeometry)Data;

            geometry.ClearValue(RectangleGeometry.RectProperty);
            ClearValue(RenderTransformProperty);
        }
    }
}

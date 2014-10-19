// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2014 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if WINDOWS_RUNTIME
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Media;
#endif

namespace MapControl
{
    public partial class MapRectangle
    {
        private void SetRect(Rect rect)
        {
            ((RectangleGeometry)Data).Rect = rect;
            RenderTransform = ParentMap.ViewportTransform;
        }
    }
}

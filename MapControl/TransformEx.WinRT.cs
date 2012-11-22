// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using Windows.Foundation;
using Windows.UI.Xaml.Media;

namespace MapControl
{
    public static class TransformEx
    {
        public static Point Transform(this GeneralTransform transform, Point point)
        {
            return transform.TransformPoint(point);
        }
    }
}

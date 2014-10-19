// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2014 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Windows;
using System.Windows.Media;

namespace MapControl
{
    public partial class MapRectangle
    {
        static MapRectangle()
        {
            FillTransform.Freeze();
        }

        private void SetRect(Rect rect)
        {
            // Apply scaling to the RectangleGeometry to compensate for inaccurate hit testing in WPF.
            // See http://stackoverflow.com/a/19335624/1136211

            var scale = 1e6 / Math.Min(rect.Width, rect.Height);
            rect.X *= scale;
            rect.Y *= scale;
            rect.Width *= scale;
            rect.Height *= scale;

            var scaleTransform = new ScaleTransform(1d / scale, 1d / scale);
            scaleTransform.Freeze();

            var transform = new TransformGroup();
            transform.Children.Add(scaleTransform); // revert scaling
            transform.Children.Add(ParentMap.ViewportTransform);

            ((RectangleGeometry)Data).Rect = rect;
            RenderTransform = transform;
        }
    }
}
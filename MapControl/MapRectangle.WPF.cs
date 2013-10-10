// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © Clemens Fischer 2012-2013
// Licensed under the Microsoft Public License (Ms-PL)

using System.Windows;
using System.Windows.Media;

namespace MapControl
{
    public partial class MapRectangle
    {
        static MapRectangle()
        {
            StrokeThicknessProperty.OverrideMetadata(
                typeof(MapRectangle), new FrameworkPropertyMetadata(0d));
        }

        protected void SetBrushTransform()
        {
            var tileBrush = Fill as TileBrush;

            if (tileBrush != null && Data != null)
            {
                var geometry = (RectangleGeometry)Data;

                tileBrush.ViewportUnits = BrushMappingMode.Absolute;
                tileBrush.Viewport = geometry.Rect;
                tileBrush.Transform = geometry.Transform;
            }
        }

        private void SetGeometry(Rect rect)
        {
            // Instead of setting RenderTransform as done in the Silverlight and
            // WinRT versions, the ViewportTransform is applied to the Transform
            // properties of the Geometry and the Fill Brush. In WPF, setting the
            // RenderTransform property results in incorrect hit testing.

            var geometry = (RectangleGeometry)Data;

            geometry.Rect = rect;
            geometry.Transform = ParentMap.ViewportTransform;
            SetBrushTransform();
        }

        private void ClearGeometry()
        {
            var geometry = (RectangleGeometry)Data;

            geometry.ClearValue(RectangleGeometry.RectProperty);
            geometry.ClearValue(Geometry.TransformProperty);
        }
    }
}

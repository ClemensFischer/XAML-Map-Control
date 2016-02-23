// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2016 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Windows;
using System.Windows.Media;

namespace MapControl
{
    public partial class TileLayer
    {
        static TileLayer()
        {
            IsHitTestVisibleProperty.OverrideMetadata(
                typeof(TileLayer), new FrameworkPropertyMetadata(false));
        }

        private Rect GetTileIndexBounds(int zoomLevel)
        {
            var scale = (double)(1 << zoomLevel) / 360d;
            var transform = parentMap.ViewportTransform.Matrix;

            transform.Invert(); // view to map coordinates
            transform.Translate(180d, -180d);
            transform.Scale(scale, -scale); // map coordinates to tile indices

            return new MatrixTransform(transform).TransformBounds(new Rect(parentMap.RenderSize));
        }

        private void SetRenderTransform()
        {
            var scale = Math.Pow(2d, parentMap.ZoomLevel - TileGrid.ZoomLevel);
            var offsetX = parentMap.ViewportOrigin.X - (180d + parentMap.MapOrigin.X) * parentMap.ViewportScale;
            var offsetY = parentMap.ViewportOrigin.Y - (180d - parentMap.MapOrigin.Y) * parentMap.ViewportScale;

            var transform = new Matrix(1d, 0d, 0d, 1d, TileSource.TileSize * TileGrid.XMin, TileSource.TileSize * TileGrid.YMin);
            transform.Scale(scale, scale);
            transform.Translate(offsetX, offsetY);
            transform.RotateAt(parentMap.Heading, parentMap.ViewportOrigin.X, parentMap.ViewportOrigin.Y);

            ((MatrixTransform)RenderTransform).Matrix = transform;
        }
    }
}

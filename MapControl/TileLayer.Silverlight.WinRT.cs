// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2016 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if NETFX_CORE
using Windows.Foundation;
using Windows.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Media;
#endif

namespace MapControl
{
    public partial class TileLayer
    {
        partial void Initialize()
        {
            IsHitTestVisible = false;

            MapPanel.AddParentMapHandlers(this);
        }

        private Rect GetTileIndexBounds(int zoomLevel)
        {
            var scale = (double)(1 << zoomLevel) / 360d;
            var transform = new MatrixTransform
            {
                Matrix = parentMap.ViewportTransform.Matrix
                    .Invert() // view to map coordinates
                    .Translate(180d, -180d)
                    .Scale(scale, -scale) // map coordinates to tile indices
            };

            return transform.TransformBounds(new Rect(new Point(), parentMap.RenderSize));
        }

        private void SetRenderTransform()
        {
            var scale = Math.Pow(2d, parentMap.ZoomLevel - TileGrid.ZoomLevel);
            var offsetX = parentMap.ViewportOrigin.X - (180d + parentMap.MapOrigin.X) * parentMap.ViewportScale;
            var offsetY = parentMap.ViewportOrigin.Y - (180d - parentMap.MapOrigin.Y) * parentMap.ViewportScale;

            ((MatrixTransform)RenderTransform).Matrix =
                new Matrix(1d, 0d, 0d, 1d, TileSource.TileSize * TileGrid.XMin, TileSource.TileSize * TileGrid.YMin)
                .Scale(scale, scale)
                .Translate(offsetX, offsetY)
                .RotateAt(parentMap.Heading, parentMap.ViewportOrigin.X, parentMap.ViewportOrigin.Y);
        }
    }
}

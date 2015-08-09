// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2015 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if NETFX_CORE
using Windows.UI.Xaml.Media;
#else
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

        private Matrix GetTileIndexMatrix(int zoomLevel)
        {
            var scale = (double)(1 << zoomLevel) / 360d;

            return parentMap.ViewportTransform.Matrix
                .Invert() // view to map coordinates
                .Translate(180d, -180d)
                .Scale(scale, -scale); // map coordinates to tile indices
        }

        private void SetRenderTransform()
        {
            var scale = Math.Pow(2d, parentMap.ZoomLevel - TileZoomLevel);
            var offsetX = parentMap.ViewportOrigin.X - (180d + parentMap.MapOrigin.X) * parentMap.ViewportScale;
            var offsetY = parentMap.ViewportOrigin.Y - (180d - parentMap.MapOrigin.Y) * parentMap.ViewportScale;

            ((MatrixTransform)RenderTransform).Matrix =
                new Matrix(1d, 0d, 0d, 1d, TileRect.X * TileSource.TileSize, TileRect.Y * TileSource.TileSize)
                .Scale(scale, scale)
                .Translate(offsetX, offsetY)
                .RotateAt(parentMap.Heading, parentMap.ViewportOrigin.X, parentMap.ViewportOrigin.Y); ;
        }
    }
}

// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2014 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if WINDOWS_RUNTIME
using Windows.Foundation;
using Windows.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Media;
#endif

namespace MapControl
{
    internal partial class TileContainer
    {
        private Matrix GetTileIndexMatrix(double scale)
        {
            return ViewportTransform.Matrix
                .Invert() // view to map coordinates
                .Translate(180d, -180d)
                .Scale(scale, -scale); // map coordinates to tile indices
        }

        private void UpdateViewportTransform(double scale, Point mapOrigin)
        {
            ViewportTransform.Matrix =
                new Matrix(1d, 0d, 0d, 1d, -mapOrigin.X, -mapOrigin.Y)
                .Scale(scale, -scale)
                .Rotate(rotation)
                .Translate(viewportOrigin.X, viewportOrigin.Y);
        }

        /// <summary>
        /// Sets a RenderTransform with origin at tileGrid.X and tileGrid.Y to minimize rounding errors.
        /// </summary>
        private void UpdateRenderTransform()
        {
            var scale = Math.Pow(2d, zoomLevel - tileZoomLevel);

            ((MatrixTransform)RenderTransform).Matrix =
                new Matrix(1d, 0d, 0d, 1d, tileGrid.X * TileSource.TileSize, tileGrid.Y * TileSource.TileSize)
                .Scale(scale, scale)
                .Translate(tileLayerOffset.X, tileLayerOffset.Y)
                .RotateAt(rotation, viewportOrigin.X, viewportOrigin.Y);
        }
    }
}

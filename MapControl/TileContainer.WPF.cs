// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © Clemens Fischer 2012-2013
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Windows.Media;

namespace MapControl
{
    internal partial class TileContainer : ContainerVisual
    {
        private void SetViewportTransform(Matrix transform)
        {
            transform.RotateAt(rotation, viewportOrigin.X, viewportOrigin.Y);
            ViewportTransform.Matrix = transform;
        }

        /// <summary>
        /// Gets a transform matrix with origin at tileGrid.X and tileGrid.Y to minimize rounding errors.
        /// </summary>
        private Matrix GetTileLayerTransformMatrix()
        {
            var scale = Math.Pow(2d, zoomLevel - tileZoomLevel);
            var transform = new Matrix(1d, 0d, 0d, 1d, tileGrid.X * TileSource.TileSize, tileGrid.Y * TileSource.TileSize);

            transform.Scale(scale, scale);
            transform.Translate(tileLayerOffset.X, tileLayerOffset.Y);
            transform.RotateAt(rotation, viewportOrigin.X, viewportOrigin.Y);

            return transform;
        }

        private Matrix GetTileIndexMatrix(int numTiles)
        {
            var scale = (double)numTiles / 360d;
            var transform = ViewportTransform.Matrix;

            transform.Invert(); // view to map coordinates
            transform.Translate(180d, -180d);
            transform.Scale(scale, -scale); // map coordinates to tile indices

            return transform;
        }
    }
}

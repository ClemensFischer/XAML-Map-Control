// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © Clemens Fischer 2012-2013
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if NETFX_CORE
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
#endif

namespace MapControl
{
    internal partial class TileContainer : Panel
    {
        private void SetViewportTransform(Matrix transform)
        {
            ViewportTransform.Matrix = transform.RotateAt(rotation, viewportOrigin.X, viewportOrigin.Y);
        }

        /// <summary>
        /// Gets a transform matrix with origin at tileGrid.X and tileGrid.Y to minimize rounding errors.
        /// </summary>
        private Matrix GetTileLayerTransformMatrix()
        {
            var scale = Math.Pow(2d, zoomLevel - tileZoomLevel);

            return new Matrix(1d, 0d, 0d, 1d, tileGrid.X * TileSource.TileSize, tileGrid.Y * TileSource.TileSize)
                .Scale(scale, scale)
                .Translate(tileLayerOffset.X, tileLayerOffset.Y)
                .RotateAt(rotation, viewportOrigin.X, viewportOrigin.Y);
        }

        private Matrix GetTileIndexMatrix(int numTiles)
        {
            var scale = (double)numTiles / 360d;

            return ViewportTransform.Matrix
                .Invert() // view to map coordinates
                .Translate(180d, -180d)
                .Scale(scale, -scale); // map coordinates to tile indices
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            foreach (TileLayer tileLayer in Children)
            {
                tileLayer.Measure(availableSize);
            }

            return new Size();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (TileLayer tileLayer in Children)
            {
                tileLayer.Arrange(new Rect());
            }

            return finalSize;
        }
    }
}

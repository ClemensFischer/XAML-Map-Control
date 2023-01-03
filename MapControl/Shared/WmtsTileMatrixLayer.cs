// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2023 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if WINUI
using Windows.Foundation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
#elif UWP
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
    public class WmtsTileMatrixLayer : Panel
    {
        // zoomLevel is index of tileMatrix in a WmtsTileMatrixSet.TileMatrixes list.
        //
        public WmtsTileMatrixLayer(WmtsTileMatrix tileMatrix, int zoomLevel)
        {
            RenderTransform = new MatrixTransform();
            WmtsTileMatrix = tileMatrix;
            TileMatrix = new TileMatrix(zoomLevel, 1, 1, 0, 0);
        }

        public WmtsTileMatrix WmtsTileMatrix { get; }

        public TileMatrix TileMatrix { get; private set; }

        public TileCollection Tiles { get; private set; } = new TileCollection();

        public void SetRenderTransform(ViewTransform viewTransform)
        {
            // Tile matrix origin in pixels.
            //
            var tileMatrixOrigin = new Point(WmtsTileMatrix.TileWidth * TileMatrix.XMin, WmtsTileMatrix.TileHeight * TileMatrix.YMin);

            ((MatrixTransform)RenderTransform).Matrix =
                viewTransform.GetTileLayerTransform(WmtsTileMatrix.Scale, WmtsTileMatrix.TopLeft, tileMatrixOrigin);
        }

        public bool UpdateTiles(ViewTransform viewTransform, Size viewSize)
        {
            // Bounds in tile pixels from view size.
            //
            var bounds = viewTransform.GetTileMatrixBounds(WmtsTileMatrix.Scale, WmtsTileMatrix.TopLeft, viewSize);

            // Tile X and Y bounds.
            //
            var xMin = (int)Math.Floor(bounds.X / WmtsTileMatrix.TileWidth);
            var yMin = (int)Math.Floor(bounds.Y / WmtsTileMatrix.TileHeight);
            var xMax = (int)Math.Floor((bounds.X + bounds.Width) / WmtsTileMatrix.TileWidth);
            var yMax = (int)Math.Floor((bounds.Y + bounds.Height) / WmtsTileMatrix.TileHeight);

            // Total tile matrix width in meters.
            //
            var totalWidth = WmtsTileMatrix.MatrixWidth * WmtsTileMatrix.TileWidth / WmtsTileMatrix.Scale;

            if (Math.Abs(totalWidth - 360d * MapProjection.Wgs84MeterPerDegree) > 1d)
            {
                // No full longitudinal coverage, restrict x index.
                //
                xMin = Math.Max(xMin, 0);
                xMax = Math.Min(Math.Max(xMax, 0), WmtsTileMatrix.MatrixWidth - 1);
            }

            yMin = Math.Max(yMin, 0);
            yMax = Math.Min(Math.Max(yMax, 0), WmtsTileMatrix.MatrixHeight - 1);

            if (TileMatrix.XMin == xMin && TileMatrix.YMin == yMin &&
                TileMatrix.XMax == xMax && TileMatrix.YMax == yMax)
            {
                // No change of the TileMatrix and the Tiles collection.
                //
                return false;
            }

            TileMatrix = new TileMatrix(TileMatrix.ZoomLevel, xMin, yMin, xMax, yMax);

            var tiles = new TileCollection();

            for (var y = yMin; y <= yMax; y++)
            {
                for (var x = xMin; x <= xMax; x++)
                {
                    tiles.Add(Tiles.GetTile(TileMatrix.ZoomLevel, x, y, WmtsTileMatrix.MatrixWidth));
                }
            }

            Tiles = tiles;

            Children.Clear();

            foreach (var tile in tiles)
            {
                Children.Add(tile.Image);
            }

            return true;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            availableSize = new Size(double.PositiveInfinity, double.PositiveInfinity);

            foreach (var tile in Tiles)
            {
                tile.Image.Measure(availableSize);
            }

            return new Size();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (var tile in Tiles)
            {
                // Arrange tiles relative to XMin/YMin.
                //
                var x = WmtsTileMatrix.TileWidth * (tile.X - TileMatrix.XMin);
                var y = WmtsTileMatrix.TileHeight * (tile.Y - TileMatrix.YMin);

                tile.Image.Width = WmtsTileMatrix.TileWidth;
                tile.Image.Height = WmtsTileMatrix.TileHeight;
                tile.Image.Arrange(new Rect(x, y, WmtsTileMatrix.TileWidth, WmtsTileMatrix.TileHeight));
            }

            return finalSize;
        }
    }
}

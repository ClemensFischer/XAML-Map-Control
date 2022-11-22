// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
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
        public WmtsTileMatrixLayer(WmtsTileMatrix tileMatrix, int zoomLevel)
        {
            RenderTransform = new MatrixTransform();
            TileMatrix = tileMatrix;
            ZoomLevel = zoomLevel;
        }

        public WmtsTileMatrix TileMatrix { get; }

        public int ZoomLevel { get; } // index of TileMatrix in WmtsTileMatrixSet.TileMatrixes

        public int XMin { get; private set; }
        public int YMin { get; private set; }
        public int XMax { get; private set; }
        public int YMax { get; private set; }

        public TileCollection Tiles { get; private set; } = new TileCollection();

        public void SetRenderTransform(ViewTransform viewTransform)
        {
            // tile matrix origin in pixels
            //
            var tileMatrixOrigin = new Point(TileMatrix.TileWidth * XMin, TileMatrix.TileHeight * YMin);

            ((MatrixTransform)RenderTransform).Matrix =
                viewTransform.GetTileLayerTransform(TileMatrix.Scale, TileMatrix.TopLeft, tileMatrixOrigin);
        }

        public bool SetBounds(ViewTransform viewTransform, Size viewSize)
        {
            // bounds in tile pixels from view size
            //
            var bounds = viewTransform.GetTileMatrixBounds(TileMatrix.Scale, TileMatrix.TopLeft, viewSize);

            // tile X and Y bounds
            //
            var xMin = (int)Math.Floor(bounds.X / TileMatrix.TileWidth);
            var yMin = (int)Math.Floor(bounds.Y / TileMatrix.TileHeight);
            var xMax = (int)Math.Floor((bounds.X + bounds.Width) / TileMatrix.TileWidth);
            var yMax = (int)Math.Floor((bounds.Y + bounds.Height) / TileMatrix.TileHeight);

            // total tile matrix width in meters
            //
            var totalWidth = TileMatrix.MatrixWidth * TileMatrix.TileWidth / TileMatrix.Scale;

            if (Math.Abs(totalWidth - 360d * MapProjection.Wgs84MeterPerDegree) > 1d)
            {
                // no full longitudinal coverage, restrict x index
                //
                xMin = Math.Max(xMin, 0);
                xMax = Math.Min(Math.Max(xMax, 0), TileMatrix.MatrixWidth - 1);
            }

            yMin = Math.Max(yMin, 0);
            yMax = Math.Min(Math.Max(yMax, 0), TileMatrix.MatrixHeight - 1);

            if (XMin == xMin && YMin == yMin && XMax == xMax && YMax == yMax)
            {
                return false;
            }

            XMin = xMin;
            YMin = yMin;
            XMax = xMax;
            YMax = yMax;

            return true;
        }

        public void UpdateTiles()
        {
            var tiles = new TileCollection();

            for (var y = YMin; y <= YMax; y++)
            {
                for (var x = XMin; x <= XMax; x++)
                {
                    tiles.Add(Tiles.GetTile(ZoomLevel, x, y, TileMatrix.MatrixWidth));
                }
            }

            Tiles = tiles;

            Children.Clear();

            foreach (var tile in Tiles)
            {
                Children.Add(tile.Image);
            }
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
                // arrange tiles relative to XMin/YMin
                //
                var x = TileMatrix.TileWidth * (tile.X - XMin);
                var y = TileMatrix.TileHeight * (tile.Y - YMin);

                tile.Image.Width = TileMatrix.TileWidth;
                tile.Image.Height = TileMatrix.TileHeight;
                tile.Image.Arrange(new Rect(x, y, TileMatrix.TileWidth, TileMatrix.TileHeight));
            }

            return finalSize;
        }
    }
}

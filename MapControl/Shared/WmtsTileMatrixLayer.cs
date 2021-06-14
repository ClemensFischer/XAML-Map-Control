// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Linq;
#if WINUI
using Windows.Foundation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
#elif WINDOWS_UWP
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

        public IReadOnlyCollection<Tile> Tiles { get; private set; } = new List<Tile>();

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

            // tile column and row index bounds
            //
            var xMin = (int)Math.Floor(bounds.X / TileMatrix.TileWidth);
            var yMin = (int)Math.Floor(bounds.Y / TileMatrix.TileHeight);
            var xMax = (int)Math.Floor((bounds.X + bounds.Width) / TileMatrix.TileWidth);
            var yMax = (int)Math.Floor((bounds.Y + bounds.Height) / TileMatrix.TileHeight);

            xMin = Math.Max(xMin, 0);
            yMin = Math.Max(yMin, 0);
            xMax = Math.Min(Math.Max(xMax, 0), TileMatrix.MatrixWidth - 1);
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
            var newTiles = new List<Tile>();

            for (var y = YMin; y <= YMax; y++)
            {
                for (var x = XMin; x <= XMax; x++)
                {
                    newTiles.Add(Tiles.FirstOrDefault(t => t.X == x && t.Y == y) ?? new Tile(ZoomLevel, x, y));
                }
            }

            Tiles = newTiles;

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

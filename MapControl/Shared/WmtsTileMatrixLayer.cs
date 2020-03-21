// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2020 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Linq;
#if WINDOWS_UWP
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
            IsHitTestVisible = false;
            RenderTransform = new MatrixTransform();
            TileMatrix = tileMatrix;
            ZoomLevel = zoomLevel;
        }

        public WmtsTileMatrix TileMatrix { get; }
        public int ZoomLevel { get; }
        public int XMin { get; private set; }
        public int YMin { get; private set; }
        public int XMax { get; private set; }
        public int YMax { get; private set; }

        public IReadOnlyCollection<Tile> Tiles { get; private set; } = new List<Tile>();

        public bool SetBounds(MapProjection projection, double heading, Size mapSize)
        {
            // top/left viewport corner in map coordinates (meters)
            //
            var tileOrigin = projection.InverseViewportTransform.Transform(new Point());

            // top/left viewport corner in tile matrix coordinates (tile column and row indexes)
            //
            var tileMatrixOrigin = new Point(
                TileMatrix.Scale * (tileOrigin.X - TileMatrix.TopLeft.X),
                TileMatrix.Scale * (TileMatrix.TopLeft.Y - tileOrigin.Y));

            // relative layer scale
            //
            var scale = TileMatrix.Scale / projection.ViewportScale;

            var transform = new MatrixTransform
            {
                Matrix = MatrixFactory.Create(scale, -heading, tileMatrixOrigin)
            };

            var bounds = transform.TransformBounds(new Rect(0d, 0d, mapSize.Width, mapSize.Height));

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

            System.Diagnostics.Debug.WriteLine("{0}: {1}..{2}, {3}..{4}", TileMatrix.Identifier, xMin, xMax, yMin, yMax);

            return true;
        }

        public void SetRenderTransform(MapProjection projection, double heading)
        {
            // XMin/YMin corner in map coordinates (meters)
            //
            var mapOrigin = new Point(
                TileMatrix.TopLeft.X + XMin * TileMatrix.TileWidth / TileMatrix.Scale,
                TileMatrix.TopLeft.Y - YMin * TileMatrix.TileHeight / TileMatrix.Scale);

            // XMin/YMin corner in viewport coordinates (pixels)
            //
            var viewOrigin = projection.ViewportTransform.Transform(mapOrigin);

            // relative layer scale
            //
            var scale = projection.ViewportScale / TileMatrix.Scale;

            ((MatrixTransform)RenderTransform).Matrix = MatrixFactory.Create(scale, heading, viewOrigin);
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

using System;
using System.Collections.Generic;
#if WPF
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
#elif UWP
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
#elif WINUI
using Windows.Foundation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
#elif AVALONIA
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
#endif

namespace MapControl
{
    public class WmtsTileMatrixLayer : Panel
    {
        // zoomLevel is index of tileMatrix in a WmtsTileMatrixSet.TileMatrixes list.
        //
        public WmtsTileMatrixLayer(WmtsTileMatrix wmtsTileMatrix, int zoomLevel)
        {
            this.SetRenderTransform(new MatrixTransform());
            WmtsTileMatrix = wmtsTileMatrix;
            TileMatrix = new TileMatrix(zoomLevel, 1, 1, 0, 0);
        }

        public WmtsTileMatrix WmtsTileMatrix { get; }

        public TileMatrix TileMatrix { get; private set; }

        public IEnumerable<ImageTile> Tiles { get; private set; } = [];

        public void UpdateRenderTransform(ViewTransform viewTransform)
        {
            // Tile matrix origin in pixels.
            //
            var tileMatrixOrigin = new Point(WmtsTileMatrix.TileWidth * TileMatrix.XMin, WmtsTileMatrix.TileHeight * TileMatrix.YMin);

            ((MatrixTransform)RenderTransform).Matrix =
                viewTransform.GetTileLayerTransform(WmtsTileMatrix.Scale, WmtsTileMatrix.TopLeft, tileMatrixOrigin);
        }

        public bool UpdateTiles(ViewTransform viewTransform, double viewWidth, double viewHeight)
        {
            // Tile matrix bounds in pixels.
            //
            var bounds = viewTransform.GetTileMatrixBounds(WmtsTileMatrix.Scale, WmtsTileMatrix.TopLeft, viewWidth, viewHeight);

            // Tile X and Y bounds.
            //
            var xMin = (int)Math.Floor(bounds.X / WmtsTileMatrix.TileWidth);
            var yMin = (int)Math.Floor(bounds.Y / WmtsTileMatrix.TileHeight);
            var xMax = (int)Math.Floor((bounds.X + bounds.Width) / WmtsTileMatrix.TileWidth);
            var yMax = (int)Math.Floor((bounds.Y + bounds.Height) / WmtsTileMatrix.TileHeight);

            if (!WmtsTileMatrix.HasFullHorizontalCoverage)
            {
                // Set X range limits.
                //
                xMin = Math.Max(xMin, 0);
                xMax = Math.Min(Math.Max(xMax, 0), WmtsTileMatrix.MatrixWidth - 1);
            }

            // Set Y range limits.
            //
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
            Tiles = new ImageTileList(Tiles, TileMatrix, WmtsTileMatrix.MatrixWidth);

            Children.Clear();

            foreach (var tile in Tiles)
            {
                Children.Add(tile.Image);
            }

            return true;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
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
                // Arrange tiles relative to TileMatrix.XMin/YMin.
                //
                var tileWidth = WmtsTileMatrix.TileWidth;
                var tileHeight = WmtsTileMatrix.TileHeight;
                var x = tileWidth * (tile.X - TileMatrix.XMin);
                var y = tileHeight * (tile.Y - TileMatrix.YMin);

                tile.Image.Width = tileWidth;
                tile.Image.Height = tileHeight;
                tile.Image.Arrange(new Rect(x, y, tileWidth, tileHeight));
            }

            return finalSize;
        }
    }
}

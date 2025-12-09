using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace MapControl
{
    public class DrawingTileMatrixLayer(WmtsTileMatrix wmtsTileMatrix, int zoomLevel) : UIElement
    {
        private readonly MatrixTransform transform = new MatrixTransform();

        public WmtsTileMatrix WmtsTileMatrix => wmtsTileMatrix;

        public TileMatrix TileMatrix { get; private set; } = new TileMatrix(zoomLevel, 1, 1, 0, 0);

        public IEnumerable<DrawingTile> Tiles { get; private set; } = [];

        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.PushTransform(transform);

            foreach (var tile in Tiles)
            {
                drawingContext.DrawDrawing(tile.Drawing);
            }
        }

        public void UpdateRenderTransform(ViewTransform viewTransform)
        {
            // Tile matrix origin in pixels.
            //
            var tileMatrixOrigin = new Point(WmtsTileMatrix.TileWidth * TileMatrix.XMin, WmtsTileMatrix.TileHeight * TileMatrix.YMin);

            transform.Matrix = viewTransform.GetTileLayerTransform(WmtsTileMatrix.Scale, WmtsTileMatrix.TopLeft, tileMatrixOrigin);
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

            CreateTiles();
            InvalidateVisual();

            return true;
        }

        private void CreateTiles()
        {
            var tiles = new List<DrawingTile>(TileMatrix.Width * TileMatrix.Height);

            for (var y = TileMatrix.YMin; y <= TileMatrix.YMax; y++)
            {
                for (var x = TileMatrix.XMin; x <= TileMatrix.XMax; x++)
                {
                    var tile = Tiles.FirstOrDefault(t => t.X == x && t.Y == y);

                    if (tile == null)
                    {
                        tile = new DrawingTile(TileMatrix.ZoomLevel, x, y, WmtsTileMatrix.MatrixWidth);

                        var equivalentTile = Tiles.FirstOrDefault(t => t.ImageDrawing.ImageSource != null && t.Column == tile.Column && t.Row == tile.Row);

                        if (equivalentTile != null)
                        {
                            tile.IsPending = false;
                            tile.ImageDrawing.ImageSource = equivalentTile.ImageDrawing.ImageSource; // no Opacity animation
                        }
                    }

                    tile.ImageDrawing.Rect = new Rect(
                        WmtsTileMatrix.TileWidth * (x - TileMatrix.XMin),
                        WmtsTileMatrix.TileHeight * (y - TileMatrix.YMin),
                        WmtsTileMatrix.TileWidth,
                        WmtsTileMatrix.TileHeight);

                    tiles.Add(tile);
                }
            }

            Tiles = tiles;
        }
    }
}

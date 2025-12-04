using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace MapControl
{
    public class ImageDrawingTile(int zoomLevel, int x, int y, int columnCount)
        : Tile(zoomLevel, x, y, columnCount)
    {
        public ImageDrawing Drawing { get; } = new ImageDrawing();

        public override async Task LoadImageAsync(Func<Task<ImageSource>> loadImageFunc)
        {
            var image = await loadImageFunc().ConfigureAwait(false);

            if (image != null)
            {
                await Drawing.Dispatcher.InvokeAsync(() => Drawing.ImageSource = image);
            }
        }
    }

    public class DrawingTileMatrixLayer(WmtsTileMatrix wmtsTileMatrix, int zoomLevel) : UIElement
    {
        public WmtsTileMatrix WmtsTileMatrix => wmtsTileMatrix;

        public TileMatrix TileMatrix { get; private set; } = new TileMatrix(zoomLevel, 1, 1, 0, 0);

        public IEnumerable<ImageDrawingTile> Tiles { get; private set; } = [];

        public DrawingGroup Drawing { get; } = new DrawingGroup { Transform = new MatrixTransform() };

        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawDrawing(Drawing);
        }

        public void UpdateRenderTransform(ViewTransform viewTransform)
        {
            // Tile matrix origin in pixels.
            //
            var tileMatrixOrigin = new Point(WmtsTileMatrix.TileWidth * TileMatrix.XMin, WmtsTileMatrix.TileHeight * TileMatrix.YMin);

            ((MatrixTransform)Drawing.Transform).Matrix =
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

            CreateTiles();

            return true;
        }

        private void CreateTiles()
        {
            var tiles = new List<ImageDrawingTile>();
            var drawings = new DrawingCollection();

            for (var y = TileMatrix.YMin; y <= TileMatrix.YMax; y++)
            {
                for (var x = TileMatrix.XMin; x <= TileMatrix.XMax; x++)
                {
                    var tile = Tiles.FirstOrDefault(t => t.X == x && t.Y == y);

                    if (tile == null)
                    {
                        tile = new ImageDrawingTile(TileMatrix.ZoomLevel, x, y, WmtsTileMatrix.MatrixWidth);

                        var equivalentTile = Tiles.FirstOrDefault(t => t.Drawing.ImageSource != null && t.Column == tile.Column && t.Row == tile.Row);

                        if (equivalentTile != null)
                        {
                            tile.IsPending = false;
                            tile.Drawing.ImageSource = equivalentTile.Drawing.ImageSource;
                        }
                    }

                    tile.Drawing.Rect = new Rect(
                        WmtsTileMatrix.TileWidth * (x - TileMatrix.XMin),
                        WmtsTileMatrix.TileHeight * (y - TileMatrix.YMin),
                        WmtsTileMatrix.TileWidth,
                        WmtsTileMatrix.TileHeight);

                    tiles.Add(tile);
                    drawings.Add(tile.Drawing);
                }
            }

            Tiles = tiles;
            Drawing.Children = drawings;
        }
    }
}

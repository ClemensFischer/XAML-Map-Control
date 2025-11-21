using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MapControl
{
    public class BitmapTile(int zoomLevel, int x, int y, int columnCount, int width, int height)
        : Tile(zoomLevel, x, y, columnCount)
    {
        public event EventHandler Completed;

        public byte[] PixelBuffer { get; set; }

        public override async Task LoadImageAsync(Func<Task<ImageSource>> loadImageFunc)
        {
            var image = await loadImageFunc().ConfigureAwait(false);

            if (image is BitmapSource bitmap)
            {
                if (bitmap.Format != PixelFormats.Pbgra32)
                {
                    bitmap = new FormatConvertedBitmap(bitmap, PixelFormats.Pbgra32, null, 0d);
                }

                PixelBuffer = new byte[4 * width * height];
                bitmap.CopyPixels(PixelBuffer, 4 * width, 0);

                Completed?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public class BitmapTileMatrixLayer(WmtsTileMatrix wmtsTileMatrix, int zoomLevel) : FrameworkElement
    {
        private readonly ImageBrush imageBrush = new ImageBrush
        {
            ViewportUnits = BrushMappingMode.Absolute,
            Transform = new MatrixTransform()
        };

        private WriteableBitmap bitmap;

        public WmtsTileMatrix WmtsTileMatrix { get; } = wmtsTileMatrix;

        public TileMatrix TileMatrix { get; private set; } = new TileMatrix(zoomLevel, 1, 1, 0, 0);

        public List<BitmapTile> Tiles { get; private set; } = [];

        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawRectangle(imageBrush, null, new Rect(RenderSize));
        }

        public void UpdateRenderTransform(ViewTransform viewTransform)
        {
            // Tile matrix origin in pixels.
            //
            var tileMatrixOrigin = new Point(WmtsTileMatrix.TileWidth * TileMatrix.XMin, WmtsTileMatrix.TileHeight * TileMatrix.YMin);

            ((MatrixTransform)imageBrush.Transform).Matrix =
                viewTransform.GetTileLayerTransform(WmtsTileMatrix.Scale, WmtsTileMatrix.TopLeft, tileMatrixOrigin);
        }

        public bool UpdateTiles(ViewTransform viewTransform, double viewWidth, double viewHeight)
        {
            // Bounds in tile pixels from view size.
            //
            var bounds = viewTransform.GetTileMatrixBounds(WmtsTileMatrix.Scale, WmtsTileMatrix.TopLeft, viewWidth, viewHeight);

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

            CreateBitmap();
            CreateTiles();

            return true;
        }

        private void CreateBitmap()
        {
            var width = WmtsTileMatrix.TileWidth * (TileMatrix.XMax - TileMatrix.XMin + 1);
            var height = WmtsTileMatrix.TileHeight * (TileMatrix.YMax - TileMatrix.YMin + 1);

            bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Pbgra32, null);

            imageBrush.ImageSource = bitmap;
            imageBrush.Viewport = new Rect(0, 0, width, height);
        }

        private void CreateTiles()
        {
            var tiles = new List<BitmapTile>();

            for (var y = TileMatrix.YMin; y <= TileMatrix.YMax; y++)
            {
                for (var x = TileMatrix.XMin; x <= TileMatrix.XMax; x++)
                {
                    var tile = Tiles.FirstOrDefault(t => t.ZoomLevel == TileMatrix.ZoomLevel && t.X == x && t.Y == y);

                    if (tile == null)
                    {
                        tile = new BitmapTile(TileMatrix.ZoomLevel, x, y, WmtsTileMatrix.MatrixWidth, WmtsTileMatrix.TileWidth, WmtsTileMatrix.TileHeight);

                        var equivalentTile = Tiles.FirstOrDefault(
                            t => t.PixelBuffer != null && t.ZoomLevel == tile.ZoomLevel && t.Column == tile.Column && t.Row == tile.Row);

                        if (equivalentTile != null)
                        {
                            tile.IsPending = false;
                            tile.PixelBuffer = equivalentTile.PixelBuffer;
                        }
                    }

                    if (tile.PixelBuffer != null)
                    {
                        CopyTile(tile);
                    }
                    else
                    {
                        tile.Completed += OnTileCompleted;
                    }

                    tiles.Add(tile);
                }
            }

            Tiles = tiles;
        }

        private void CopyTile(BitmapTile tile)
        {
            var rect = new Int32Rect(
                WmtsTileMatrix.TileWidth * (tile.X - TileMatrix.XMin),
                WmtsTileMatrix.TileHeight * (tile.Y - TileMatrix.YMin),
                WmtsTileMatrix.TileWidth,
                WmtsTileMatrix.TileHeight);

            bitmap.WritePixels(rect, tile.PixelBuffer, 4 * WmtsTileMatrix.TileWidth, 0);
        }

        private void OnTileCompleted(object sender, EventArgs e)
        {
            var tile = (BitmapTile)sender;

            tile.Completed -= OnTileCompleted;

            Dispatcher.Invoke(() =>
            {
                if (tile.X >= TileMatrix.XMin && tile.X <= TileMatrix.XMax &&
                    tile.Y >= TileMatrix.YMin && tile.Y <= TileMatrix.YMax)
                {
                    CopyTile(tile);
                }
            });
        }
    }
}

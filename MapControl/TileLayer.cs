using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;

namespace MapControl
{
    [ContentProperty("TileSource")]
    public class TileLayer : DrawingVisual
    {
        private readonly TileImageLoader tileImageLoader = new TileImageLoader();
        private readonly List<Tile> tiles = new List<Tile>();
        private string description = string.Empty;
        private Int32Rect grid;
        private int zoomLevel;

        public TileLayer()
        {
            VisualEdgeMode = EdgeMode.Aliased;
            VisualTransform = new MatrixTransform();
            MinZoomLevel = 1;
            MaxZoomLevel = 18;
            MaxDownloads = 8;
        }

        public TileSource TileSource { get; set; }
        public bool HasDarkBackground { get; set; }
        public int MinZoomLevel { get; set; }
        public int MaxZoomLevel { get; set; }

        public int MaxDownloads
        {
            get { return tileImageLoader.MaxDownloads; }
            set { tileImageLoader.MaxDownloads = value; }
        }

        public string Name
        {
            get { return tileImageLoader.TileLayerName; }
            set { tileImageLoader.TileLayerName = value; }
        }

        public string Description
        {
            get { return description; }
            set { description = value.Replace("{y}", DateTime.Now.Year.ToString()); }
        }

        public Matrix TransformMatrix
        {
            get { return ((MatrixTransform)VisualTransform).Matrix; }
            set { ((MatrixTransform)VisualTransform).Matrix = value; }
        }

        public void UpdateTiles(int zoomLevel, Int32Rect grid)
        {
            this.grid = grid;
            this.zoomLevel = zoomLevel;

            tileImageLoader.EndDownloadTiles();

            if (VisualParent != null && TileSource != null)
            {
                SelectTiles();
                RenderTiles();

                tileImageLoader.BeginDownloadTiles(tiles);
            }
        }

        public void ClearTiles()
        {
            tiles.Clear();
            tileImageLoader.EndDownloadTiles();
        }

        private Int32Rect GetTileGrid(int tileZoomLevel)
        {
            int tileSize = 1 << (zoomLevel - tileZoomLevel);
            int max = (1 << tileZoomLevel) - 1;
            int x1 = grid.X / tileSize - 1;
            int x2 = (grid.X + grid.Width - 1) / tileSize + 1;
            int y1 = Math.Max(0, grid.Y / tileSize - 1);
            int y2 = Math.Min(max, (grid.Y + grid.Height - 1) / tileSize + 1);

            return new Int32Rect(x1, y1, x2 - x1 + 1, y2 - y1 + 1);
        }

        private void SelectTiles()
        {
            TileContainer tileContainer = VisualParent as TileContainer;
            int maxZoom = Math.Min(zoomLevel, MaxZoomLevel);
            int minZoom = maxZoom;

            if (tileContainer != null && tileContainer.TileLayers.IndexOf(this) == 0)
            {
                minZoom = MinZoomLevel;
            }

            tiles.RemoveAll(t =>
            {
                if (t.ZoomLevel > maxZoom || t.ZoomLevel < minZoom)
                {
                    return true;
                }

                Int32Rect tileGrid = GetTileGrid(t.ZoomLevel);
                return t.X < tileGrid.X || t.X >= tileGrid.X + tileGrid.Width || t.Y < tileGrid.Y || t.Y >= tileGrid.Y + tileGrid.Height;
            });

            for (int tileZoomLevel = minZoom; tileZoomLevel <= maxZoom; tileZoomLevel++)
            {
                Int32Rect tileGrid = GetTileGrid(tileZoomLevel);

                for (int y = tileGrid.Y; y < tileGrid.Y + tileGrid.Height; y++)
                {
                    for (int x = tileGrid.X; x < tileGrid.X + tileGrid.Width; x++)
                    {
                        if (tiles.Find(t => t.ZoomLevel == tileZoomLevel && t.X == x && t.Y == y) == null)
                        {
                            Tile tile = new Tile(TileSource, tileZoomLevel, x, y);
                            Tile equivalent = tiles.Find(t => t.Image != null && t.ZoomLevel == tile.ZoomLevel && t.XIndex == tile.XIndex && t.Y == tile.Y);

                            if (equivalent != null)
                            {
                                tile.Image = equivalent.Image;
                            }

                            tiles.Add(tile);
                        }
                    }
                }
            }

            tiles.Sort((t1, t2) => t1.ZoomLevel - t2.ZoomLevel);

            System.Diagnostics.Trace.TraceInformation("{0} Tiles: {1}", tiles.Count, string.Join(", ", tiles.Select(t => t.ZoomLevel.ToString())));
        }

        private void RenderTiles()
        {
            using (DrawingContext drawingContext = RenderOpen())
            {
                tiles.ForEach(tile =>
                {
                    int tileSize = 256 << (zoomLevel - tile.ZoomLevel);
                    Rect tileRect = new Rect(tileSize * tile.X - 256 * grid.X, tileSize * tile.Y - 256 * grid.Y, tileSize, tileSize);

                    drawingContext.DrawRectangle(tile.Brush, null, tileRect);

                    //if (tile.ZoomLevel == zoomLevel)
                    //    drawingContext.DrawText(new FormattedText(tile.ToString(), System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Segoe UI"), 14, Brushes.Black), tileRect.TopLeft);
                });
            }
        }
    }
}
// WPF MapControl - http://wpfmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;

namespace MapControl
{
    /// <summary>
    /// Fills a rectangular area with map tiles from a TileSource.
    /// </summary>
    [ContentProperty("TileSource")]
    public class TileLayer : DrawingVisual
    {
        private readonly TileImageLoader tileImageLoader;
        private List<Tile> tiles = new List<Tile>();
        private string description = string.Empty;
        private Int32Rect grid;
        private int zoomLevel;

        public TileLayer()
        {
            tileImageLoader = new TileImageLoader(this);
            VisualTransform = new MatrixTransform();
            VisualEdgeMode = EdgeMode.Aliased;
            MinZoomLevel = 1;
            MaxZoomLevel = 18;
            MaxParallelDownloads = 8;
        }

        public string SourceName { get; set; }
        public TileSource TileSource { get; set; }
        public int MinZoomLevel { get; set; }
        public int MaxZoomLevel { get; set; }
        public int MaxParallelDownloads { get; set; }
        public bool HasDarkBackground { get; set; }

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

        internal void UpdateTiles(int zoomLevel, Int32Rect grid)
        {
            this.grid = grid;
            this.zoomLevel = zoomLevel;

            tileImageLoader.CancelGetTiles();

            if (TileSource != null)
            {
                SelectTiles();
                RenderTiles();
                tileImageLoader.BeginGetTiles(tiles);
            }
        }

        internal void ClearTiles()
        {
            tileImageLoader.CancelGetTiles();
            tiles.Clear();
            RenderTiles();
        }

        private void SelectTiles()
        {
            int maxZoomLevel = Math.Min(zoomLevel, MaxZoomLevel);
            int minZoomLevel = maxZoomLevel;
            ContainerVisual parent = Parent as ContainerVisual;

            if (parent != null && parent.Children.IndexOf(this) == 0)
            {
                minZoomLevel = MinZoomLevel;
            }

            List<Tile> newTiles = new List<Tile>();

            for (int z = minZoomLevel; z <= maxZoomLevel; z++)
            {
                int tileSize = 1 << (zoomLevel - z);
                int x1 = grid.X / tileSize;
                int x2 = (grid.X + grid.Width - 1) / tileSize;
                int y1 = Math.Max(0, grid.Y / tileSize);
                int y2 = Math.Min((1 << z) - 1, (grid.Y + grid.Height - 1) / tileSize);

                for (int y = y1; y <= y2; y++)
                {
                    for (int x = x1; x <= x2; x++)
                    {
                        Tile tile = tiles.Find(t => t.ZoomLevel == z && t.X == x && t.Y == y);

                        if (tile == null)
                        {
                            tile = new Tile(z, x, y);
                            Tile equivalent = tiles.Find(t => t.Source != null && t.ZoomLevel == z && t.XIndex == tile.XIndex && t.Y == y);

                            if (equivalent != null)
                            {
                                tile.Source = equivalent.Source;
                            }
                        }

                        newTiles.Add(tile);
                    }
                }
            }

            tiles = newTiles;
            //System.Diagnostics.Trace.TraceInformation("{0} Tiles: {1}", tiles.Count, string.Join(", ", tiles.Select(t => t.ZoomLevel.ToString())));
        }

        private void RenderTiles()
        {
            using (DrawingContext drawingContext = RenderOpen())
            {
                foreach (Tile tile in tiles)
                {
                    int tileSize = 256 << (zoomLevel - tile.ZoomLevel);
                    Rect tileRect = new Rect(tileSize * tile.X - 256 * grid.X, tileSize * tile.Y - 256 * grid.Y, tileSize, tileSize);

                    drawingContext.DrawRectangle(tile.Brush, null, tileRect);

                    //if (tile.ZoomLevel == zoomLevel)
                    //    drawingContext.DrawText(new FormattedText(string.Format("{0}-{1}-{2}", tile.ZoomLevel, tile.X, tile.Y),
                    //        System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Segoe UI"), 14, Brushes.Black), tileRect.TopLeft);
                }
            }
        }
    }
}

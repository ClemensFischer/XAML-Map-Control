// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2013 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Linq;
#if NETFX_CORE
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
#endif

namespace MapControl
{
    /// <summary>
    /// Fills a rectangular area with map tiles from a TileSource.
    /// </summary>
#if NETFX_CORE
    [ContentProperty(Name = "TileSource")]
#else
    [ContentProperty("TileSource")]
#endif
    public partial class TileLayer
    {
        public static TileLayer Default
        {
            get
            {
                return new TileLayer
                {
                    SourceName = "OpenStreetMap",
                    Description = "© {y} OpenStreetMap Contributors, CC-BY-SA",
                    TileSource = new TileSource("http://{c}.tile.openstreetmap.org/{z}/{x}/{y}.png")
                };
            }
        }

        private readonly MatrixTransform transform = new MatrixTransform();
        private readonly TileImageLoader tileImageLoader;
        private List<Tile> tiles = new List<Tile>();
        private string description = string.Empty;
        private Int32Rect grid;
        private int zoomLevel;

        public TileLayer()
        {
            tileImageLoader = new TileImageLoader(this);
            MinZoomLevel = 1;
            MaxZoomLevel = 18;
            MaxParallelDownloads = 8;
            LoadLowerZoomLevels = true;
            Initialize();
        }

        partial void Initialize();

        public string SourceName { get; set; }
        public TileSource TileSource { get; set; }
        public int MinZoomLevel { get; set; }
        public int MaxZoomLevel { get; set; }
        public int MaxParallelDownloads { get; set; }
        public bool LoadLowerZoomLevels { get; set; }
        public bool HasDarkBackground { get; set; }

        public string Description
        {
            get { return description; }
            set { description = value.Replace("{y}", DateTime.Now.Year.ToString()); }
        }

        public string TileSourceUriFormat
        {
            get { return TileSource != null ? TileSource.UriFormat : string.Empty; }
            set { TileSource = new TileSource(value); }
        }

        internal void SetTransformMatrix(Matrix transformMatrix)
        {
            transform.Matrix = transformMatrix;
        }

        protected internal virtual void UpdateTiles(int zoomLevel, Int32Rect grid)
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

        protected internal virtual void ClearTiles()
        {
            tileImageLoader.CancelGetTiles();
            tiles.Clear();
            RenderTiles();
        }

        protected void SelectTiles()
        {
            var maxZoomLevel = Math.Min(zoomLevel, MaxZoomLevel);
            var minZoomLevel = maxZoomLevel;
            var container = TileContainer;

            if (LoadLowerZoomLevels && container != null && container.Children.IndexOf(this) == 0)
            {
                minZoomLevel = MinZoomLevel;
            }

            var newTiles = new List<Tile>();

            for (var z = minZoomLevel; z <= maxZoomLevel; z++)
            {
                var tileSize = 1 << (zoomLevel - z);
                var x1 = grid.X / tileSize;
                var x2 = (grid.X + grid.Width - 1) / tileSize;
                var y1 = Math.Max(0, grid.Y / tileSize);
                var y2 = Math.Min((1 << z) - 1, (grid.Y + grid.Height - 1) / tileSize);

                for (var y = y1; y <= y2; y++)
                {
                    for (var x = x1; x <= x2; x++)
                    {
                        var tile = tiles.FirstOrDefault(t => t.ZoomLevel == z && t.X == x && t.Y == y);

                        if (tile == null)
                        {
                            tile = new Tile(z, x, y);

                            var equivalentTile = tiles.FirstOrDefault(
                                t => t.ImageSource != null && t.ZoomLevel == z && t.XIndex == tile.XIndex && t.Y == y);

                            if (equivalentTile != null)
                            {
                                // do not animate to avoid flicker when crossing date line
                                tile.SetImageSource(equivalentTile.ImageSource, false);
                            }
                        }

                        newTiles.Add(tile);
                    }
                }
            }

            tiles = newTiles;
        }
    }
}

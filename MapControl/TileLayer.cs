// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2014 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Linq;
#if WINDOWS_RUNTIME
using Windows.Foundation;
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
#if WINDOWS_RUNTIME
    [ContentProperty(Name = "TileSource")]
#else
    [ContentProperty("TileSource")]
#endif
    public class TileLayer : PanelBase
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

        private readonly TileImageLoader tileImageLoader = new TileImageLoader();
        private string description = string.Empty;
        private TileSource tileSource;
        private List<Tile> tiles = new List<Tile>();
        private Int32Rect grid;
        private int zoomLevel;

        public TileLayer()
        {
            MinZoomLevel = 0;
            MaxZoomLevel = 18;
            MaxParallelDownloads = 8;
            LoadLowerZoomLevels = true;
            AnimateTileOpacity = true;
        }

        public string SourceName { get; set; }
        public int MinZoomLevel { get; set; }
        public int MaxZoomLevel { get; set; }
        public int MaxParallelDownloads { get; set; }
        public bool LoadLowerZoomLevels { get; set; }
        public bool AnimateTileOpacity { get; set; }
        public Brush Foreground { get; set; }

        public string Description
        {
            get { return description; }
            set { description = value.Replace("{y}", DateTime.Now.Year.ToString()); }
        }

        public TileSource TileSource
        {
            get { return tileSource; }
            set
            {
                tileSource = value;

                if (grid.Width > 0 && grid.Height > 0)
                {
                    tileImageLoader.CancelGetTiles();
                    tiles.Clear();

                    if (tileSource != null)
                    {
                        SelectTiles();
                        RenderTiles();
                        tileImageLoader.BeginGetTiles(this, tiles.Where(t => !t.HasImageSource));
                    }
                    else
                    {
                        RenderTiles();
                    }
                }
            }
        }

        internal void UpdateTiles(int zoomLevel, Int32Rect grid)
        {
            this.grid = grid;
            this.zoomLevel = zoomLevel;

            if (tileSource != null)
            {
                tileImageLoader.CancelGetTiles();
                SelectTiles();
                RenderTiles();
                tileImageLoader.BeginGetTiles(this, tiles.Where(t => !t.HasImageSource));
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
            var maxZoomLevel = Math.Min(zoomLevel, MaxZoomLevel);
            var minZoomLevel = maxZoomLevel;

            if (LoadLowerZoomLevels && Parent is TileContainer && ((TileContainer)Parent).TileLayers.FirstOrDefault() == this)
            {
                minZoomLevel = MinZoomLevel;
            }

            var newTiles = new List<Tile>();

            for (var z = minZoomLevel; z <= maxZoomLevel; z++)
            {
                var tileSize = 1 << (zoomLevel - z);
                var x1 = (int)Math.Floor((double)grid.X / tileSize); // may be negative
                var x2 = (grid.X + grid.Width - 1) / tileSize;
                var y1 = Math.Max(grid.Y / tileSize, 0);
                var y2 = Math.Min((grid.Y + grid.Height - 1) / tileSize, (1 << z) - 1);

                for (var y = y1; y <= y2; y++)
                {
                    for (var x = x1; x <= x2; x++)
                    {
                        var tile = tiles.FirstOrDefault(t => t.ZoomLevel == z && t.X == x && t.Y == y);

                        if (tile == null)
                        {
                            tile = new Tile(z, x, y);

                            var equivalentTile = tiles.FirstOrDefault(
                                t => t.Image.Source != null && t.ZoomLevel == z && t.XIndex == tile.XIndex && t.Y == y);

                            if (equivalentTile != null)
                            {
                                // do not animate to avoid flicker when crossing date line
                                tile.SetImageSource(equivalentTile.Image.Source, false);
                            }
                        }

                        newTiles.Add(tile);
                    }
                }
            }

            tiles = newTiles;
        }

        private void RenderTiles()
        {
            InternalChildren.Clear();

            foreach (var tile in tiles)
            {
                InternalChildren.Add(tile.Image);
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (var tile in tiles)
            {
                var tileSize = (double)(256 << (zoomLevel - tile.ZoomLevel));
                tile.Image.Width = tileSize;
                tile.Image.Height = tileSize;
                tile.Image.Arrange(new Rect(tileSize * tile.X - 256 * grid.X, tileSize * tile.Y - 256 * grid.Y, tileSize, tileSize));
            }

            return finalSize;
        }
    }
}

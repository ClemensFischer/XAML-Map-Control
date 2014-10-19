// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2014 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Linq;
#if WINDOWS_RUNTIME
using Windows.Foundation;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
#endif

namespace MapControl
{
    public interface ITileImageLoader
    {
        void BeginLoadTiles(TileLayer tileLayer, IEnumerable<Tile> tiles);
        void CancelLoadTiles(TileLayer tileLayer);
    }

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
                    Description="© [OpenStreetMap Contributors](http://www.openstreetmap.org/copyright)",
                    TileSource = new TileSource { UriFormat = "http://{c}.tile.openstreetmap.org/{z}/{x}/{y}.png" }
                };
            }
        }

        private readonly ITileImageLoader tileImageLoader;
        private TileSource tileSource;
        private List<Tile> tiles = new List<Tile>();
        private int zoomLevel;
        private Int32Rect grid;

        public TileLayer()
            : this(new TileImageLoader())
        {
        }

        public TileLayer(ITileImageLoader tileImageLoader)
        {
            this.tileImageLoader = tileImageLoader;
            MinZoomLevel = 0;
            MaxZoomLevel = 18;
            MaxParallelDownloads = 4;
            LoadLowerZoomLevels = true;
            AnimateTileOpacity = true;
        }

        public string SourceName { get; set; }
        public string Description { get; set; }
        public int MinZoomLevel { get; set; }
        public int MaxZoomLevel { get; set; }
        public int MaxParallelDownloads { get; set; }
        public bool LoadLowerZoomLevels { get; set; }
        public bool AnimateTileOpacity { get; set; }
        public Brush Foreground { get; set; }

        /// <summary>
        /// In case the Description text contains copyright links in markdown syntax [text](url),
        /// the DescriptionInlines property may be used to create a collection of Run and Hyperlink
        /// inlines to be displayed in e.g. a TextBlock or a Silverlight RichTextBlock.
        /// </summary>
        public ICollection<Inline> DescriptionInlines
        {
            get { return Description.ToInlines(); }
        }

        public TileSource TileSource
        {
            get { return tileSource; }
            set
            {
                tileSource = value;

                if (grid.Width > 0 && grid.Height > 0)
                {
                    ClearTiles();
                    UpdateTiles();
                }
            }
        }

        internal void ClearTiles()
        {
            tileImageLoader.CancelLoadTiles(this);
            tiles.Clear();
            Children.Clear();
        }

        internal void UpdateTiles(int zoomLevel, Int32Rect grid)
        {
            this.zoomLevel = zoomLevel;
            this.grid = grid;

            UpdateTiles();
        }

        private void UpdateTiles()
        {
            if (tileSource != null)
            {
                tileImageLoader.CancelLoadTiles(this);

                SelectTiles();
                Children.Clear();

                foreach (var tile in tiles)
                {
                    Children.Add(tile.Image);
                }

                tileImageLoader.BeginLoadTiles(this, tiles.Where(t => !t.HasImageSource));
            }
        }

        private void SelectTiles()
        {
            var maxZoomLevel = Math.Min(zoomLevel, MaxZoomLevel);
            var minZoomLevel = maxZoomLevel;

            if (LoadLowerZoomLevels &&
                Parent is TileContainer &&
                ((TileContainer)Parent).TileLayers.FirstOrDefault() == this)
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

// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2015 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Linq;
#if WINDOWS_RUNTIME
using Windows.Foundation;
using Windows.UI.Xaml;
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
    public partial class TileLayer : PanelBase, IMapElement
    {
        public static TileLayer Default
        {
            get
            {
                return new TileLayer
                {
                    SourceName = "OpenStreetMap",
                    Description = "© [OpenStreetMap Contributors](http://www.openstreetmap.org/copyright)",
                    TileSource = new TileSource { UriFormat = "http://{c}.tile.openstreetmap.org/{z}/{x}/{y}.png" }
                };
            }
        }

        public static readonly DependencyProperty TileSourceProperty = DependencyProperty.Register(
            "TileSource", typeof(TileSource), typeof(TileLayer),
            new PropertyMetadata(null, (o, e) => ((TileLayer)o).UpdateTiles(true)));

        public static readonly DependencyProperty SourceNameProperty = DependencyProperty.Register(
            "SourceName", typeof(string), typeof(TileLayer), new PropertyMetadata(null));

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
            "Description", typeof(string), typeof(TileLayer), new PropertyMetadata(null));

        public static readonly DependencyProperty MinZoomLevelProperty = DependencyProperty.Register(
            "MinZoomLevel", typeof(int), typeof(TileLayer), new PropertyMetadata(0));

        public static readonly DependencyProperty MaxZoomLevelProperty = DependencyProperty.Register(
            "MaxZoomLevel", typeof(int), typeof(TileLayer), new PropertyMetadata(18));

        public static readonly DependencyProperty MaxParallelDownloadsProperty = DependencyProperty.Register(
            "MaxParallelDownloads", typeof(int), typeof(TileLayer), new PropertyMetadata(4));

        public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register(
            "Foreground", typeof(Brush), typeof(TileLayer), new PropertyMetadata(null));

        public static readonly new DependencyProperty BackgroundProperty = DependencyProperty.Register(
            "Background", typeof(Brush), typeof(TileLayer), new PropertyMetadata(null));

        private readonly ITileImageLoader tileImageLoader;
        private List<Tile> tiles = new List<Tile>();
        private MapBase parentMap;

        public TileLayer()
            : this(new TileImageLoader())
        {
        }

        public TileLayer(ITileImageLoader tileImageLoader)
        {
            this.tileImageLoader = tileImageLoader;
            Initialize();
        }

        partial void Initialize();

        /// <summary>
        /// Controls how map tiles are loaded.
        /// </summary>
        public TileSource TileSource
        {
            get { return (TileSource)GetValue(TileSourceProperty); }
            set { SetValue(TileSourceProperty, value); }
        }

        /// <summary>
        /// Name of the TileSource. Used as key in a TileLayerCollection and to name an optional tile cache.
        /// </summary>
        public string SourceName
        {
            get { return (string)GetValue(SourceNameProperty); }
            set { SetValue(SourceNameProperty, value); }
        }

        /// <summary>
        /// Description of the TileLayer.
        /// </summary>
        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        /// <summary>
        /// Minimum zoom level supported by the TileLayer.
        /// </summary>
        public int MinZoomLevel
        {
            get { return (int)GetValue(MinZoomLevelProperty); }
            set { SetValue(MinZoomLevelProperty, value); }
        }

        /// <summary>
        /// Maximum zoom level supported by the TileLayer.
        /// </summary>
        public int MaxZoomLevel
        {
            get { return (int)GetValue(MaxZoomLevelProperty); }
            set { SetValue(MaxZoomLevelProperty, value); }
        }

        /// <summary>
        /// Maximum number of parallel downloads that may be performed by the TileLayer's ITileImageLoader.
        /// </summary>
        public int MaxParallelDownloads
        {
            get { return (int)GetValue(MaxParallelDownloadsProperty); }
            set { SetValue(MaxParallelDownloadsProperty, value); }
        }

        /// <summary>
        /// Sets MapBase.Foreground, if not null.
        /// </summary>
        public Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }

        /// <summary>
        /// Sets MapBase.Background, if not null.
        /// New property prevents filling of RenderTransformed TileLayer with Panel.Background.
        /// </summary>
        public new Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        public MapBase ParentMap
        {
            get { return parentMap; }
            set
            {
                if (parentMap != null)
                {
                    parentMap.TileGridChanged -= UpdateTiles;
                    ClearValue(RenderTransformProperty);
                }

                parentMap = value;

                if (parentMap != null)
                {
                    parentMap.TileGridChanged += UpdateTiles;
                    RenderTransform = parentMap.TileLayerTransform;
                }

                UpdateTiles();
            }
        }

        protected virtual void UpdateTiles(bool clearTiles = false)
        {
            if (tiles.Count > 0)
            {
                tileImageLoader.CancelLoadTiles(this);
            }

            if (clearTiles)
            {
                tiles.Clear();
            }

            SelectTiles();

            Children.Clear();

            if (tiles.Count > 0)
            {
                foreach (var tile in tiles)
                {
                    Children.Add(tile.Image);
                }

                tileImageLoader.BeginLoadTiles(this, tiles.Where(t => t.Pending));
            }
        }

        private void UpdateTiles(object sender, EventArgs e)
        {
            UpdateTiles();
        }

        private void SelectTiles()
        {
            var newTiles = new List<Tile>();

            if (parentMap != null && TileSource != null)
            {
                var grid = parentMap.TileGrid;
                var zoomLevel = parentMap.TileZoomLevel;
                var maxZoomLevel = Math.Min(zoomLevel, MaxZoomLevel);
                var minZoomLevel = MinZoomLevel;

                if (minZoomLevel < maxZoomLevel && this != parentMap.TileLayers.FirstOrDefault())
                {
                    // do not load background tiles if this is not the base layer
                    minZoomLevel = maxZoomLevel;
                }

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
                                    t => t.ZoomLevel == z && t.XIndex == tile.XIndex && t.Y == y && t.Image.Source != null);

                                if (equivalentTile != null)
                                {
                                    // do not animate to avoid flicker when crossing 180°
                                    tile.SetImage(equivalentTile.Image.Source, false);
                                }
                            }

                            newTiles.Add(tile);
                        }
                    }
                }
            }

            tiles = newTiles;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (parentMap != null)
            {
                foreach (var tile in tiles)
                {
                    var tileSize = (double)(256 << (parentMap.TileZoomLevel - tile.ZoomLevel));
                    var x = tileSize * tile.X - 256 * parentMap.TileGrid.X;
                    var y = tileSize * tile.Y - 256 * parentMap.TileGrid.Y;

                    tile.Image.Width = tileSize;
                    tile.Image.Height = tileSize;
                    tile.Image.Arrange(new Rect(x, y, tileSize, tileSize));
                }
            }

            return finalSize;
        }
    }
}

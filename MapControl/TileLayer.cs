// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2016 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Linq;
#if NETFX_CORE
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
#endif

namespace MapControl
{
    /// <summary>
    /// Fills the map viewport with map tiles from a TileSource.
    /// </summary>
#if NETFX_CORE
    [ContentProperty(Name = "TileSource")]
#else
    [ContentProperty("TileSource")]
#endif
    public partial class TileLayer : PanelBase, IMapElement
    {
        public static TileLayer OpenStreetMapTileLayer
        {
            get
            {
                return new TileLayer
                {
                    SourceName = "OpenStreetMap",
                    Description = "© [OpenStreetMap Contributors](http://www.openstreetmap.org/copyright)",
                    TileSource = new TileSource { UriFormat = "http://{c}.tile.openstreetmap.org/{z}/{x}/{y}.png" },
                    MaxZoomLevel = 19
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

        public static readonly DependencyProperty LogoImageProperty = DependencyProperty.Register(
            "LogoImage", typeof(ImageSource), typeof(TileLayer), new PropertyMetadata(null));

        public static readonly DependencyProperty ZoomLevelOffsetProperty = DependencyProperty.Register(
            "ZoomLevelOffset", typeof(double), typeof(TileLayer),
            new PropertyMetadata(0d, (o, e) => ((TileLayer)o).UpdateTileGrid()));

        public static readonly DependencyProperty MinZoomLevelProperty = DependencyProperty.Register(
            "MinZoomLevel", typeof(int), typeof(TileLayer), new PropertyMetadata(0));

        public static readonly DependencyProperty MaxZoomLevelProperty = DependencyProperty.Register(
            "MaxZoomLevel", typeof(int), typeof(TileLayer), new PropertyMetadata(18));

        public static readonly DependencyProperty MaxParallelDownloadsProperty = DependencyProperty.Register(
            "MaxParallelDownloads", typeof(int), typeof(TileLayer), new PropertyMetadata(4));

        public static readonly DependencyProperty UpdateIntervalProperty = DependencyProperty.Register(
            "UpdateInterval", typeof(TimeSpan), typeof(TileLayer),
            new PropertyMetadata(TimeSpan.FromSeconds(0.5), (o, e) => ((TileLayer)o).updateTimer.Interval = (TimeSpan)e.NewValue));

        public static readonly DependencyProperty UpdateWhileViewportChangingProperty = DependencyProperty.Register(
            "UpdateWhileViewportChanging", typeof(bool), typeof(TileLayer), new PropertyMetadata(true));

        public static readonly DependencyProperty LoadTilesDescendingProperty = DependencyProperty.Register(
            "LoadTilesDescending", typeof(bool), typeof(TileLayer), new PropertyMetadata(false));

        public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register(
            "Foreground", typeof(Brush), typeof(TileLayer), new PropertyMetadata(null));

        public static readonly new DependencyProperty BackgroundProperty = DependencyProperty.Register(
            "Background", typeof(Brush), typeof(TileLayer), new PropertyMetadata(null));

        private readonly DispatcherTimer updateTimer;
        private MapBase parentMap;
        private double mapOriginX;

        public TileLayer()
            : this(new TileImageLoader())
        {
        }

        public TileLayer(ITileImageLoader tileImageLoader)
        {
            Initialize();

            RenderTransform = new MatrixTransform();
            TileImageLoader = tileImageLoader;
            Tiles = new List<Tile>();

            updateTimer = new DispatcherTimer { Interval = UpdateInterval };
            updateTimer.Tick += (s, e) => UpdateTileGrid();
        }

        partial void Initialize(); // Windows Runtime and Silverlight only

        public ITileImageLoader TileImageLoader { get; private set; }
        public ICollection<Tile> Tiles { get; private set; }
        public TileGrid TileGrid { get; private set; }

        /// <summary>
        /// Provides map tile URIs or images.
        /// </summary>
        public TileSource TileSource
        {
            get { return (TileSource)GetValue(TileSourceProperty); }
            set { SetValue(TileSourceProperty, value); }
        }

        /// <summary>
        /// Name of the TileSource. Used as key in a TileLayerCollection and as component of a tile cache key.
        /// </summary>
        public string SourceName
        {
            get { return (string)GetValue(SourceNameProperty); }
            set { SetValue(SourceNameProperty, value); }
        }

        /// <summary>
        /// Description of the TileLayer. Used to display copyright information on top of the map.
        /// </summary>
        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        /// <summary>
        /// Logo image. Used to display a provider brand logo on top of the map.
        /// </summary>
        public ImageSource LogoImage
        {
            get { return (ImageSource)GetValue(LogoImageProperty); }
            set { SetValue(LogoImageProperty, value); }
        }

        /// <summary>
        /// Adds an offset to the Map's ZoomLevel for a relative scale between the Map and the TileLayer.
        /// </summary>
        public double ZoomLevelOffset
        {
            get { return (double)GetValue(ZoomLevelOffsetProperty); }
            set { SetValue(ZoomLevelOffsetProperty, value); }
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
        /// Minimum time interval between tile updates.
        /// </summary>
        public TimeSpan UpdateInterval
        {
            get { return (TimeSpan)GetValue(UpdateIntervalProperty); }
            set { SetValue(UpdateIntervalProperty, value); }
        }

        /// <summary>
        /// Controls if tiles are updates while the viewport is still changing.
        /// </summary>
        public bool UpdateWhileViewportChanging
        {
            get { return (bool)GetValue(UpdateWhileViewportChangingProperty); }
            set { SetValue(UpdateWhileViewportChangingProperty, value); }
        }

        /// <summary>
        /// Controls the order of zoom levels in which map tiles are loaded.
        /// The default is value is false, i.e. tiles are loaded in ascending order.
        /// </summary>
        public bool LoadTilesDescending
        {
            get { return (bool)GetValue(LoadTilesDescendingProperty); }
            set { SetValue(LoadTilesDescendingProperty, value); }
        }

        /// <summary>
        /// Optional foreground brush. Sets MapBase.Foreground, if not null.
        /// </summary>
        public Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }

        /// <summary>
        /// Optional background brush. Sets MapBase.Background, if not null.
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
                    parentMap.ViewportChanged -= OnViewportChanged;
                }

                parentMap = value;

                if (parentMap != null)
                {
                    parentMap.ViewportChanged += OnViewportChanged;
                    mapOriginX = parentMap.MapOrigin.X;
                }

                UpdateTileGrid();
            }
        }

        private void OnViewportChanged(object sender, EventArgs e)
        {
            if (TileGrid == null || Math.Abs(parentMap.MapOrigin.X - mapOriginX) > 180d)
            {
                // immediately handle map origin leap when map center moves across 180° longitude
                UpdateTileGrid();
            }
            else
            {
                SetRenderTransform();

                if (updateTimer.IsEnabled && !UpdateWhileViewportChanging)
                {
                    updateTimer.Stop(); // restart
                }

                if (!updateTimer.IsEnabled)
                {
                    updateTimer.Start();
                }
            }

            mapOriginX = parentMap.MapOrigin.X;
        }

        protected void UpdateTileGrid()
        {
            updateTimer.Stop();

            if (parentMap != null)
            {
                var zoomLevel = Math.Max(0, (int)Math.Round(parentMap.ZoomLevel + ZoomLevelOffset));
                var bounds = GetTileIndexBounds(zoomLevel);
                var tileGrid = new TileGrid(zoomLevel,
                    (int)Math.Floor(bounds.X), (int)Math.Floor(bounds.Y),
                    (int)Math.Floor(bounds.X + bounds.Width), (int)Math.Floor(bounds.Y + bounds.Height));

                if (!tileGrid.Equals(TileGrid))
                {
                    TileGrid = tileGrid;
                    SetRenderTransform();
                    UpdateTiles(false);
                }
            }
            else
            {
                TileGrid = null;
                UpdateTiles(true);
            }
        }

        protected virtual void UpdateTiles(bool clearTiles)
        {
            if (Tiles.Count > 0)
            {
                TileImageLoader.CancelLoadTiles(this);
            }

            if (clearTiles)
            {
                Tiles.Clear();
            }

            SelectTiles();

            Children.Clear();

            if (Tiles.Count > 0)
            {
                foreach (var tile in Tiles)
                {
                    Children.Add(tile.Image);
                }

                var pendingTiles = Tiles.Where(t => t.Pending);

                if (LoadTilesDescending)
                {
                    pendingTiles = pendingTiles.OrderByDescending(t => t.ZoomLevel); // higher zoom levels first
                }

                TileImageLoader.BeginLoadTiles(this, pendingTiles);
            }
        }

        protected void SelectTiles()
        {
            var newTiles = new List<Tile>();

            if (TileGrid != null && parentMap != null && TileSource != null)
            {
                var maxZoomLevel = Math.Min(TileGrid.ZoomLevel, MaxZoomLevel);
                var minZoomLevel = MinZoomLevel;

                if (minZoomLevel < maxZoomLevel && this != parentMap.TileLayers.FirstOrDefault())
                {
                    // do not load background tiles if this is not the base layer
                    minZoomLevel = maxZoomLevel;
                }

                for (var z = minZoomLevel; z <= maxZoomLevel; z++)
                {
                    var tileSize = 1 << (TileGrid.ZoomLevel - z);
                    var x1 = (int)Math.Floor((double)TileGrid.XMin / tileSize); // may be negative
                    var x2 = TileGrid.XMax / tileSize;
                    var y1 = Math.Max(TileGrid.YMin / tileSize, 0);
                    var y2 = Math.Min(TileGrid.YMax / tileSize, (1 << z) - 1);

                    for (var y = y1; y <= y2; y++)
                    {
                        for (var x = x1; x <= x2; x++)
                        {
                            var tile = Tiles.FirstOrDefault(t => t.ZoomLevel == z && t.X == x && t.Y == y);

                            if (tile == null)
                            {
                                tile = new Tile(z, x, y);

                                var equivalentTile = Tiles.FirstOrDefault(
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

            Tiles = newTiles;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (TileGrid != null)
            {
                foreach (var tile in Tiles)
                {
                    var tileSize = TileSource.TileSize << (TileGrid.ZoomLevel - tile.ZoomLevel);
                    var x = tileSize * tile.X - TileSource.TileSize * TileGrid.XMin;
                    var y = tileSize * tile.Y - TileSource.TileSize * TileGrid.YMin;

                    tile.Image.Width = tileSize;
                    tile.Image.Height = tileSize;
                    tile.Image.Arrange(new Rect(x, y, tileSize, tileSize));
                }
            }

            return finalSize;
        }
    }
}

// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2020 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Linq;
#if WINDOWS_UWP
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Media;
#endif

namespace MapControl
{
    /// <summary>
    /// Fills the map viewport with map tiles from a TileSource.
    /// </summary>
    public class MapTileLayer : MapTileLayerBase
    {
        private const double WebMercatorMapSize = 2 * Math.PI * MapProjection.Wgs84EquatorialRadius;

        /// <summary>
        /// A default MapTileLayer using OpenStreetMap data.
        /// </summary>
        public static MapTileLayer OpenStreetMapTileLayer
        {
            get
            {
                return new MapTileLayer
                {
                    SourceName = "OpenStreetMap",
                    Description = "© [OpenStreetMap Contributors](http://www.openstreetmap.org/copyright)",
                    TileSource = new TileSource { UriFormat = "https://{c}.tile.openstreetmap.org/{z}/{x}/{y}.png" },
                    MaxZoomLevel = 19
                };
            }
        }

        public static readonly DependencyProperty MinZoomLevelProperty = DependencyProperty.Register(
            nameof(MinZoomLevel), typeof(int), typeof(MapTileLayer), new PropertyMetadata(0));

        public static readonly DependencyProperty MaxZoomLevelProperty = DependencyProperty.Register(
            nameof(MaxZoomLevel), typeof(int), typeof(MapTileLayer), new PropertyMetadata(18));

        public MapTileLayer()
            : this(new TileImageLoader())
        {
        }

        public MapTileLayer(ITileImageLoader tileImageLoader)
            : base(tileImageLoader)
        {
        }

        public IReadOnlyCollection<Tile> Tiles { get; private set; } = new List<Tile>();

        public TileGrid TileGrid { get; private set; }

        /// <summary>
        /// Minimum zoom level supported by the MapTileLayer. Default value is 0.
        /// </summary>
        public int MinZoomLevel
        {
            get { return (int)GetValue(MinZoomLevelProperty); }
            set { SetValue(MinZoomLevelProperty, value); }
        }

        /// <summary>
        /// Maximum zoom level supported by the MapTileLayer. Default value is 18.
        /// </summary>
        public int MaxZoomLevel
        {
            get { return (int)GetValue(MaxZoomLevelProperty); }
            set { SetValue(MaxZoomLevelProperty, value); }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            availableSize = new Size(double.PositiveInfinity, double.PositiveInfinity);

            foreach (var tile in Tiles)
            {
                tile.Image.Measure(availableSize);
            }

            return new Size();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (TileGrid != null)
            {
                foreach (var tile in Tiles)
                {
                    var tileSize = MapProjection.TileSize << (TileGrid.ZoomLevel - tile.ZoomLevel);
                    var x = tileSize * tile.X - MapProjection.TileSize * TileGrid.XMin;
                    var y = tileSize * tile.Y - MapProjection.TileSize * TileGrid.YMin;

                    tile.Image.Width = tileSize;
                    tile.Image.Height = tileSize;
                    tile.Image.Arrange(new Rect(x, y, tileSize, tileSize));
                }
            }

            return finalSize;
        }

        protected override void TileSourcePropertyChanged()
        {
            if (TileGrid != null)
            {
                Tiles = new List<Tile>();
                UpdateTiles();
            }
        }

        protected override void UpdateTileLayer()
        {
            UpdateTimer.Stop();

            if (ParentMap == null || !ParentMap.MapProjection.IsWebMercator)
            {
                TileGrid = null;
                UpdateTiles();
            }
            else if (SetTileGrid())
            {
                SetRenderTransform();
                UpdateTiles();
            }
        }

        protected override void SetRenderTransform()
        {
            var tileGridSize = (double)(1 << TileGrid.ZoomLevel);

            // top/left tile grid corner in map coordinates
            //
            var tileGridOrigin = new Point(
                WebMercatorMapSize * (TileGrid.XMin / tileGridSize - 0.5),
                WebMercatorMapSize * (0.5 - TileGrid.YMin / tileGridSize));

            // top/left tile grid corner in viewport coordinates
            //
            var viewOrigin = ParentMap.MapProjection.ViewportTransform.Transform(tileGridOrigin);

            // tile pixels per viewport unit, 0.5 .. 2
            //
            var tileScale = Math.Pow(2d, ParentMap.ZoomLevel - TileGrid.ZoomLevel);

            ((MatrixTransform)RenderTransform).Matrix = MatrixFactory.Create(tileScale, ParentMap.Heading, viewOrigin);
        }

        private bool SetTileGrid()
        {
            var tileGridZoomLevel = (int)Math.Round(ParentMap.ZoomLevel);
            var tileGridSize = (double)(1 << tileGridZoomLevel);

            // top/left viewport corner in map coordinates
            //
            var tileOrigin = ParentMap.MapProjection.InverseViewportTransform.Transform(new Point());

            // top/left viewport corner in tile grid coordinates
            //
            var tileGridOrigin = new Point(
                tileGridSize * (0.5 + tileOrigin.X / WebMercatorMapSize),
                tileGridSize * (0.5 - tileOrigin.Y / WebMercatorMapSize));

            // transforms viewport bounds to tile grid bounds
            //
            var transform = new MatrixTransform
            {
                Matrix = MatrixFactory.Create(1d / MapProjection.TileSize, -ParentMap.Heading, tileGridOrigin)
            };

            var bounds = transform.TransformBounds(new Rect(0d, 0d, ParentMap.RenderSize.Width, ParentMap.RenderSize.Height));

            var xMin = (int)Math.Floor(bounds.X);
            var yMin = (int)Math.Floor(bounds.Y);
            var xMax = (int)Math.Floor(bounds.X + bounds.Width);
            var yMax = (int)Math.Floor(bounds.Y + bounds.Height);

            if (TileGrid != null &&
                TileGrid.ZoomLevel == tileGridZoomLevel &&
                TileGrid.XMin == xMin && TileGrid.YMin == yMin &&
                TileGrid.XMax == xMax && TileGrid.YMax == yMax)
            {
                return false;
            }

            TileGrid = new TileGrid(tileGridZoomLevel, xMin, yMin, xMax, yMax);

            return true;
        }

        private void UpdateTiles()
        {
            var newTiles = new List<Tile>();

            if (ParentMap != null && TileGrid != null && TileSource != null)
            {
                var maxZoomLevel = Math.Min(TileGrid.ZoomLevel, MaxZoomLevel);

                if (maxZoomLevel >= MinZoomLevel)
                {
                    var minZoomLevel = maxZoomLevel;

                    if (this == ParentMap.MapLayer) // load background tiles
                    {
                        minZoomLevel = Math.Max(TileGrid.ZoomLevel - MaxBackgroundLevels, MinZoomLevel);
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
                                        tile.SetImage(equivalentTile.Image.Source, false); // no fade-in animation
                                    }
                                }

                                newTiles.Add(tile);
                            }
                        }
                    }
                }
            }

            Tiles = newTiles;

            Children.Clear();

            foreach (var tile in Tiles)
            {
                Children.Add(tile.Image);
            }

            TileImageLoader.LoadTilesAsync(Tiles, TileSource, SourceName);
        }
    }
}

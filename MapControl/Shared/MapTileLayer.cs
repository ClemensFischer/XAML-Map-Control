using System;
using System.Collections.Generic;
using System.Linq;
#if WPF
using System.Windows;
using System.Windows.Media;
#elif UWP
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#elif WINUI
using Windows.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
#elif AVALONIA
using Avalonia;
using Avalonia.Media;
#endif

namespace MapControl
{
    /// <summary>
    /// Displays a Web Mercator tile pyramid.
    /// </summary>
    public partial class MapTileLayer : TilePyramidLayer
    {
        private const int TileSize = 256;

        private static readonly Point MapTopLeft = new(-180d * MapProjection.Wgs84MeterPerDegree,
                                                        180d * MapProjection.Wgs84MeterPerDegree);

        public static readonly DependencyProperty MinZoomLevelProperty =
            DependencyPropertyHelper.Register<MapTileLayer, int>(nameof(MinZoomLevel), 0);

        public static readonly DependencyProperty MaxZoomLevelProperty =
            DependencyPropertyHelper.Register<MapTileLayer, int>(nameof(MaxZoomLevel), 19);

        public static readonly DependencyProperty ZoomLevelOffsetProperty =
            DependencyPropertyHelper.Register<MapTileLayer, double>(nameof(ZoomLevelOffset), 0d);

        /// <summary>
        /// A default MapTileLayer using OpenStreetMap data.
        /// </summary>
        public static MapTileLayer OpenStreetMapTileLayer => new()
        {
            TileSource = TileSource.Parse("https://tile.openstreetmap.org/{z}/{x}/{y}.png"),
            SourceName = "OpenStreetMap",
            Description = "© [OpenStreetMap Contributors](http://www.openstreetmap.org/copyright)"
        };

        public MapTileLayer()
        {
            MapPanel.SetRenderTransform(this, new MatrixTransform());
        }

        public override IReadOnlyCollection<string> SupportedCrsIds { get; } = [WebMercatorProjection.DefaultCrsId];

        public TileMatrix TileMatrix { get; private set; }

        public ImageTileList Tiles { get; private set; } = [];

        /// <summary>
        /// Minimum zoom level supported by the MapTileLayer. Default value is 0.
        /// </summary>
        public int MinZoomLevel
        {
            get => (int)GetValue(MinZoomLevelProperty);
            set => SetValue(MinZoomLevelProperty, value);
        }

        /// <summary>
        /// Maximum zoom level supported by the MapTileLayer. Default value is 19.
        /// </summary>
        public int MaxZoomLevel
        {
            get => (int)GetValue(MaxZoomLevelProperty);
            set => SetValue(MaxZoomLevelProperty, value);
        }

        /// <summary>
        /// Optional offset between the map zoom level and the topmost tile zoom level.
        /// Default value is 0.
        /// </summary>
        public double ZoomLevelOffset
        {
            get => (double)GetValue(ZoomLevelOffsetProperty);
            set => SetValue(ZoomLevelOffsetProperty, value);
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
            if (TileMatrix != null)
            {
                foreach (var tile in Tiles)
                {
                    // Arrange tiles relative to XMin/YMin.
                    //
                    var tileSize = TileSize << (TileMatrix.ZoomLevel - tile.ZoomLevel);
                    var x = tileSize * tile.X - TileSize * TileMatrix.XMin;
                    var y = tileSize * tile.Y - TileSize * TileMatrix.YMin;

                    tile.Image.Width = tileSize;
                    tile.Image.Height = tileSize;
                    tile.Image.Arrange(new Rect(x, y, tileSize, tileSize));
                }
            }

            return finalSize;
        }

        protected override void UpdateTileCollection(bool reset)
        {
            if (ParentMap == null || !SupportedCrsIds.Contains(ParentMap.MapProjection.CrsId))
            {
                TileMatrix = null;
                Children.Clear();
                CancelLoadTiles();
            }
            else if (SetTileMatrix() || reset)
            {
                UpdateRenderTransform();
                UpdateTiles(reset);
                BeginLoadTiles(Tiles, SourceName);
            }
        }

        protected override void UpdateRenderTransform()
        {
            if (TileMatrix != null)
            {
                // Tile matrix origin in pixels.
                //
                var tileMatrixOrigin = new Point(TileSize * TileMatrix.XMin, TileSize * TileMatrix.YMin);
                var tileMatrixScale = MapBase.ZoomLevelToScale(TileMatrix.ZoomLevel);

                ((MatrixTransform)RenderTransform).Matrix =
                    ParentMap.ViewTransform.GetTileLayerTransform(tileMatrixScale, MapTopLeft, tileMatrixOrigin);
            }
        }

        private bool SetTileMatrix()
        {
            // Add 0.001 to avoid rounding issues.
            //
            var tileMatrixZoomLevel = (int)Math.Floor(ParentMap.ZoomLevel - ZoomLevelOffset + 0.001);
            var tileMatrixScale = MapBase.ZoomLevelToScale(tileMatrixZoomLevel);

            // Bounds in tile pixels from view size.
            //
            var bounds = ParentMap.ViewTransform.GetTileMatrixBounds(tileMatrixScale, MapTopLeft, ParentMap.ActualWidth, ParentMap.ActualHeight);

            // Tile X and Y bounds.
            //
            var xMin = (int)Math.Floor(bounds.X / TileSize);
            var yMin = (int)Math.Floor(bounds.Y / TileSize);
            var xMax = (int)Math.Floor((bounds.X + bounds.Width) / TileSize);
            var yMax = (int)Math.Floor((bounds.Y + bounds.Height) / TileSize);

            if (TileMatrix != null &&
                TileMatrix.ZoomLevel == tileMatrixZoomLevel &&
                TileMatrix.XMin == xMin && TileMatrix.YMin == yMin &&
                TileMatrix.XMax == xMax && TileMatrix.YMax == yMax)
            {
                return false;
            }

            TileMatrix = new TileMatrix(tileMatrixZoomLevel, xMin, yMin, xMax, yMax);

            return true;
        }

        private void UpdateTiles(bool reset)
        {
            var tiles = new ImageTileList();

            if (TileSource != null && TileMatrix != null)
            {
                if (reset)
                {
                    Tiles.Clear();
                }

                var maxZoomLevel = Math.Min(TileMatrix.ZoomLevel, MaxZoomLevel);

                if (maxZoomLevel >= MinZoomLevel)
                {
                    var minZoomLevel = IsBaseMapLayer
                        ? Math.Max(TileMatrix.ZoomLevel - MaxBackgroundLevels, MinZoomLevel)
                        : maxZoomLevel;

                    for (var zoomLevel = minZoomLevel; zoomLevel <= maxZoomLevel; zoomLevel++)
                    {
                        var tileCount = 1 << zoomLevel; // per row and column

                        // Right-shift divides with rounding down also negative values, https://stackoverflow.com/q/55196178
                        //
                        var shift = TileMatrix.ZoomLevel - zoomLevel;
                        var xMin = TileMatrix.XMin >> shift; // may be < 0
                        var xMax = TileMatrix.XMax >> shift; // may be >= tileCount
                        var yMin = Math.Max(TileMatrix.YMin >> shift, 0);
                        var yMax = Math.Min(TileMatrix.YMax >> shift, tileCount - 1);

                        tiles.FillMatrix(Tiles, zoomLevel, xMin, yMin, xMax, yMax, tileCount);
                    }
                }
            }

            Tiles = tiles;
            Children.Clear();

            foreach (var tile in tiles)
            {
                Children.Add(tile.Image);
            }
        }
    }
}

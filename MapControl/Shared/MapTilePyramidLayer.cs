using System;
using System.Collections.Generic;
using System.Linq;
#if WPF
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
#elif UWP
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;
#elif WINUI
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media;
#elif AVALONIA
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
#endif

namespace MapControl
{
    public abstract class MapTilePyramidLayer : Panel, IMapLayer
    {
        public static readonly DependencyProperty TileSourceProperty =
            DependencyPropertyHelper.Register<MapTilePyramidLayer, TileSource>(nameof(TileSource), null,
                (layer, oldValue, newValue) => layer.Update(true));

        public static readonly DependencyProperty SourceNameProperty =
            DependencyPropertyHelper.Register<MapTilePyramidLayer, string>(nameof(SourceName));

        public static readonly DependencyProperty DescriptionProperty =
            DependencyPropertyHelper.Register<MapTilePyramidLayer, string>(nameof(Description));

        public static readonly DependencyProperty MaxBackgroundLevelsProperty =
            DependencyPropertyHelper.Register<MapTilePyramidLayer, int>(nameof(MaxBackgroundLevels), 5);

        public static readonly DependencyProperty UpdateIntervalProperty =
            DependencyPropertyHelper.Register<MapTilePyramidLayer, TimeSpan>(nameof(UpdateInterval), TimeSpan.FromSeconds(0.2),
                (layer, oldValue, newValue) => layer.updateTimer.Interval = newValue);

        public static readonly DependencyProperty UpdateWhileViewportChangingProperty =
            DependencyPropertyHelper.Register<MapTilePyramidLayer, bool>(nameof(UpdateWhileViewportChanging));

        public static readonly DependencyProperty MapBackgroundProperty =
            DependencyPropertyHelper.Register<MapTilePyramidLayer, Brush>(nameof(MapBackground));

        public static readonly DependencyProperty MapForegroundProperty =
            DependencyPropertyHelper.Register<MapTilePyramidLayer, Brush>(nameof(MapForeground));

        public static readonly DependencyProperty LoadingProgressProperty =
            DependencyPropertyHelper.Register<MapTilePyramidLayer, double>(nameof(LoadingProgress), 1d);

        private readonly Progress<double> loadingProgress;
        private readonly DispatcherTimer updateTimer;
        private ITileImageLoader tileImageLoader;
        private MapBase parentMap;

        protected MapTilePyramidLayer()
        {
            IsHitTestVisible = false;

            loadingProgress = new Progress<double>(p => SetValue(LoadingProgressProperty, p));

            updateTimer = this.CreateTimer(UpdateInterval);
            updateTimer.Tick += (s, e) => Update(false);

            MapPanel.SetRenderTransform(this, new MatrixTransform());
#if WPF
            RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
#elif UWP || WINUI
            ElementCompositionPreview.GetElementVisual(this).BorderMode = CompositionBorderMode.Hard;
            MapPanel.InitMapElement(this);
#endif
        }

        public ITileImageLoader TileImageLoader
        {
            get => tileImageLoader ??= new TileImageLoader();
            set => tileImageLoader = value;
        }

        /// <summary>
        /// Provides map tile URIs or images.
        /// </summary>
        public TileSource TileSource
        {
            get => (TileSource)GetValue(TileSourceProperty);
            set => SetValue(TileSourceProperty, value);
        }

        /// <summary>
        /// Name of the tile source that is used as component of a tile cache key.
        /// Tile images are not cached when SourceName is null or empty.
        /// </summary>
        public string SourceName
        {
            get => (string)GetValue(SourceNameProperty);
            set => SetValue(SourceNameProperty, value);
        }

        /// <summary>
        /// Description of the layer. Used to display copyright information on top of the map.
        /// </summary>
        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        /// <summary>
        /// Maximum number of background tile levels. Default value is 5.
        /// Only effective in a MapTileLayer or WmtsTileLayer that is the MapLayer of its ParentMap.
        /// </summary>
        public int MaxBackgroundLevels
        {
            get => (int)GetValue(MaxBackgroundLevelsProperty);
            set => SetValue(MaxBackgroundLevelsProperty, value);
        }

        /// <summary>
        /// Minimum time interval between tile updates.
        /// </summary>
        public TimeSpan UpdateInterval
        {
            get => (TimeSpan)GetValue(UpdateIntervalProperty);
            set => SetValue(UpdateIntervalProperty, value);
        }

        /// <summary>
        /// Controls if tiles are updated while the viewport is still changing.
        /// </summary>
        public bool UpdateWhileViewportChanging
        {
            get => (bool)GetValue(UpdateWhileViewportChangingProperty);
            set => SetValue(UpdateWhileViewportChangingProperty, value);
        }

        /// <summary>
        /// Optional background brush. Sets MapBase.Background if not null and this layer is the base map layer.
        /// </summary>
        public Brush MapBackground
        {
            get => (Brush)GetValue(MapBackgroundProperty);
            set => SetValue(MapBackgroundProperty, value);
        }

        /// <summary>
        /// Optional foreground brush. Sets MapBase.Foreground if not null and this layer is the base map layer.
        /// </summary>
        public Brush MapForeground
        {
            get => (Brush)GetValue(MapForegroundProperty);
            set => SetValue(MapForegroundProperty, value);
        }

        /// <summary>
        /// Gets the progress of the TileImageLoader as a double value between 0 and 1.
        /// </summary>
        public double LoadingProgress => (double)GetValue(LoadingProgressProperty);

        /// <summary>
        /// Implements IMapElement.ParentMap.
        /// </summary>
        public MapBase ParentMap
        {
            get => parentMap;
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
                }

                updateTimer.Run();
            }
        }

        public abstract IReadOnlyCollection<string> SupportedCrsIds { get; }

        protected bool IsBaseMapLayer => parentMap != null && parentMap.Children.Count > 0 && parentMap.Children[0] == this;

        protected void BeginLoadTiles(IEnumerable<Tile> tiles, string cacheName)
        {
            if (TileSource != null && tiles != null && tiles.Any(tile => tile.IsPending))
            {
                TileImageLoader.BeginLoadTiles(tiles, TileSource, cacheName, loadingProgress);
            }
        }

        protected void CancelLoadTiles()
        {
            TileImageLoader.CancelLoadTiles();

            ClearValue(LoadingProgressProperty);
        }

        protected abstract void SetRenderTransform();

        protected abstract void UpdateTiles(bool resetTiles);

        private void Update(bool resetTiles)
        {
            updateTimer.Stop();

            UpdateTiles(resetTiles);
        }

        private void OnViewportChanged(object sender, ViewportChangedEventArgs e)
        {
            if (e.TransformCenterChanged || e.ProjectionChanged || Children.Count == 0)
            {
                Update(false); // update immediately
            }
            else
            {
                SetRenderTransform();

                updateTimer.Run(!UpdateWhileViewportChanging);
            }
        }
    }
}

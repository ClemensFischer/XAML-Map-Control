using System;
using System.Collections.Generic;
#if WPF
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
using Brush = Avalonia.Media.IBrush;
#endif

namespace MapControl
{
    public abstract class TilePyramidLayer : Panel, IMapLayer
    {
        public static readonly DependencyProperty SourceNameProperty =
            DependencyPropertyHelper.Register<TilePyramidLayer, string>(nameof(SourceName));

        public static readonly DependencyProperty DescriptionProperty =
            DependencyPropertyHelper.Register<TilePyramidLayer, string>(nameof(Description));

        public static readonly DependencyProperty MaxBackgroundLevelsProperty =
            DependencyPropertyHelper.Register<TilePyramidLayer, int>(nameof(MaxBackgroundLevels), 5);

        public static readonly DependencyProperty UpdateIntervalProperty =
            DependencyPropertyHelper.Register<TilePyramidLayer, TimeSpan>(nameof(UpdateInterval), TimeSpan.FromSeconds(0.2),
                (layer, oldValue, newValue) => layer.updateTimer.Interval = newValue);

        public static readonly DependencyProperty UpdateWhileViewportChangingProperty =
            DependencyPropertyHelper.Register<TilePyramidLayer, bool>(nameof(UpdateWhileViewportChanging));

        public static readonly DependencyProperty MapBackgroundProperty =
            DependencyPropertyHelper.Register<TilePyramidLayer, Brush>(nameof(MapBackground));

        public static readonly DependencyProperty MapForegroundProperty =
            DependencyPropertyHelper.Register<TilePyramidLayer, Brush>(nameof(MapForeground));

        public static readonly DependencyProperty LoadingProgressProperty =
            DependencyPropertyHelper.Register<TilePyramidLayer, double>(nameof(LoadingProgress), 1d);

        private readonly Progress<double> loadingProgress;
        private readonly UpdateTimer updateTimer;

        protected TilePyramidLayer()
        {
            IsHitTestVisible = false;

            loadingProgress = new Progress<double>(p => SetValue(LoadingProgressProperty, p));

            updateTimer = new UpdateTimer { Interval = UpdateInterval };
            updateTimer.Tick += (_, _) => UpdateTiles();
#if WPF
            RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
#elif UWP || WINUI
            ElementCompositionPreview.GetElementVisual(this).BorderMode = CompositionBorderMode.Hard;
            MapPanel.InitMapElement(this);
#endif
        }

        public ITileImageLoader TileImageLoader
        {
            get => field ??= new TileImageLoader();
            set;
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
            get;
            set
            {
                if (field != null)
                {
                    field.ViewportChanged -= OnViewportChanged;
                }

                field = value;

                if (field != null)
                {
                    field.ViewportChanged += OnViewportChanged;
                }

                updateTimer.Run();
            }
        }

        public bool IsBaseMapLayer => ParentMap != null && ParentMap.Children.Count > 0 && ParentMap.Children[0] == this;

        public abstract IReadOnlyCollection<string> SupportedCrsIds { get; }

        protected void BeginLoadTiles(IEnumerable<Tile> tiles, TileSource tileSource, string cacheName)
        {
            TileImageLoader.BeginLoadTiles(tiles, tileSource, cacheName, loadingProgress);
        }

        protected void CancelLoadTiles()
        {
            TileImageLoader.CancelLoadTiles();
            ClearValue(LoadingProgressProperty);
        }

        protected abstract void UpdateRenderTransform();

        protected abstract void UpdateTileCollection();

        private void UpdateTiles()
        {
            updateTimer.Stop();
            UpdateTileCollection();
        }

        private void OnViewportChanged(object sender, ViewportChangedEventArgs e)
        {
            if (e.TransformCenterChanged || e.ProjectionChanged || Children.Count == 0)
            {
                UpdateTiles(); // update immediately
            }
            else
            {
                UpdateRenderTransform();
                updateTimer.Run(!UpdateWhileViewportChanging);
            }
        }
    }
}

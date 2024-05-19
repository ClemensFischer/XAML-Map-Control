// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MapControl
{
    public abstract class MapTileLayerBase : Panel
    {
        public static readonly StyledProperty<TileSource> TileSourceProperty
            = AvaloniaProperty.Register<MapTileLayerBase, TileSource>(nameof(TileSource));

        public static readonly StyledProperty<string> SourceNameProperty
            = AvaloniaProperty.Register<MapTileLayerBase, string>(nameof(SourceName));

        public static readonly StyledProperty<string> DescriptionProperty
            = AvaloniaProperty.Register<MapTileLayerBase, string>(nameof(Description));

        public static readonly StyledProperty<int> MaxBackgroundLevelsProperty
            = AvaloniaProperty.Register<MapTileLayerBase, int>(nameof(MaxBackgroundLevels), 5);

        public static readonly StyledProperty<TimeSpan> UpdateIntervalProperty
            = AvaloniaProperty.Register<MapTileLayerBase, TimeSpan>(nameof(AvaloniaProperty), TimeSpan.FromSeconds(0.2));

        public static readonly StyledProperty<bool> UpdateWhileViewportChangingProperty
            = AvaloniaProperty.Register<MapTileLayerBase, bool>(nameof(UpdateWhileViewportChanging));

        public static readonly StyledProperty<IBrush> MapBackgroundProperty
            = AvaloniaProperty.Register<MapTileLayerBase, IBrush>(nameof(MapBackground));

        public static readonly StyledProperty<IBrush> MapForegroundProperty
            = AvaloniaProperty.Register<MapTileLayerBase, IBrush>(nameof(MapForeground));

        public static readonly DirectProperty<MapTileLayerBase, double> LoadingProgressProperty
            = AvaloniaProperty.RegisterDirect<MapTileLayerBase, double>(nameof(LoadingProgress), layer => layer.loadingProgressValue);

        private readonly DispatcherTimer updateTimer;
        private readonly Progress<double> loadingProgress;
        private double loadingProgressValue;
        private ITileImageLoader tileImageLoader;

        static MapTileLayerBase()
        {
            MapPanel.ParentMapProperty.Changed.AddClassHandler<MapTileLayerBase, MapBase>(
                (layer, args) => layer.OnParentMapPropertyChanged(args.NewValue.Value));

            TileSourceProperty.Changed.AddClassHandler<MapTileLayerBase, TileSource>(
                async (layer, args) => await layer.Update(true));

            UpdateIntervalProperty.Changed.AddClassHandler<MapTileLayerBase, TimeSpan>(
                (layer, args) => layer.updateTimer.Interval = args.NewValue.Value);
        }

        protected MapTileLayerBase()
        {
            RenderTransform = new MatrixTransform();
            RenderTransformOrigin = new RelativePoint();

            loadingProgress = new Progress<double>(p => SetAndRaise(LoadingProgressProperty, ref loadingProgressValue, p));

            updateTimer = this.CreateTimer(UpdateInterval);
            updateTimer.Tick += async (s, e) => await Update(false);
        }

        public MapBase ParentMap { get; private set; }

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
            get => GetValue(TileSourceProperty);
            set => SetValue(TileSourceProperty, value);
        }

        /// <summary>
        /// Name of the TileSource. Used as component of a tile cache key.
        /// </summary>
        public string SourceName
        {
            get => GetValue(SourceNameProperty);
            set => SetValue(SourceNameProperty, value);
        }

        /// <summary>
        /// Description of the layer. Used to display copyright information on top of the map.
        /// </summary>
        public string Description
        {
            get => GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        /// <summary>
        /// Maximum number of background tile levels. Default value is 5.
        /// Only effective in a MapTileLayer or WmtsTileLayer that is the MapLayer of its ParentMap.
        /// </summary>
        public int MaxBackgroundLevels
        {
            get => GetValue(MaxBackgroundLevelsProperty);
            set => SetValue(MaxBackgroundLevelsProperty, value);
        }

        /// <summary>
        /// Minimum time interval between tile updates.
        /// </summary>
        public TimeSpan UpdateInterval
        {
            get => GetValue(UpdateIntervalProperty);
            set => SetValue(UpdateIntervalProperty, value);
        }

        /// <summary>
        /// Controls if tiles are updated while the viewport is still changing.
        /// </summary>
        public bool UpdateWhileViewportChanging
        {
            get => GetValue(UpdateWhileViewportChangingProperty);
            set => SetValue(UpdateWhileViewportChangingProperty, value);
        }

        /// <summary>
        /// Optional background brush. Sets MapBase.Background if not null and this layer is the base map layer.
        /// </summary>
        public IBrush MapBackground
        {
            get => GetValue(MapBackgroundProperty);
            set => SetValue(MapBackgroundProperty, value);
        }

        /// <summary>
        /// Optional foreground brush. Sets MapBase.Foreground if not null and this layer is the base map layer.
        /// </summary>
        public IBrush MapForeground
        {
            get => GetValue(MapForegroundProperty);
            set => SetValue(MapForegroundProperty, value);
        }

        /// <summary>
        /// Gets the progress of the TileImageLoader as a double value between 0 and 1.
        /// </summary>
        public double LoadingProgress => loadingProgressValue;

        protected bool IsBaseMapLayer
        {
            get
            {
                var parentMap = MapPanel.GetParentMap(this);

                return parentMap != null && parentMap.Children.Count > 0 && parentMap.Children[0] == this;
            }
        }

        protected abstract void SetRenderTransform();

        protected abstract Task UpdateTileLayer(bool tileSourceChanged);

        protected Task LoadTiles(IEnumerable<Tile> tiles, string cacheName)
        {
            return TileImageLoader.LoadTilesAsync(tiles, TileSource, cacheName, loadingProgress);
        }

        private Task Update(bool tileSourceChanged)
        {
            updateTimer.Stop();

            return UpdateTileLayer(tileSourceChanged);
        }

        private async void OnViewportChanged(object sender, ViewportChangedEventArgs e)
        {
            if (e.TransformCenterChanged || e.ProjectionChanged || Children.Count == 0)
            {
                await Update(false); // update immediately
            }
            else
            {
                SetRenderTransform();

                updateTimer.Run(!UpdateWhileViewportChanging);
            }
        }

        private void OnParentMapPropertyChanged(MapBase parentMap)
        {
            if (ParentMap != null)
            {
                ParentMap.ViewportChanged -= OnViewportChanged;
            }

            ParentMap = parentMap;

            if (ParentMap != null)
            {
                ParentMap.ViewportChanged += OnViewportChanged;
            }

            updateTimer.Run();
        }
    }
}

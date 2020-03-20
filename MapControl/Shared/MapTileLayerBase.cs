// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2020 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
#endif

namespace MapControl
{
    public interface ITileImageLoader
    {
        void LoadTilesAsync(IEnumerable<Tile> tiles, TileSource tileSource, string sourceName);
    }

    public abstract class MapTileLayerBase : Panel, IMapLayer
    {
        public static readonly DependencyProperty TileSourceProperty = DependencyProperty.Register(
            nameof(TileSource), typeof(TileSource), typeof(MapTileLayerBase),
            new PropertyMetadata(null, (o, e) => ((MapTileLayerBase)o).TileSourcePropertyChanged()));

        public static readonly DependencyProperty SourceNameProperty = DependencyProperty.Register(
            nameof(SourceName), typeof(string), typeof(MapTileLayerBase), new PropertyMetadata(null));

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
            nameof(Description), typeof(string), typeof(MapTileLayerBase), new PropertyMetadata(null));

        public static readonly DependencyProperty MaxBackgroundLevelsProperty = DependencyProperty.Register(
            nameof(MaxBackgroundLevels), typeof(int), typeof(MapTileLayerBase), new PropertyMetadata(8));

        public static readonly DependencyProperty UpdateIntervalProperty = DependencyProperty.Register(
            nameof(UpdateInterval), typeof(TimeSpan), typeof(MapTileLayerBase),
            new PropertyMetadata(TimeSpan.FromSeconds(0.2), (o, e) => ((MapTileLayerBase)o).UpdateTimer.Interval = (TimeSpan)e.NewValue));

        public static readonly DependencyProperty UpdateWhileViewportChangingProperty = DependencyProperty.Register(
            nameof(UpdateWhileViewportChanging), typeof(bool), typeof(MapTileLayerBase), new PropertyMetadata(false));

        public static readonly DependencyProperty MapBackgroundProperty = DependencyProperty.Register(
            nameof(MapBackground), typeof(Brush), typeof(MapTileLayerBase), new PropertyMetadata(null));

        public static readonly DependencyProperty MapForegroundProperty = DependencyProperty.Register(
            nameof(MapForeground), typeof(Brush), typeof(MapTileLayerBase), new PropertyMetadata(null));

        private MapBase parentMap;

        public MapTileLayerBase()
            : this(new TileImageLoader())
        {
        }

        public MapTileLayerBase(ITileImageLoader tileImageLoader)
        {
            IsHitTestVisible = false;
            RenderTransform = new MatrixTransform();
            TileImageLoader = tileImageLoader;

            UpdateTimer = new DispatcherTimer { Interval = UpdateInterval };
            UpdateTimer.Tick += (s, e) => UpdateTileLayer();

            MapPanel.InitMapElement(this);
        }

        public ITileImageLoader TileImageLoader { get; }

        public DispatcherTimer UpdateTimer { get; }

        /// <summary>
        /// Provides map tile URIs or images.
        /// </summary>
        public TileSource TileSource
        {
            get { return (TileSource)GetValue(TileSourceProperty); }
            set { SetValue(TileSourceProperty, value); }
        }

        /// <summary>
        /// Name of the TileSource. Used as component of a tile cache key.
        /// </summary>
        public string SourceName
        {
            get { return (string)GetValue(SourceNameProperty); }
            set { SetValue(SourceNameProperty, value); }
        }

        /// <summary>
        /// Description of the tile layer. Used to display copyright information on top of the map.
        /// </summary>
        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        /// <summary>
        /// Maximum number of background tile levels. Default value is 8.
        /// Only effective in a MapTileLayer or WmtsTileLayer that is the MapLayer of its ParentMap.
        /// </summary>
        public int MaxBackgroundLevels
        {
            get { return (int)GetValue(MaxBackgroundLevelsProperty); }
            set { SetValue(MaxBackgroundLevelsProperty, value); }
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
        /// Controls if tiles are updated while the viewport is still changing.
        /// </summary>
        public bool UpdateWhileViewportChanging
        {
            get { return (bool)GetValue(UpdateWhileViewportChangingProperty); }
            set { SetValue(UpdateWhileViewportChangingProperty, value); }
        }

        /// <summary>
        /// Optional background brush. Sets MapBase.Background if not null and this layer is the base map layer.
        /// </summary>
        public Brush MapBackground
        {
            get { return (Brush)GetValue(MapBackgroundProperty); }
            set { SetValue(MapBackgroundProperty, value); }
        }

        /// <summary>
        /// Optional foreground brush. Sets MapBase.Foreground if not null and this layer is the base map layer.
        /// </summary>
        public Brush MapForeground
        {
            get { return (Brush)GetValue(MapForegroundProperty); }
            set { SetValue(MapForegroundProperty, value); }
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
                }

                UpdateTileLayer();
            }
        }

        private void OnViewportChanged(object sender, ViewportChangedEventArgs e)
        {
            if (Children.Count == 0 || e.ProjectionChanged || Math.Abs(e.LongitudeOffset) > 180d)
            {
                UpdateTileLayer(); // update immediately when projection has changed or center has moved across 180° longitude
            }
            else
            {
                SetRenderTransform();

                if (UpdateTimer.IsEnabled && !UpdateWhileViewportChanging)
                {
                    UpdateTimer.Stop(); // restart
                }

                if (!UpdateTimer.IsEnabled)
                {
                    UpdateTimer.Start();
                }
            }
        }

        protected abstract void UpdateTileLayer();

        protected abstract void SetRenderTransform();

        protected abstract void TileSourcePropertyChanged();
    }
}

﻿// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
#if WPF
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
#elif UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
#elif WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using DispatcherTimer = Microsoft.UI.Dispatching.DispatcherQueueTimer;
#elif AVALONIA
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Brush = Avalonia.Media.IBrush;
using DependencyProperty = Avalonia.AvaloniaProperty;
#endif

namespace MapControl
{
    public abstract partial class MapTileLayerBase : Panel, IMapLayer
    {
        public static readonly DependencyProperty TileSourceProperty =
            DependencyPropertyHelper.Register<MapTileLayerBase, TileSource>(nameof(TileSource), null,
                async (layer, oldValue, newValue) => await layer.Update(true));

        public static readonly DependencyProperty SourceNameProperty =
            DependencyPropertyHelper.Register<MapTileLayerBase, string>(nameof(SourceName));

        public static readonly DependencyProperty DescriptionProperty =
            DependencyPropertyHelper.Register<MapTileLayerBase, string>(nameof(Description));

        public static readonly DependencyProperty MaxBackgroundLevelsProperty =
            DependencyPropertyHelper.Register<MapTileLayerBase, int>(nameof(MaxBackgroundLevels), 5);

        public static readonly DependencyProperty UpdateIntervalProperty =
            DependencyPropertyHelper.Register<MapTileLayerBase, TimeSpan>(nameof(UpdateInterval), TimeSpan.FromSeconds(0.2));

        public static readonly DependencyProperty UpdateWhileViewportChangingProperty =
            DependencyPropertyHelper.Register<MapTileLayerBase, bool>(nameof(UpdateWhileViewportChanging));

        public static readonly DependencyProperty MapBackgroundProperty =
            DependencyPropertyHelper.Register<MapTileLayerBase, Brush>(nameof(MapBackground));

        public static readonly DependencyProperty MapForegroundProperty =
            DependencyPropertyHelper.Register<MapTileLayerBase, Brush>(nameof(MapForeground));

        public static readonly DependencyProperty LoadingProgressProperty =
            DependencyPropertyHelper.Register<MapTileLayerBase, double>(nameof(LoadingProgress), 1d);

        private readonly Progress<double> loadingProgress;
        private readonly DispatcherTimer updateTimer;
        private ITileImageLoader tileImageLoader;
        private MapBase parentMap;

        protected MapTileLayerBase()
        {
            IsHitTestVisible = false;

            loadingProgress = new Progress<double>(p => SetValue(LoadingProgressProperty, p));

            updateTimer = this.CreateTimer(UpdateInterval);
            updateTimer.Tick += async (s, e) => await Update(false);

#if UWP || WINUI
            MapPanel.InitMapElement(this);
#endif
            MapPanel.SetRenderTransform(this, new MatrixTransform());
        }

        public ITileImageLoader TileImageLoader
        {
            get => tileImageLoader ?? (tileImageLoader = new TileImageLoader());
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
        /// Name of the TileSource. Used as component of a tile cache key.
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

        protected bool IsBaseMapLayer => parentMap != null && parentMap.Children.Count > 0 && parentMap.Children[0] == this;

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
    }
}

// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2023 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
#if WINUI
using Windows.Foundation;
using Microsoft.UI.Xaml;
#elif UWP
using Windows.Foundation;
using Windows.UI.Xaml;
#else
using System.Windows;
#endif

namespace MapControl
{
    /// <summary>
    /// Displays map tiles from a Web Map Tile Service (WMTS).
    /// </summary>
    public class WmtsTileLayer : MapTileLayerBase
    {
        public static readonly DependencyProperty CapabilitiesUriProperty = DependencyProperty.Register(
            nameof(CapabilitiesUri), typeof(Uri), typeof(WmtsTileLayer),
            new PropertyMetadata(null, (o, e) => ((WmtsTileLayer)o).TileMatrixSets.Clear()));

        public static readonly DependencyProperty LayerProperty = DependencyProperty.Register(
            nameof(Layer), typeof(string), typeof(WmtsTileLayer), new PropertyMetadata(null));

        public static readonly DependencyProperty PreferredTileMatrixSetsProperty = DependencyProperty.Register(
            nameof(PreferredTileMatrixSets), typeof(string[]), typeof(WmtsTileLayer), new PropertyMetadata(null));

        public WmtsTileLayer()
            : this(new TileImageLoader())
        {
        }

        public WmtsTileLayer(ITileImageLoader tileImageLoader)
            : base(tileImageLoader)
        {
            Loaded += OnLoaded;
        }

        /// <summary>
        /// The Uri of a XML file or web response that contains the service capabilities.
        /// </summary>
        public Uri CapabilitiesUri
        {
            get => (Uri)GetValue(CapabilitiesUriProperty);
            set => SetValue(CapabilitiesUriProperty, value);
        }

        /// <summary>
        /// The Identifier of the Layer that should be displayed. If not set, the first Layer is displayed.
        /// </summary>
        public string Layer
        {
            get => (string)GetValue(LayerProperty);
            set => SetValue(LayerProperty, value);
        }

        /// <summary>
        /// In case there are TileMatrixSets with identical SupportedCRS values,
        /// the ones with Identifiers contained in this collection take precedence.
        /// </summary>
        public string[] PreferredTileMatrixSets
        {
            get => (string[])GetValue(PreferredTileMatrixSetsProperty);
            set => SetValue(PreferredTileMatrixSetsProperty, value);
        }

        public IEnumerable<WmtsTileMatrixLayer> ChildLayers => Children.Cast<WmtsTileMatrixLayer>();

        public Dictionary<string, WmtsTileMatrixSet> TileMatrixSets { get; } = new Dictionary<string, WmtsTileMatrixSet>();

        protected override Size MeasureOverride(Size availableSize)
        {
            foreach (var layer in ChildLayers)
            {
                layer.Measure(availableSize);
            }

            return new Size();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (var layer in ChildLayers)
            {
                layer.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
            }

            return finalSize;
        }

        protected override Task UpdateTileLayer()
        {
            if (ParentMap == null ||
                !TileMatrixSets.TryGetValue(ParentMap.MapProjection.CrsId, out WmtsTileMatrixSet tileMatrixSet))
            {
                Children.Clear();

                return LoadTiles(null); // stop TileImageLoader
            }

            if (UpdateChildLayers(tileMatrixSet))
            {
                return LoadTiles(tileMatrixSet);
            }

            return Task.CompletedTask;
        }

        protected override void SetRenderTransform()
        {
            foreach (var layer in ChildLayers)
            {
                layer.SetRenderTransform(ParentMap.ViewTransform);
            }
        }

        private bool UpdateChildLayers(WmtsTileMatrixSet tileMatrixSet)
        {
            // Multiply scale by 1.001 to avoid rounding issues.
            //
            var maxScale = 1.001 * ParentMap.ViewTransform.Scale;

            // Show all WmtsTileMatrix layers with Scale <= maxScale, at least the first layer.
            //
            var currentMatrixes = tileMatrixSet.TileMatrixes
                .Where((matrix, i) => i == 0 || matrix.Scale <= maxScale)
                .ToList();

            if (!IsBaseMapLayer)
            {
                // Show only the last layer.
                //
                currentMatrixes = currentMatrixes.Skip(currentMatrixes.Count - 1).ToList();
            }
            else if (currentMatrixes.Count > MaxBackgroundLevels + 1)
            {
                // Show not more than MaxBackgroundLevels + 1 layers.
                //
                currentMatrixes = currentMatrixes.Skip(currentMatrixes.Count - MaxBackgroundLevels - 1).ToList();
            }

            var currentLayers = ChildLayers.Where(layer => currentMatrixes.Contains(layer.WmtsTileMatrix)).ToList();
            var tilesChanged = false;

            Children.Clear();

            foreach (var tileMatrix in currentMatrixes)
            {
                var layer = currentLayers.FirstOrDefault(l => l.WmtsTileMatrix == tileMatrix) ??
                            new WmtsTileMatrixLayer(tileMatrix, tileMatrixSet.TileMatrixes.IndexOf(tileMatrix));

                if (layer.UpdateTiles(ParentMap.ViewTransform, ParentMap.RenderSize))
                {
                    tilesChanged = true;
                }

                layer.SetRenderTransform(ParentMap.ViewTransform);

                Children.Add(layer);
            }

            return tilesChanged;
        }

        private Task LoadTiles(WmtsTileMatrixSet tileMatrixSet)
        {
            var cacheName = SourceName;

            if (tileMatrixSet != null && TileSource is WmtsTileSource tileSource)
            {
                tileSource.TileMatrixSet = tileMatrixSet;

                if (!string.IsNullOrEmpty(cacheName))
                {
                    if (!string.IsNullOrEmpty(Layer))
                    {
                        cacheName += "/" + Layer.Replace(':', '_');
                    }

                    cacheName += "/" + tileMatrixSet.Identifier.Replace(':', '_');
                }
            }

            var tiles = ChildLayers.SelectMany(layer => layer.Tiles);

            return TileImageLoader.LoadTiles(tiles, TileSource, cacheName);
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;

            if (TileMatrixSets.Count == 0 && CapabilitiesUri != null)
            {
                try
                {
                    var capabilities = await WmtsCapabilities.ReadCapabilitiesAsync(CapabilitiesUri, Layer);

                    foreach (var tileMatrixSet in capabilities.TileMatrixSets
                        .Where(s => !TileMatrixSets.ContainsKey(s.SupportedCrs) ||
                                    PreferredTileMatrixSets != null && PreferredTileMatrixSets.Contains(s.Identifier)))
                    {
                        TileMatrixSets[tileMatrixSet.SupportedCrs] = tileMatrixSet;
                    }

                    Layer = capabilities.Layer;
                    TileSource = capabilities.TileSource;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"WmtsTileLayer: {CapabilitiesUri}: {ex.Message}");
                }
            }
        }
    }
}

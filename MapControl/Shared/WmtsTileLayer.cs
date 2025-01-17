// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
#if WPF
using System.Windows;
#elif UWP
using Windows.Foundation;
using Windows.UI.Xaml;
#elif WINUI
using Windows.Foundation;
using Microsoft.UI.Xaml;
#endif

namespace MapControl
{
    /// <summary>
    /// Displays map tiles from a Web Map Tile Service (WMTS).
    /// </summary>
    public class WmtsTileLayer : MapTileLayerBase
    {
        public static readonly DependencyProperty CapabilitiesUriProperty =
            DependencyPropertyHelper.Register<WmtsTileLayer, Uri>(nameof(CapabilitiesUri), null,
                (layer, oldValue, newValue) => layer.TileMatrixSets.Clear());

        public static readonly DependencyProperty LayerProperty =
            DependencyPropertyHelper.Register<WmtsTileLayer, string>(nameof(Layer));

        public static readonly DependencyProperty PreferredTileMatrixSetsProperty =
            DependencyPropertyHelper.Register<WmtsTileLayer, string[]>(nameof(PreferredTileMatrixSets));

        public WmtsTileLayer()
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

        protected virtual WmtsTileSource CreateTileSource(string uriTemplate) => new WmtsTileSource { UriTemplate = uriTemplate };

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

        protected override async Task UpdateTileLayer(bool tileSourceChanged)
        {
            // tileSourceChanged is ignored here because it is always false.

            if (ParentMap == null ||
                !TileMatrixSets.TryGetValue(ParentMap.MapProjection.CrsId, out WmtsTileMatrixSet tileMatrixSet))
            {
                Children.Clear();

                await LoadTiles(null, null); // stop TileImageLoader
            }
            else if (UpdateChildLayers(tileMatrixSet))
            {
                ((WmtsTileSource)TileSource).TileMatrixSet = tileMatrixSet;

                var cacheName = SourceName;

                if (!string.IsNullOrEmpty(cacheName))
                {
                    if (!string.IsNullOrEmpty(Layer))
                    {
                        cacheName += "/" + Layer.Replace(':', '_');
                    }

                    cacheName += "/" + tileMatrixSet.Identifier.Replace(':', '_');
                }

                var tiles = ChildLayers.SelectMany(layer => layer.Tiles);

                await LoadTiles(tiles, cacheName);
            }
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

                if (layer.UpdateTiles(ParentMap.ViewTransform, ParentMap.ActualWidth, ParentMap.ActualHeight))
                {
                    tilesChanged = true;
                }

                layer.SetRenderTransform(ParentMap.ViewTransform);

                Children.Add(layer);
            }

            return tilesChanged;
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
                    TileSource = CreateTileSource(capabilities.UrlTemplate);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{nameof(WmtsTileLayer)}: {CapabilitiesUri}: {ex.Message}");
                }
            }
        }
    }
}

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
#if WPF
using System.Windows;
#elif UWP
using Windows.Foundation;
using Windows.UI.Xaml;
#elif WINUI
using Windows.Foundation;
using Microsoft.UI.Xaml;
#elif AVALONIA
using Avalonia;
using Avalonia.Interactivity;
#endif

namespace MapControl
{
    /// <summary>
    /// Displays map tiles from a Web Map Tile Service (WMTS).
    /// </summary>
    public class WmtsTileLayer : TilePyramidLayer
    {
        private static ILogger logger;
        private static ILogger Logger => logger ??= ImageLoader.LoggerFactory?.CreateLogger(typeof(WmtsTileLayer));

        public static readonly DependencyProperty CapabilitiesUriProperty =
            DependencyPropertyHelper.Register<WmtsTileLayer, Uri>(nameof(CapabilitiesUri));

        public static readonly DependencyProperty TileUriTemplateProperty =
            DependencyPropertyHelper.Register<WmtsTileLayer, string>(nameof(TileUriTemplate));

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
        /// The Uri template string used for the UriTemplate property of WmtsTileSource instances.
        /// Usually set internally from WmtsCapabilities requested by a Loaded event handler.
        /// </summary>
        public string TileUriTemplate
        {
            get => (string)GetValue(TileUriTemplateProperty);
            set => SetValue(TileUriTemplateProperty, value);
        }

        /// <summary>
        /// The Identifier of the Layer that should be displayed.
        /// If not set, the value is defined by WmtsCapabilities.
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

        /// <summary>
        /// Gets a dictionary of all tile matrix sets supported by a WMTS, with their CRS as dictionary key.
        /// </summary>
        public Dictionary<string, WmtsTileMatrixSet> TileMatrixSets { get; } = [];

        /// <summary>
        /// Gets a collection of all CRSs supported by a WMTS.
        /// </summary>
        public override IReadOnlyCollection<string> SupportedCrsIds => TileMatrixSets.Keys;

        protected IEnumerable<WmtsTileMatrixLayer> ChildLayers => Children.Cast<WmtsTileMatrixLayer>();

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

        protected override void UpdateRenderTransform()
        {
            foreach (var layer in ChildLayers)
            {
                layer.UpdateRenderTransform(ParentMap.ViewTransform);
            }
        }

        protected override void UpdateTileCollection()
        {
            if (ParentMap == null ||
                !TileMatrixSets.TryGetValue(ParentMap.MapProjection.CrsId, out WmtsTileMatrixSet tileMatrixSet))
            {
                Children.Clear();
                CancelLoadTiles();
            }
            else if (UpdateChildLayers(tileMatrixSet))
            {
                var tileSource = new WmtsTileSource
                {
                    UriTemplate = TileUriTemplate,
                    TileMatrixSet = tileMatrixSet
                };

                var cacheName = SourceName;

                if (!string.IsNullOrEmpty(cacheName))
                {
                    if (!string.IsNullOrEmpty(Layer))
                    {
                        cacheName += "/" + Layer.Replace(':', '_');
                    }

                    if (!string.IsNullOrEmpty(tileMatrixSet.Identifier))
                    {
                        cacheName += "/" + tileMatrixSet.Identifier.Replace(':', '_');
                    }
                }

                BeginLoadTiles(ChildLayers.SelectMany(layer => layer.Tiles), tileSource, cacheName);
            }
        }

        private bool UpdateChildLayers(WmtsTileMatrixSet tileMatrixSet)
        {
            // Multiply scale by 1.001 to avoid floating point precision issues
            // and get all WmtsTileMatrixes with Scale <= maxScale.
            //
            var maxScale = 1.001 * ParentMap.ViewTransform.Scale;
            var tileMatrixes = tileMatrixSet.TileMatrixes.Where(matrix => matrix.Scale <= maxScale).ToList();

            if (tileMatrixes.Count == 0)
            {
                Children.Clear();
                return false;
            }

            var maxLayers = Math.Max(MaxBackgroundLevels, 0) + 1;

            if (!IsBaseMapLayer)
            {
                // Show only the last layer.
                //
                tileMatrixes = tileMatrixes.GetRange(tileMatrixes.Count - 1, 1);
            }
            else if (tileMatrixes.Count > maxLayers)
            {
                // Show not more than MaxBackgroundLevels + 1 layers.
                //
                tileMatrixes = tileMatrixes.GetRange(tileMatrixes.Count - maxLayers, maxLayers);
            }

            // Get reusable layers.
            //
            var layers = ChildLayers.Where(layer => tileMatrixes.Contains(layer.WmtsTileMatrix)).ToList();
            var tilesChanged = false;

            Children.Clear();

            foreach (var tileMatrix in tileMatrixes)
            {
                // Pass index of tileMatrix in tileMatrixSet.TileMatrixes as zoom level to WmtsTileMatrixLayer ctor.
                //
                var layer = layers.FirstOrDefault(layer => layer.WmtsTileMatrix == tileMatrix) ??
                    new WmtsTileMatrixLayer(tileMatrix, tileMatrixSet.TileMatrixes.IndexOf(tileMatrix));

                if (layer.UpdateTiles(ParentMap.ViewTransform, ParentMap.ActualWidth, ParentMap.ActualHeight))
                {
                    tilesChanged = true;
                }

                layer.UpdateRenderTransform(ParentMap.ViewTransform);

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

                    foreach (var tms in capabilities.TileMatrixSets
                        .Where(tms => !TileMatrixSets.ContainsKey(tms.SupportedCrsId) ||
                                      PreferredTileMatrixSets != null &&
                                      PreferredTileMatrixSets.Contains(tms.Identifier)))
                    {
                        TileMatrixSets[tms.SupportedCrsId] = tms;
                    }

                    Layer = capabilities.Layer;
                    TileUriTemplate = capabilities.UriTemplate;

                    UpdateTileCollection();
                }
                catch (Exception ex)
                {
                    Logger?.LogError(ex, "Failed reading capabilities from {uri}", CapabilitiesUri);
                }
            }
        }
    }
}

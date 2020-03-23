// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2020 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
#if WINDOWS_UWP
using Windows.Foundation;
using Windows.UI.Xaml;
#else
using System.Windows;
#endif

namespace MapControl
{
    public class WmtsTileLayer : MapTileLayerBase
    {
        public static readonly DependencyProperty CapabilitiesUriProperty = DependencyProperty.Register(
            nameof(CapabilitiesUri), typeof(Uri), typeof(WmtsTileLayer),
            new PropertyMetadata(null, (o, e) => ((WmtsTileLayer)o).TileMatrixSets.Clear()));

        public static readonly DependencyProperty LayerIdentifierProperty = DependencyProperty.Register(
            nameof(LayerIdentifier), typeof(string), typeof(WmtsTileLayer), new PropertyMetadata(null));

        public WmtsTileLayer()
            : this(new TileImageLoader())
        {
        }

        public WmtsTileLayer(ITileImageLoader tileImageLoader)
            : base(tileImageLoader)
        {
            IsHitTestVisible = false;

            Loaded += OnLoaded;
        }

        public Uri CapabilitiesUri
        {
            get { return (Uri)GetValue(CapabilitiesUriProperty); }
            set { SetValue(CapabilitiesUriProperty, value); }
        }

        public string LayerIdentifier
        {
            get { return (string)GetValue(LayerIdentifierProperty); }
            set { SetValue(LayerIdentifierProperty, value); }
        }

        public IEnumerable<WmtsTileMatrixLayer> ChildLayers
        {
            get { return Children.Cast<WmtsTileMatrixLayer>(); }
        }

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

        protected override void TileSourcePropertyChanged()
        {
        }

        protected override void UpdateTileLayer()
        {
            UpdateTimer.Stop();

            WmtsTileMatrixSet tileMatrixSet;

            if (ParentMap == null ||
                !TileMatrixSets.TryGetValue(ParentMap.MapProjection.CrsId, out tileMatrixSet))
            {
                Children.Clear();
                UpdateTiles(null);
            }
            else if (UpdateChildLayers(tileMatrixSet))
            {
                SetRenderTransform();
                UpdateTiles(tileMatrixSet);
            }
        }

        protected override void SetRenderTransform()
        {
            foreach (var layer in ChildLayers)
            {
                layer.SetRenderTransform(ParentMap.MapProjection);
            }
        }

        private bool UpdateChildLayers(WmtsTileMatrixSet tileMatrixSet)
        {
            var layersChanged = false;
            var maxScale = 1.001 * ParentMap.MapProjection.ViewportScale; // avoid rounding issues

            // show all TileMatrix layers with Scale <= maxScale, at least the first layer
            //
            var currentMatrixes = tileMatrixSet.TileMatrixes
                .Where((matrix, i) => i == 0 || matrix.Scale <= maxScale)
                .ToList();

            if (this != ParentMap.MapLayer) // do not load background tiles
            {
                currentMatrixes = currentMatrixes.Skip(currentMatrixes.Count - 1).ToList(); // last element only
            }
            else if (currentMatrixes.Count > MaxBackgroundLevels + 1)
            {
                currentMatrixes = currentMatrixes.Skip(currentMatrixes.Count - MaxBackgroundLevels - 1).ToList();
            }

            var currentLayers = ChildLayers.Where(layer => currentMatrixes.Contains(layer.TileMatrix)).ToList();

            Children.Clear();

            foreach (var tileMatrix in currentMatrixes)
            {
                var layer = currentLayers.FirstOrDefault(l => l.TileMatrix == tileMatrix);

                if (layer == null)
                {
                    layer = new WmtsTileMatrixLayer(tileMatrix, tileMatrixSet.TileMatrixes.IndexOf(tileMatrix));
                    layersChanged = true;
                }

                if (layer.SetBounds(ParentMap.MapProjection, ParentMap.RenderSize))
                {
                    layersChanged = true;
                }

                Children.Add(layer);
            }

            return layersChanged;
        }

        private void UpdateTiles(WmtsTileMatrixSet tileMatrixSet)
        {
            var tiles = new List<Tile>();

            foreach (var layer in ChildLayers)
            {
                layer.UpdateTiles();

                tiles.AddRange(layer.Tiles);
            }

            var tileSource = TileSource as WmtsTileSource;
            var sourceName = SourceName;

            if (tileSource != null && tileMatrixSet != null)
            {
                tileSource.TileMatrixSet = tileMatrixSet;

                if (sourceName != null)
                {
                    sourceName += "/" + tileMatrixSet.Identifier;
                }
            }

            TileImageLoader.LoadTilesAsync(tiles, TileSource, sourceName);
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (TileMatrixSets.Count == 0 && CapabilitiesUri != null)
            {
                try
                {
                    if (CapabilitiesUri.IsAbsoluteUri && (CapabilitiesUri.Scheme == "http" || CapabilitiesUri.Scheme == "https"))
                    {
                        using (var stream = await ImageLoader.HttpClient.GetStreamAsync(CapabilitiesUri))
                        {
                            ReadCapabilities(XDocument.Load(stream).Root);
                        }
                    }
                    else
                    {
                        ReadCapabilities(XDocument.Load(CapabilitiesUri.ToString()).Root);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("WmtsTileLayer: {0}: {1}", CapabilitiesUri, ex.Message);
                }
            }
        }

        private void ReadCapabilities(XElement capabilitiesElement)
        {
            TileMatrixSets.Clear();

            XNamespace ns = capabilitiesElement.Name.Namespace;
            XNamespace ows = "http://www.opengis.net/ows/1.1";

            var contentsElement = capabilitiesElement.Element(ns + "Contents");

            if (contentsElement == null)
            {
                throw new ArgumentException("Contents element not found.");
            }

            XElement layerElement;

            if (!string.IsNullOrEmpty(LayerIdentifier))
            {
                layerElement = contentsElement.Descendants(ns + "Layer")
                    .FirstOrDefault(e => e.Element(ows + "Identifier")?.Value == LayerIdentifier);

                if (layerElement == null)
                {
                    throw new ArgumentException("Layer element \"" + LayerIdentifier + "\" not found.");
                }
            }
            else
            {
                layerElement = capabilitiesElement.Descendants(ns + "Layer").FirstOrDefault();

                if (layerElement == null)
                {
                    throw new ArgumentException("No Layer element found.");
                }

                LayerIdentifier = layerElement.Element(ows + "Identifier")?.Value ?? "";
            }

            var urlTemplate = layerElement.Element(ns + "ResourceURL")?.Attribute("template")?.Value;

            if (string.IsNullOrEmpty(urlTemplate))
            {
                throw new ArgumentException("No valid ResourceURL element found in Layer \"" + LayerIdentifier + "\".");
            }

            var styleElement = layerElement.Descendants(ns + "Style")
                .FirstOrDefault(e => e.Attribute("isDefault")?.Value == "true");

            if (styleElement == null)
            {
                styleElement = layerElement.Descendants(ns + "Style").FirstOrDefault();
            }

            var style = styleElement?.Element(ows + "Identifier")?.Value;

            if (string.IsNullOrEmpty(style))
            {
                throw new ArgumentException("No valid Style element found in Layer \"" + LayerIdentifier + "\".");
            }

            var tileMatrixSetIds = layerElement
                .Descendants(ns + "TileMatrixSetLink")
                .Select(e => e.Element(ns + "TileMatrixSet")?.Value)
                .Where(id => !string.IsNullOrEmpty(id))
                .ToList();

            foreach (var tileMatrixSetId in tileMatrixSetIds)
            {
                var tileMatrixSetElement = capabilitiesElement.Descendants(ns + "TileMatrixSet")
                    .FirstOrDefault(e => e.Element(ows + "Identifier")?.Value == tileMatrixSetId);

                if (tileMatrixSetElement == null)
                {
                    throw new ArgumentException("Linked TileMatrixSet element not found in Layer \"" + LayerIdentifier + "\".");
                }

                var tileMatrixSet = WmtsTileMatrixSet.Create(tileMatrixSetElement);

                TileMatrixSets.Add(tileMatrixSet.SupportedCrs, tileMatrixSet);
            }

            TileSource = new WmtsTileSource(urlTemplate.Replace("{Style}", style));

            UpdateTileLayer();
        }
    }
}

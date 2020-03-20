// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2020 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
            new PropertyMetadata(null, (o, e) => ((WmtsTileLayer)o).TileMatrixSet = null));

        public static readonly DependencyProperty LayerIdentifierProperty = DependencyProperty.Register(
            nameof(LayerIdentifier), typeof(string), typeof(WmtsTileLayer), new PropertyMetadata(null));

        public static readonly DependencyProperty TileMatrixSetProperty = DependencyProperty.Register(
            nameof(TileMatrixSet), typeof(WmtsTileMatrixSet), typeof(WmtsTileLayer),
            new PropertyMetadata(null, (o, e) => ((WmtsTileLayer)o).UpdateTileLayer()));

        public WmtsTileLayer()
            : this(new TileImageLoader())
        {
        }

        public WmtsTileLayer(ITileImageLoader tileImageLoader)
            : base(tileImageLoader)
        {
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

        public WmtsTileMatrixSet TileMatrixSet
        {
            get { return (WmtsTileMatrixSet)GetValue(TileMatrixSetProperty); }
            set { SetValue(TileMatrixSetProperty, value); }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            foreach (var layer in Children.Cast<WmtsTileMatrixLayer>())
            {
                layer.Measure(availableSize);
            }

            return new Size();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (var layer in Children.Cast<WmtsTileMatrixLayer>())
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

            if (ParentMap == null ||
                TileMatrixSet == null ||
                ParentMap.MapProjection.CrsId != TileMatrixSet.SupportedCrs)
            {
                Children.Clear();
                UpdateTiles();
            }
            else if (UpdateChildLayers())
            {
                SetRenderTransform();
                UpdateTiles();
            }
        }

        protected override void SetRenderTransform()
        {
            foreach (var layer in Children.Cast<WmtsTileMatrixLayer>())
            {
                layer.SetRenderTransform(ParentMap.MapProjection, ParentMap.Heading);
            }
        }

        private bool UpdateChildLayers()
        {
            bool layersChanged = false;

            if (TileMatrixSet != null)
            {
                // show all TileMatrix layers with Scale <= ViewportScale, or at least the first layer
                //
                var currentMatrixes = TileMatrixSet.TileMatrixes
                    .Where((matrix, i) => i == 0 || matrix.Scale <= ParentMap.MapProjection.ViewportScale)
                    .ToList();

                if (this != ParentMap.MapLayer) // do not load background tiles
                {
                    currentMatrixes = currentMatrixes.Skip(currentMatrixes.Count - 1).ToList(); // last element only
                }
                else if (currentMatrixes.Count > MaxBackgroundLevels + 1)
                {
                    currentMatrixes = currentMatrixes.Skip(currentMatrixes.Count - MaxBackgroundLevels - 1).ToList();
                }

                var currentLayers = Children.Cast<WmtsTileMatrixLayer>()
                    .Where(layer => currentMatrixes.Contains(layer.TileMatrix))
                    .ToList();

                Children.Clear();

                foreach (var tileMatrix in currentMatrixes)
                {
                    var layer = currentLayers.FirstOrDefault(l => l.TileMatrix == tileMatrix);

                    if (layer == null)
                    {
                        layer = new WmtsTileMatrixLayer(tileMatrix, TileMatrixSet.TileMatrixes.IndexOf(tileMatrix));
                        layersChanged = true;
                    }

                    if (layer.SetBounds(ParentMap.MapProjection, ParentMap.Heading, ParentMap.RenderSize))
                    {
                        layersChanged = true;
                    }

                    Children.Add(layer);
                }
            }

            return layersChanged;
        }

        private void UpdateTiles()
        {
            var tiles = new List<Tile>();

            foreach (var layer in Children.Cast<WmtsTileMatrixLayer>())
            {
                layer.UpdateTiles();

                tiles.AddRange(layer.Tiles);
            }

            TileImageLoader.LoadTilesAsync(tiles, TileSource, SourceName);
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (TileMatrixSet == null && CapabilitiesUri != null)
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

        private void ReadCapabilities(XElement capabilities)
        {
            var ns = capabilities.Name.Namespace;
            var contentsElement = capabilities.Element(ns + "Contents");

            if (contentsElement == null)
            {
                throw new ArgumentException("Contents element not found.");
            }

            XNamespace ows = "http://www.opengis.net/ows/1.1";
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
                layerElement = capabilities.Descendants(ns + "Layer").FirstOrDefault();

                if (layerElement == null)
                {
                    throw new ArgumentException("No Layer element found.");
                }

                LayerIdentifier = layerElement.Element(ows + "Identifier")?.Value ?? "";
            }

            var tileMatrixSetId = layerElement.Element(ns + "TileMatrixSetLink")?.Element(ns + "TileMatrixSet")?.Value;

            if (string.IsNullOrEmpty(tileMatrixSetId))
            {
                throw new ArgumentException("TileMatrixSetLink element not found.");
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
                throw new ArgumentException("Style element not found.");
            }

            var urlTemplate = layerElement.Element(ns + "ResourceURL")?.Attribute("template")?.Value;

            if (string.IsNullOrEmpty(urlTemplate))
            {
                throw new ArgumentException("ResourceURL element (or template attribute) not found in Layer \"" + LayerIdentifier + "\".");
            }

            var tileMatrixSetElement = capabilities.Descendants(ns + "TileMatrixSet")
                .FirstOrDefault(e => e.Element(ows + "Identifier")?.Value == tileMatrixSetId);

            if (tileMatrixSetElement == null)
            {
                throw new ArgumentException("Linked TileMatrixSet element not found in Layer \"" + LayerIdentifier + "\".");
            }

            var supportedCrs = tileMatrixSetElement.Element(ows + "SupportedCRS")?.Value;

            if (string.IsNullOrEmpty(supportedCrs))
            {
                throw new ArgumentException("ows:SupportedCRS element not found in TileMatrixSet \"" + tileMatrixSetId + "\".");
            }

            var tileMatrixes = new List<WmtsTileMatrix>();

            foreach (var tileMatrix in tileMatrixSetElement.Descendants(ns + "TileMatrix"))
            {
                var tileMatrixId = tileMatrix.Element(ows + "Identifier")?.Value;

                if (string.IsNullOrEmpty(tileMatrixId))
                {
                    throw new ArgumentException("ows:Identifier element not found in TileMatrix.");
                }

                string[] topLeftCornerStrings;
                double scaleDenominator, top, left;
                int tileWidth, tileHeight, matrixWidth, matrixHeight;

                var valueString = tileMatrix.Element(ns + "ScaleDenominator")?.Value;

                if (string.IsNullOrEmpty(valueString) ||
                    !double.TryParse(valueString, NumberStyles.Float, CultureInfo.InvariantCulture, out scaleDenominator))
                {
                    throw new ArgumentException("ScaleDenominator element not found in TileMatrix \"" + tileMatrixId + "\".");
                }

                valueString = tileMatrix.Element(ns + "TopLeftCorner")?.Value;

                if (string.IsNullOrEmpty(valueString) ||
                    (topLeftCornerStrings = valueString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)).Length < 2 ||
                    !double.TryParse(topLeftCornerStrings[0], NumberStyles.Float, CultureInfo.InvariantCulture, out left) ||
                    !double.TryParse(topLeftCornerStrings[1], NumberStyles.Float, CultureInfo.InvariantCulture, out top))
                {
                    throw new ArgumentException("TopLeftCorner element not found in TileMatrix \"" + tileMatrixId + "\".");
                }

                valueString = tileMatrix.Element(ns + "TileWidth")?.Value;

                if (string.IsNullOrEmpty(valueString) || !int.TryParse(valueString, out tileWidth))
                {
                    throw new ArgumentException("TileWidth element not found in TileMatrix \"" + tileMatrixId + "\".");
                }

                valueString = tileMatrix.Element(ns + "TileHeight")?.Value;

                if (string.IsNullOrEmpty(valueString) || !int.TryParse(valueString, out tileHeight))
                {
                    throw new ArgumentException("TileHeight element not found in TileMatrix \"" + tileMatrixId + "\".");
                }

                valueString = tileMatrix.Element(ns + "MatrixWidth")?.Value;

                if (string.IsNullOrEmpty(valueString) || !int.TryParse(valueString, out matrixWidth))
                {
                    throw new ArgumentException("MatrixWidth element not found in TileMatrix \"" + tileMatrixId + "\".");
                }

                valueString = tileMatrix.Element(ns + "MatrixHeight")?.Value;

                if (string.IsNullOrEmpty(valueString) || !int.TryParse(valueString, out matrixHeight))
                {
                    throw new ArgumentException("MatrixHeight element not found in TileMatrix \"" + tileMatrixId + "\".");
                }

                tileMatrixes.Add(new WmtsTileMatrix(
                    tileMatrixId, scaleDenominator, new Point(left, top), tileWidth, tileHeight, matrixWidth, matrixHeight));
            }

            if (tileMatrixes.Count <= 0)
            {
                throw new ArgumentException("No TileMatrix elements found in TileMatrixSet \"" + tileMatrixSetId + "\".");
            }

            var tileMatrixSet = new WmtsTileMatrixSet(tileMatrixSetId, supportedCrs, tileMatrixes);

            urlTemplate = urlTemplate
                .Replace("{Style}", style)
                .Replace("{TileMatrixSet}", tileMatrixSet.Identifier);

            TileSource = new WmtsTileSource(urlTemplate, tileMatrixSet.TileMatrixes);

            TileMatrixSet = tileMatrixSet; // calls UpdateTileLayer()
        }
    }
}

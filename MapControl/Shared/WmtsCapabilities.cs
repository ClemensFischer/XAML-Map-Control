// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using System.Threading.Tasks;
#if !WINUI && !WINDOWS_UWP
using System.Windows;
#endif

namespace MapControl
{
    public class WmtsCapabilities
    {
        public string LayerIdentifier { get; private set; }
        public WmtsTileSource TileSource { get; private set; }
        public List<WmtsTileMatrixSet> TileMatrixSets { get; private set; }

        public static async Task<WmtsCapabilities> ReadCapabilities(Uri capabilitiesUri, string layerIdentifier)
        {
            WmtsCapabilities capabilities;

            if (capabilitiesUri.IsAbsoluteUri && (capabilitiesUri.Scheme == "http" || capabilitiesUri.Scheme == "https"))
            {
                using (var stream = await ImageLoader.HttpClient.GetStreamAsync(capabilitiesUri))
                {
                    capabilities = ReadCapabilities(XDocument.Load(stream).Root, layerIdentifier, capabilitiesUri.ToString());
                }
            }
            else
            {
                capabilities = ReadCapabilities(XDocument.Load(capabilitiesUri.ToString()).Root, layerIdentifier, null);
            }

            return capabilities;
        }

        public static WmtsCapabilities ReadCapabilities(XElement capabilitiesElement, string layerIdentifier, string capabilitiesUrl)
        {
            XNamespace ns = capabilitiesElement.Name.Namespace;
            XNamespace ows = "http://www.opengis.net/ows/1.1";

            var contentsElement = capabilitiesElement.Element(ns + "Contents");

            if (contentsElement == null)
            {
                throw new ArgumentException("Contents element not found.");
            }

            XElement layerElement;

            if (!string.IsNullOrEmpty(layerIdentifier))
            {
                layerElement = contentsElement.Descendants(ns + "Layer")
                    .FirstOrDefault(e => e.Element(ows + "Identifier")?.Value == layerIdentifier);

                if (layerElement == null)
                {
                    throw new ArgumentException("Layer element \"" + layerIdentifier + "\" not found.");
                }
            }
            else
            {
                layerElement = capabilitiesElement.Descendants(ns + "Layer").FirstOrDefault();

                if (layerElement == null)
                {
                    throw new ArgumentException("No Layer element found.");
                }

                layerIdentifier = layerElement.Element(ows + "Identifier")?.Value ?? "";
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
                throw new ArgumentException("No valid Style element found in Layer \"" + layerIdentifier + "\".");
            }

            var urlTemplate = ReadUrlTemplate(layerElement, layerIdentifier, style, capabilitiesUrl);

            var tileMatrixSetIds = layerElement
                .Descendants(ns + "TileMatrixSetLink")
                .Select(e => e.Element(ns + "TileMatrixSet")?.Value)
                .Where(id => !string.IsNullOrEmpty(id))
                .ToList();

            var tileMatrixSets = new List<WmtsTileMatrixSet>();

            foreach (var tileMatrixSetId in tileMatrixSetIds)
            {
                var tileMatrixSetElement = capabilitiesElement.Descendants(ns + "TileMatrixSet")
                    .FirstOrDefault(e => e.Element(ows + "Identifier")?.Value == tileMatrixSetId);

                if (tileMatrixSetElement == null)
                {
                    throw new ArgumentException("Linked TileMatrixSet element not found in Layer \"" + layerIdentifier + "\".");
                }

                tileMatrixSets.Add(ReadTileMatrixSet(tileMatrixSetElement));
            }

            return new WmtsCapabilities
            {
                LayerIdentifier = layerIdentifier,
                TileSource = new WmtsTileSource { UriFormat = urlTemplate },
                TileMatrixSets = tileMatrixSets
            };
        }

        public static string ReadUrlTemplate(XElement layerElement, string layerIdentifier, string style, string capabilitiesUrl)
        {
            XNamespace ns = layerElement.Name.Namespace;
            const string formatPng = "image/png";
            const string formatJpg = "image/jpeg";
            string urlTemplate = null;

            var resourceUrls = layerElement.Descendants(ns + "ResourceURL")
                .ToLookup(e => e.Attribute("format")?.Value ?? "", e => e.Attribute("template")?.Value ?? "");

            if (resourceUrls.Any())
            {
                var urlTemplates
                    = resourceUrls.Contains(formatPng) ? resourceUrls[formatPng]
                    : resourceUrls.Contains(formatJpg) ? resourceUrls[formatJpg]
                    : resourceUrls.First();

                urlTemplate = urlTemplates.First().Replace("{Style}", style);
            }
            else if (capabilitiesUrl != null)
            {
                var requestIndex = capabilitiesUrl.IndexOf("Request=GetCapabilities", StringComparison.OrdinalIgnoreCase);

                if (requestIndex > 0)
                {
                    var formats = layerElement.Descendants(ns + "Format").Select(e => e.Value).ToList();

                    if (formats.Count == 0)
                    {
                        throw new ArgumentException("No Format element found in Layer \"" + layerIdentifier + "\".");
                    }

                    var format
                        = formats.Contains(formatPng) ? formatPng
                        : formats.Contains(formatJpg) ? formatJpg
                        : formats[0];

                    urlTemplate = capabilitiesUrl.Substring(0, requestIndex)
                        + "Request=GetTile"
                        + capabilitiesUrl.Substring(requestIndex + 23)
                        + "&Version=1.0.0"
                        + "&Layer=" + layerIdentifier
                        + "&Format=" + format
                        + "&Style=" + style
                        + "&TileMatrixSet={TileMatrixSet}"
                        + "&TileMatrix={TileMatrix}"
                        + "&TileCol={TileCol}"
                        + "&TileRow={TileRow}";
                }
            }

            if (string.IsNullOrEmpty(urlTemplate))
            {
                throw new ArgumentException("No valid ResourceURL element found in Layer \"" + layerIdentifier + "\".");
            }

            return urlTemplate;
        }

        public static WmtsTileMatrixSet ReadTileMatrixSet(XElement tileMatrixSetElement)
        {
            XNamespace ns = tileMatrixSetElement.Name.Namespace;
            XNamespace ows = "http://www.opengis.net/ows/1.1";

            var identifier = tileMatrixSetElement.Element(ows + "Identifier")?.Value;

            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("No ows:Identifier element found in TileMatrixSet.");
            }

            var supportedCrs = tileMatrixSetElement.Element(ows + "SupportedCRS")?.Value;

            if (string.IsNullOrEmpty(supportedCrs))
            {
                throw new ArgumentException("No ows:SupportedCRS element found in TileMatrixSet \"" + identifier + "\".");
            }

            var tileMatrixes = new List<WmtsTileMatrix>();

            foreach (var tileMatrixElement in tileMatrixSetElement.Descendants(ns + "TileMatrix"))
            {
                tileMatrixes.Add(ReadTileMatrix(tileMatrixElement, supportedCrs));
            }

            if (tileMatrixes.Count <= 0)
            {
                throw new ArgumentException("No TileMatrix elements found in TileMatrixSet \"" + identifier + "\".");
            }

            return new WmtsTileMatrixSet(identifier, supportedCrs, tileMatrixes);
        }

        public static WmtsTileMatrix ReadTileMatrix(XElement tileMatrixElement, string supportedCrs)
        {
            XNamespace ns = tileMatrixElement.Name.Namespace;
            XNamespace ows = "http://www.opengis.net/ows/1.1";

            var identifier = tileMatrixElement.Element(ows + "Identifier")?.Value;

            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("No ows:Identifier element found in TileMatrix.");
            }

            var valueString = tileMatrixElement.Element(ns + "ScaleDenominator")?.Value;

            if (string.IsNullOrEmpty(valueString) ||
                !double.TryParse(valueString, NumberStyles.Float, CultureInfo.InvariantCulture, out double scaleDenominator))
            {
                throw new ArgumentException("No ScaleDenominator element found in TileMatrix \"" + identifier + "\".");
            }

            valueString = tileMatrixElement.Element(ns + "TopLeftCorner")?.Value;
            string[] topLeftCornerStrings;

            if (string.IsNullOrEmpty(valueString) ||
                (topLeftCornerStrings = valueString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)).Length < 2 ||
                !double.TryParse(topLeftCornerStrings[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double left) ||
                !double.TryParse(topLeftCornerStrings[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double top))
            {
                throw new ArgumentException("No TopLeftCorner element found in TileMatrix \"" + identifier + "\".");
            }

            valueString = tileMatrixElement.Element(ns + "TileWidth")?.Value;

            if (string.IsNullOrEmpty(valueString) || !int.TryParse(valueString, out int tileWidth))
            {
                throw new ArgumentException("No TileWidth element found in TileMatrix \"" + identifier + "\".");
            }

            valueString = tileMatrixElement.Element(ns + "TileHeight")?.Value;

            if (string.IsNullOrEmpty(valueString) || !int.TryParse(valueString, out int tileHeight))
            {
                throw new ArgumentException("No TileHeight element found in TileMatrix \"" + identifier + "\".");
            }

            valueString = tileMatrixElement.Element(ns + "MatrixWidth")?.Value;

            if (string.IsNullOrEmpty(valueString) || !int.TryParse(valueString, out int matrixWidth))
            {
                throw new ArgumentException("No MatrixWidth element found in TileMatrix \"" + identifier + "\".");
            }

            valueString = tileMatrixElement.Element(ns + "MatrixHeight")?.Value;

            if (string.IsNullOrEmpty(valueString) || !int.TryParse(valueString, out int matrixHeight))
            {
                throw new ArgumentException("No MatrixHeight element found in TileMatrix \"" + identifier + "\".");
            }

            var topLeft = supportedCrs == "EPSG:4326"
                ? new Point(MapProjection.Wgs84MetersPerDegree * top, MapProjection.Wgs84MetersPerDegree * left)
                : new Point(left, top);

            return new WmtsTileMatrix(
                identifier, scaleDenominator, topLeft, tileWidth, tileHeight, matrixWidth, matrixHeight);
        }
    }
}

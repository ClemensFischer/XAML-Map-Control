// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
#if !WINUI && !UWP
using System.Windows;
#endif

namespace MapControl
{
    /// <summary>
    /// For reference see https://www.ogc.org/standards/wmts, 07-057r7_Web_Map_Tile_Service_Standard.pdf
    /// </summary>
    public class WmtsCapabilities
    {
        private static readonly XNamespace ows = "http://www.opengis.net/ows/1.1";
        private static readonly XNamespace wmts = "http://www.opengis.net/wmts/1.0";
        private static readonly XNamespace xlink = "http://www.w3.org/1999/xlink";

        public string LayerIdentifier { get; private set; }
        public WmtsTileSource TileSource { get; private set; }
        public List<WmtsTileMatrixSet> TileMatrixSets { get; private set; }

        public static async Task<WmtsCapabilities> ReadCapabilitiesAsync(Uri capabilitiesUri, string layerIdentifier)
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
            var contentsElement = capabilitiesElement.Element(wmts + "Contents");

            if (contentsElement == null)
            {
                throw new ArgumentException("Contents element not found.");
            }

            XElement layerElement;

            if (!string.IsNullOrEmpty(layerIdentifier))
            {
                layerElement = contentsElement
                    .Elements(wmts + "Layer")
                    .FirstOrDefault(layer => layer.Element(ows + "Identifier")?.Value == layerIdentifier);

                if (layerElement == null)
                {
                    throw new ArgumentException($"Layer element \"{layerIdentifier}\" not found.");
                }
            }
            else
            {
                layerElement = contentsElement
                    .Elements(wmts + "Layer")
                    .FirstOrDefault();

                if (layerElement == null)
                {
                    throw new ArgumentException("No Layer element found.");
                }

                layerIdentifier = layerElement.Element(ows + "Identifier")?.Value ?? "";
            }

            var styleElement = layerElement
                .Elements(wmts + "Style")
                .FirstOrDefault(style => style.Attribute("isDefault")?.Value == "true");

            if (styleElement == null)
            {
                styleElement = layerElement
                    .Elements(wmts + "Style")
                    .FirstOrDefault();
            }

            var styleIdentifier = styleElement?.Element(ows + "Identifier")?.Value;

            if (string.IsNullOrEmpty(styleIdentifier))
            {
                throw new ArgumentException($"No Style element found in Layer \"{layerIdentifier}\".");
            }

            var urlTemplate = ReadUrlTemplate(capabilitiesElement, layerElement, layerIdentifier, styleIdentifier, capabilitiesUrl);

            var tileMatrixSetIds = layerElement
                .Elements(wmts + "TileMatrixSetLink")
                .Select(tmsl => tmsl.Element(wmts + "TileMatrixSet")?.Value)
                .Where(val => !string.IsNullOrEmpty(val));

            var tileMatrixSets = new List<WmtsTileMatrixSet>();

            foreach (var tileMatrixSetId in tileMatrixSetIds)
            {
                var tileMatrixSetElement = contentsElement
                    .Elements(wmts + "TileMatrixSet")
                    .FirstOrDefault(tms => tms.Element(ows + "Identifier")?.Value == tileMatrixSetId);

                if (tileMatrixSetElement == null)
                {
                    throw new ArgumentException($"Linked TileMatrixSet element not found in Layer \"{layerIdentifier}\".");
                }

                tileMatrixSets.Add(ReadTileMatrixSet(tileMatrixSetElement));
            }

            return new WmtsCapabilities
            {
                LayerIdentifier = layerIdentifier,
                TileSource = new WmtsTileSource { UriTemplate = urlTemplate },
                TileMatrixSets = tileMatrixSets
            };
        }

        public static string ReadUrlTemplate(XElement capabilitiesElement, XElement layerElement, string layerIdentifier, string styleIdentifier, string capabilitiesUrl)
        {
            const string formatPng = "image/png";
            const string formatJpg = "image/jpeg";
            string urlTemplate = null;

            var resourceUrls = layerElement
                .Elements(wmts + "ResourceURL")
                .Where(res => res.Attribute("resourceType")?.Value == "tile" &&
                              res.Attribute("format")?.Value != null &&
                              res.Attribute("template")?.Value != null)
                .ToLookup(res => res.Attribute("format").Value,
                          res => res.Attribute("template").Value);

            if (resourceUrls.Any())
            {
                var urlTemplates = resourceUrls.Contains(formatPng) ? resourceUrls[formatPng]
                                 : resourceUrls.Contains(formatJpg) ? resourceUrls[formatJpg]
                                 : resourceUrls.First();

                urlTemplate = urlTemplates.First().Replace("{Style}", styleIdentifier);
            }
            else
            {
                urlTemplate = capabilitiesElement
                    .Elements(ows + "OperationsMetadata")
                    .Elements(ows + "Operation")
                    .Where(op => op.Attribute("name")?.Value == "GetTile")
                    .Elements(ows + "DCP")
                    .Elements(ows + "HTTP")
                    .Elements(ows + "Get")
                    .Where(get => get.Elements(ows + "Constraint")
                                     .Any(con => con.Attribute("name")?.Value == "GetEncoding" &&
                                                 con.Element(ows + "AllowedValues")?.Element(ows + "Value")?.Value == "KVP"))
                    .Select(get => get.Attribute(xlink + "href")?.Value)
                    .Where(href => !string.IsNullOrEmpty(href))
                    .Select(href => href.Split('?')[0])
                    .FirstOrDefault();

                if (urlTemplate == null &&
                    capabilitiesUrl != null &&
                    capabilitiesUrl.IndexOf("Request=GetCapabilities", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    urlTemplate = capabilitiesUrl.Split('?')[0];
                }

                if (urlTemplate != null)
                {
                    var formats = layerElement
                        .Elements(wmts + "Format")
                        .Select(fmt => fmt.Value);

                    var format = formats.Contains(formatPng) ? formatPng
                               : formats.Contains(formatJpg) ? formatJpg
                               : formats.FirstOrDefault();

                    if (string.IsNullOrEmpty(format))
                    {
                        format = formatPng;
                    }

                    urlTemplate += "?Service=WMTS"
                        + "&Request=GetTile"
                        + "&Version=1.0.0"
                        + "&Layer=" + layerIdentifier
                        + "&Style=" + styleIdentifier
                        + "&Format=" + format
                        + "&TileMatrixSet={TileMatrixSet}"
                        + "&TileMatrix={TileMatrix}"
                        + "&TileRow={TileRow}"
                        + "&TileCol={TileCol}";
                }
            }

            if (string.IsNullOrEmpty(urlTemplate))
            {
                throw new ArgumentException($"No ResourceURL element in Layer \"{layerIdentifier}\" and no GetTile KVP Operation Metadata found.");
            }

            return urlTemplate;
        }

        public static WmtsTileMatrixSet ReadTileMatrixSet(XElement tileMatrixSetElement)
        {
            var identifier = tileMatrixSetElement.Element(ows + "Identifier")?.Value;

            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("No Identifier element found in TileMatrixSet.");
            }

            var supportedCrs = tileMatrixSetElement.Element(ows + "SupportedCRS")?.Value;

            if (string.IsNullOrEmpty(supportedCrs))
            {
                throw new ArgumentException($"No SupportedCRS element found in TileMatrixSet \"{identifier}\".");
            }

            const string urnPrefix = "urn:ogc:def:crs:EPSG:";

            if (supportedCrs.StartsWith(urnPrefix)) // e.g. "urn:ogc:def:crs:EPSG:6.18:3857")
            {
                var crs = supportedCrs.Substring(urnPrefix.Length).Split(':');

                if (crs.Length > 1)
                {
                    supportedCrs = "EPSG:" + crs[1];
                }
            }

            var tileMatrixes = new List<WmtsTileMatrix>();

            foreach (var tileMatrixElement in tileMatrixSetElement.Elements(wmts + "TileMatrix"))
            {
                tileMatrixes.Add(ReadTileMatrix(tileMatrixElement, supportedCrs));
            }

            if (tileMatrixes.Count <= 0)
            {
                throw new ArgumentException($"No TileMatrix elements found in TileMatrixSet \"{identifier}\".");
            }

            return new WmtsTileMatrixSet(identifier, supportedCrs, tileMatrixes);
        }

        public static WmtsTileMatrix ReadTileMatrix(XElement tileMatrixElement, string supportedCrs)
        {
            var identifier = tileMatrixElement.Element(ows + "Identifier")?.Value;

            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("No Identifier element found in TileMatrix.");
            }

            var valueString = tileMatrixElement.Element(wmts + "ScaleDenominator")?.Value;

            if (string.IsNullOrEmpty(valueString) ||
                !double.TryParse(valueString, NumberStyles.Float, CultureInfo.InvariantCulture, out double scaleDenominator))
            {
                throw new ArgumentException($"No ScaleDenominator element found in TileMatrix \"{identifier}\".");
            }

            valueString = tileMatrixElement.Element(wmts + "TopLeftCorner")?.Value;
            string[] topLeftCornerStrings;

            if (string.IsNullOrEmpty(valueString) ||
                (topLeftCornerStrings = valueString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)).Length < 2 ||
                !double.TryParse(topLeftCornerStrings[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double left) ||
                !double.TryParse(topLeftCornerStrings[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double top))
            {
                throw new ArgumentException($"No TopLeftCorner element found in TileMatrix \"{identifier}\".");
            }

            valueString = tileMatrixElement.Element(wmts + "TileWidth")?.Value;

            if (string.IsNullOrEmpty(valueString) || !int.TryParse(valueString, out int tileWidth))
            {
                throw new ArgumentException($"No TileWidth element found in TileMatrix \"{identifier}\".");
            }

            valueString = tileMatrixElement.Element(wmts + "TileHeight")?.Value;

            if (string.IsNullOrEmpty(valueString) || !int.TryParse(valueString, out int tileHeight))
            {
                throw new ArgumentException($"No TileHeight element found in TileMatrix \"{identifier}\".");
            }

            valueString = tileMatrixElement.Element(wmts + "MatrixWidth")?.Value;

            if (string.IsNullOrEmpty(valueString) || !int.TryParse(valueString, out int matrixWidth))
            {
                throw new ArgumentException($"No MatrixWidth element found in TileMatrix \"{identifier}\".");
            }

            valueString = tileMatrixElement.Element(wmts + "MatrixHeight")?.Value;

            if (string.IsNullOrEmpty(valueString) || !int.TryParse(valueString, out int matrixHeight))
            {
                throw new ArgumentException($"No MatrixHeight element found in TileMatrix \"{identifier}\".");
            }

            var topLeft = supportedCrs == "EPSG:4326"
                ? new Point(MapProjection.Wgs84MeterPerDegree * top, MapProjection.Wgs84MeterPerDegree * left)
                : new Point(left, top);

            return new WmtsTileMatrix(
                identifier, scaleDenominator, topLeft, tileWidth, tileHeight, matrixWidth, matrixHeight);
        }
    }
}

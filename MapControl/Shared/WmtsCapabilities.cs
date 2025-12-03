using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
#if WPF
using System.Windows;
#elif AVALONIA
using Avalonia;
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

        public string Layer { get; private set; }
        public List<WmtsTileMatrixSet> TileMatrixSets { get; private set; }

        public static async Task<WmtsCapabilities> ReadCapabilitiesAsync(Uri uri, string layer)
        {
            Stream xmlStream;
            string defaultUri = null;

            if (!uri.IsAbsoluteUri)
            {
                xmlStream = File.OpenRead(uri.OriginalString);
            }
            else if (uri.IsFile)
            {
                xmlStream = File.OpenRead(uri.LocalPath);
            }
            else if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            {
                defaultUri = uri.OriginalString.Split('?')[0];

                xmlStream = await ImageLoader.HttpClient.GetStreamAsync(uri);
            }
            else
            {
                throw new ArgumentException($"Invalid Capabilities Uri: {uri}");
            }

            using var stream = xmlStream;

            var element = await XDocument.LoadRootElementAsync(stream);

            return ReadCapabilities(element, layer, defaultUri);
        }

        public static WmtsCapabilities ReadCapabilities(XElement capabilitiesElement, string layer, string defaultUri)
        {
            var contentsElement = capabilitiesElement.Element(wmts + "Contents") ??
                throw new ArgumentException("Contents element not found.");

            XElement layerElement;

            if (!string.IsNullOrEmpty(layer))
            {
                layerElement = contentsElement
                    .Elements(wmts + "Layer")
                    .FirstOrDefault(l => l.Element(ows + "Identifier")?.Value == layer) ??
                    throw new ArgumentException($"Layer element \"{layer}\" not found.");
            }
            else
            {
                layerElement = contentsElement
                    .Elements(wmts + "Layer")
                    .FirstOrDefault() ??
                    throw new ArgumentException("No Layer element found.");

                layer = layerElement.Element(ows + "Identifier")?.Value ?? "";
            }

            var styleElement = layerElement
                .Elements(wmts + "Style")
                .FirstOrDefault(s => s.Attribute("isDefault")?.Value == "true") ??
                layerElement
                .Elements(wmts + "Style")
                .FirstOrDefault();

            var style = styleElement?.Element(ows + "Identifier")?.Value ?? "";

            var uriTemplate = ReadUriTemplate(capabilitiesElement, layerElement, layer, style, defaultUri);

            var tileMatrixSetIds = layerElement
                .Elements(wmts + "TileMatrixSetLink")
                .Select(l => l.Element(wmts + "TileMatrixSet")?.Value)
                .Where(v => !string.IsNullOrEmpty(v));

            var tileMatrixSets = new List<WmtsTileMatrixSet>();

            foreach (var tileMatrixSetId in tileMatrixSetIds)
            {
                var tileMatrixSetElement = contentsElement
                    .Elements(wmts + "TileMatrixSet")
                    .FirstOrDefault(s => s.Element(ows + "Identifier")?.Value == tileMatrixSetId) ??
                    throw new ArgumentException($"Linked TileMatrixSet element not found in Layer \"{layer}\".");

                tileMatrixSets.Add(ReadTileMatrixSet(tileMatrixSetElement, uriTemplate));
            }

            return new WmtsCapabilities
            {
                Layer = layer,
                TileMatrixSets = tileMatrixSets
            };
        }

        public static string ReadUriTemplate(XElement capabilitiesElement, XElement layerElement, string layer, string style, string defaultUri)
        {
            const string formatPng = "image/png";
            const string formatJpg = "image/jpeg";
            string uriTemplate = null;

            var resourceUrls = layerElement
                .Elements(wmts + "ResourceURL")
                .Where(r => r.Attribute("resourceType")?.Value == "tile" &&
                            r.Attribute("format")?.Value != null &&
                            r.Attribute("template")?.Value != null)
                .ToLookup(r => r.Attribute("format").Value,
                          r => r.Attribute("template").Value);

            if (resourceUrls.Count != 0)
            {
                var uriTemplates = resourceUrls.Contains(formatPng) ? resourceUrls[formatPng]
                                 : resourceUrls.Contains(formatJpg) ? resourceUrls[formatJpg]
                                 : resourceUrls.First();

                uriTemplate = uriTemplates.First().Replace("{Style}", style);
            }
            else
            {
                uriTemplate = capabilitiesElement
                    .Elements(ows + "OperationsMetadata")
                    .Elements(ows + "Operation")
                    .Where(o => o.Attribute("name")?.Value == "GetTile")
                    .Elements(ows + "DCP")
                    .Elements(ows + "HTTP")
                    .Elements(ows + "Get")
                    .Where(g => g.Elements(ows + "Constraint")
                                 .Any(con => con.Attribute("name")?.Value == "GetEncoding" &&
                                             con.Element(ows + "AllowedValues")?.Element(ows + "Value")?.Value == "KVP"))
                    .Select(g => g.Attribute(xlink + "href")?.Value)
                    .Where(h => !string.IsNullOrEmpty(h))
                    .Select(h => h.Split('?')[0])
                    .FirstOrDefault() ??
                    defaultUri;

                if (uriTemplate != null)
                {
                    var formats = layerElement
                        .Elements(wmts + "Format")
                        .Select(f => f.Value);

                    var format = formats.Contains(formatPng) ? formatPng
                               : formats.Contains(formatJpg) ? formatJpg
                               : formats.FirstOrDefault();

                    if (string.IsNullOrEmpty(format))
                    {
                        format = formatPng;
                    }

                    uriTemplate += "?Service=WMTS"
                        + "&Request=GetTile"
                        + "&Version=1.0.0"
                        + "&Layer=" + layer
                        + "&Style=" + style
                        + "&Format=" + format
                        + "&TileMatrixSet={TileMatrixSet}"
                        + "&TileMatrix={TileMatrix}"
                        + "&TileRow={TileRow}"
                        + "&TileCol={TileCol}";
                }
            }

            if (string.IsNullOrEmpty(uriTemplate))
            {
                throw new ArgumentException($"No ResourceURL element in Layer \"{layer}\" and no GetTile KVP Operation Metadata found.");
            }

            return uriTemplate;
        }

        public static WmtsTileMatrixSet ReadTileMatrixSet(XElement tileMatrixSetElement, string uriTemplate)
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

            const string urnPrefix = "urn:ogc:def:crs:";

            if (supportedCrs.StartsWith(urnPrefix)) // e.g. "urn:ogc:def:crs:EPSG:6.18:3857")
            {
                var crsComponents = supportedCrs.Substring(urnPrefix.Length).Split(':');

                supportedCrs = crsComponents.First() + ":" + crsComponents.Last();
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

            return new WmtsTileMatrixSet(identifier, supportedCrs, uriTemplate, tileMatrixes);
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
                (topLeftCornerStrings = valueString.Split([' '], StringSplitOptions.RemoveEmptyEntries)).Length < 2 ||
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

            // See 07-057r7_Web_Map_Tile_Service_Standard.pdf, section 6.1.a, page 8:
            // "standardized rendering pixel size" is 0.28 mm.
            //
            return new WmtsTileMatrix(identifier,
                1d / (scaleDenominator * 0.00028),
                topLeft, tileWidth, tileHeight, matrixWidth, matrixHeight);
        }
    }
}

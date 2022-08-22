// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
#if WINUI
using Windows.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
#elif UWP
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Media;
#endif

namespace MapControl
{
    /// <summary>
    /// Displays a single map image from a Web Map Service (WMS).
    /// </summary>
    public partial class WmsImageLayer : MapImageLayer
    {
        public static readonly DependencyProperty ServiceUriProperty = DependencyProperty.Register(
            nameof(ServiceUri), typeof(Uri), typeof(WmsImageLayer),
            new PropertyMetadata(null, async (o, e) => await ((WmsImageLayer)o).UpdateImageAsync()));

        public static readonly DependencyProperty LayersProperty = DependencyProperty.Register(
            nameof(Layers), typeof(string), typeof(WmsImageLayer),
            new PropertyMetadata(null,
                async (o, e) =>
                {
                    if (e.OldValue != null) // ignore property change from GetImageAsync
                    {
                        await ((WmsImageLayer)o).UpdateImageAsync();
                    }
                }));

        public static readonly DependencyProperty StylesProperty = DependencyProperty.Register(
            nameof(Styles), typeof(string), typeof(WmsImageLayer),
            new PropertyMetadata(string.Empty, async (o, e) => await ((WmsImageLayer)o).UpdateImageAsync()));

        public WmsImageLayer()
        {
            foreach (FrameworkElement child in Children)
            {
                child.UseLayoutRounding = true;
            }
        }

        /// <summary>
        /// The base request URL. 
        /// </summary>
        public Uri ServiceUri
        {
            get => (Uri)GetValue(ServiceUriProperty);
            set => SetValue(ServiceUriProperty, value);
        }

        /// <summary>
        /// Comma-separated list of Layer names to be displayed. If not set, the first Layer is displayed.
        /// </summary>
        public string Layers
        {
            get => (string)GetValue(LayersProperty);
            set => SetValue(LayersProperty, value);
        }

        /// <summary>
        /// Comma-separated list of requested styles. Default is an empty string.
        /// </summary>
        public string Styles
        {
            get => (string)GetValue(StylesProperty);
            set => SetValue(StylesProperty, value);
        }

        /// <summary>
        /// Gets a list of all layer names returned by a GetCapabilities response.
        /// </summary>
        public async Task<IEnumerable<string>> GetLayerNamesAsync()
        {
            IEnumerable<string> layerNames = null;

            var capabilities = await GetCapabilitiesAsync();

            if (capabilities != null)
            {
                var ns = capabilities.Name.Namespace;

                layerNames = capabilities
                    .Descendants(ns + "Layer")
                    .Select(e => e.Element(ns + "Name")?.Value)
                    .Where(n => !string.IsNullOrEmpty(n));
            }

            return layerNames;
        }

        /// <summary>
        /// Loads an XElement from the URL returned by GetCapabilitiesRequestUri().
        /// </summary>
        public async Task<XElement> GetCapabilitiesAsync()
        {
            XElement element = null;

            if (ServiceUri != null)
            {
                var uri = GetCapabilitiesRequestUri();

                if (!string.IsNullOrEmpty(uri))
                {
                    try
                    {
                        using (var stream = await ImageLoader.HttpClient.GetStreamAsync(uri))
                        {
                            element = XDocument.Load(stream).Root;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"WmsImageLayer: {uri}: {ex.Message}");
                    }
                }
            }

            return element;
        }

        /// <summary>
        /// Loads an XElement from the URL returned by GetFeatureInfoRequestUri().
        /// </summary>
        public async Task<XElement> GetFeatureInfoAsync(Point position)
        {
            XElement element = null;

            if (ServiceUri != null)
            {
                var uri = GetFeatureInfoRequestUri(position, "text/xml");

                if (!string.IsNullOrEmpty(uri))
                {
                    try
                    {
                        using (var stream = await ImageLoader.HttpClient.GetStreamAsync(uri))
                        {
                            element = XDocument.Load(stream).Root;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"WmsImageLayer: {uri}: {ex.Message}");
                    }
                }
            }

            return element;
        }

        /// <summary>
        /// Gets a response string from the URL returned by GetFeatureInfoRequestUri().
        /// </summary>
        public async Task<string> GetFeatureInfoTextAsync(Point position, string format = "text/plain")
        {
            string response = null;

            if (ServiceUri != null)
            {
                var uri = GetFeatureInfoRequestUri(position, format);

                if (!string.IsNullOrEmpty(uri))
                {
                    try
                    {
                        response = await ImageLoader.HttpClient.GetStringAsync(uri);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"WmsImageLayer: {uri}: {ex.Message}");
                    }
                }
            }

            return response;
        }

        /// <summary>
        /// Loads an ImageSource from the URL returned by GetMapRequestUri().
        /// </summary>
        protected override async Task<ImageSource> GetImageAsync(IProgress<double> progress)
        {
            ImageSource image = null;

            if (ServiceUri != null)
            {
                if (Layers == null &&
                    ServiceUri.ToString().IndexOf("LAYERS=", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    Layers = (await GetLayerNamesAsync())?.FirstOrDefault() ?? ""; // get first Layer from Capabilities
                }

                var uri = GetMapRequestUri();

                if (!string.IsNullOrEmpty(uri))
                {
                    image = await ImageLoader.LoadImageAsync(new Uri(uri), progress);
                }
            }

            return image;
        }

        /// <summary>
        /// Returns a GetCapabilities request URL string.
        /// </summary>
        protected virtual string GetCapabilitiesRequestUri()
        {
            return GetRequestUri(new Dictionary<string, string>
            {
                { "SERVICE", "WMS" },
                { "VERSION", "1.3.0" },
                { "REQUEST", "GetCapabilities" }
            });
        }

        /// <summary>
        /// Returns a GetMap request URL string.
        /// </summary>
        protected virtual string GetMapRequestUri()
        {
            var projection = ParentMap?.MapProjection;

            if (projection == null)
            {
                return null;
            }

            var mapRect = projection.BoundingBoxToRect(BoundingBox);
            var viewScale = ParentMap.ViewTransform.Scale;

            return GetRequestUri(new Dictionary<string, string>
            {
                { "SERVICE", "WMS" },
                { "VERSION", "1.3.0" },
                { "REQUEST", "GetMap" },
                { "LAYERS", Layers ?? "" },
                { "STYLES", Styles ?? "" },
                { "FORMAT", "image/png" },
                { "CRS", GetCrsValue(projection) },
                { "BBOX", GetBboxValue(projection, mapRect) },
                { "WIDTH", Math.Round(viewScale * mapRect.Width).ToString("F0") },
                { "HEIGHT", Math.Round(viewScale * mapRect.Height).ToString("F0") }
            });
        }

        /// <summary>
        /// Returns a GetFeatureInfo request URL string.
        /// </summary>
        protected virtual string GetFeatureInfoRequestUri(Point position, string format)
        {
            var projection = ParentMap?.MapProjection;

            if (projection == null)
            {
                return null;
            }

            var mapRect = projection.BoundingBoxToRect(BoundingBox);
            var viewRect = GetViewRect(mapRect);
            var viewSize = ParentMap.RenderSize;

            var transform = new Matrix(1, 0, 0, 1, -viewSize.Width / 2, -viewSize.Height / 2);
            transform.Rotate(-viewRect.Rotation);
            transform.Translate(viewRect.Width / 2, viewRect.Height / 2);

            var imagePos = transform.Transform(position);

            var queryParameters = new Dictionary<string, string>
            {
                { "SERVICE", "WMS" },
                { "VERSION", "1.3.0" },
                { "REQUEST", "GetFeatureInfo" },
                { "LAYERS", Layers ?? "" },
                { "STYLES", Styles ?? "" },
                { "FORMAT", "image/png" },
                { "INFO_FORMAT", format },
                { "CRS", GetCrsValue(projection) },
                { "BBOX", GetBboxValue(projection, mapRect) },
                { "WIDTH", Math.Round(viewRect.Width).ToString("F0") },
                { "HEIGHT", Math.Round(viewRect.Height).ToString("F0") },
                { "I", Math.Round(imagePos.X).ToString("F0") },
                { "J", Math.Round(imagePos.Y).ToString("F0") }
            };

            return GetRequestUri(queryParameters) + "&QUERY_LAYERS=" + queryParameters["LAYERS"];
        }

        protected virtual string GetCrsValue(MapProjection projection)
        {
            return projection.GetCrsValue();
        }

        protected virtual string GetBboxValue(MapProjection projection, Rect mapRect)
        {
            return projection.GetBboxValue(mapRect);
        }

        protected string GetRequestUri(IDictionary<string, string> queryParameters)
        {
            var query = ServiceUri.Query;

            if (!string.IsNullOrEmpty(query))
            {
                foreach (var param in query.Substring(1).Split('&'))
                {
                    var pair = param.Split('=');
                    queryParameters[pair[0].ToUpper()] = pair.Length > 1 ? pair[1] : "";
                }
            }

            var uri = ServiceUri.GetLeftPart(UriPartial.Path) + "?"
                + string.Join("&", queryParameters.Select(kv => kv.Key + "=" + kv.Value));

            Debug.WriteLine(uri);

            return uri.Replace(" ", "%20");
        }
    }
}

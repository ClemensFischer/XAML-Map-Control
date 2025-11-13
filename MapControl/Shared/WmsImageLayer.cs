using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
#if WPF
using System.Windows;
using System.Windows.Media;
#elif UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#elif WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
#elif AVALONIA
using Avalonia;
using Avalonia.Interactivity;
using ImageSource = Avalonia.Media.IImage;
#endif

namespace MapControl
{
    /// <summary>
    /// Displays a single map image from a Web Map Service (WMS).
    /// </summary>
    public partial class WmsImageLayer : MapImageLayer
    {
        private static ILogger logger;
        private static ILogger Logger => logger ??= ImageLoader.LoggerFactory?.CreateLogger(typeof(WmsImageLayer));

        public static readonly DependencyProperty ServiceUriProperty =
            DependencyPropertyHelper.Register<WmsImageLayer, Uri>(nameof(ServiceUri), null,
                async (layer, oldValue, newValue) => await layer.UpdateImageAsync());

        public static readonly DependencyProperty RequestStylesProperty =
            DependencyPropertyHelper.Register<WmsImageLayer, string>(nameof(RequestStyles), "",
                async (layer, oldValue, newValue) => await layer.UpdateImageAsync());

        public static readonly DependencyProperty RequestLayersProperty =
            DependencyPropertyHelper.Register<WmsImageLayer, string>(nameof(RequestLayers), null,
                async (layer, oldValue, newValue) => await layer.UpdateImageAsync());

        /// <summary>
        /// The base request URL. 
        /// </summary>
        public Uri ServiceUri
        {
            get => (Uri)GetValue(ServiceUriProperty);
            set => SetValue(ServiceUriProperty, value);
        }

        /// <summary>
        /// Comma-separated sequence of requested WMS Styles. Default is an empty string.
        /// </summary>
        public string RequestStyles
        {
            get => (string)GetValue(RequestStylesProperty);
            set => SetValue(RequestStylesProperty, value);
        }

        /// <summary>
        /// Comma-separated sequence of WMS Layer names to be displayed. If not set, the default Layer is displayed.
        /// </summary>
        public string RequestLayers
        {
            get => (string)GetValue(RequestLayersProperty);
            set => SetValue(RequestLayersProperty, value);
        }

        /// <summary>
        /// Gets a collection of all Layer names available in a WMS.
        /// </summary>
        public IReadOnlyCollection<string> AvailableLayers { get; private set; }

        private bool HasLayer =>
            RequestLayers != null ||
            AvailableLayers?.Count > 0 ||
            ServiceUri.Query?.IndexOf("LAYERS=", StringComparison.OrdinalIgnoreCase) > 0;

        public WmsImageLayer()
        {
            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;

            if (ServiceUri != null && !HasLayer)
            {
                await InitializeAsync();

                if (AvailableLayers != null && AvailableLayers.Count > 0)
                {
                    await UpdateImageAsync();
                }
            }
        }

        /// <summary>
        /// Initializes the AvailableLayers and SupportedCrsIds properties.
        /// Calling this method is only necessary when no layer name is known in advance.
        /// It is called internally in a Loaded event handler when the RequestLayers and AvailableLayers
        /// properties are null and the ServiceUri.Query part does not contain a LAYERS parameter.
        /// </summary>
        public async Task InitializeAsync()
        {
            var capabilities = await GetCapabilitiesAsync();

            if (capabilities != null)
            {
                var ns = capabilities.Name.Namespace;
                var capability = capabilities.Element(ns + "Capability");

                SupportedCrsIds = capability
                    .Descendants(ns + "Layer")
                    .Descendants(ns + "CRS")
                    .Select(e => e.Value)
                    .ToList();

                AvailableLayers = capability
                    .Descendants(ns + "Layer")
                    .Select(e => e.Element(ns + "Name")?.Value)
                    .Where(n => !string.IsNullOrEmpty(n))
                    .ToList();
            }
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

                if (uri != null)
                {
                    try
                    {
                        using var stream = await ImageLoader.HttpClient.GetStreamAsync(uri);

                        element = await XDocument.LoadRootElementAsync(stream);
                    }
                    catch (Exception ex)
                    {
                        Logger?.LogError(ex, "Failed reading capabilities from {uri}", uri);
                    }
                }
            }

            return element;
        }

        /// <summary>
        /// Gets a response string from the URL returned by GetFeatureInfoRequestUri().
        /// </summary>
        public async Task<string> GetFeatureInfoAsync(Point position, string format = "text/plain")
        {
            string response = null;

            if (ServiceUri != null &&
                HasLayer &&
                ParentMap?.MapProjection != null &&
                ParentMap.ActualWidth > 0d &&
                ParentMap.ActualHeight > 0d)
            {
                var boundingBox = ParentMap.ViewRectToBoundingBox(new Rect(0d, 0d, ParentMap.ActualWidth, ParentMap.ActualHeight));

                var uri = GetFeatureInfoRequestUri(boundingBox, position, format);

                if (uri != null)
                {
                    try
                    {
                        response = await ImageLoader.HttpClient.GetStringAsync(uri);
                    }
                    catch (Exception ex)
                    {
                        Logger?.LogError(ex, "Failed reading feature info from {uri}", uri);
                    }
                }
            }

            return response;
        }

        /// <summary>
        /// Loads an ImageSource from the URL returned by GetMapRequestUri().
        /// </summary>
        protected override async Task<ImageSource> GetImageAsync(BoundingBox boundingBox, IProgress<double> progress)
        {
            ImageSource image = null;

            if (ServiceUri != null && HasLayer)
            {
                if (boundingBox.West >= -180d && boundingBox.East <= 180d ||
                    ParentMap.MapProjection.Type > MapProjectionType.NormalCylindrical)
                {
                    var uri = GetMapRequestUri(boundingBox);

                    if (uri != null)
                    {
                        try
                        {
                            image = await ImageLoader.LoadImageAsync(new Uri(uri), progress);
                        }
                        catch (Exception ex)
                        {
                            Logger?.LogError(ex, "ImageLoader.LoadImageAsync");
                        }
                    }
                }
                else
                {
                    BoundingBox bbox1, bbox2;

                    if (boundingBox.West < -180d)
                    {
                        bbox1 = new BoundingBox(boundingBox.South, boundingBox.West + 360, boundingBox.North, 180d);
                        bbox2 = new BoundingBox(boundingBox.South, -180d, boundingBox.North, boundingBox.East);
                    }
                    else
                    {
                        bbox1 = new BoundingBox(boundingBox.South, boundingBox.West, boundingBox.North, 180d);
                        bbox2 = new BoundingBox(boundingBox.South, -180d, boundingBox.North, boundingBox.East - 360d);
                    }

                    var uri1 = GetMapRequestUri(bbox1);
                    var uri2 = GetMapRequestUri(bbox2);

                    if (uri1 != null && uri2 != null)
                    {
                        try
                        {
                            image = await ImageLoader.LoadMergedImageAsync(new Uri(uri1), new Uri(uri2), progress);
                        }
                        catch (Exception ex)
                        {
                            Logger?.LogError(ex, "ImageLoader.LoadMergedImageAsync");
                        }
                    }
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
        protected virtual string GetMapRequestUri(BoundingBox boundingBox)
        {
            string uri = null;
            var bbox = ParentMap.MapProjection.BoundingBoxToMap(boundingBox);

            if (bbox.HasValue)
            {
                var width = ParentMap.ViewTransform.Scale * bbox.Value.Width;
                var height = ParentMap.ViewTransform.Scale * bbox.Value.Height;

                uri = GetRequestUri(new Dictionary<string, string>
                {
                    { "SERVICE", "WMS" },
                    { "VERSION", "1.3.0" },
                    { "REQUEST", "GetMap" },
                    { "LAYERS", RequestLayers ?? AvailableLayers?.FirstOrDefault() ?? "" },
                    { "STYLES", RequestStyles ?? "" },
                    { "FORMAT", "image/png" },
                    { "CRS", GetCrsValue() },
                    { "BBOX", GetBboxValue(boundingBox, bbox.Value) },
                    { "WIDTH", Math.Round(width).ToString("F0") },
                    { "HEIGHT", Math.Round(height).ToString("F0") }
                });
            }

            return uri;
        }

        /// <summary>
        /// Returns a GetFeatureInfo request URL string.
        /// </summary>
        protected virtual string GetFeatureInfoRequestUri(BoundingBox boundingBox, Point position, string format)
        {
            string uri = null;
            var bbox = ParentMap.MapProjection.BoundingBoxToMap(boundingBox);

            if (bbox.HasValue)
            {
                var width = ParentMap.ViewTransform.Scale * bbox.Value.Width;
                var height = ParentMap.ViewTransform.Scale * bbox.Value.Height;

                var transform = ViewTransform.CreateTransformMatrix(
                    -ParentMap.ActualWidth / 2d, -ParentMap.ActualWidth / 2d,
                    -ParentMap.ViewTransform.Rotation,
                    width / 2d, height / 2d);

                var imagePos = transform.Transform(position);

                var queryParameters = new Dictionary<string, string>
                {
                    { "SERVICE", "WMS" },
                    { "VERSION", "1.3.0" },
                    { "REQUEST", "GetFeatureInfo" },
                    { "LAYERS", RequestLayers ?? AvailableLayers?.FirstOrDefault() ?? "" },
                    { "STYLES", RequestStyles ?? "" },
                    { "FORMAT", "image/png" },
                    { "INFO_FORMAT", format },
                    { "CRS", GetCrsValue() },
                    { "BBOX", GetBboxValue(boundingBox, bbox.Value) },
                    { "WIDTH", Math.Round(width).ToString("F0") },
                    { "HEIGHT", Math.Round(height).ToString("F0") },
                    { "I", Math.Round(imagePos.X).ToString("F0") },
                    { "J", Math.Round(imagePos.Y).ToString("F0") }
                };

                // GetRequestUri may modify queryParameters["LAYERS"]
                //
                uri = GetRequestUri(queryParameters) + "&QUERY_LAYERS=" + queryParameters["LAYERS"];
            }

            return uri;
        }

        protected virtual string GetCrsValue()
        {
            var projection = ParentMap.MapProjection;
            var crs = projection.CrsId;

            if (crs.StartsWith("AUTO2:") || crs.StartsWith("AUTO:"))
            {
                crs = string.Format(CultureInfo.InvariantCulture, "{0},1,{1},{2}", crs, projection.Center.Longitude, projection.Center.Latitude);
            }

            return crs;
        }

        protected virtual string GetBboxValue(BoundingBox boundingBox, Rect mapBoundingBox)
        {
            var crs = ParentMap.MapProjection.CrsId;
            string format;
            double x1, y1, x2, y2;

            if (crs == "CRS:84" || crs == "EPSG:4326")
            {
                format = crs == "CRS:84" ? "{0:F8},{1:F8},{2:F8},{3:F8}" : "{1:F8},{0:F8},{3:F8},{2:F8}";
                x1 = boundingBox.West;
                y1 = boundingBox.South;
                x2 = boundingBox.East;
                y2 = boundingBox.North;
            }
            else
            {
                format = "{0:F2},{1:F2},{2:F2},{3:F2}";
                x1 = mapBoundingBox.X;
                y1 = mapBoundingBox.Y;
                x2 = mapBoundingBox.X + mapBoundingBox.Width;
                y2 = mapBoundingBox.Y + mapBoundingBox.Height;
            }

            return string.Format(CultureInfo.InvariantCulture, format, x1, y1, x2, y2);
        }

        protected string GetRequestUri(IDictionary<string, string> queryParameters)
        {
            var query = ServiceUri.Query;

            if (!string.IsNullOrEmpty(query))
            {
                // Parameters from ServiceUri.Query take higher precedence than queryParameters.
                //
                foreach (var param in query.Substring(1).Split('&'))
                {
                    var pair = param.Split('=');
                    queryParameters[pair[0].ToUpper()] = pair.Length > 1 ? pair[1] : "";
                }
            }

            var uri = ServiceUri.GetLeftPart(UriPartial.Path) + "?"
                + string.Join("&", queryParameters.Select(kv => kv.Key + "=" + kv.Value));

            return uri.Replace(" ", "%20");
        }
    }
}

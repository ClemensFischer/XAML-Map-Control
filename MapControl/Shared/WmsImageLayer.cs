// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
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
#elif WINDOWS_UWP
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
            new PropertyMetadata(null, async (o, e) => await ((WmsImageLayer)o).UpdateImageAsync()));

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
            get { return (Uri)GetValue(ServiceUriProperty); }
            set { SetValue(ServiceUriProperty, value); }
        }

        /// <summary>
        /// Comma-separated list of Layer names to be displayed. If not set, the first Layer is displayed.
        /// </summary>
        public string Layers
        {
            get { return (string)GetValue(LayersProperty); }
            set { SetValue(LayersProperty, value); }
        }

        /// <summary>
        /// Comma-separated list of requested styles. Default is an empty string.
        /// </summary>
        public string Styles
        {
            get { return (string)GetValue(StylesProperty); }
            set { SetValue(StylesProperty, value); }
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
        protected override async Task<ImageSource> GetImageAsync()
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
                    image = await ImageLoader.LoadImageAsync(new Uri(uri));
                }
            }

            return image;
        }

        /// <summary>
        /// Returns a GetCapabilities request URL string.
        /// </summary>
        protected virtual string GetCapabilitiesRequestUri()
        {
            return GetRequestUri("GetCapabilities").Replace(" ", "%20");
        }

        /// <summary>
        /// Returns a GetMap request URL string.
        /// </summary>
        protected virtual string GetMapRequestUri()
        {
            string uri = null;
            var projection = ParentMap?.MapProjection;

            if (projection != null)
            {
                uri = GetRequestUri("GetMap");

                if (uri.IndexOf("LAYERS=", StringComparison.OrdinalIgnoreCase) < 0 && Layers != null)
                {
                    uri += "&LAYERS=" + Layers;
                }

                if (uri.IndexOf("STYLES=", StringComparison.OrdinalIgnoreCase) < 0 && Styles != null)
                {
                    uri += "&STYLES=" + Styles;
                }

                if (uri.IndexOf("FORMAT=", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    uri += "&FORMAT=image/png";
                }

                var mapRect = projection.BoundingBoxToRect(BoundingBox);
                var viewScale = ParentMap.ViewTransform.Scale;

                uri += "&" + GetCrsParam(projection);
                uri += "&" + GetBboxParam(projection, mapRect);
                uri += "&WIDTH=" + (int)Math.Round(viewScale * mapRect.Width);
                uri += "&HEIGHT=" + (int)Math.Round(viewScale * mapRect.Height);

                uri = uri.Replace(" ", "%20");
            }

            return uri;
        }

        /// <summary>
        /// Returns a GetFeatureInfo request URL string.
        /// </summary>
        protected virtual string GetFeatureInfoRequestUri(Point position, string format)
        {
            string uri = null;
            var projection = ParentMap?.MapProjection;

            if (projection != null)
            {
                uri = GetRequestUri("GetFeatureInfo");

                var i = uri.IndexOf("LAYERS=", StringComparison.OrdinalIgnoreCase);

                if (i >= 0)
                {
                    i += 7;
                    var j = uri.IndexOf('&', i);
                    var layers = j >= i ? uri.Substring(i, j - i) : uri.Substring(i);
                    uri += "&QUERY_LAYERS=" + layers;
                }
                else if (Layers != null)
                {
                    uri += "&LAYERS=" + Layers;
                    uri += "&QUERY_LAYERS=" + Layers;
                }

                var mapRect = projection.BoundingBoxToRect(BoundingBox);
                var viewRect = GetViewRect(mapRect);
                var viewSize = ParentMap.RenderSize;

                var transform = new Matrix(1, 0, 0, 1, -viewSize.Width / 2, -viewSize.Height / 2);
                transform.Rotate(-viewRect.Rotation);
                transform.Translate(viewRect.Width / 2, viewRect.Height / 2);

                var imagePos = transform.Transform(position);

                uri += "&" + GetCrsParam(projection);
                uri += "&" + GetBboxParam(projection, mapRect);
                uri += "&WIDTH=" + (int)Math.Round(viewRect.Width);
                uri += "&HEIGHT=" + (int)Math.Round(viewRect.Height);
                uri += "&I=" + (int)Math.Round(imagePos.X);
                uri += "&J=" + (int)Math.Round(imagePos.Y);
                uri += "&INFO_FORMAT=" + format;

                uri = uri.Replace(" ", "%20");
            }

            return uri;
        }

        protected virtual string GetCrsParam(MapProjection projection)
        {
            return "CRS=" + projection.GetCrsValue();
        }

        protected virtual string GetBboxParam(MapProjection projection, Rect mapRect)
        {
            return "BBOX=" + projection.GetBboxValue(mapRect);
        }

        protected string GetRequestUri(string request)
        {
            var uri = ServiceUri.ToString();

            if (!uri.EndsWith("?") && !uri.EndsWith("&"))
            {
                uri += !uri.Contains("?") ? "?" : "&";
            }

            if (uri.IndexOf("SERVICE=", StringComparison.OrdinalIgnoreCase) < 0)
            {
                uri += "SERVICE=WMS&";
            }

            if (uri.IndexOf("VERSION=", StringComparison.OrdinalIgnoreCase) < 0)
            {
                uri += "VERSION=1.3.0&";
            }

            return uri + "REQUEST=" + request;
        }
    }
}

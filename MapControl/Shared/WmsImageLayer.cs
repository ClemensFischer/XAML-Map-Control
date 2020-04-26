// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2020 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
#if WINDOWS_UWP
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

        public static readonly DependencyProperty FormatProperty = DependencyProperty.Register(
            nameof(Format), typeof(string), typeof(WmsImageLayer),
            new PropertyMetadata("image/png", async (o, e) => await ((WmsImageLayer)o).UpdateImageAsync()));

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
        /// Requested image format. Default is image/png.
        /// </summary>
        public string Format
        {
            get { return (string)GetValue(FormatProperty); }
            set { SetValue(FormatProperty, value); }
        }

        /// <summary>
        /// Gets a list of all layer names returned by a GetCapabilities response.
        /// </summary>
        public async Task<IEnumerable<string>> GetLayerNamesAsync()
        {
            IEnumerable<string> layerNames = null;

            if (ServiceUri != null)
            {
                var uri = GetRequestUri("GetCapabilities").Replace(" ", "%20");

                try
                {
                    using (var stream = await ImageLoader.HttpClient.GetStreamAsync(uri))
                    {
                        var capabilities = XDocument.Load(stream).Root;
                        var ns = capabilities.Name.Namespace;

                        layerNames = capabilities
                            .Descendants(ns + "Layer")
                            .Select(e => e.Element(ns + "Name")?.Value)
                            .Where(n => !string.IsNullOrEmpty(n));
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("WmsImageLayer: {0}: {1}", uri, ex.Message);
                }
            }

            return layerNames;
        }

        /// <summary>
        /// Calls GetImageUri() and asynchronously loads an ImageSource from the returned GetMap URL.
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

                var uri = GetImageUri();

                if (uri != null)
                {
                    image = await ImageLoader.LoadImageAsync(new Uri(uri.Replace(" ", "%20")));
                }
            }

            return image;
        }

        /// <summary>
        /// Returns a GetMap request URL string.
        /// </summary>
        protected virtual string GetImageUri()
        {
            string uri = null;
            var projection = ParentMap?.MapProjection;

            if (projection != null && !string.IsNullOrEmpty(projection.CrsId))
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

                if (uri.IndexOf("FORMAT=", StringComparison.OrdinalIgnoreCase) < 0 && Format != null)
                {
                    uri += "&FORMAT=" + Format;
                }

                var rect = projection.BoundingBoxToRect(BoundingBox);

                uri += "&CRS=" + projection.GetCrsValue();
                uri += "&BBOX=" + projection.GetBboxValue(rect);
                uri += "&WIDTH=" + (int)Math.Round(ParentMap.ViewTransform.Scale * rect.Width);
                uri += "&HEIGHT=" + (int)Math.Round(ParentMap.ViewTransform.Scale * rect.Height);
            }

            return uri;
        }

        private string GetRequestUri(string request)
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

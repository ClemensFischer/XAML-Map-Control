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
    public partial class WmsImageLayer : MapImageLayer
    {
        public static readonly DependencyProperty ServiceUriProperty = DependencyProperty.Register(
            nameof(ServiceUri), typeof(Uri), typeof(WmsImageLayer),
            new PropertyMetadata(null, async (o, e) => await ((WmsImageLayer)o).UpdateImageAsync()));

        public static readonly DependencyProperty LayersProperty = DependencyProperty.Register(
            nameof(Layers), typeof(string), typeof(WmsImageLayer),
            new PropertyMetadata(string.Empty, async (o, e) => await ((WmsImageLayer)o).UpdateImageAsync()));

        public static readonly DependencyProperty StylesProperty = DependencyProperty.Register(
            nameof(Styles), typeof(string), typeof(WmsImageLayer),
            new PropertyMetadata(string.Empty, async (o, e) => await ((WmsImageLayer)o).UpdateImageAsync()));

        public static readonly DependencyProperty FormatProperty = DependencyProperty.Register(
            nameof(Format), typeof(string), typeof(WmsImageLayer),
            new PropertyMetadata("image/png", async (o, e) => await ((WmsImageLayer)o).UpdateImageAsync()));

        public Uri ServiceUri
        {
            get { return (Uri)GetValue(ServiceUriProperty); }
            set { SetValue(ServiceUriProperty, value); }
        }

        public string Layers
        {
            get { return (string)GetValue(LayersProperty); }
            set { SetValue(LayersProperty, value); }
        }

        public string Styles
        {
            get { return (string)GetValue(StylesProperty); }
            set { SetValue(StylesProperty, value); }
        }

        public string Format
        {
            get { return (string)GetValue(FormatProperty); }
            set { SetValue(FormatProperty, value); }
        }

        /// <summary>
        /// Gets a list of all layer names returned by a GetCapabilities response.
        /// </summary>
        public async Task<List<string>> GetLayerNamesAsync()
        {
            List<string> layerNames = null;

            if (ServiceUri != null)
            {
                var uri = GetRequestUri("GetCapabilities").Replace(" ", "%20");

                try
                {
                    XElement capabilities;

                    using (var stream = await ImageLoader.HttpClient.GetStreamAsync(uri))
                    {
                        capabilities = XDocument.Load(stream).Root;
                    }

                    var ns = capabilities.Name.Namespace;

                    layerNames = capabilities
                        .Descendants(ns + "Layer")
                        .Where(e => e.Attribute("queryable")?.Value == "1")
                        .Select(e => e.Element(ns + "Name")?.Value)
                        .Where(n => !string.IsNullOrEmpty(n))
                        .ToList();
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
            var uri = GetImageUri();

            return uri != null
                ? await ImageLoader.LoadImageAsync(new Uri(uri.Replace(" ", "%20")))
                : null;
        }

        /// <summary>
        /// Returns a GetMap request URL string.
        /// </summary>
        protected virtual string GetImageUri()
        {
            string uri = null;
            var projection = ParentMap?.MapProjection;

            if (ServiceUri != null && projection != null && !string.IsNullOrEmpty(projection.CrsId))
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
                uri += "&WIDTH=" + (int)Math.Round(projection.ViewportScale * rect.Width);
                uri += "&HEIGHT=" + (int)Math.Round(projection.ViewportScale * rect.Height);
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

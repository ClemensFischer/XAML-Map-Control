// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
#if WINDOWS_UWP
using Windows.Data.Xml.Dom;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#else
using System.Xml;
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

        public async Task<IList<string>> GetLayerNamesAsync()
        {
            IList<string> layerNames = null;

            if (ServiceUri != null)
            {
                var uri = GetRequestUri("GetCapabilities");

                try
                {
                    var document = await XmlDocument.LoadFromUriAsync(new Uri(uri.Replace(" ", "%20")));
                    layerNames = new List<string>();

                    var capability = ChildElements(document.DocumentElement, "Capability").FirstOrDefault();
                    if (capability != null)
                    {
                        var rootLayer = ChildElements(capability, "Layer").FirstOrDefault();
                        if (rootLayer != null)
                        {
                            foreach (var layer in ChildElements(rootLayer, "Layer"))
                            {
                                var name = ChildElements(layer, "Name").FirstOrDefault();
                                if (name != null)
                                {
                                    layerNames.Add(name.InnerText);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("WmsImageLayer: {0}: {1}", uri, ex.Message);
                }
            }

            return layerNames;
        }

        protected override async Task<ImageSource> GetImageAsync(BoundingBox boundingBox)
        {
            ImageSource imageSource = null;
            var projectionParameters = ParentMap.MapProjection.WmsQueryParameters(boundingBox);

            if (ServiceUri != null && !string.IsNullOrEmpty(projectionParameters))
            {
                var uri = GetRequestUri("GetMap&" + projectionParameters);

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

                imageSource = await ImageLoader.LoadImageAsync(new Uri(uri.Replace(" ", "%20")));
            }

            return imageSource;
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

        private static IEnumerable<XmlElement> ChildElements(XmlElement element, string name)
        {
            return element.ChildNodes.OfType<XmlElement>().Where(e => (string)e.LocalName == name);
        }
    }
}

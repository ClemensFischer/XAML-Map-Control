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
        private const string DefaultVersion = "1.3.0";

        public static readonly DependencyProperty ServiceUriProperty = DependencyProperty.Register(
            nameof(ServiceUri), typeof(Uri), typeof(WmsImageLayer),
            new PropertyMetadata(null, async (o, e) => await ((WmsImageLayer)o).UpdateImageAsync()));

        public static readonly DependencyProperty VersionProperty = DependencyProperty.Register(
            nameof(Version), typeof(string), typeof(WmsImageLayer),
            new PropertyMetadata(DefaultVersion, async (o, e) => await ((WmsImageLayer)o).UpdateImageAsync()));

        public static readonly DependencyProperty LayersProperty = DependencyProperty.Register(
            nameof(Layers), typeof(string), typeof(WmsImageLayer),
            new PropertyMetadata(string.Empty, async (o, e) => await ((WmsImageLayer)o).UpdateImageAsync()));

        public static readonly DependencyProperty StylesProperty = DependencyProperty.Register(
            nameof(Styles), typeof(string), typeof(WmsImageLayer),
            new PropertyMetadata(string.Empty, async (o, e) => await ((WmsImageLayer)o).UpdateImageAsync()));

        public static readonly DependencyProperty FormatProperty = DependencyProperty.Register(
            nameof(Format), typeof(string), typeof(WmsImageLayer),
            new PropertyMetadata("image/png", async (o, e) => await ((WmsImageLayer)o).UpdateImageAsync()));

        private string layers = string.Empty;

        public Uri ServiceUri
        {
            get { return (Uri)GetValue(ServiceUriProperty); }
            set { SetValue(ServiceUriProperty, value); }
        }

        public string Version
        {
            get { return (string)GetValue(VersionProperty); }
            set { SetValue(VersionProperty, value); }
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

        protected override async Task<ImageSource> GetImageAsync(BoundingBox boundingBox)
        {
            ImageSource imageSource = null;

            if (ServiceUri != null)
            {
                var version = Version;
                var uri = GetRequestUri(ref version) + "REQUEST=GetMap&";
                var projectionParameters = ParentMap.MapProjection.WmsQueryParameters(boundingBox, version.StartsWith("1.1."));

                if (!string.IsNullOrEmpty(projectionParameters))
                {
                    if (uri.IndexOf("LAYERS=", StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        uri += "LAYERS=" + (Layers ?? "") + "&";
                    }

                    if (uri.IndexOf("STYLES=", StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        uri += "STYLES=" + (Styles ?? "") + "&";
                    }

                    if (uri.IndexOf("FORMAT=", StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        uri += "FORMAT=" + (Format ?? "") + "&";
                    }

                    uri += projectionParameters;

                    try
                    {
                        imageSource = await ImageLoader.LoadImageAsync(new Uri(uri.Replace(" ", "%20")), false);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("WmsImageLayer: {0}: {1}", uri, ex.Message);
                    }
                }
            }

            return imageSource;
        }

        public async Task<IList<string>> GetLayerNamesAsync()
        {
            IList<string> layerNames = null;

            if (ServiceUri != null)
            {
                var version = Version;
                var uri = GetRequestUri(ref version) + "REQUEST=GetCapabilities";

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

        private string GetRequestUri(ref string version)
        {
            if (version == null)
            {
                version = DefaultVersion;
            }

            var uri = ServiceUri.ToString();

            if (!uri.EndsWith("?") && !uri.EndsWith("&"))
            {
                uri += !uri.Contains("?") ? "?" : "&";
            }

            if (uri.IndexOf("SERVICE=", StringComparison.OrdinalIgnoreCase) < 0)
            {
                uri += "SERVICE=WMS&";
            }

            int versionStart = uri.IndexOf("VERSION=", StringComparison.OrdinalIgnoreCase);
            int versionEnd;

            if (versionStart < 0)
            {
                uri += "VERSION=" + version + "&";
            }
            else if ((versionEnd = uri.IndexOf("&", versionStart + 8)) >= versionStart + 8)
            {
                version = uri.Substring(versionStart, versionEnd - versionStart);
            }

            return uri;
        }

        private static IEnumerable<XmlElement> ChildElements(XmlElement element, string name)
        {
            return element.ChildNodes.OfType<XmlElement>().Where(e => (string)e.LocalName == name);
        }
    }
}

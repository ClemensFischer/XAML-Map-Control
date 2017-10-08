// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2017 Clemens Fischer
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
using Windows.UI.Xaml.Media.Imaging;
#else
using System.Xml;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
#endif

namespace MapControl
{
    public partial class WmsImageLayer : MapImageLayer
    {
        public static readonly DependencyProperty ServerUriProperty = DependencyProperty.Register(
            nameof(ServerUri), typeof(Uri), typeof(WmsImageLayer),
            new PropertyMetadata(null, (o, e) => ((WmsImageLayer)o).UpdateImage()));

        public static readonly DependencyProperty VersionProperty = DependencyProperty.Register(
            nameof(Version), typeof(string), typeof(WmsImageLayer),
            new PropertyMetadata("1.3.0", (o, e) => ((WmsImageLayer)o).UpdateImage()));

        public static readonly DependencyProperty LayersProperty = DependencyProperty.Register(
            nameof(Layers), typeof(string), typeof(WmsImageLayer),
            new PropertyMetadata(string.Empty, (o, e) => ((WmsImageLayer)o).UpdateImage()));

        public static readonly DependencyProperty StylesProperty = DependencyProperty.Register(
            nameof(Styles), typeof(string), typeof(WmsImageLayer),
            new PropertyMetadata(string.Empty, (o, e) => ((WmsImageLayer)o).UpdateImage()));

        public static readonly DependencyProperty FormatProperty = DependencyProperty.Register(
            nameof(Format), typeof(string), typeof(WmsImageLayer),
            new PropertyMetadata("image/png", (o, e) => ((WmsImageLayer)o).UpdateImage()));

        public static readonly DependencyProperty TransparentProperty = DependencyProperty.Register(
            nameof(Transparent), typeof(bool), typeof(WmsImageLayer),
            new PropertyMetadata(false, (o, e) => ((WmsImageLayer)o).UpdateImage()));

        private string layers = string.Empty;

        public Uri ServerUri
        {
            get { return (Uri)GetValue(ServerUriProperty); }
            set { SetValue(ServerUriProperty, value); }
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

        public bool Transparent
        {
            get { return (bool)GetValue(TransparentProperty); }
            set { SetValue(TransparentProperty, value); }
        }

        protected override ImageSource GetImage(BoundingBox boundingBox)
        {
            if (ServerUri == null)
            {
                return null;
            }

            var projectionParameters = ParentMap.MapProjection.WmsQueryParameters(boundingBox, Version);

            if (string.IsNullOrEmpty(projectionParameters))
            {
                return null;
            }

            var uri = GetRequestUri("GetMap"
                + "&LAYERS=" + Layers + "&STYLES=" + Styles + "&FORMAT=" + Format
                + "&TRANSPARENT=" + (Transparent ? "TRUE" : "FALSE") + "&" + projectionParameters);

            return new BitmapImage(uri);
        }

        public async Task<IList<string>> GetLayerNamesAsync()
        {
            if (ServerUri == null)
            {
                return null;
            }

            var layerNames = new List<string>();

            try
            {
                var document = await XmlDocument.LoadFromUriAsync(GetRequestUri("GetCapabilities"));

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
                Debug.WriteLine("WmsImageLayer: {0}: {1}", ServerUri, ex.Message);
            }

            return layerNames;
        }

        private Uri GetRequestUri(string query)
        {
            var uri = ServerUri.ToString();

            if (!uri.EndsWith("?") && !uri.EndsWith("&"))
            {
                uri += "?";
            }

            uri += "SERVICE=WMS&VERSION=" + Version + "&REQUEST=" + query;

            return new Uri(uri.Replace(" ", "%20"));
        }

        private static IEnumerable<XmlElement> ChildElements(XmlElement element, string name)
        {
            return element.ChildNodes.OfType<XmlElement>().Where(e => (string)e.LocalName == name);
        }
    }
}

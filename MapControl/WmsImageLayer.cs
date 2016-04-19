// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2016 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
#if NETFX_CORE
using Windows.Data.Xml.Dom;
using Windows.UI.Xaml;
#else
using System.Windows;
using System.Xml;
#endif

namespace MapControl
{
    public partial class WmsImageLayer : MapImageLayer
    {
        public static readonly DependencyProperty BaseUriProperty = DependencyProperty.Register(
            "ServerUri", typeof(string), typeof(WmsImageLayer),
            new PropertyMetadata(null, async (o, e) => await ((WmsImageLayer)o).UpdateUriFormat(true)));

        public static readonly DependencyProperty LayersProperty = DependencyProperty.Register(
            "Layers", typeof(string), typeof(WmsImageLayer),
            new PropertyMetadata(null, async (o, e) => await ((WmsImageLayer)o).UpdateUriFormat()));

        public static readonly DependencyProperty ParametersProperty = DependencyProperty.Register(
            "Parameters", typeof(string), typeof(WmsImageLayer),
            new PropertyMetadata(null, async (o, e) => await ((WmsImageLayer)o).UpdateUriFormat()));

        public static readonly DependencyProperty TransparentProperty = DependencyProperty.Register(
            "Transparent", typeof(bool), typeof(WmsImageLayer),
            new PropertyMetadata(false, async (o, e) => await ((WmsImageLayer)o).UpdateUriFormat()));

        public string ServerUri
        {
            get { return (string)GetValue(BaseUriProperty); }
            set { SetValue(BaseUriProperty, value); }
        }

        public string Layers
        {
            get { return (string)GetValue(LayersProperty); }
            set { SetValue(LayersProperty, value); }
        }

        public string Parameters
        {
            get { return (string)GetValue(ParametersProperty); }
            set { SetValue(ParametersProperty, value); }
        }

        public bool Transparent
        {
            get { return (bool)GetValue(TransparentProperty); }
            set { SetValue(TransparentProperty, value); }
        }

        public async Task<List<string>> GetAllLayers()
        {
            var layers = new List<string>();

            if (!string.IsNullOrEmpty(ServerUri))
            {
                try
                {
                    var document = await LoadDocument(ServerUri
                        + "?SERVICE=WMS"
                        + "&VERSION=1.3.0"
                        + "&REQUEST=GetCapabilities");

                    var capability = FirstChild(document.DocumentElement, "Capability");
                    if (capability != null)
                    {
                        var rootLayer = FirstChild(capability, "Layer");
                        if (rootLayer != null)
                        {
                            foreach (var layer in ChildElements(rootLayer, "Layer"))
                            {
                                var name = FirstChild(layer, "Name");
                                if (name != null)
                                {
                                    layers.Add(name.InnerText);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }

            return layers;
        }

        private string allLayers;

        private async Task UpdateUriFormat(bool baseUriChanged = false)
        {
            if (baseUriChanged)
            {
                allLayers = null;
            }

            string uriFormat = null;
            var layers = Layers;

            if (!string.IsNullOrEmpty(ServerUri) && !string.IsNullOrEmpty(layers))
            {
                if (layers == "*")
                {
                    layers = allLayers ?? (allLayers = string.Join(",", await GetAllLayers()));
                }

                uriFormat = ServerUri
                    + "?SERVICE=WMS"
                    + "&VERSION=1.3.0"
                    + "&REQUEST=GetMap"
                    + "&LAYERS=" + layers.Replace(" ", "%20")
                    + "&STYLES="
                    + "&CRS=EPSG:3857"
                    + "&BBOX={W},{S},{E},{N}"
                    + "&WIDTH={X}"
                    + "&HEIGHT={Y}"
                    + "&FORMAT=image/png"
                    + "&TRANSPARENT=" + (Transparent ? "TRUE" : "FALSE");

                if (!string.IsNullOrEmpty(Parameters))
                {
                    uriFormat += "&" + Parameters;
                }
            }

            UriFormat = uriFormat;
        }

        private static IEnumerable<XmlElement> ChildElements(XmlElement element, string name)
        {
            return element.ChildNodes.OfType<XmlElement>().Where(e => (string)e.LocalName == name);
        }

        private static XmlElement FirstChild(XmlElement element, string name)
        {
            return element.ChildNodes.OfType<XmlElement>().FirstOrDefault(e => (string)e.LocalName == name);
        }
    }
}

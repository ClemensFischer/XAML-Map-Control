// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2017 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
#if NETFX_CORE
using Windows.Data.Xml.Dom;
#else
using System.Xml;
#endif

namespace MapControl
{
    public static class WmsLayers
    {
        public static async Task<List<string>> GetAllLayers(Uri serverUri)
        {
            var allLayers = new List<string>();

            if (serverUri != null)
            {
                try
                {
                    var document = await LoadDocument(new Uri(serverUri, "?SERVICE=WMS&VERSION=1.3.0&REQUEST=GetCapabilities"));

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
                                    allLayers.Add(name.InnerText);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            return allLayers;
        }

        private static IEnumerable<XmlElement> ChildElements(XmlElement element, string name)
        {
            return element.ChildNodes.OfType<XmlElement>().Where(e => (string)e.LocalName == name);
        }

        private static XmlElement FirstChild(XmlElement element, string name)
        {
            return element.ChildNodes.OfType<XmlElement>().FirstOrDefault(e => (string)e.LocalName == name);
        }

        private static async Task<XmlDocument> LoadDocument(Uri requestUri)
        {
#if NETFX_CORE
            return await XmlDocument.LoadFromUriAsync(requestUri);
#else
            var document = new XmlDocument();
            await Task.Run(() => document.Load(requestUri.ToString()));
            return document;
#endif
        }
    }
}

// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2016 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Xml;
#if NETFX_CORE
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
#else
using System.Windows;
using System.Windows.Media.Imaging;
#endif

namespace MapControl
{
    /// <summary>
    /// Displays Bing Maps tiles. The static ApiKey property must be set to a Bing Maps API Key.
    /// </summary>
    public class BingMapsTileLayer : TileLayer
    {
        public enum MapMode
        {
            Road, Aerial, AerialWithLabels
        }

        public BingMapsTileLayer()
            : this(new TileImageLoader())
        {
        }

        public BingMapsTileLayer(ITileImageLoader tileImageLoader)
            : base(tileImageLoader)
        {
            MinZoomLevel = 1;
            MaxZoomLevel = 21;
            Loaded += OnLoaded;
        }

        public static string ApiKey { get; set; }

        public MapMode Mode { get; set; }
        public string Culture { get; set; }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;

            if (string.IsNullOrEmpty(ApiKey))
            {
                throw new InvalidOperationException("BingMapsTileLayer requires a Bing Maps API Key.");
            }

            var uri = string.Format("http://dev.virtualearth.net/REST/V1/Imagery/Metadata/{0}?output=xml&key={1}", Mode, ApiKey);
            var request = WebRequest.CreateHttp(uri);

            request.BeginGetResponse(HandleImageryMetadataResponse, request);
        }

        private void HandleImageryMetadataResponse(IAsyncResult asyncResult)
        {
            try
            {
                var request = (HttpWebRequest)asyncResult.AsyncState;

                using (var response = request.EndGetResponse(asyncResult))
                using (var xmlReader = XmlReader.Create(response.GetResponseStream()))
                {
                    ReadImageryMetadataResponse(xmlReader);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void ReadImageryMetadataResponse(XmlReader xmlReader)
        {
            string logoUri = null;
            string imageUrl = null;
            string[] imageUrlSubdomains = null;
            int? zoomMin = null;
            int? zoomMax = null;

            do
            {
                if (xmlReader.NodeType == XmlNodeType.Element)
                {
                    switch (xmlReader.Name)
                    {
                        case "BrandLogoUri":
                            logoUri = xmlReader.ReadElementContentAsString();
                            break;
                        case "ImageUrl":
                            imageUrl = xmlReader.ReadElementContentAsString();
                            break;
                        case "ImageUrlSubdomains":
                            imageUrlSubdomains = ReadStrings(xmlReader.ReadSubtree());
                            break;
                        case "ZoomMin":
                            zoomMin = xmlReader.ReadElementContentAsInt();
                            break;
                        case "ZoomMax":
                            zoomMax = xmlReader.ReadElementContentAsInt();
                            break;
                        default:
                            xmlReader.Read();
                            break;
                    }
                }
                else
                {
                    xmlReader.Read();
                }
            }
            while (xmlReader.NodeType != XmlNodeType.None);

            if (!string.IsNullOrEmpty(imageUrl) && imageUrlSubdomains != null && imageUrlSubdomains.Length > 0)
            {
                var _ = Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (string.IsNullOrEmpty(Culture))
                    {
                        Culture = CultureInfo.CurrentUICulture.Name;
                    }

                    TileSource = new BingMapsTileSource(imageUrl.Replace("{culture}", Culture), imageUrlSubdomains);

                    if (zoomMin.HasValue && zoomMin.Value > MinZoomLevel)
                    {
                        MinZoomLevel = zoomMin.Value;
                    }

                    if (zoomMax.HasValue && zoomMax.Value < MaxZoomLevel)
                    {
                        MaxZoomLevel = zoomMax.Value;
                    }

                    if (!string.IsNullOrEmpty(logoUri))
                    {
                        LogoImage = new BitmapImage(new Uri(logoUri));
                    }
                }));
            }
        }

        private static string[] ReadStrings(XmlReader xmlReader)
        {
            var strings = new List<string>();

            do
            {
                if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "string")
                {
                    strings.Add(xmlReader.ReadElementContentAsString());
                }
                else
                {
                    xmlReader.Read();
                }
            }
            while (xmlReader.NodeType != XmlNodeType.None);

            return strings.ToArray();
        }
    }
}

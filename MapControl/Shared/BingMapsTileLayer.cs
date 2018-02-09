// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
#if WINDOWS_UWP
using Windows.Data.Xml.Dom;
using Windows.UI.Xaml;
#else
using System.Windows;
using System.Xml;
#endif

namespace MapControl
{
    /// <summary>
    /// Displays Bing Maps tiles. The static ApiKey property must be set to a Bing Maps API Key.
    /// Tile image URLs and min/max zoom levels are retrieved from the Imagery Metadata Service
    /// (see http://msdn.microsoft.com/en-us/library/ff701716.aspx).
    /// </summary>
    public class BingMapsTileLayer : MapTileLayer
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
        public Uri LogoImageUri { get; private set; }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;

            if (string.IsNullOrEmpty(ApiKey))
            {
                Debug.WriteLine("BingMapsTileLayer requires a Bing Maps API Key");
                return;
            }

            var imageryMetadataUrl = "http://dev.virtualearth.net/REST/V1/Imagery/Metadata/" + Mode;

            try
            {
                var uri = new Uri(imageryMetadataUrl + "?output=xml&key=" + ApiKey);
                var document = await XmlDocument.LoadFromUriAsync(uri);
                var imageryMetadata = document.DocumentElement.GetElementsByTagName("ImageryMetadata").OfType<XmlElement>().FirstOrDefault();

                if (imageryMetadata != null)
                {
                    ReadImageryMetadata(imageryMetadata);
                }

                var brandLogoUri = document.DocumentElement.GetElementsByTagName("BrandLogoUri").OfType<XmlElement>().FirstOrDefault();

                if (brandLogoUri != null)
                {
                    LogoImageUri = new Uri(brandLogoUri.InnerText);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("BingMapsTileLayer: {0}: {1}", imageryMetadataUrl, ex.Message);
            }
        }

        private void ReadImageryMetadata(XmlElement imageryMetadata)
        {
            string imageUrl = null;
            string[] imageUrlSubdomains = null;
            int? zoomMin = null;
            int? zoomMax = null;

            foreach (var element in imageryMetadata.ChildNodes.OfType<XmlElement>())
            {
                switch ((string)element.LocalName)
                {
                    case "ImageUrl":
                        imageUrl = element.InnerText;
                        break;
                    case "ImageUrlSubdomains":
                        imageUrlSubdomains = element.ChildNodes
                            .OfType<XmlElement>()
                            .Where(e => (string)e.LocalName == "string")
                            .Select(e => e.InnerText)
                            .ToArray();
                        break;
                    case "ZoomMin":
                        zoomMin = int.Parse(element.InnerText);
                        break;
                    case "ZoomMax":
                        zoomMax = int.Parse(element.InnerText);
                        break;
                    default:
                        break;
                }
            }

            if (!string.IsNullOrEmpty(imageUrl) && imageUrlSubdomains != null && imageUrlSubdomains.Length > 0)
            {
                if (zoomMin.HasValue && zoomMin.Value > MinZoomLevel)
                {
                    MinZoomLevel = zoomMin.Value;
                }

                if (zoomMax.HasValue && zoomMax.Value < MaxZoomLevel)
                {
                    MaxZoomLevel = zoomMax.Value;
                }

                if (string.IsNullOrEmpty(Culture))
                {
                    Culture = CultureInfo.CurrentUICulture.Name;
                }

                TileSource = new BingMapsTileSource(imageUrl.Replace("{culture}", Culture), imageUrlSubdomains);
            }
        }
    }
}

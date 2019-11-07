// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2019 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using System.Threading.Tasks;
#if WINDOWS_UWP
using Windows.UI.Xaml;
#else
using System.Windows;
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
        private static readonly XNamespace imageryMetadataNamespace = "http://schemas.microsoft.com/search/local/ws/rest/v1";

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

        private async void OnLoaded(object sender, RoutedEventArgs args)
        {
            Loaded -= OnLoaded;

            if (string.IsNullOrEmpty(ApiKey))
            {
                Debug.WriteLine("BingMapsTileLayer requires a Bing Maps API Key");
                return;
            }

            var uri = "http://dev.virtualearth.net/REST/V1/Imagery/Metadata/" + Mode + "?output=xml&key=" + ApiKey;

            try
            {
                var document = await Task.Run(() => XDocument.Load(uri));
                var imageryMetadata = document.Descendants(imageryMetadataNamespace + "ImageryMetadata").FirstOrDefault();
                var brandLogoUri = document.Descendants(imageryMetadataNamespace + "BrandLogoUri").FirstOrDefault();

                if (imageryMetadata != null)
                {
                    ReadImageryMetadata(imageryMetadata);
                }

                if (brandLogoUri != null)
                {
                    LogoImageUri = new Uri(brandLogoUri.Value);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("BingMapsTileLayer: {0}: {1}", uri, ex.Message);
            }
        }

        private void ReadImageryMetadata(XElement imageryMetadata)
        {
            var imageUrl = imageryMetadata.Element(imageryMetadataNamespace + "ImageUrl")?.Value;
            var imageUrlSubdomains = imageryMetadata.Element(imageryMetadataNamespace + "ImageUrlSubdomains")?
                .Elements()
                .Where(e => e.Name.LocalName == "string")
                .Select(e => e.Value)
                .ToArray();

            if (!string.IsNullOrEmpty(imageUrl) &&
                imageUrlSubdomains != null &&
                imageUrlSubdomains.Length > 0)
            {
                var zoomMin = imageryMetadata.Element(imageryMetadataNamespace + "ZoomMin")?.Value;
                var zoomMax = imageryMetadata.Element(imageryMetadataNamespace + "ZoomMax")?.Value;
                int zoomLevel;

                if (!string.IsNullOrEmpty(zoomMin) &&
                    int.TryParse(zoomMin, out zoomLevel) &&
                    MinZoomLevel < zoomLevel)
                {
                    MinZoomLevel = zoomLevel;
                }

                if (!string.IsNullOrEmpty(zoomMax) &&
                    int.TryParse(zoomMax, out zoomLevel) &&
                    MaxZoomLevel > zoomLevel)
                {
                    MaxZoomLevel = zoomLevel;
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

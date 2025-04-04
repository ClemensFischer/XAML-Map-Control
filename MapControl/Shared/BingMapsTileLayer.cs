﻿using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
#if WPF
using System.Windows;
#elif UWP
using Windows.UI.Xaml;
#elif WINUI
using Microsoft.UI.Xaml;
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

            if (!string.IsNullOrEmpty(ApiKey))
            {
                var metadataUri = $"https://dev.virtualearth.net/REST/V1/Imagery/Metadata/{Mode}?output=xml&key={ApiKey}";

                try
                {
                    using (var stream = await ImageLoader.HttpClient.GetStreamAsync(metadataUri))
                    {
                        ReadImageryMetadata(XDocument.Load(stream).Root);
                    }
                }
                catch (Exception ex)
                {
                    ImageLoader.LoggerFactory?.CreateLogger<BingMapsTileLayer>()?.LogError(ex, "Failed loading metadata from {uri}", metadataUri);
                }
            }
            else
            {
                ImageLoader.LoggerFactory?.CreateLogger<BingMapsTileLayer>()?.LogError("Bing Maps API key required");
            }
        }

        private void ReadImageryMetadata(XElement metadataResponse)
        {
            var ns = metadataResponse.Name.Namespace;
            var metadata = metadataResponse.Descendants(ns + "ImageryMetadata").FirstOrDefault();

            if (metadata != null)
            {
                var imageUrl = metadata.Element(ns + "ImageUrl")?.Value;
                var subdomains = metadata.Element(ns + "ImageUrlSubdomains")?.Elements(ns + "string").Select(e => e.Value).ToArray();

                if (!string.IsNullOrEmpty(imageUrl) && subdomains != null && subdomains.Length > 0)
                {
                    var zoomMin = metadata.Element(ns + "ZoomMin")?.Value;
                    var zoomMax = metadata.Element(ns + "ZoomMax")?.Value;

                    if (zoomMin != null && int.TryParse(zoomMin, out int zoomLevel) && MinZoomLevel < zoomLevel)
                    {
                        MinZoomLevel = zoomLevel;
                    }

                    if (zoomMax != null && int.TryParse(zoomMax, out zoomLevel) && MaxZoomLevel > zoomLevel)
                    {
                        MaxZoomLevel = zoomLevel;
                    }

                    if (string.IsNullOrEmpty(Culture))
                    {
                        Culture = CultureInfo.CurrentUICulture.Name;
                    }

                    TileSource = new BingMapsTileSource
                    {
                        UriTemplate = imageUrl.Replace("{culture}", Culture),
                        Subdomains = subdomains
                    };
                }
            }

            var logoUri = metadataResponse.Element(ns + "BrandLogoUri");

            if (logoUri != null)
            {
                LogoImageUri = new Uri(logoUri.Value);
            }
        }
    }
}

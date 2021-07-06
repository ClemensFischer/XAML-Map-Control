// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
#if WINUI
using Microsoft.UI.Xaml;
#elif WINDOWS_UWP
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

            if (!string.IsNullOrEmpty(ApiKey))
            {
                var metadataUri = $"http://dev.virtualearth.net/REST/V1/Imagery/Metadata/{Mode}?output=xml&key={ApiKey}";

                try
                {
                    using (var stream = await ImageLoader.HttpClient.GetStreamAsync(metadataUri))
                    {
                        ReadImageryMetadata(XDocument.Load(stream).Root);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"BingMapsTileLayer: {metadataUri}: {ex.Message}");
                }
            }
            else
            {
                Debug.WriteLine("BingMapsTileLayer requires a Bing Maps API Key");
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
                        UriFormat = imageUrl.Replace("{culture}", Culture),
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

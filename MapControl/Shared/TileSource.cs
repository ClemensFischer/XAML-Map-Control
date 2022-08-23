// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Threading.Tasks;
#if WINUI
using Microsoft.UI.Xaml.Media;
#elif UWP
using Windows.UI.Xaml.Media;
#else
using System.Windows.Media;
#endif

namespace MapControl
{
    /// <summary>
    /// Provides the download Uri or ImageSource of map tiles.
    /// </summary>
#if !UWP
    [System.ComponentModel.TypeConverter(typeof(TileSourceConverter))]
#endif
    public class TileSource
    {
        private string uriTemplate;

        /// <summary>
        /// Gets or sets the template string for tile request Uris.
        /// </summary>
        public string UriTemplate
        {
            get => uriTemplate;
            set
            {
                uriTemplate = value;

                if (Subdomains == null && uriTemplate != null && uriTemplate.Contains("{s}"))
                {
                    Subdomains = new string[] { "a", "b", "c" }; // default OpenStreetMap subdomains
                }
            }
        }

        /// <summary>
        /// Gets or sets an array of request subdomain names that are replaced for the {s} format specifier.
        /// </summary>
        public string[] Subdomains { get; set; }

        /// <summary>
        /// Gets the image Uri for the specified tile indices and zoom level.
        /// </summary>
        public virtual Uri GetUri(int x, int y, int zoomLevel)
        {
            Uri uri = null;

            if (UriTemplate != null && x >= 0 && y >= 0 && zoomLevel >= 0)
            {
                var uriString = UriTemplate
                    .Replace("{x}", x.ToString())
                    .Replace("{y}", y.ToString())
                    .Replace("{z}", zoomLevel.ToString());

                if (Subdomains != null && Subdomains.Length > 0)
                {
                    uriString = uriString.Replace("{s}", Subdomains[(x + y) % Subdomains.Length]);
                }

                uri = new Uri(uriString, UriKind.RelativeOrAbsolute);
            }

            return uri;
        }

        /// <summary>
        /// Loads a tile ImageSource asynchronously from GetUri(x, y, zoomLevel).
        /// </summary>
        public virtual Task<ImageSource> LoadImageAsync(int x, int y, int zoomLevel)
        {
            var uri = GetUri(x, y, zoomLevel);

            return uri != null ? ImageLoader.LoadImageAsync(uri) : Task.FromResult((ImageSource)null);
        }
    }

    public class TmsTileSource : TileSource
    {
        public override Uri GetUri(int x, int y, int zoomLevel)
        {
            return base.GetUri(x, (1 << zoomLevel) - 1 - y, zoomLevel);
        }
    }
}

// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Threading.Tasks;
#if WPF
using System.Windows.Media;
#elif UWP
using Windows.UI.Xaml.Media;
#elif WINUI
using Microsoft.UI.Xaml.Media;
#endif

namespace MapControl
{
    /// <summary>
    /// Provides the download Uri or ImageSource of map tiles.
    /// </summary>
#if UWP || WINUI
    [Windows.Foundation.Metadata.CreateFromString(MethodName = "Parse")]
#else
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

                if (uriTemplate != null && uriTemplate.Contains("{s}") && Subdomains == null)
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
        public virtual Uri GetUri(int column, int row, int zoomLevel)
        {
            Uri uri = null;

            if (UriTemplate != null && column >= 0 && row >= 0 && zoomLevel >= 0)
            {
                var uriString = UriTemplate
                    .Replace("{x}", column.ToString())
                    .Replace("{y}", row.ToString())
                    .Replace("{z}", zoomLevel.ToString());

                if (Subdomains != null && Subdomains.Length > 0)
                {
                    uriString = uriString.Replace("{s}", Subdomains[(column + row) % Subdomains.Length]);
                }

                uri = new Uri(uriString, UriKind.RelativeOrAbsolute);
            }

            return uri;
        }

        /// <summary>
        /// Loads a tile ImageSource asynchronously from GetUri(column, row, zoomLevel).
        /// This method is called by TileImageLoader when caching is disabled.
        /// </summary>
        public virtual Task<ImageSource> LoadImageAsync(int column, int row, int zoomLevel)
        {
            var uri = GetUri(column, row, zoomLevel);

            return uri != null ? ImageLoader.LoadImageAsync(uri) : Task.FromResult((ImageSource)null);
        }

        /// <summary>
        /// Loads a tile ImageSource asynchronously from an encoded frame buffer in a byte array.
        /// This method is called by TileImageLoader when caching is enabled.
        /// </summary>
        public virtual Task<ImageSource> LoadImageAsync(byte[] buffer)
        {
            return ImageLoader.LoadImageAsync(buffer);
        }

        /// <summary>
        /// Creates a TileSource instance from an Uri template string.
        /// </summary>
        public static TileSource Parse(string uriTemplate)
        {
            return new TileSource { UriTemplate = uriTemplate };
        }
    }

    public class TmsTileSource : TileSource
    {
        public override Uri GetUri(int column, int row, int zoomLevel)
        {
            return base.GetUri(column, (1 << zoomLevel) - 1 - row, zoomLevel);
        }
    }
}

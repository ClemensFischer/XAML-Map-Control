// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2020 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Globalization;
using System.Threading.Tasks;
#if WINDOWS_UWP
using Windows.UI.Xaml.Media;
#else
using System.ComponentModel;
using System.Windows.Media;
#endif

namespace MapControl
{
    /// <summary>
    /// Provides the download Uri or ImageSource of map tiles.
    /// </summary>
#if !WINDOWS_UWP
    [TypeConverter(typeof(TileSourceConverter))]
#endif
    public class TileSource
    {
        private string uriFormat;

        /// <summary>
        /// Gets or sets the format string to produce tile request Uris.
        /// </summary>
        public string UriFormat
        {
            get { return uriFormat; }
            set
            {
                uriFormat = value?.Replace("{c}", "{s}"); // for backwards compatibility since 5.4.0

                if (Subdomains == null && uriFormat != null && uriFormat.Contains("{s}"))
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
        /// Loads a tile ImageSource asynchronously from GetUri(x, y, zoomLevel).
        /// </summary>
        public virtual Task<ImageSource> LoadImageAsync(int x, int y, int zoomLevel)
        {
            var uri = GetUri(x, y, zoomLevel);

            return uri != null ? ImageLoader.LoadImageAsync(uri) : Task.FromResult((ImageSource)null);
        }

        /// <summary>
        /// Gets the image Uri for the specified tile indices and zoom level.
        /// </summary>
        public virtual Uri GetUri(int x, int y, int zoomLevel)
        {
            Uri uri = null;

            if (UriFormat != null)
            {
                var uriString = UriFormat
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
    }

    public class TmsTileSource : TileSource
    {
        public override Uri GetUri(int x, int y, int zoomLevel)
        {
            return base.GetUri(x, (1 << zoomLevel) - 1 - y, zoomLevel);
        }
    }

    public class BoundingBoxTileSource : TileSource
    {
        public override Uri GetUri(int x, int y, int zoomLevel)
        {
            Uri uri = null;

            if (UriFormat != null)
            {
                var tileSize = 360d / (1 << zoomLevel); // tile width in degrees
                var west = MapProjection.Wgs84MetersPerDegree * (x * tileSize - 180d);
                var east = MapProjection.Wgs84MetersPerDegree * ((x + 1) * tileSize - 180d);
                var south = MapProjection.Wgs84MetersPerDegree * (180d - (y + 1) * tileSize);
                var north = MapProjection.Wgs84MetersPerDegree * (180d - y * tileSize);

                uri = new Uri(UriFormat
                    .Replace("{west}", west.ToString(CultureInfo.InvariantCulture))
                    .Replace("{south}", south.ToString(CultureInfo.InvariantCulture))
                    .Replace("{east}", east.ToString(CultureInfo.InvariantCulture))
                    .Replace("{north}", north.ToString(CultureInfo.InvariantCulture)));
            }

            return uri;
        }
    }
}

// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
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
        private Func<int, int, int, string> getUri;
        private string uriFormat;
        private int subdomainIndex = -1;

        public TileSource()
        {
        }

        protected TileSource(string uriFormat)
        {
            this.uriFormat = uriFormat;
        }

        public string[] Subdomains { get; set; }

        /// <summary>
        /// Gets or sets the format string to produce tile Uris.
        /// </summary>
        public string UriFormat
        {
            get { return uriFormat; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("The value of the UriFormat property must not be null or empty.");
                }

                uriFormat = value;

                if (uriFormat.Contains("{x}") && uriFormat.Contains("{z}"))
                {
                    if (uriFormat.Contains("{y}"))
                    {
                        getUri = GetDefaultUri;
                    }
                    else if (uriFormat.Contains("{v}"))
                    {
                        getUri = GetTmsUri;
                    }
                }
                else if (uriFormat.Contains("{q}")) // {i} is optional
                {
                    getUri = GetQuadKeyUri;
                }
                else if (uriFormat.Contains("{W}") && uriFormat.Contains("{S}") && uriFormat.Contains("{E}") && uriFormat.Contains("{N}"))
                {
                    getUri = GetBoundingBoxUri;
                }
                else if (uriFormat.Contains("{w}") && uriFormat.Contains("{s}") && uriFormat.Contains("{e}") && uriFormat.Contains("{n}"))
                {
                    getUri = GetLatLonBoundingBoxUri;
                }

                if (Subdomains == null && uriFormat.Contains("{c}"))
                {
                    Subdomains = new string[] { "a", "b", "c" };
                }
            }
        }

        /// <summary>
        /// Gets the image Uri for the specified tile indices and zoom level.
        /// </summary>
        public virtual Uri GetUri(int x, int y, int zoomLevel)
        {
            if (getUri == null)
            {
                return null;
            }

            var uri = getUri(x, y, zoomLevel);

            if (Subdomains != null && Subdomains.Length > 0)
            {
                subdomainIndex = (subdomainIndex + 1) % Subdomains.Length;

                uri = uri.Replace("{c}", Subdomains[subdomainIndex]);
            }

            return new Uri(uri, UriKind.RelativeOrAbsolute);
        }

        /// <summary>
        /// Loads a tile ImageSource asynchronously from GetUri(x, y, zoomLevel).
        /// </summary>
        public virtual async Task<ImageSource> LoadImageAsync(int x, int y, int zoomLevel)
        {
            ImageSource imageSource = null;
            var uri = GetUri(x, y, zoomLevel);

            if (uri != null)
            {
                imageSource = await ImageLoader.LoadImageAsync(uri);
            }

            return imageSource;
        }

        private string GetDefaultUri(int x, int y, int zoomLevel)
        {
            return uriFormat
                .Replace("{x}", x.ToString())
                .Replace("{y}", y.ToString())
                .Replace("{z}", zoomLevel.ToString());
        }

        private string GetTmsUri(int x, int y, int zoomLevel)
        {
            y = (1 << zoomLevel) - 1 - y;

            return uriFormat
                .Replace("{x}", x.ToString())
                .Replace("{v}", y.ToString())
                .Replace("{z}", zoomLevel.ToString());
        }

        private string GetQuadKeyUri(int x, int y, int zoomLevel)
        {
            if (zoomLevel < 1)
            {
                return null;
            }

            var quadkey = new char[zoomLevel];

            for (var z = zoomLevel - 1; z >= 0; z--, x /= 2, y /= 2)
            {
                quadkey[z] = (char)('0' + 2 * (y % 2) + (x % 2));
            }

            return uriFormat
                .Replace("{i}", new string(quadkey, zoomLevel - 1, 1))
                .Replace("{q}", new string(quadkey));
        }

        private string GetBoundingBoxUri(int x, int y, int zoomLevel)
        {
            var tileSize = 360d / (1 << zoomLevel); // tile width in degrees
            var west = MapProjection.MetersPerDegree * (x * tileSize - 180d);
            var east = MapProjection.MetersPerDegree * ((x + 1) * tileSize - 180d);
            var south = MapProjection.MetersPerDegree * (180d - (y + 1) * tileSize);
            var north = MapProjection.MetersPerDegree * (180d - y * tileSize);

            return uriFormat
                .Replace("{W}", west.ToString(CultureInfo.InvariantCulture))
                .Replace("{S}", south.ToString(CultureInfo.InvariantCulture))
                .Replace("{E}", east.ToString(CultureInfo.InvariantCulture))
                .Replace("{N}", north.ToString(CultureInfo.InvariantCulture))
                .Replace("{X}", MapProjection.TileSize.ToString())
                .Replace("{Y}", MapProjection.TileSize.ToString());
        }

        private string GetLatLonBoundingBoxUri(int x, int y, int zoomLevel)
        {
            var tileSize = 360d / (1 << zoomLevel); // tile width in degrees
            var west = x * tileSize - 180d;
            var east = (x + 1) * tileSize - 180d;
            var south = WebMercatorProjection.YToLatitude(180d - (y + 1) * tileSize);
            var north = WebMercatorProjection.YToLatitude(180d - y * tileSize);

            return uriFormat
                .Replace("{w}", west.ToString(CultureInfo.InvariantCulture))
                .Replace("{s}", south.ToString(CultureInfo.InvariantCulture))
                .Replace("{e}", east.ToString(CultureInfo.InvariantCulture))
                .Replace("{n}", north.ToString(CultureInfo.InvariantCulture))
                .Replace("{X}", MapProjection.TileSize.ToString())
                .Replace("{Y}", MapProjection.TileSize.ToString());
        }
    }
}

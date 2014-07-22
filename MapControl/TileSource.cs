// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2014 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Globalization;
using System.Text;
#if WINDOWS_RUNTIME
using Windows.Foundation;
#else
using System.Windows;
#endif

namespace MapControl
{
    /// <summary>
    /// Provides the URI of a map tile.
    /// </summary>
    public partial class TileSource
    {
        public const int TileSize = 256;
        public const double MetersPerDegree = 6378137d * Math.PI / 180d; // WGS 84 semi major axis

        private Func<int, int, int, Uri> getUri;
        private string uriFormat = string.Empty;

        public TileSource()
        {
        }

        public TileSource(string uriFormat)
            : this()
        {
            UriFormat = uriFormat;
        }

        public string UriFormat
        {
            get { return uriFormat; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("The value of the UriFormat property must not be null or empty or white-space only.", "value");
                }

                uriFormat = value;

                if (uriFormat.Contains("{x}") && uriFormat.Contains("{y}") && uriFormat.Contains("{z}"))
                {
                    if (uriFormat.Contains("{c}"))
                    {
                        getUri = GetOpenStreetMapUri;
                    }
                    else if (uriFormat.Contains("{i}"))
                    {
                        getUri = GetGoogleMapsUri;
                    }
                    else if (uriFormat.Contains("{n}"))
                    {
                        getUri = GetMapQuestUri;
                    }
                    else
                    {
                        getUri = GetBasicUri;
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
                else if (uriFormat.Contains("{x}") && uriFormat.Contains("{v}") && uriFormat.Contains("{z}"))
                {
                    getUri = GetTmsUri;
                }
            }
        }

        public virtual Uri GetUri(int x, int y, int zoomLevel)
        {
            return getUri != null ? getUri(x, y, zoomLevel) : null;
        }

        private Uri GetBasicUri(int x, int y, int zoomLevel)
        {
            return new Uri(uriFormat.
                Replace("{x}", x.ToString()).
                Replace("{y}", y.ToString()).
                Replace("{z}", zoomLevel.ToString()));
        }

        private Uri GetOpenStreetMapUri(int x, int y, int zoomLevel)
        {
            var hostIndex = (x + y) % 3;

            return new Uri(uriFormat.
                Replace("{c}", "abc".Substring(hostIndex, 1)).
                Replace("{x}", x.ToString()).
                Replace("{y}", y.ToString()).
                Replace("{z}", zoomLevel.ToString()));
        }

        private Uri GetGoogleMapsUri(int x, int y, int zoomLevel)
        {
            var hostIndex = (x + y) % 4;

            return new Uri(uriFormat.
                Replace("{i}", hostIndex.ToString()).
                Replace("{x}", x.ToString()).
                Replace("{y}", y.ToString()).
                Replace("{z}", zoomLevel.ToString()));
        }

        private Uri GetMapQuestUri(int x, int y, int zoomLevel)
        {
            var hostIndex = (x + y) % 4 + 1;

            return new Uri(uriFormat.
                Replace("{n}", hostIndex.ToString()).
                Replace("{x}", x.ToString()).
                Replace("{y}", y.ToString()).
                Replace("{z}", zoomLevel.ToString()));
        }

        private Uri GetTmsUri(int x, int y, int zoomLevel)
        {
            y = (1 << zoomLevel) - 1 - y;

            return new Uri(uriFormat.
                Replace("{x}", x.ToString()).
                Replace("{v}", y.ToString()).
                Replace("{z}", zoomLevel.ToString()));
        }

        private Uri GetQuadKeyUri(int x, int y, int zoomLevel)
        {
            if (zoomLevel < 1)
            {
                return null;
            }

            var key = new StringBuilder { Length = zoomLevel };

            for (var z = zoomLevel - 1; z >= 0; z--, x /= 2, y /= 2)
            {
                key[z] = (char)('0' + 2 * (y % 2) + (x % 2));
            }

            return new Uri(uriFormat.
                Replace("{i}", key.ToString(key.Length - 1, 1)).
                Replace("{q}", key.ToString()));
        }

        private Uri GetBoundingBoxUri(int x, int y, int zoomLevel)
        {
            var n = (double)(1 << zoomLevel);
            var x1 = MetersPerDegree * ((double)x * 360d / n - 180d);
            var x2 = MetersPerDegree * ((double)(x + 1) * 360d / n - 180d);
            var y1 = MetersPerDegree * (180d - (double)(y + 1) * 360d / n);
            var y2 = MetersPerDegree * (180d - (double)y * 360d / n);

            return new Uri(uriFormat.
                Replace("{W}", x1.ToString(CultureInfo.InvariantCulture)).
                Replace("{S}", y1.ToString(CultureInfo.InvariantCulture)).
                Replace("{E}", x2.ToString(CultureInfo.InvariantCulture)).
                Replace("{N}", y2.ToString(CultureInfo.InvariantCulture)));
        }

        private Uri GetLatLonBoundingBoxUri(int x, int y, int zoomLevel)
        {
            var t = new MercatorTransform();
            var n = (double)(1 << zoomLevel);
            var x1 = (double)x * 360d / n - 180d;
            var x2 = (double)(x + 1) * 360d / n - 180d;
            var y1 = 180d - (double)(y + 1) * 360d / n;
            var y2 = 180d - (double)y * 360d / n;
            var p1 = t.Transform(new Point(x1, y1));
            var p2 = t.Transform(new Point(x2, y2));

            return new Uri(uriFormat.
                Replace("{w}", p1.Longitude.ToString(CultureInfo.InvariantCulture)).
                Replace("{s}", p1.Latitude.ToString(CultureInfo.InvariantCulture)).
                Replace("{e}", p2.Longitude.ToString(CultureInfo.InvariantCulture)).
                Replace("{n}", p2.Latitude.ToString(CultureInfo.InvariantCulture)));
        }
    }
}

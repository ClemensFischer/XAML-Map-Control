// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2013 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Globalization;
using System.Text;
#if NETFX_CORE
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
        private Func<int, int, int, Uri> getUri;
        private string uriFormat = string.Empty;
        private int hostIndex = -1;

        public TileSource()
        {
        }

        public TileSource(string uriFormat)
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
                    throw new ArgumentException("The value of the UriFormat property must not be null or empty or white-space only.");
                }

                uriFormat = value;
            }
        }

        public virtual Uri GetUri(int x, int y, int zoomLevel)
        {
            if (getUri == null)
            {
                SelectGetUriMethod();
            }

            return getUri(x, y, zoomLevel);
        }

        private void SelectGetUriMethod()
        {
            getUri = GetDefaultUri;

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
            }
            else if (uriFormat.Contains("{q}")) // {i} is optional
            {
                getUri = GetQuadKeyUri;
            }
            else if (uriFormat.Contains("{w}") && uriFormat.Contains("{s}") && uriFormat.Contains("{e}") && uriFormat.Contains("{n}"))
            {
                getUri = GetBoundingBoxUri;
            }
        }

        private Uri GetDefaultUri(int x, int y, int zoomLevel)
        {
            return new Uri(UriFormat.
                Replace("{x}", x.ToString()).
                Replace("{y}", y.ToString()).
                Replace("{z}", zoomLevel.ToString()));
        }

        private Uri GetOpenStreetMapUri(int x, int y, int zoomLevel)
        {
            lock (getUri) // protect hostIndex
            {
                hostIndex = (hostIndex + 1) % 3;

                return new Uri(UriFormat.
                    Replace("{c}", "abc".Substring(hostIndex, 1)).
                    Replace("{x}", x.ToString()).
                    Replace("{y}", y.ToString()).
                    Replace("{z}", zoomLevel.ToString()));
            }
        }

        private Uri GetGoogleMapsUri(int x, int y, int zoomLevel)
        {
            lock (getUri) // protect hostIndex
            {
                hostIndex = (hostIndex + 1) % 4;

                return new Uri(UriFormat.
                    Replace("{i}", hostIndex.ToString()).
                    Replace("{x}", x.ToString()).
                    Replace("{y}", y.ToString()).
                    Replace("{z}", zoomLevel.ToString()));
            }
        }

        private Uri GetMapQuestUri(int x, int y, int zoomLevel)
        {
            lock (getUri) // protect hostIndex
            {
                hostIndex = (hostIndex % 4) + 1;

                return new Uri(UriFormat.
                    Replace("{n}", hostIndex.ToString()).
                    Replace("{x}", x.ToString()).
                    Replace("{y}", y.ToString()).
                    Replace("{z}", zoomLevel.ToString()));
            }
        }

        private Uri GetQuadKeyUri(int x, int y, int zoomLevel)
        {
            var key = new StringBuilder { Length = zoomLevel };

            for (var z = zoomLevel - 1; z >= 0; z--, x /= 2, y /= 2)
            {
                key[z] = (char)('0' + 2 * (y % 2) + (x % 2));
            }

            return new Uri(UriFormat.
                Replace("{i}", key.ToString(key.Length - 1, 1)).
                Replace("{q}", key.ToString()));
        }

        private Uri GetBoundingBoxUri(int x, int y, int zoomLevel)
        {
            var t = new MercatorTransform();
            var n = (double)(1 << zoomLevel);
            var x1 = (double)x * 360d / n - 180d;
            var x2 = (double)(x + 1) * 360d / n - 180d;
            var y1 = 180d - (double)(y + 1) * 360d / n;
            var y2 = 180d - (double)y * 360d / n;
            var p1 = t.Transform(new Point(x1, y1));
            var p2 = t.Transform(new Point(x2, y2));

            return new Uri(UriFormat.
                Replace("{w}", p1.Longitude.ToString(CultureInfo.InvariantCulture)).
                Replace("{s}", p1.Latitude.ToString(CultureInfo.InvariantCulture)).
                Replace("{e}", p2.Longitude.ToString(CultureInfo.InvariantCulture)).
                Replace("{n}", p2.Latitude.ToString(CultureInfo.InvariantCulture)));
        }
    }
}

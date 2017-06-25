﻿// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2017 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Globalization;

namespace MapControl
{
    /// <summary>
    /// Provides the URI of a map tile.
    /// </summary>
    public partial class TileSource
    {
        public const int TileSize = 256;

        private Func<int, int, int, Uri> getUri;
        private string uriFormat = string.Empty;

        public TileSource()
        {
        }

        protected TileSource(string uriFormat)
        {
            this.uriFormat = uriFormat;
        }

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
                else if (uriFormat.Contains("{x}") && uriFormat.Contains("{v}") && uriFormat.Contains("{z}"))
                {
                    getUri = GetTmsUri;
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
            }
        }

        public virtual Uri GetUri(int x, int y, int zoomLevel)
        {
            return getUri?.Invoke(x, y, zoomLevel);
        }

        private Uri GetBasicUri(int x, int y, int zoomLevel)
        {
            return new Uri(uriFormat
                .Replace("{x}", x.ToString())
                .Replace("{y}", y.ToString())
                .Replace("{z}", zoomLevel.ToString()),
                UriKind.RelativeOrAbsolute);
        }

        private Uri GetOpenStreetMapUri(int x, int y, int zoomLevel)
        {
            var hostIndex = (x + y) % 3;

            return new Uri(uriFormat
                .Replace("{c}", "abc".Substring(hostIndex, 1))
                .Replace("{x}", x.ToString())
                .Replace("{y}", y.ToString())
                .Replace("{z}", zoomLevel.ToString()),
                UriKind.RelativeOrAbsolute);
        }

        private Uri GetGoogleMapsUri(int x, int y, int zoomLevel)
        {
            var hostIndex = (x + y) % 4;

            return new Uri(uriFormat
                .Replace("{i}", hostIndex.ToString())
                .Replace("{x}", x.ToString())
                .Replace("{y}", y.ToString())
                .Replace("{z}", zoomLevel.ToString()),
                UriKind.RelativeOrAbsolute);
        }

        private Uri GetMapQuestUri(int x, int y, int zoomLevel)
        {
            var hostIndex = (x + y) % 4 + 1;

            return new Uri(uriFormat
                .Replace("{n}", hostIndex.ToString())
                .Replace("{x}", x.ToString())
                .Replace("{y}", y.ToString())
                .Replace("{z}", zoomLevel.ToString()),
                UriKind.RelativeOrAbsolute);
        }

        private Uri GetTmsUri(int x, int y, int zoomLevel)
        {
            y = (1 << zoomLevel) - 1 - y;

            return new Uri(uriFormat
                .Replace("{x}", x.ToString())
                .Replace("{v}", y.ToString())
                .Replace("{z}", zoomLevel.ToString()),
                UriKind.RelativeOrAbsolute);
        }

        private Uri GetQuadKeyUri(int x, int y, int zoomLevel)
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

            return new Uri(uriFormat
                .Replace("{i}", new string(quadkey, zoomLevel - 1, 1))
                .Replace("{q}", new string(quadkey)),
                UriKind.RelativeOrAbsolute);
        }

        private Uri GetBoundingBoxUri(int x, int y, int zoomLevel)
        {
            var tileSize = 360d / (1 << zoomLevel); // tile width in degrees
            var west = MapProjection.MetersPerDegree * (x * tileSize - 180d);
            var east = MapProjection.MetersPerDegree * ((x + 1) * tileSize - 180d);
            var south = MapProjection.MetersPerDegree * (180d - (y + 1) * tileSize);
            var north = MapProjection.MetersPerDegree * (180d - y * tileSize);

            return new Uri(uriFormat
                .Replace("{W}", west.ToString(CultureInfo.InvariantCulture))
                .Replace("{S}", south.ToString(CultureInfo.InvariantCulture))
                .Replace("{E}", east.ToString(CultureInfo.InvariantCulture))
                .Replace("{N}", north.ToString(CultureInfo.InvariantCulture))
                .Replace("{X}", TileSize.ToString())
                .Replace("{Y}", TileSize.ToString()));
        }

        private Uri GetLatLonBoundingBoxUri(int x, int y, int zoomLevel)
        {
            var tileSize = 360d / (1 << zoomLevel); // tile width in degrees
            var west = x * tileSize - 180d;
            var east = (x + 1) * tileSize - 180d;
            var south = WebMercatorProjection.YToLatitude(180d - (y + 1) * tileSize);
            var north = WebMercatorProjection.YToLatitude(180d - y * tileSize);

            return new Uri(uriFormat
                .Replace("{w}", west.ToString(CultureInfo.InvariantCulture))
                .Replace("{s}", south.ToString(CultureInfo.InvariantCulture))
                .Replace("{e}", east.ToString(CultureInfo.InvariantCulture))
                .Replace("{n}", north.ToString(CultureInfo.InvariantCulture))
                .Replace("{X}", TileSize.ToString())
                .Replace("{Y}", TileSize.ToString()));
        }
    }
}

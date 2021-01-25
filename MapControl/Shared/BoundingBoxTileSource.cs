// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Globalization;

namespace MapControl
{
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

                if (UriFormat.Contains("{bbox}"))
                {
                    uri = new Uri(UriFormat.Replace("{bbox}",
                        string.Format(CultureInfo.InvariantCulture, "{0:F2},{1:F2},{2:F2},{3:F2}", west, south, east, north)));
                }
                else
                {
                    uri = new Uri(UriFormat
                        .Replace("{west}", west.ToString("F2", CultureInfo.InvariantCulture))
                        .Replace("{south}", south.ToString("F2", CultureInfo.InvariantCulture))
                        .Replace("{east}", east.ToString("F2", CultureInfo.InvariantCulture))
                        .Replace("{north}", north.ToString("F2", CultureInfo.InvariantCulture)));
                }
            }

            return uri;
        }
    }
}

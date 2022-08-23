// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
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

            if (UriTemplate != null)
            {
                var tileSize = 360d / (1 << zoomLevel); // tile width in degrees
                var west = MapProjection.Wgs84MeterPerDegree * (x * tileSize - 180d);
                var east = MapProjection.Wgs84MeterPerDegree * ((x + 1) * tileSize - 180d);
                var south = MapProjection.Wgs84MeterPerDegree * (180d - (y + 1) * tileSize);
                var north = MapProjection.Wgs84MeterPerDegree * (180d - y * tileSize);

                if (UriTemplate.Contains("{bbox}"))
                {
                    uri = new Uri(UriTemplate.Replace("{bbox}",
                        string.Format(CultureInfo.InvariantCulture, "{0:F2},{1:F2},{2:F2},{3:F2}", west, south, east, north)));
                }
                else
                {
                    uri = new Uri(UriTemplate
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

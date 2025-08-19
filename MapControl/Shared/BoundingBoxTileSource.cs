using System;
using System.Globalization;

namespace MapControl
{
    public class BoundingBoxTileSource : TileSource
    {
        public override Uri GetUri(int column, int row, int zoomLevel)
        {
            GetTileBounds(column, row, zoomLevel, out double west, out double south, out double east, out double north);

            return GetUri(west, south, east, north);
        }

        protected virtual Uri GetUri(double west, double south, double east, double north)
        {
            Uri uri = null;

            if (UriTemplate != null)
            {
                var w = west.ToString("F2", CultureInfo.InvariantCulture);
                var e = east.ToString("F2", CultureInfo.InvariantCulture);
                var s = south.ToString("F2", CultureInfo.InvariantCulture);
                var n = north.ToString("F2", CultureInfo.InvariantCulture);

                uri = UriTemplate.Contains("{bbox}")
                    ? new Uri(UriTemplate.Replace("{bbox}", $"{w},{s},{e},{n}"))
                    : new Uri(UriTemplate.Replace("{west}", w).Replace("{south}", s).Replace("{east}", e).Replace("{north}", n));
            }

            return uri;
        }

        /// <summary>
        /// Gets the bounding box in meters of a standard Web Mercator tile,
        /// specified by grid column and row indices and zoom level.
        /// </summary>
        public static void GetTileBounds(int column, int row, int zoomLevel,
            out double west, out double south, out double east, out double north)
        {
            var tileSize = 360d / (1 << zoomLevel); // tile size in degrees

            west = MapProjection.Wgs84MeterPerDegree * (column * tileSize - 180d);
            east = MapProjection.Wgs84MeterPerDegree * ((column + 1) * tileSize - 180d);
            south = MapProjection.Wgs84MeterPerDegree * (180d - (row + 1) * tileSize);
            north = MapProjection.Wgs84MeterPerDegree * (180d - row * tileSize);
        }
    }
}

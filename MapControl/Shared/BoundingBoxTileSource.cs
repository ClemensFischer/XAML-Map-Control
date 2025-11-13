using System;
using System.Globalization;
using System.Text;

namespace MapControl
{
    public class BoundingBoxTileSource : UriTileSource
    {
        public override Uri GetUri(int zoomLevel, int column, int row)
        {
            GetTileBounds(zoomLevel, column, row, out double west, out double south, out double east, out double north);

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

                if (UriTemplate.Contains("{bbox}"))
                {
                    uri = new Uri(UriTemplate.Replace("{bbox}", $"{w},{s},{e},{n}"));
                }
                else
                {
                    var uriBuilder = new StringBuilder(UriTemplate);

                    uriBuilder.Replace("{west}", w);
                    uriBuilder.Replace("{south}", s);
                    uriBuilder.Replace("{east}", e);
                    uriBuilder.Replace("{north}", n);

                    uri = new Uri(uriBuilder.ToString());
                }
            }

            return uri;
        }

        /// <summary>
        /// Gets the bounding box in meters of a standard Web Mercator tile,
        /// specified by zoom level and grid column and row indices.
        /// </summary>
        public static void GetTileBounds(int zoomLevel, int column, int row,
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

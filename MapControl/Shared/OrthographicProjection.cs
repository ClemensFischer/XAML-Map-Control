using System;
#if WPF
using System.Windows;
#elif AVALONIA
using Avalonia;
#endif

namespace MapControl
{
    /// <summary>
    /// Spherical Orthographic Projection - AUTO2:42003.
    /// See "Map Projections - A Working Manual" (https://pubs.usgs.gov/publication/pp1395), p.148-150.
    /// </summary>
    public class OrthographicProjection : AzimuthalProjection
    {
        public const string DefaultCrsId = "AUTO2:42003";

        public OrthographicProjection()
            : this(DefaultCrsId)
        {
            // XAML needs parameterless constructor
        }

        public OrthographicProjection(string crsId)
        {
            CrsId = crsId;
        }

        public override Point? LocationToMap(double latitude, double longitude)
        {
            if (Location.Equals(latitude, Center.Latitude) &&
                Location.Equals(longitude, Center.Longitude))
            {
                return new Point();
            }

            var lat0 = Center.Latitude * Math.PI / 180d;
            var lat = latitude * Math.PI / 180d;
            var dLon = (longitude - Center.Longitude) * Math.PI / 180d;

            if (Math.Abs(lat - lat0) > Math.PI / 2d || Math.Abs(dLon) > Math.PI / 2d)
            {
                return null;
            }

            return new Point(
                Wgs84MeanRadius * Math.Cos(lat) * Math.Sin(dLon),
                Wgs84MeanRadius * (Math.Cos(lat0) * Math.Sin(lat) - Math.Sin(lat0) * Math.Cos(lat) * Math.Cos(dLon)));
        }

        public override Location MapToLocation(double x, double y)
        {
            if (x == 0d && y == 0d)
            {
                return new Location(Center.Latitude, Center.Longitude);
            }

            x /= Wgs84MeanRadius;
            y /= Wgs84MeanRadius;
            var r2 = x * x + y * y;

            if (r2 > 1d)
            {
                return null;
            }

            var r = Math.Sqrt(r2);
            var sinC = r;
            var cosC = Math.Sqrt(1 - r2);

            var lat0 = Center.Latitude * Math.PI / 180d;
            var cosLat0 = Math.Cos(lat0);
            var sinLat0 = Math.Sin(lat0);

            return new Location(
                180d / Math.PI * Math.Asin(cosC * sinLat0 + y * sinC * cosLat0 / r),
                180d / Math.PI * Math.Atan2(x * sinC, r * cosC * cosLat0 - y * sinC * sinLat0) + Center.Longitude);
        }
    }
}

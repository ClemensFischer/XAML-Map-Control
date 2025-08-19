using System;
#if WPF
using System.Windows;
#elif AVALONIA
using Avalonia;
#endif

namespace MapControl
{
    /// <summary>
    /// Spherical Stereographic Projection - AUTO2:97002.
    /// See "Map Projections - A Working Manual" (https://pubs.usgs.gov/publication/pp1395), p.157-160.
    /// </summary>
    public class StereographicProjection : AzimuthalProjection
    {
        public const string DefaultCrsId = "AUTO2:97002"; // GeoServer non-standard CRS ID

        public StereographicProjection()
            : this(DefaultCrsId)
        {
            // XAML needs parameterless constructor
        }

        public StereographicProjection(string crsId)
        {
            CrsId = crsId;
        }

        public override Point? LocationToMap(Location location)
        {
            if (location.Equals(Center))
            {
                return new Point();
            }

            Center.GetAzimuthDistance(location, out double azimuth, out double distance);

            var mapDistance = Math.Tan(distance / 2d) * 2d * Wgs84EquatorialRadius;

            return new Point(mapDistance * Math.Sin(azimuth), mapDistance * Math.Cos(azimuth));
        }

        public override Location MapToLocation(Point point)
        {
            if (point.X == 0d && point.Y == 0d)
            {
                return new Location(Center.Latitude, Center.Longitude);
            }

            var azimuth = Math.Atan2(point.X, point.Y);
            var mapDistance = Math.Sqrt(point.X * point.X + point.Y * point.Y);

            var distance = 2d * Math.Atan(mapDistance / (2d * Wgs84EquatorialRadius));

            return Center.GetLocation(azimuth, distance);
        }
    }
}

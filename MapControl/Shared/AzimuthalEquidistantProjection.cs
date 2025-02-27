using System;
#if WPF
using System.Windows;
#endif

namespace MapControl
{
    /// <summary>
    /// Spherical Azimuthal Equidistant Projection - No standard CRS ID.
    /// See "Map Projections - A Working Manual" (https://pubs.usgs.gov/publication/pp1395), p.195-197.
    /// </summary>
    public class AzimuthalEquidistantProjection : AzimuthalProjection
    {
        public const string DefaultCrsId = "AUTO2:97003"; // proprietary CRS ID

        public AzimuthalEquidistantProjection()
            : this(DefaultCrsId)
        {
            // XAML needs parameterless constructor
        }

        public AzimuthalEquidistantProjection(string crsId)
        {
            CrsId = crsId;
        }

        public override Point? LocationToMap(Location location)
        {
            if (location.Equals(Center))
            {
                return new Point();
            }

            GetAzimuthDistance(Center, location, out double azimuth, out double distance);

            var mapDistance = distance * Wgs84EquatorialRadius;

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

            var distance = mapDistance / Wgs84EquatorialRadius;

            return GetLocation(Center, azimuth, distance);
        }
    }
}

using System;
#if WPF
using System.Windows;
#elif AVALONIA
using Avalonia;
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

        public override Point? LocationToMap(double latitude, double longitude)
        {
            if (Location.Equals(latitude, Center.Latitude) &&
                Location.Equals(longitude, Center.Longitude))
            {
                return new Point();
            }

            Center.GetAzimuthDistance(latitude, longitude, out double azimuth, out double distance);

            var mapDistance = distance * Wgs84EquatorialRadius;

            return new Point(mapDistance * Math.Sin(azimuth), mapDistance * Math.Cos(azimuth));
        }

        public override Location MapToLocation(double x, double y)
        {
            if (x == 0d && y == 0d)
            {
                return new Location(Center.Latitude, Center.Longitude);
            }

            var azimuth = Math.Atan2(x, y);
            var mapDistance = Math.Sqrt(x * x + y * y);
            var distance = mapDistance / Wgs84EquatorialRadius;

            return Center.GetLocation(azimuth, distance);
        }
    }
}

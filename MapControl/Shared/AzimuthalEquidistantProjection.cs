using System;
#if WPF
using System.Windows;
#elif AVALONIA
using Avalonia;
#endif

namespace MapControl
{
    /// <summary>
    /// Spherical Azimuthal Equidistant Projection - No standard CRS identifier.
    /// See "Map Projections - A Working Manual" (https://pubs.usgs.gov/publication/pp1395), p.195-197.
    /// </summary>
    public class AzimuthalEquidistantProjection : MapProjection
    {
        public const string DefaultCrsId = "AUTO2:97003"; // proprietary CRS identifier

        public AzimuthalEquidistantProjection() // parameterless constructor for XAML
            : this(DefaultCrsId)
        {
        }

        public AzimuthalEquidistantProjection(string crsId)
        {
            Type = MapProjectionType.Azimuthal;
            CrsId = crsId;
        }

        public double EarthRadius { get; set; } = Wgs84MeanRadius;

        public override Point? LocationToMap(double latitude, double longitude)
        {
            if (Center.Equals(latitude, longitude))
            {
                return new Point();
            }

            Center.GetAzimuthDistance(latitude, longitude, out double azimuth, out double distance);

            var mapDistance = distance * EarthRadius;

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
            var distance = mapDistance / EarthRadius;

            return Center.GetLocation(azimuth, distance);
        }
    }
}

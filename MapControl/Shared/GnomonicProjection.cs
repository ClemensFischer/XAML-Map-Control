using System;
#if WPF
using System.Windows;
#elif AVALONIA
using Avalonia;
#endif

namespace MapControl
{
    /// <summary>
    /// Spherical Gnomonic Projection - AUTO2:97001.
    /// See "Map Projections - A Working Manual" (https://pubs.usgs.gov/publication/pp1395), p.165-167.
    /// </summary>
    public class GnomonicProjection : MapProjection
    {
        public const string DefaultCrsId = "AUTO2:97001"; // GeoServer non-standard CRS identifier

        public GnomonicProjection() // parameterless constructor for XAML
            : this(DefaultCrsId)
        {
        }

        public GnomonicProjection(string crsId)
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

            var mapDistance = distance < Math.PI / 2d
                ? Math.Tan(distance) * EarthRadius
                : double.PositiveInfinity;

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
            var distance = Math.Atan(mapDistance / EarthRadius);

            return Center.GetLocation(azimuth, distance);
        }
    }
}

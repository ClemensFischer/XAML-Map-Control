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
        public const string DefaultCrsId = "AUTO2:97002"; // GeoServer non-standard CRS identifier

        public StereographicProjection() // parameterless constructor for XAML
            : this(DefaultCrsId)
        {
        }

        public StereographicProjection(string crsId)
        {
            CrsId = crsId;
        }

        public override Point? LocationToMap(double latitude, double longitude)
        {
            if (Center.Equals(latitude, longitude))
            {
                return new Point();
            }

            Center.GetAzimuthDistance(latitude, longitude, out double azimuth, out double distance);

            var mapDistance = Math.Tan(distance / 2d) * 2d * Wgs84MeanRadius;

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
            var distance = 2d * Math.Atan(mapDistance / (2d * Wgs84MeanRadius));

            return Center.GetLocation(azimuth, distance);
        }
    }
}

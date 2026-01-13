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

        public OrthographicProjection() // parameterless constructor for XAML
            : this(DefaultCrsId)
        {
        }

        public OrthographicProjection(string crsId)
        {
            CrsId = crsId;
        }

        public override Point? LocationToMap(double latitude, double longitude)
        {
            if (Center.Equals(latitude, longitude))
            {
                return new Point();
            }

            var phi = latitude * Math.PI / 180d;
            var phi1 = Center.Latitude * Math.PI / 180d;
            var lambda = (longitude - Center.Longitude) * Math.PI / 180d; // λ - λ0

            if (Math.Abs(phi - phi1) > Math.PI / 2d || Math.Abs(lambda) > Math.PI / 2d)
            {
                return null;
            }

            var x = Wgs84MeanRadius * Math.Cos(phi) * Math.Sin(lambda); // p.149 (20-3)
            var y = Wgs84MeanRadius * (Math.Cos(phi1) * Math.Sin(phi) -
                                       Math.Sin(phi1) * Math.Cos(phi) * Math.Cos(lambda)); // p.149 (20-4)
            return new Point(x, y);
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

            var phi1 = Center.Latitude * Math.PI / 180d;
            var cosPhi1 = Math.Cos(phi1);
            var sinPhi1 = Math.Sin(phi1);

            var phi = Math.Asin(cosC * sinPhi1 + y * sinC * cosPhi1 / r); // p.150 (20-14)
            var lambda = Math.Atan2(x * sinC, r * cosC * cosPhi1 - y * sinC * sinPhi1); // p.150 (20-15)

            return new Location(180d / Math.PI * phi, 180d / Math.PI * lambda + Center.Longitude);
        }
    }
}

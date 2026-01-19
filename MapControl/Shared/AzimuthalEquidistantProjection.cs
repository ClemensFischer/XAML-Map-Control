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
    public class AzimuthalEquidistantProjection : AzimuthalProjection
    {
        public const string DefaultCrsId = "AUTO2:97003"; // proprietary CRS identifier

        public AzimuthalEquidistantProjection() // parameterless constructor for XAML
            : this(DefaultCrsId)
        {
        }

        public AzimuthalEquidistantProjection(string crsId)
        {
            CrsId = crsId;
        }

        public override Point RelativeScale(double latitude, double longitude)
        {
            (var cosC, var _, var _) = GetPointValues(latitude, longitude);
            var k = 1d;

            if (cosC < 1d)
            {
                var c = Math.Acos(cosC);
                k = c / Math.Sin(c); // p.195 (25-2)
            }

            return new Point(k, k);
        }

        public override Point? LocationToMap(double latitude, double longitude)
        {
            (var cosC, var x, var y) = GetPointValues(latitude, longitude);
            var k = 1d;

            if (cosC < 1d)
            {
                var c = Math.Acos(cosC);
                k = c / Math.Sin(c); // p.195 (25-2)
            }

            return new Point(EarthRadius * k * x, EarthRadius * k * y); // p.195 (22-4/5)
        }

        public override Location MapToLocation(double x, double y)
        {
            var rho = Math.Sqrt(x * x + y * y);
            var c = rho / EarthRadius; // p.196 (25-15)

            return GetLocation(x, y, rho, Math.Sin(c));
        }
    }
}

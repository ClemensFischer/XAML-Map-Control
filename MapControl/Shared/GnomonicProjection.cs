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
    public class GnomonicProjection : AzimuthalProjection
    {
        public const string DefaultCrsId = "AUTO2:97001"; // GeoServer non-standard CRS identifier

        public GnomonicProjection() // parameterless constructor for XAML
            : this(DefaultCrsId)
        {
        }

        public GnomonicProjection(string crsId)
        {
            CrsId = crsId;
        }

        public override Point RelativeScale(double latitude, double longitude)
        {
            (var cosC, var _, var _) = GetPointValues(latitude, longitude);
            var h = 1d / (cosC * cosC); // p.165 (22-2)

            return new Point(h, h); // TODO: rotate
        }

        public override Point? LocationToMap(double latitude, double longitude)
        {
            (var cosC, var x, var y) = GetPointValues(latitude, longitude);

            if (cosC <= 0d)
            {
                return null;
            }

            var k = 1d / cosC; // p.165 (22-3)

            return new Point(EarthRadius * k * x, EarthRadius * k * y); // p.165 (22-4/5)
        }

        public override Location MapToLocation(double x, double y)
        {
            var rho = Math.Sqrt(x * x + y * y);
            var c = Math.Atan(rho / EarthRadius); // p.167 (22-16)

            return GetLocation(x, y, rho, Math.Sin(c));
        }
    }
}

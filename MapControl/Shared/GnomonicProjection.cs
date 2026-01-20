using System;
#if WPF
using System.Windows;
using System.Windows.Media;
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

        public override Matrix RelativeScale(double latitude, double longitude)
        {
            (var cosC, var x, var y) = GetPointValues(latitude, longitude);
            var k = 1d / cosC; // p.165 (22-3)
            var h = k * k; // p.165 (22-2)

            var scale = new Matrix(h, 0d, 0d, k, 0d, 0d);
            scale.Rotate(-Math.Atan2(y, x) * 180d / Math.PI);
            return scale;
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

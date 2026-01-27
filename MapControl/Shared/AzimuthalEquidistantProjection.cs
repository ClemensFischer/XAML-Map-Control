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
    /// Spherical Azimuthal Equidistant Projection - AUTO2:97003.
    /// See "Map Projections - A Working Manual" (https://pubs.usgs.gov/publication/pp1395), p.195-197.
    /// </summary>
    public class AzimuthalEquidistantProjection : AzimuthalProjection
    {
        public const string DefaultCrsId = "AUTO2:97003"; // GeoServer non-standard CRS identifier

        public AzimuthalEquidistantProjection() // parameterless constructor for XAML
            : this(DefaultCrsId)
        {
        }

        public AzimuthalEquidistantProjection(string crsId)
        {
            CrsId = crsId;
        }

        public override Matrix RelativeTransform(double latitude, double longitude)
        {
            var p = GetProjectedPoint(latitude, longitude);

            if (p.CosC == 1d)
            {
                return new Matrix(1d, 0d, 0d, 1d, 0d, 0d);
            }

            if (p.CosC == -1d)
            {
                return new Matrix(1d, 0d, 0d, double.PositiveInfinity, 0d, 0d);
            }

            var c = Math.Acos(p.CosC);
            var k = c / Math.Sin(c); // p.195 (25-2)

            return p.RelativeScale(1d, k);
        }

        public override Point? LocationToMap(double latitude, double longitude)
        {
            var p = GetProjectedPoint(latitude, longitude);

            if (p.CosC == 1d) // p.195 "If cos c = 1, ... k' = 1, and x = y = 0."
            {
                return new Point();
            }

            if (p.CosC == -1)
            {
                return null; // p.195 "If cos c = -1, the point ... is plotted as a circle of radius πR."
            }

            var c = Math.Acos(p.CosC);
            var k = c / Math.Sin(c); // p.195 (25-2)

            return new Point(EquatorialRadius * k * p.X, EquatorialRadius * k * p.Y); // p.195 (22-4/5)
        }

        public override Location MapToLocation(double x, double y)
        {
            var rho = Math.Sqrt(x * x + y * y);
            var c = rho / EquatorialRadius; // p.196 (25-15)

            return GetLocation(x, y, rho, Math.Sin(c));
        }
    }
}

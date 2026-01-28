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

        public override Matrix RelativeTransform(double latitude, double longitude)
        {
            var p = GetProjectedPoint(latitude, longitude);
            var k = 1d / p.CosC; // p.165 (22-3)
            var h = k * k; // p.165 (22-2)
            (var scaleX, var scaleY) = p.RelativeScale(h, k);

            return RelativeTransform(latitude, longitude, scaleX, scaleY);
        }

        public override Point? LocationToMap(double latitude, double longitude)
        {
            var p = GetProjectedPoint(latitude, longitude);

            if (p.CosC <= 0d) // p.167 "If cos c is zero or negative, the point is to be rejected."
            {
                return null;
            }

            var k = 1d / p.CosC; // p.165 (22-3)

            return new Point(EquatorialRadius * k * p.X, EquatorialRadius * k * p.Y); // p.165 (22-4/5)
        }

        public override Location MapToLocation(double x, double y)
        {
            var rho = Math.Sqrt(x * x + y * y);
            var c = Math.Atan(rho / EquatorialRadius); // p.167 (22-16)

            return GetLocation(x, y, rho, Math.Sin(c));
        }
    }
}

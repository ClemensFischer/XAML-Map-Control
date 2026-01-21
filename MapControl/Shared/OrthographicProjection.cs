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

        public override Matrix RelativeScale(double latitude, double longitude)
        {
            var p = GetProjectedPoint(latitude, longitude);
            var h = p.CosC; // p.149 (20-5)

            var scale = new Matrix(h, 0d, 0d, 1d, 0d, 0d); // k == 1
            scale.Rotate(-Math.Atan2(p.Y, p.X) * 180d / Math.PI);
            return scale;
        }

        public override Point? LocationToMap(double latitude, double longitude)
        {
            var p = GetProjectedPoint(latitude, longitude);

            if (p.CosC < 0d) // p.149 "If cos c is negative, the point is not to be plotted."
            {
                return null;
            }

            return new Point(EarthRadius * p.X, EarthRadius * p.Y); // p.149 (20-3/4)
        }

        public override Location MapToLocation(double x, double y)
        {
            var rho = Math.Sqrt(x * x + y * y);
            var sinC = rho / EarthRadius; // p.150 (20-19)

            return GetLocation(x, y, rho, sinC);
        }
    }
}

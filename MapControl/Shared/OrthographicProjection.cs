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
            (var cosC, var x, var y) = GetPointValues(latitude, longitude);
            var h = cosC; // p.149 (20-5)

            var scale = new Matrix(h, 0d, 0d, h, 0d, 0d);
            return scale;
        }

        public override Point? LocationToMap(double latitude, double longitude)
        {
            (var cosC, var x, var y) = GetPointValues(latitude, longitude);

            return cosC >= 0d ? new Point(EarthRadius * x, EarthRadius * y) : null; // p.149 (20-3/4)
        }

        public override Location MapToLocation(double x, double y)
        {
            var rho = Math.Sqrt(x * x + y * y);
            var sinC = rho / EarthRadius; // p.150 (20-19)

            return sinC <= 1d ? GetLocation(x, y, rho, sinC) : null;
        }
    }
}

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

        public override Matrix RelativeScale(double latitude, double longitude)
        {
            var p = GetProjectedPoint(latitude, longitude);
            var k = 2d / (1d + p.CosC); // p.157 (21-4), k0 == 1

            return p.RelativeScale(k, k);
        }

        public override Point? LocationToMap(double latitude, double longitude)
        {
            var p = GetProjectedPoint(latitude, longitude);
            var k = 2d / (1d + p.CosC); // p.157 (21-4), k0 == 1

            return new Point(EarthRadius * k * p.X, EarthRadius * k * p.Y); // p.157 (21-2/3)
        }

        public override Location MapToLocation(double x, double y)
        {
            var rho = Math.Sqrt(x * x + y * y);
            var c = 2d * Math.Atan(rho / (2d * EarthRadius)); // p.159 (21-15), k0 == 1

            return GetLocation(x, y, rho, Math.Sin(c));
        }
    }
}

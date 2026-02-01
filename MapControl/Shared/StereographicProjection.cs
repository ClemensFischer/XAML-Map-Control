using System;
using System.Globalization;
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
    public class StereographicProjection : MapProjection
    {
        public const string DefaultCrsId = "AUTO2:97002"; // GeoServer non-standard CRS identifier

        public StereographicProjection(double centerLongitude, double centerLatitude, string crsId = DefaultCrsId)
        {
            CentralMeridian = centerLongitude;
            LatitudeOfOrigin = centerLatitude;
            CrsId = string.Format(CultureInfo.InvariantCulture,
                "{0},1,{1:0.########},{2:0.########}", crsId, centerLongitude, centerLatitude);
        }

        public override double GridConvergence(double latitude, double longitude)
        {
            var p0 = LocationToMap(latitude, longitude);
            var p1 = LocationToMap(latitude + 1e-3, longitude);

            return Math.Atan2(p0.X - p1.X, p1.Y - p0.Y) * 180d / Math.PI;
        }

        public override Matrix RelativeTransform(double latitude, double longitude)
        {
            return new Matrix(1d, 0d, 0d, 1d, 0d, 0d);
        }

        public override Point LocationToMap(double latitude, double longitude)
        {
            var phi = latitude * Math.PI / 180d;
            var phi1 = LatitudeOfOrigin * Math.PI / 180d;
            var dLambda = (longitude - CentralMeridian) * Math.PI / 180d; // λ - λ0
            var cosPhi = Math.Cos(phi);
            var sinPhi = Math.Sin(phi);
            var cosPhi1 = Math.Cos(phi1);
            var sinPhi1 = Math.Sin(phi1);
            var cosLambda = Math.Cos(dLambda);
            var sinLambda = Math.Sin(dLambda);
            var x = cosPhi * sinLambda;
            var y = cosPhi1 * sinPhi - sinPhi1 * cosPhi * cosLambda;
            var cosC = sinPhi1 * sinPhi + cosPhi1 * cosPhi * cosLambda; // (5-3)
            cosC = Math.Min(Math.Max(cosC, -1d), 1d); // protect against rounding errors
            var k = 2d / (1d + cosC); // p.157 (21-4), k0 == 1

            return new Point(
                EquatorialRadius * k * x,
                EquatorialRadius * k * y); // p.157 (21-2/3)
        }

        public override Location MapToLocation(double x, double y)
        {
            var rho = Math.Sqrt(x * x + y * y);
            var c = 2d * Math.Atan(rho / (2d * EquatorialRadius)); // p.159 (21-15), k0 == 1
            var cosC = Math.Cos(c);
            var sinC = Math.Sin(c);

            var phi1 = LatitudeOfOrigin * Math.PI / 180d;
            var cosPhi1 = Math.Cos(phi1);
            var sinPhi1 = Math.Sin(phi1);
            var phi = Math.Asin(cosC * sinPhi1 + y * sinC * cosPhi1 / rho); // (20-14)
            double u, v;

            if (LatitudeOfOrigin == 90d) // (20-16)
            {
                u = x;
                v = -y;
            }
            else if (LatitudeOfOrigin == -90d) // (20-17)
            {
                u = x;
                v = y;
            }
            else // (20-15)
            {
                u = x * sinC;
                v = rho * cosPhi1 * cosC - y * sinPhi1 * sinC;
            }

            return new Location(
                phi * 180d / Math.PI,
                Math.Atan2(u, v) * 180d / Math.PI + CentralMeridian);
        }
    }
}

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
            var phi0 = LatitudeOfOrigin * Math.PI / 180d; // φ1
            var phi1 = latitude * Math.PI / 180d;
            var phi2 = (latitude + 1e-3) * Math.PI / 180d;
            var dLambda = (longitude - CentralMeridian) * Math.PI / 180d; // λ - λ0
            var sinPhi0 = Math.Sin(phi0);
            var cosPhi0 = Math.Cos(phi0);
            var sinPhi1 = Math.Sin(phi1);
            var cosPhi1 = Math.Cos(phi1);
            var sinPhi2 = Math.Sin(phi2);
            var cosPhi2 = Math.Cos(phi2);
            var sinLambda = Math.Sin(dLambda);
            var cosLambda = Math.Cos(dLambda);
            var k1 = 2d / (1d + sinPhi0 * sinPhi1 + cosPhi0 * cosPhi1 * cosLambda);
            var k2 = 2d / (1d + sinPhi0 * sinPhi2 + cosPhi0 * cosPhi2 * cosLambda);
            var dCosPhi = k2 * cosPhi2 - k1 * cosPhi1;
            var dSinPhi = k2 * sinPhi2 - k1 * sinPhi1;

            return Math.Atan2(-sinLambda * dCosPhi,
                cosPhi0 * dSinPhi - sinPhi0 * cosLambda * dCosPhi) * 180d / Math.PI;
        }

        public override Matrix RelativeTransform(double latitude, double longitude)
        {
            return new Matrix(1d, 0d, 0d, 1d, 0d, 0d);
        }

        public override Point LocationToMap(double latitude, double longitude)
        {
            var phi0 = LatitudeOfOrigin * Math.PI / 180d; // φ1
            var phi = latitude * Math.PI / 180d; // φ
            var dLambda = (longitude - CentralMeridian) * Math.PI / 180d; // λ - λ0
            var sinPhi0 = Math.Sin(phi0);
            var cosPhi0 = Math.Cos(phi0);
            var sinPhi = Math.Sin(phi);
            var cosPhi = Math.Cos(phi);
            var sinLambda = Math.Sin(dLambda);
            var cosPhiCosLambda = cosPhi * Math.Cos(dLambda);
            var x = cosPhi * sinLambda;
            var y = cosPhi0 * sinPhi - sinPhi0 * cosPhiCosLambda;
            var k = 2d / (1d + sinPhi0 * sinPhi + cosPhi0 * cosPhiCosLambda); // p.157 (21-4), k0 == 1

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

            var phi0 = LatitudeOfOrigin * Math.PI / 180d; // φ1
            var cosPhi0 = Math.Cos(phi0);
            var sinPhi0 = Math.Sin(phi0);
            var phi = Math.Asin(cosC * sinPhi0 + y * sinC * cosPhi0 / rho); // (20-14)
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
                v = rho * cosPhi0 * cosC - y * sinPhi0 * sinC;
            }

            return new Location(
                phi * 180d / Math.PI,
                Math.Atan2(u, v) * 180d / Math.PI + CentralMeridian);
        }
    }
}

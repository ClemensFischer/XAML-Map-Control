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
    /// Elliptical Mercator Projection - EPSG:3395.
    /// See "Map Projections - A Working Manual" (https://pubs.usgs.gov/publication/pp1395), p.44-45.
    /// </summary>
    public class WorldMercatorProjection : MapProjection
    {
        public const string DefaultCrsId = "EPSG:3395";

        public WorldMercatorProjection() // parameterless constructor for XAML
            : this(DefaultCrsId)
        {
        }

        public WorldMercatorProjection(string crsId)
        {
            IsNormalCylindrical = true;
            CrsId = crsId;
        }

        public override Matrix RelativeTransform(double latitude, double longitude)
        {
            var e2 = (2d - Flattening) * Flattening;
            var phi = latitude * Math.PI / 180d;
            var sinPhi = Math.Sin(phi);
            var k = Math.Sqrt(1d - e2 * sinPhi * sinPhi) / Math.Cos(phi); // p.44 (7-8)

            return new Matrix(k, 0d, 0d, k, 0d, 0d);
        }

        public override Point LocationToMap(double latitude, double longitude)
        {
            var x = EquatorialRadius * longitude * Math.PI / 180d;
            double y;

            if (latitude <= -90d)
            {
                y = double.NegativeInfinity;
            }
            else if (latitude >= 90d)
            {
                y = double.PositiveInfinity;
            }
            else
            {
                var phi = latitude * Math.PI / 180d;
                var e = Math.Sqrt((2d - Flattening) * Flattening);
                var eSinPhi = e * Math.Sin(phi);
                var p = Math.Pow((1d - eSinPhi) / (1d + eSinPhi), e / 2d);

                y = EquatorialRadius * Math.Log(Math.Tan(phi / 2d + Math.PI / 4d) * p); // p.44 (7-7)
            }

            return new Point(x, y);
        }

        public override Location MapToLocation(double x, double y)
        {
            var t = Math.Exp(-y / EquatorialRadius); // p.44 (7-10)
            var phi = ApproximateLatitude((2d - Flattening) * Flattening, t); // p.45 (3-5)
            var lambda = x / EquatorialRadius;

            return new Location(phi * 180d / Math.PI, lambda * 180d / Math.PI);
        }

        internal static double ApproximateLatitude(double e2, double t)
        {
            var e4 = e2 * e2;
            var e6 = e2 * e4;
            var e8 = e2 * e6;
            var chi = Math.PI / 2d - 2d * Math.Atan(t); // p.45 (7-13)

            return chi +
                (e2 / 2d + e4 * 5d / 24d + e6 / 12d + e8 * 13d / 360d) * Math.Sin(2d * chi) +
                (e4 * 7d / 48d + e6 * 29d / 240d + e8 * 811d / 11520d) * Math.Sin(4d * chi) +
                (e6 * 7d / 120d + e8 * 81d / 1120d) * Math.Sin(6d * chi) +
                 e8 * 4279d / 161280d * Math.Sin(8d * chi); // p.45 (3-5)
        }
    }
}

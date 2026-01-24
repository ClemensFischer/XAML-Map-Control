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
        public static readonly double Wgs84Eccentricity = Math.Sqrt((2d - Wgs84Flattening) * Wgs84Flattening);

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

        public override Matrix RelativeScale(double latitude, double longitude)
        {
            var k = RelativeScale(latitude);

            return new Matrix(k, 0d, 0d, k, 0d, 0d);
        }

        public override Point? LocationToMap(double latitude, double longitude)
        {
            return new Point(
                MeterPerDegree * longitude,
                MeterPerDegree * LatitudeToY(latitude));
        }

        public override Location MapToLocation(double x, double y)
        {
            return new Location(
                YToLatitude(y / MeterPerDegree),
                x / MeterPerDegree);
        }

        public static double RelativeScale(double latitude)
        {
            var phi = latitude * Math.PI / 180d;
            var eSinPhi = Wgs84Eccentricity * Math.Sin(phi);

            return Math.Sqrt(1d - eSinPhi * eSinPhi) / Math.Cos(phi); // p.44 (7-8)
        }

        public static double LatitudeToY(double latitude)
        {
            if (latitude <= -90d)
            {
                return double.NegativeInfinity;
            }

            if (latitude >= 90d)
            {
                return double.PositiveInfinity;
            }

            var phi = latitude * Math.PI / 180d;
            var eSinPhi = Wgs84Eccentricity * Math.Sin(phi);
            var f = Math.Pow((1d - eSinPhi) / (1d + eSinPhi), Wgs84Eccentricity / 2d);

            return Math.Log(Math.Tan(phi / 2d + Math.PI / 4d) * f) * 180d / Math.PI; // p.44 (7-7)
        }

        public static double YToLatitude(double y)
        {
            var t = Math.Exp(-y * Math.PI / 180d); // p.44 (7-10)

            return ApproximateLatitude(Wgs84Eccentricity, t) * 180d / Math.PI;
        }

        internal static double ApproximateLatitude(double e, double t)
        {
            var e2 = e * e;
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

using System;
#if WPF
using System.Windows;
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
            Type = MapProjectionType.NormalCylindrical;
            CrsId = crsId;
        }

        public override Point RelativeScale(double latitude, double longitude)
        {
            var k = RelativeScale(latitude);

            return new Point(k, k);
        }

        public override Point? LocationToMap(double latitude, double longitude)
        {
            return new Point(
                Wgs84MeterPerDegree * longitude,
                Wgs84MeterPerDegree * LatitudeToY(latitude));
        }

        public override Location MapToLocation(double x, double y)
        {
            return new Location(
                YToLatitude(y / Wgs84MeterPerDegree),
                x / Wgs84MeterPerDegree);
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

            return chi
                + (e2 / 2d + 5d * e4 / 24d + e6 / 12d + 13d * e8 / 360d) * Math.Sin(2d * chi)
                + (7d * e4 / 48d + 29d * e6 / 240d + 811d * e8 / 11520d) * Math.Sin(4d * chi)
                + (7d * e6 / 120d + 81d * e8 / 1120d) * Math.Sin(6d * chi)
                + 4279d * e8 / 161280d * Math.Sin(8d * chi); // p.45 (3-5)
        }
    }
}

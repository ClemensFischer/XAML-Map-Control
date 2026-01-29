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
    /// Spherical Mercator Projection - EPSG:3857.
    /// See "Map Projections - A Working Manual" (https://pubs.usgs.gov/publication/pp1395), p.41-44.
    /// </summary>
    public class WebMercatorProjection : MapProjection
    {
        public const string DefaultCrsId = "EPSG:3857";

        public WebMercatorProjection() // parameterless constructor for XAML
            : this(DefaultCrsId)
        {
        }

        public WebMercatorProjection(string crsId)
        {
            IsNormalCylindrical = true;
            CrsId = crsId;
        }

        public override Matrix RelativeTransform(double latitude, double longitude)
        {
            var k = 1d / Math.Cos(latitude * Math.PI / 180d); // p.44 (7-3)

            return new Matrix(k, 0d, 0d, k, 0d, 0d);
        }

        public override Point LocationToMap(double latitude, double longitude)
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

            return Math.Log(Math.Tan((latitude + 90d) * Math.PI / 360d)) * 180d / Math.PI;
        }

        public static double YToLatitude(double y)
        {
            return 90d - Math.Atan(Math.Exp(-y * Math.PI / 180d)) * 360d / Math.PI;
        }
    }
}

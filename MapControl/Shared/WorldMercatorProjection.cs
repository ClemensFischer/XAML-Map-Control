// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if !WINDOWS_UWP
using System.Windows;
#endif

namespace MapControl
{
    /// <summary>
    /// Transforms map coordinates according to the "World Mercator" Projection, EPSG:3395.
    /// Longitude values are transformed linearly to X values in meters, by multiplying with TrueScale.
    /// Latitude values are transformed according to the elliptical versions of the Mercator equations,
    /// as shown in "Map Projections - A Working Manual" (https://pubs.usgs.gov/pp/1395/report.pdf), p.44.
    /// </summary>
    public class WorldMercatorProjection : MapProjection
    {
        public const double Wgs84Flattening = 1d / 298.257223563;
        public static readonly double Wgs84Eccentricity = Math.Sqrt((2d - Wgs84Flattening) * Wgs84Flattening);

        public static double MinLatitudeDelta = 1d / Wgs84EquatorialRadius; // corresponds to 1 meter
        public static int MaxIterations = 10;

        public WorldMercatorProjection()
            : this("EPSG:3395")
        {
        }

        public WorldMercatorProjection(string crsId)
        {
            CrsId = crsId;
            IsCylindrical = true;
            MaxLatitude = YToLatitude(180d);
        }

        public override Vector GetMapScale(Location location)
        {
            var lat = location.Latitude * Math.PI / 180d;
            var eSinLat = Wgs84Eccentricity * Math.Sin(lat);
            var scale = ViewportScale * Math.Sqrt(1d - eSinLat * eSinLat) / Math.Cos(lat);

            return new Vector(scale, scale);
        }

        public override Point LocationToPoint(Location location)
        {
            return new Point(
                TrueScale * location.Longitude,
                TrueScale * LatitudeToY(location.Latitude));
        }

        public override Location PointToLocation(Point point)
        {
            return new Location(
                YToLatitude(point.Y / TrueScale),
                point.X / TrueScale);
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

            var lat = latitude * Math.PI / 180d;

            return Math.Log(Math.Tan(lat / 2d + Math.PI / 4d) * ConformalFactor(lat)) / Math.PI * 180d;
        }

        public static double YToLatitude(double y)
        {
            var t = Math.Exp(-y * Math.PI / 180d);
            var lat = Math.PI / 2d - 2d * Math.Atan(t);
            var latDelta = 1d;

            for (int i = 0; i < MaxIterations && latDelta > MinLatitudeDelta; i++)
            {
                var newLat = Math.PI / 2d - 2d * Math.Atan(t * ConformalFactor(lat));

                latDelta = Math.Abs(newLat - lat);
                lat = newLat;
            }

            return lat / Math.PI * 180d;
        }

        private static double ConformalFactor(double lat)
        {
            var eSinLat = Wgs84Eccentricity * Math.Sin(lat);

            return Math.Pow((1d - eSinLat) / (1d + eSinLat), Wgs84Eccentricity / 2d);
        }
    }
}

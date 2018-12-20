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
    /// Elliptical Mercator Projection, EPSG:3395.
    /// See "Map Projections - A Working Manual" (https://pubs.usgs.gov/pp/1395/report.pdf), p.44-45.
    /// </summary>
    public class WorldMercatorProjection : MapProjection
    {
        public static double ConvergenceTolerance = 1e-6;
        public static int MaxIterations = 10;

        public WorldMercatorProjection()
            : this("EPSG:3395")
        {
        }

        public WorldMercatorProjection(string crsId)
        {
            CrsId = crsId;
            IsNormalCylindrical = true;
            MaxLatitude = YToLatitude(180d);
        }

        public override Vector GetMapScale(Location location)
        {
            var lat = location.Latitude * Math.PI / 180d;
            var eSinLat = Wgs84Eccentricity * Math.Sin(lat);
            var k = Math.Sqrt(1d - eSinLat * eSinLat) / Math.Cos(lat); // p.44 (7-8)

            return new Vector(ViewportScale * k, ViewportScale * k);
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

            return Math.Log(Math.Tan(lat / 2d + Math.PI / 4d)
                * ConformalFactor(lat)) * 180d / Math.PI; // p.44 (7-7)
        }

        public static double YToLatitude(double y)
        {
            var t = Math.Exp(-y * Math.PI / 180d); // p.44 (7-10)
            var lat = Math.PI / 2d - 2d * Math.Atan(t); // p.44 (7-11)
            var relChange = 1d;

            for (var i = 0; i < MaxIterations && relChange > ConvergenceTolerance; i++)
            {
                var newLat = Math.PI / 2d - 2d * Math.Atan(t * ConformalFactor(lat)); // p.44 (7-9)
                relChange = Math.Abs(1d - newLat / lat);
                lat = newLat;
            }

            return lat * 180d / Math.PI;
        }

        private static double ConformalFactor(double lat)
        {
            var eSinLat = Wgs84Eccentricity * Math.Sin(lat);

            return Math.Pow((1d - eSinLat) / (1d + eSinLat), Wgs84Eccentricity / 2d);
        }
    }
}

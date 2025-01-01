// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if WPF
using System.Windows;
#endif

namespace MapControl
{
    /// <summary>
    /// Elliptical Mercator Projection - EPSG:3395.
    /// See "Map Projections - A Working Manual" (https://pubs.usgs.gov/pp/1395/report.pdf), p.44-45.
    /// </summary>
    public class WorldMercatorProjection : MapProjection
    {
        public const string DefaultCrsId = "EPSG:3395";

        public WorldMercatorProjection()
            : this(DefaultCrsId)
        {
            // XAML needs parameterless constructor
        }

        public WorldMercatorProjection(string crsId)
        {
            Type = MapProjectionType.NormalCylindrical;
            CrsId = crsId;
        }

        public override Point GetRelativeScale(Location location)
        {
            var lat = location.Latitude * Math.PI / 180d;
            var eSinLat = Wgs84Eccentricity * Math.Sin(lat);
            var k = Math.Sqrt(1d - eSinLat * eSinLat) / Math.Cos(lat); // p.44 (7-8)

            return new Point(k, k);
        }

        public override Point? LocationToMap(Location location)
        {
            return new Point(
                Wgs84MeterPerDegree * location.Longitude,
                Wgs84MeterPerDegree * LatitudeToY(location.Latitude));
        }

        public override Location MapToLocation(Point point)
        {
            return new Location(
                YToLatitude(point.Y / Wgs84MeterPerDegree),
                point.X / Wgs84MeterPerDegree);
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
            var eSinLat = Wgs84Eccentricity * Math.Sin(lat);
            var f = Math.Pow((1d - eSinLat) / (1d + eSinLat), Wgs84Eccentricity / 2d);

            return Math.Log(Math.Tan(lat / 2d + Math.PI / 4d) * f) * 180d / Math.PI; // p.44 (7-7)
        }

        public static double YToLatitude(double y)
        {
            var t = Math.Exp(-y * Math.PI / 180d); // p.44 (7-10)

            return LatitudeFromSeriesApproximation(Wgs84Eccentricity, t) * 180d / Math.PI;
        }

        internal static double LatitudeFromSeriesApproximation(double e, double t)
        {
            var e_2 = e * e;
            var e_4 = e_2 * e_2;
            var e_6 = e_2 * e_4;
            var e_8 = e_2 * e_6;

            var lat = Math.PI / 2d - 2d * Math.Atan(t); // p.45 (7-13)

            return lat
                + (e_2 / 2d + 5d * e_4 / 24d + e_6 / 12d + 13d * e_8 / 360d) * Math.Sin(2d * lat)
                + (7d * e_4 / 48d + 29d * e_6 / 240d + 811d * e_8 / 11520d) * Math.Sin(4d * lat)
                + (7d * e_6 / 120d + 81d * e_8 / 1120d) * Math.Sin(6d * lat)
                + (4279d * e_8 / 161280d) * Math.Sin(8d * lat); // p.45 (3-5)
        }
    }
}

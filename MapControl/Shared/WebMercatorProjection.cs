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
    /// Spherical Mercator Projection - EPSG:3857.
    /// See "Map Projections - A Working Manual" (https://pubs.usgs.gov/pp/1395/report.pdf), p.41-44.
    /// </summary>
    public class WebMercatorProjection : MapProjection
    {
        public const string DefaultCrsId = "EPSG:3857";

        public WebMercatorProjection()
            : this(DefaultCrsId)
        {
            // XAML needs parameterless constructor
        }

        public WebMercatorProjection(string crsId)
        {
            Type = MapProjectionType.WebMercator;
            CrsId = crsId;
        }

        public override Point GetRelativeScale(Location location)
        {
            var k = 1d / Math.Cos(location.Latitude * Math.PI / 180d); // p.44 (7-3)

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

            return Math.Log(Math.Tan((latitude + 90d) * Math.PI / 360d)) * 180d / Math.PI;
        }

        public static double YToLatitude(double y)
        {
            return 90d - Math.Atan(Math.Exp(-y * Math.PI / 180d)) * 360d / Math.PI;
        }
    }
}

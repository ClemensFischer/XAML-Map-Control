﻿// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if !WINUI && !UWP
using System.Windows;
#endif

namespace MapControl
{
    /// <summary>
    /// Spherical Orthographic Projection - AUTO2:42003.
    /// See "Map Projections - A Working Manual" (https://pubs.usgs.gov/pp/1395/report.pdf), p.148-150.
    /// </summary>
    public class OrthographicProjection : AzimuthalProjection
    {
        public const string DefaultCrsId = "AUTO2:42003";

        public OrthographicProjection()
        {
            CrsId = DefaultCrsId;
        }

        public override Point LocationToMap(Location location)
        {
            if (location.Equals(Center))
            {
                return new Point();
            }

            var lat0 = Center.Latitude * Math.PI / 180d;
            var lat = location.Latitude * Math.PI / 180d;
            var dLon = (location.Longitude - Center.Longitude) * Math.PI / 180d;

            if (Math.Abs(lat - lat0) > Math.PI / 2d || Math.Abs(dLon) > Math.PI / 2d)
            {
                return new Point(double.NaN, double.NaN);
            }

            return new Point(
                Wgs84EquatorialRadius * Math.Cos(lat) * Math.Sin(dLon),
                Wgs84EquatorialRadius * (Math.Cos(lat0) * Math.Sin(lat) - Math.Sin(lat0) * Math.Cos(lat) * Math.Cos(dLon)));
        }

        public override Location MapToLocation(Point point)
        {
            if (point.X == 0d && point.Y == 0d)
            {
                return new Location(Center.Latitude, Center.Longitude);
            }

            var x = point.X / Wgs84EquatorialRadius;
            var y = point.Y / Wgs84EquatorialRadius;
            var r2 = x * x + y * y;

            if (r2 > 1d)
            {
                return null;
            }

            var r = Math.Sqrt(r2);
            var sinC = r;
            var cosC = Math.Sqrt(1 - r2);

            var lat0 = Center.Latitude * Math.PI / 180d;
            var cosLat0 = Math.Cos(lat0);
            var sinLat0 = Math.Sin(lat0);

            return new Location(
                180d / Math.PI * Math.Asin(cosC * sinLat0 + y * sinC * cosLat0 / r),
                180d / Math.PI * Math.Atan2(x * sinC, r * cosC * cosLat0 - y * sinC * sinLat0) + Center.Longitude);
        }
    }
}

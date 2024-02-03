// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if !WINUI && !UWP
using System.Windows;
#endif

namespace MapControl
{
    /// <summary>
    /// Spherical Gnomonic Projection - AUTO2:97001.
    /// See "Map Projections - A Working Manual" (https://pubs.usgs.gov/pp/1395/report.pdf), p.165-167.
    /// </summary>
    public class GnomonicProjection : AzimuthalProjection
    {
        public const string DefaultCrsId = "AUTO2:97001"; // GeoServer non-standard CRS ID

        public GnomonicProjection()
        {
            CrsId = DefaultCrsId;
        }

        public override Point? LocationToMap(Location location)
        {
            if (location.Equals(Center))
            {
                return new Point();
            }

            GetAzimuthDistance(Center, location, out double azimuth, out double distance);

            var mapDistance = distance < Math.PI / 2d
                ? Math.Tan(distance) * Wgs84EquatorialRadius
                : double.PositiveInfinity;

            return new Point(mapDistance * Math.Sin(azimuth), mapDistance * Math.Cos(azimuth));
        }

        public override Location MapToLocation(Point point)
        {
            if (point.X == 0d && point.Y == 0d)
            {
                return new Location(Center.Latitude, Center.Longitude);
            }

            var azimuth = Math.Atan2(point.X, point.Y);
            var mapDistance = Math.Sqrt(point.X * point.X + point.Y * point.Y);

            var distance = Math.Atan(mapDistance / Wgs84EquatorialRadius);

            return GetLocation(Center, azimuth, distance);
        }
    }
}

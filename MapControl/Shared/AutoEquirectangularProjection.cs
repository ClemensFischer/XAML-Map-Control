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
    /// Auto-Equirectangular Projection - AUTO2:42004.
    /// Equidistant cylindrical projection with standard parallel and central meridian set by the Center property.
    /// See "Map Projections - A Working Manual" (https://pubs.usgs.gov/pp/1395/report.pdf), p.90-91.
    /// </summary>
    public class AutoEquirectangularProjection : MapProjection
    {
        public const string DefaultCrsId = "AUTO2:42004";

        public AutoEquirectangularProjection()
        {
            Type = MapProjectionType.NormalCylindrical;
            CrsId = DefaultCrsId;
        }

        public override Point? LocationToMap(Location location)
        {
            return new Point(
                Wgs84MeterPerDegree * (location.Longitude - Center.Longitude) * Math.Cos(Center.Latitude * Math.PI / 180d),
                Wgs84MeterPerDegree * location.Latitude);
        }

        public override Location MapToLocation(Point point)
        {
            return new Location(
                point.Y / Wgs84MeterPerDegree,
                point.X / (Wgs84MeterPerDegree * Math.Cos(Center.Latitude * Math.PI / 180d)) + Center.Longitude);
        }
    }
}

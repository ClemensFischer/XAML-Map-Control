// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2023 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Globalization;
#if !WINUI && !UWP
using System.Windows;
#endif

namespace MapControl
{
    /// <summary>
    /// Equirectangular Projection - EPSG:4326.
    /// Equidistant cylindrical projection with zero standard parallel and central meridian.
    /// See "Map Projections - A Working Manual" (https://pubs.usgs.gov/pp/1395/report.pdf), p.90-91.
    /// </summary>
    public class EquirectangularProjection : MapProjection
    {
        public const int DefaultEpsgCode = 4326;
        public static readonly string DefaultCrsId = $"EPSG:{DefaultEpsgCode}";

        public EquirectangularProjection()
        {
            Type = MapProjectionType.NormalCylindrical;
            CrsId = DefaultCrsId;
        }

        public override Scale GetRelativeScale(Location location)
        {
            return new Scale(
                1d / Math.Cos(location.Latitude * Math.PI / 180d),
                1d);
        }

        public override Point? LocationToMap(Location location)
        {
            return new Point(
                Wgs84MeterPerDegree * location.Longitude,
                Wgs84MeterPerDegree * location.Latitude);
        }

        public override Location MapToLocation(Point point)
        {
            return new Location(
                point.Y / Wgs84MeterPerDegree,
                point.X / Wgs84MeterPerDegree);
        }

        public override string GetBboxValue(MapRect mapRect)
        {
            return string.Format(CultureInfo.InvariantCulture,
                CrsId == "CRS:84" ? "{0},{1},{2},{3}" : "{1},{0},{3},{2}",
                mapRect.XMin / Wgs84MeterPerDegree,
                mapRect.YMin / Wgs84MeterPerDegree,
                mapRect.XMax / Wgs84MeterPerDegree,
                mapRect.YMax / Wgs84MeterPerDegree);
        }
    }
}

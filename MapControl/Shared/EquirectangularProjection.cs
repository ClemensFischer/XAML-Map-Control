// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Globalization;
#if WPF
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
        public const string DefaultCrsId = "EPSG:4326";

        public EquirectangularProjection(string crsId = DefaultCrsId)
        {
            Type = MapProjectionType.NormalCylindrical;
            CrsId = crsId;
        }

        public override Point GetRelativeScale(Location location)
        {
            return new Point(
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

        public override string GetBboxValue(Rect rect)
        {
            if (CrsId == DefaultCrsId || CrsId == "CRS:84")
            {
                return string.Format(CultureInfo.InvariantCulture,
                    CrsId == DefaultCrsId ? "{1:F8},{0:F8},{3:F8},{2:F8}" : "{0:F8},{1:F8},{2:F8},{3:F8}",
                    rect.X / Wgs84MeterPerDegree,
                    rect.Y / Wgs84MeterPerDegree,
                    (rect.X + rect.Width) / Wgs84MeterPerDegree,
                    (rect.Y + rect.Height) / Wgs84MeterPerDegree);
            }

            return base.GetBboxValue(rect);
        }
    }
}

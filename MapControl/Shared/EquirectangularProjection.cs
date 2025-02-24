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
    /// Equirectangular Projection - EPSG:4326.
    /// Equidistant cylindrical projection with zero standard parallel and central meridian.
    /// See "Map Projections - A Working Manual" (https://pubs.usgs.gov/publication/pp1395), p.90-91.
    /// </summary>
    public class EquirectangularProjection : MapProjection
    {
        public const string DefaultCrsId = "EPSG:4326";

        public EquirectangularProjection()
            : this(DefaultCrsId)
        {
            // XAML needs parameterless constructor
        }

        public EquirectangularProjection(string crsId)
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
    }
}

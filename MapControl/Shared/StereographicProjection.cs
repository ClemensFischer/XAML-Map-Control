// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if !WINUI && !WINDOWS_UWP
using System.Windows;
#endif

namespace MapControl
{
    /// <summary>
    /// Spherical Stereographic Projection.
    /// </summary>
    public class StereographicProjection : AzimuthalProjection
    {
        public StereographicProjection()
        {
            CrsId = "AUTO2:97002"; // GeoServer non-standard CRS ID
        }

        public override Point LocationToMap(Location location)
        {
            if (location.Equals(Center))
            {
                return new Point();
            }

            GetAzimuthDistance(Center, location, out double azimuth, out double distance);

            var mapDistance = Math.Tan(distance / 2d) * 2d * Wgs84EquatorialRadius;

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

            var distance = 2d * Math.Atan(mapDistance / (2d * Wgs84EquatorialRadius));

            return GetLocation(Center, azimuth, distance);
        }
    }
}

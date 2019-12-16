// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2019 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if !WINDOWS_UWP
using System.Windows;
#endif

namespace MapControl
{
    /// <summary>
    /// Spherical Azimuthal Equidistant Projection.
    /// </summary>
    public class AzimuthalEquidistantProjection : AzimuthalProjection
    {
        // No standard CRS ID

        public override Point LocationToPoint(Location location)
        {
            if (location.Equals(ProjectionCenter))
            {
                return new Point();
            }

            double azimuth, distance;

            GetAzimuthDistance(ProjectionCenter, location, out azimuth, out distance);

            var mapDistance = distance * TrueScale * 180d / Math.PI;

            return new Point(mapDistance * Math.Sin(azimuth), mapDistance * Math.Cos(azimuth));
        }

        public override Location PointToLocation(Point point)
        {
            if (point.X == 0d && point.Y == 0d)
            {
                return new Location(ProjectionCenter.Latitude, ProjectionCenter.Longitude);
            }

            var azimuth = Math.Atan2(point.X, point.Y);
            var mapDistance = Math.Sqrt(point.X * point.X + point.Y * point.Y);

            var distance = mapDistance / (TrueScale * 180d / Math.PI);

            return GetLocation(ProjectionCenter, azimuth, distance);
        }
    }
}

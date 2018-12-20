// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
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
        public AzimuthalEquidistantProjection()
        {
            // No known standard or de-facto standard CRS ID
        }

        public AzimuthalEquidistantProjection(string crsId)
        {
            CrsId = crsId;
        }

        public override Point LocationToPoint(Location location)
        {
            if (location.Equals(ProjectionCenter))
            {
                return new Point();
            }

            double azimuth, distance;

            GetAzimuthDistance(ProjectionCenter, location, out azimuth, out distance);

            distance *= TrueScale * 180d / Math.PI;

            return new Point(distance * Math.Sin(azimuth), distance * Math.Cos(azimuth));
        }

        public override Location PointToLocation(Point point)
        {
            if (point.X == 0d && point.Y == 0d)
            {
                return ProjectionCenter;
            }

            var azimuth = Math.Atan2(point.X, point.Y);
            var distance = Math.Sqrt(point.X * point.X + point.Y * point.Y) / (TrueScale * 180d / Math.PI);

            return GetLocation(ProjectionCenter, azimuth, distance);
        }
    }
}

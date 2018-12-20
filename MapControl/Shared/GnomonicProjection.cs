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
    /// Spherical Gnomonic Projection.
    /// </summary>
    public class GnomonicProjection : AzimuthalProjection
    {
        public GnomonicProjection()
            : this("AUTO2:97001") // GeoServer non-standard CRS ID
        {
        }

        public GnomonicProjection(string crsId)
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

            var mapDistance = distance < Math.PI / 2d
                ? Math.Tan(distance) * TrueScale * 180d / Math.PI
                : double.PositiveInfinity;

            return new Point(mapDistance * Math.Sin(azimuth), mapDistance * Math.Cos(azimuth));
        }

        public override Location PointToLocation(Point point)
        {
            if (point.X == 0d && point.Y == 0d)
            {
                return ProjectionCenter;
            }

            var azimuth = Math.Atan2(point.X, point.Y);
            var mapDistance = Math.Sqrt(point.X * point.X + point.Y * point.Y);
            var distance = Math.Atan(mapDistance / (TrueScale * 180d / Math.PI));

            return GetLocation(ProjectionCenter, azimuth, distance);
        }
    }
}

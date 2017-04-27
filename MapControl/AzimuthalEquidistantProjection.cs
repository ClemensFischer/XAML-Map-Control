// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2017 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if NETFX_CORE
using Windows.Foundation;
#else
using System.Windows;
#endif

namespace MapControl
{
    /// <summary>
    /// Transforms map coordinates according to the Azimuthal Equidistant Projection.
    /// </summary>
    public class AzimuthalEquidistantProjection : AzimuthalProjection
    {
        public override string CrsId { get; set; } = "AUTO2:99999";

        public override Point LocationToPoint(Location location)
        {
            double azimuth, distance;

            GetAzimuthDistance(centerLocation, location, out azimuth, out distance);

            distance *= centerRadius;

            return new Point(distance * Math.Sin(azimuth), distance * Math.Cos(azimuth));
        }

        public override Location PointToLocation(Point point)
        {
            if (point.X == 0d && point.Y == 0d)
            {
                return centerLocation;
            }

            var azimuth = Math.Atan2(point.X, point.Y);
            var distance = Math.Sqrt(point.X * point.X + point.Y * point.Y) / centerRadius;

            return GetLocation(centerLocation, azimuth, distance);
        }
    }
}

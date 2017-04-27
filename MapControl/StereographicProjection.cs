﻿// XAML Map Control - http://xamlmapcontrol.codeplex.com/
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
    /// Transforms map coordinates according to the Gnomonic Projection.
    /// </summary>
    public class StereographicProjection : AzimuthalProjection
    {
        public override string CrsId { get; set; } = "AUTO2:97002";

        public override Point LocationToPoint(Location location)
        {
            double azimuth, distance;

            GetAzimuthDistance(centerLocation, location, out azimuth, out distance);

            var mapDistance = 2d * centerRadius * Math.Tan(distance / 2d);

            return new Point(mapDistance * Math.Sin(azimuth), mapDistance * Math.Cos(azimuth));
        }

        public override Location PointToLocation(Point point)
        {
            if (point.X == 0d && point.Y == 0d)
            {
                return centerLocation;
            }

            var azimuth = Math.Atan2(point.X, point.Y);
            var mapDistance = Math.Sqrt(point.X * point.X + point.Y * point.Y);
            var distance = 2d * Math.Atan(mapDistance / (2d * centerRadius));

            return GetLocation(centerLocation, azimuth, distance);
        }
    }
}

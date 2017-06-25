﻿// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
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
            if (location.Equals(projectionCenter))
            {
                return new Point();
            }

            double azimuth, distance;

            GetAzimuthDistance(projectionCenter, location, out azimuth, out distance);

            var mapDistance = distance < Math.PI / 2d
                ? Wgs84EquatorialRadius * Math.Tan(distance)
                : double.PositiveInfinity;

            return new Point(mapDistance * Math.Sin(azimuth), mapDistance * Math.Cos(azimuth));
        }

        public override Location PointToLocation(Point point)
        {
            if (point.X == 0d && point.Y == 0d)
            {
                return projectionCenter;
            }

            var azimuth = Math.Atan2(point.X, point.Y);
            var mapDistance = Math.Sqrt(point.X * point.X + point.Y * point.Y);
            var distance = Math.Atan(mapDistance / Wgs84EquatorialRadius);

            return GetLocation(projectionCenter, azimuth, distance);
        }
    }
}

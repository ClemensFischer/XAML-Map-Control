// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2020 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if !WINDOWS_UWP
using System.Windows;
#endif

namespace MapControl
{
    public class AutoEquirectangularProjection : MapProjection
    {
        public AutoEquirectangularProjection()
        {
            CrsId = "AUTO2:42004";
        }

        public override Point LocationToPoint(Location location)
        {
            var xScale = Wgs84MetersPerDegree * Math.Cos(ProjectionCenter.Latitude * Math.PI / 180d);

            return new Point(
                xScale * (location.Longitude - ProjectionCenter.Longitude),
                Wgs84MetersPerDegree * location.Latitude);
        }

        public override Location PointToLocation(Point point)
        {
            var xScale = Wgs84MetersPerDegree * Math.Cos(ProjectionCenter.Latitude * Math.PI / 180d);

            return new Location(
                point.Y / Wgs84MetersPerDegree,
                point.X / xScale + ProjectionCenter.Longitude);
        }
    }
}

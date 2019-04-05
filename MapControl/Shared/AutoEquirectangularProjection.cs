// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2019 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if WINDOWS_UWP
using Windows.Foundation;
#else
using System.Windows;
#endif

namespace MapControl
{
    public class AutoEquirectangularProjection : MapProjection
    {
        public AutoEquirectangularProjection()
            : this("AUTO2:42004")
        {
        }

        public AutoEquirectangularProjection(string crsId)
        {
            CrsId = crsId;
            IsNormalCylindrical = true;
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

// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if !WINUI && !UWP
using System.Windows;
#endif

namespace MapControl
{
    public class AutoEquirectangularProjection : MapProjection
    {
        public const string DefaultCrsId = "AUTO2:42004";

        public AutoEquirectangularProjection()
        {
            CrsId = DefaultCrsId;
        }

        public override Point LocationToMap(Location location)
        {
            var xScale = Wgs84MetersPerDegree * Math.Cos(Center.Latitude * Math.PI / 180d);

            return new Point(
                xScale * (location.Longitude - Center.Longitude),
                Wgs84MetersPerDegree * location.Latitude);
        }

        public override Location MapToLocation(Point point)
        {
            var xScale = Wgs84MetersPerDegree * Math.Cos(Center.Latitude * Math.PI / 180d);

            return new Location(
                point.Y / Wgs84MetersPerDegree,
                point.X / xScale + Center.Longitude);
        }
    }
}

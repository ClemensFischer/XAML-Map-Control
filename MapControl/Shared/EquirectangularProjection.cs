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
    /// Equirectangular Projection.
    /// Longitude and Latitude values are transformed identically to X and Y.
    /// </summary>
    public class EquirectangularProjection : MapProjection
    {
        public EquirectangularProjection()
            : this("EPSG:4326")
        {
        }

        public EquirectangularProjection(string crsId)
        {
            CrsId = crsId;
            IsNormalCylindrical = true;
            TrueScale = 1d;
        }

        public override Vector GetMapScale(Location location)
        {
            return new Vector(
                ViewportScale / (MetersPerDegree * Math.Cos(location.Latitude * Math.PI / 180d)),
                ViewportScale / MetersPerDegree);
        }

        public override Point LocationToPoint(Location location)
        {
            return new Point(location.Longitude, location.Latitude);
        }

        public override Location PointToLocation(Point point)
        {
            return new Location(point.Y, point.X);
        }
    }
}

// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2019 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Globalization;
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
        {
            CrsId = "EPSG:4326";
        }

        public override double TrueScale
        {
            get { return 1d; }
        }

        public override Vector GetMapScale(Location location)
        {
            return new Vector(
                ViewportScale / (Wgs84MetersPerDegree * Math.Cos(location.Latitude * Math.PI / 180d)),
                ViewportScale / Wgs84MetersPerDegree);
        }

        public override Point LocationToPoint(Location location)
        {
            return new Point(location.Longitude, location.Latitude);
        }

        public override Location PointToLocation(Point point)
        {
            return new Location(point.Y, point.X);
        }

        public override string GetBboxValue(Rect rect)
        {
            return string.Format(CultureInfo.InvariantCulture,
                CrsId != "CRS:84" ? "{1},{0},{3},{2}" : "{0},{1},{2},{3}",
                rect.X, rect.Y, (rect.X + rect.Width), (rect.Y + rect.Height));
        }
    }
}

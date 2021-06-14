// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Globalization;
#if WINUI || WINDOWS_UWP
using Windows.Foundation;
#else
using System.Windows;
#endif

namespace MapControl
{
    /// <summary>
    /// Equirectangular Projection.
    /// Longitude and Latitude values are transformed linearly to X and Y values in meters.
    /// </summary>
    public class EquirectangularProjection : MapProjection
    {
        public EquirectangularProjection()
        {
            CrsId = "EPSG:4326";
        }

        public override bool IsNormalCylindrical
        {
            get { return true; }
        }

        public override Vector GetRelativeScale(Location location)
        {
            return new Vector(
                1d / Math.Cos(location.Latitude * Math.PI / 180d),
                1d);
        }

        public override Point LocationToMap(Location location)
        {
            return new Point(
                Wgs84MetersPerDegree * location.Longitude,
                Wgs84MetersPerDegree * location.Latitude);
        }

        public override Location MapToLocation(Point point)
        {
            return new Location(
                point.Y / Wgs84MetersPerDegree,
                point.X / Wgs84MetersPerDegree);
        }

        public override string GetBboxValue(Rect rect)
        {
            return string.Format(CultureInfo.InvariantCulture,
                CrsId == "CRS:84" ? "{0},{1},{2},{3}" : "{1},{0},{3},{2}",
                rect.X / Wgs84MetersPerDegree, rect.Y / Wgs84MetersPerDegree,
                (rect.X + rect.Width) / Wgs84MetersPerDegree, (rect.Y + rect.Height) / Wgs84MetersPerDegree);
        }
    }
}

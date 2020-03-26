// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2020 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Globalization;
#if WINDOWS_UWP
using Windows.Foundation;
#else
using System.Windows;
#endif

namespace MapControl
{
    /// <summary>
    /// Equirectangular Projection.
    /// Longitude and Latitude values are transformed linearly to X and Y in meters.
    /// </summary>
    public class EquirectangularProjection : MapProjection
    {
        public EquirectangularProjection()
        {
            CrsId = "EPSG:4326";
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
                location.Longitude * UnitsPerDegree,
                location.Latitude * UnitsPerDegree);
        }

        public override Location MapToLocation(Point point)
        {
            return new Location(
                point.Y / UnitsPerDegree,
                point.X / UnitsPerDegree);
        }

        public override string GetBboxValue(Rect rect)
        {
            return string.Format(CultureInfo.InvariantCulture,
                CrsId != "CRS:84" ? "{1},{0},{3},{2}" : "{0},{1},{2},{3}",
                rect.X / UnitsPerDegree, rect.Y / UnitsPerDegree,
                (rect.X + rect.Width) / UnitsPerDegree, (rect.Y + rect.Height) / UnitsPerDegree);
        }
    }
}

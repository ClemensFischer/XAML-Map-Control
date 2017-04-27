// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
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
    /// Transforms map coordinates according to the Equirectangular Projection.
    /// Longitude and Latitude values are transformed identically to X and Y.
    /// </summary>
    public class EquirectangularProjection : MapProjection
    {
        public override string CrsId { get; set; } = "EPSG:4326";

        public override Point GetMapScale(Location location)
        {
            return new Point(
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

        public override Location TranslateLocation(Location location, Point translation)
        {
            return new Location(
                location.Latitude - translation.Y / ViewportScale,
                location.Longitude + translation.X / ViewportScale);
        }
    }
}

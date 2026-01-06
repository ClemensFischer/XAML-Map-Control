using System;
#if WPF
using System.Windows;
#elif AVALONIA
using Avalonia;
#endif

namespace MapControl
{
    /// <summary>
    /// Auto-Equirectangular Projection - AUTO2:42004.
    /// Equidistant cylindrical projection with standard parallel and central meridian set by the Center property.
    /// See "Map Projections - A Working Manual" (https://pubs.usgs.gov/publication/pp1395), p.90-91.
    /// </summary>
    public class AutoEquirectangularProjection : MapProjection
    {
        public const string DefaultCrsId = "AUTO2:42004";

        public AutoEquirectangularProjection()
            : this(DefaultCrsId)
        {
            // XAML needs parameterless constructor
        }

        public AutoEquirectangularProjection(string crsId)
        {
            Type = MapProjectionType.NormalCylindrical;
            CrsId = crsId;
        }

        public override Point RelativeScale(double latitude, double longitude)
        {
            return new Point(
                Math.Cos(Center.Latitude * Math.PI / 180d) / Math.Cos(latitude * Math.PI / 180d),
                1d);
        }

        public override Point? LocationToMap(double latitude, double longitude)
        {
            return new Point(
                Wgs84MeterPerDegree * (longitude - Center.Longitude) * Math.Cos(Center.Latitude * Math.PI / 180d),
                Wgs84MeterPerDegree * latitude);
        }

        public override Location MapToLocation(double x, double y)
        {
            return new Location(
                y / Wgs84MeterPerDegree,
                x / (Wgs84MeterPerDegree * Math.Cos(Center.Latitude * Math.PI / 180d)) + Center.Longitude);
        }
    }
}

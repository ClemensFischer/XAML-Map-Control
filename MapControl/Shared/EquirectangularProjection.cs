using System;
#if WPF
using System.Windows;
#elif AVALONIA
using Avalonia;
#endif

namespace MapControl
{
    /// <summary>
    /// Equirectangular Projection - EPSG:4326.
    /// Equidistant cylindrical projection with zero standard parallel and central meridian.
    /// See "Map Projections - A Working Manual" (https://pubs.usgs.gov/publication/pp1395), p.90-91.
    /// </summary>
    public class EquirectangularProjection : MapProjection
    {
        public const string DefaultCrsId = "EPSG:4326";

        public EquirectangularProjection() // parameterless constructor for XAML
            : this(DefaultCrsId)
        {
        }

        public EquirectangularProjection(string crsId)
        {
            Type = MapProjectionType.NormalCylindrical;
            CrsId = crsId;
        }

        public override Point RelativeScale(double latitude, double longitude)
        {
            return new Point(1d / Math.Cos(latitude * Math.PI / 180d), 1d);
        }

        public override Point? LocationToMap(double latitude, double longitude)
        {
            return new Point(Wgs84MeterPerDegree * longitude, Wgs84MeterPerDegree * latitude);
        }

        public override Location MapToLocation(double x, double y)
        {
            return new Location(y / Wgs84MeterPerDegree, x / Wgs84MeterPerDegree);
        }
    }
}

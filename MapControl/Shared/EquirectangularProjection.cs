using System;
#if WPF
using System.Windows;
using System.Windows.Media;
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
            IsNormalCylindrical = true;
            CrsId = crsId;
        }

        public override Matrix RelativeTransform(double latitude, double longitude)
        {
            return new Matrix(1d / Math.Cos(latitude * Math.PI / 180d), 0d, 0d, 1d, 0d, 0d);
        }

        public override Point? LocationToMap(double latitude, double longitude)
        {
            return new Point(MeterPerDegree * longitude, MeterPerDegree * latitude);
        }

        public override Location MapToLocation(double x, double y)
        {
            return new Location(y / MeterPerDegree, x / MeterPerDegree);
        }
    }
}

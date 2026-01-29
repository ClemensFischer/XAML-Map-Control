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
    /// Elliptical Polar Stereographic Projection with scale factor at the pole and
    /// false easting and northing, as used by the UPS North and UPS South projections.
    /// See "Map Projections - A Working Manual" (https://pubs.usgs.gov/publication/pp1395), p.154-163.
    /// </summary>
    public class PolarStereographicProjection : MapProjection
    {
        public double Flattening { get; set; } = Wgs84Flattening;
        public double ScaleFactor { get; set; } = 0.994;
        public double FalseEasting { get; set; } = 2e6;
        public double FalseNorthing { get; set; } = 2e6;
        public Hemisphere Hemisphere { get; set; }

        public override Point LocationToMap(double latitude, double longitude)
        {
            var sign = Hemisphere == Hemisphere.North ? 1d : -1d;
            var phi = sign * latitude * Math.PI / 180d;
            var lambda = sign * longitude * Math.PI / 180d;

            var e = Math.Sqrt((2d - Flattening) * Flattening);
            var eSinPhi = e * Math.Sin(phi);

            var t = Math.Tan(Math.PI / 4d - phi / 2d)
                  / Math.Pow((1d - eSinPhi) / (1d + eSinPhi), e / 2d); // p.161 (15-9)
            // ρ
            var r = 2d * EquatorialRadius * ScaleFactor * t
                  / Math.Sqrt(Math.Pow(1d + e, 1d + e) * Math.Pow(1d - e, 1d - e)); // p.161 (21-33)

            var x = sign * r * Math.Sin(lambda); // p.161 (21-30)
            var y = sign * -r * Math.Cos(lambda); // p.161 (21-31)

            return new Point(x + FalseEasting, y + FalseNorthing);
        }

        public override Matrix RelativeTransform(double latitude, double longitude)
        {
            var sign = Hemisphere == Hemisphere.North ? 1d : -1d;
            var phi = sign * latitude * Math.PI / 180d;

            var e = Math.Sqrt((2d - Flattening) * Flattening);
            var eSinPhi = e * Math.Sin(phi);

            var t = Math.Tan(Math.PI / 4d - phi / 2d)
                  / Math.Pow((1d - eSinPhi) / (1d + eSinPhi), e / 2d); // p.161 (15-9)

            // r == ρ/a
            var r = 2d * ScaleFactor * t / Math.Sqrt(Math.Pow(1d + e, 1d + e) * Math.Pow(1d - e, 1d - e)); // p.161 (21-33)
            var m = Math.Cos(phi) / Math.Sqrt(1d - eSinPhi * eSinPhi); // p.160 (14-15)
            var k = r / m; // p.161 (21-32)

            var transform = new Matrix(k, 0d, 0d, k, 0d, 0d);
            transform.Rotate(-sign * longitude);
            return transform;
        }

        public override Location MapToLocation(double x, double y)
        {
            var sign = Hemisphere == Hemisphere.North ? 1d : -1d;
            x = sign * (x - FalseEasting);
            y = sign * (y - FalseNorthing);

            var e2 = (2d - Flattening) * Flattening;
            var e = Math.Sqrt(e2);
            var r = Math.Sqrt(x * x + y * y); // p.162 (20-18)
            var t = r * Math.Sqrt(Math.Pow(1d + e, 1d + e) * Math.Pow(1d - e, 1d - e))
                  / (2d * EquatorialRadius * ScaleFactor); // p.162 (21-39)

            var phi = WorldMercatorProjection.ApproximateLatitude(e2, t); // p.162 (3-5)
            var lambda = Math.Atan2(x, -y); // p.162 (20-16)

            return new Location(sign * phi * 180d / Math.PI, sign * lambda * 180d / Math.PI);
        }

        public override double GridConvergence(double x, double y)
        {
            var sign = Hemisphere == Hemisphere.North ? 1d : -1d;
            x = sign * (x - FalseEasting);
            y = sign * (y - FalseNorthing);

            var lambda = Math.Atan2(x, -y); // p.162 (20-16)

            return lambda * 180d / Math.PI;
        }
    }

    /// <summary>
    /// Universal Polar Stereographic North Projection - EPSG:32661.
    /// </summary>
    public class Wgs84UpsNorthProjection : PolarStereographicProjection
    {
        public const string DefaultCrsId = "EPSG:32661";

        public Wgs84UpsNorthProjection() // parameterless constructor for XAML
            : this(DefaultCrsId)
        {
        }

        public Wgs84UpsNorthProjection(string crsId)
        {
            CrsId = crsId;
            Hemisphere = Hemisphere.North;
        }
    }

    /// <summary>
    /// Universal Polar Stereographic South Projection - EPSG:32761.
    /// </summary>
    public class Wgs84UpsSouthProjection : PolarStereographicProjection
    {
        public const string DefaultCrsId = "EPSG:32761";

        public Wgs84UpsSouthProjection() // parameterless constructor for XAML
            : this(DefaultCrsId)
        {
        }

        public Wgs84UpsSouthProjection(string crsId)
        {
            CrsId = crsId;
            Hemisphere = Hemisphere.South;
        }
    }
}

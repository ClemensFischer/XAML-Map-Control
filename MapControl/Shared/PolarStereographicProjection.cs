﻿using System;
#if WPF
using System.Windows;
#endif

namespace MapControl
{
    /// <summary>
    /// Elliptical Polar Stereographic Projection with a given scale factor at the pole and
    /// optional false easting and northing, as used by the UPS North and UPS South projections.
    /// See "Map Projections - A Working Manual" (https://pubs.usgs.gov/publication/pp1395), p.154-163.
    /// </summary>
    public class PolarStereographicProjection : MapProjection
    {
        public PolarStereographicProjection()
        {
            Type = MapProjectionType.Azimuthal;
        }

        public double EquatorialRadius { get; set; } = Wgs84EquatorialRadius;
        public double Flattening { get; set; } = Wgs84Flattening;
        public double ScaleFactor { get; set; } = 0.994;
        public double FalseEasting { get; set; } = 2e6;
        public double FalseNorthing { get; set; } = 2e6;
        public bool IsNorth { get; set; }

        public override Point GetRelativeScale(Location location)
        {
            var lat = location.Latitude * Math.PI / 180d;

            if (!IsNorth)
            {
                lat = -lat;
            }

            var e = Math.Sqrt((2d - Flattening) * Flattening);
            var eSinLat = e * Math.Sin(lat);

            var t = Math.Tan(Math.PI / 4d - lat / 2d)
                  / Math.Pow((1d - eSinLat) / (1d + eSinLat), e / 2d); // p.161 (15-9)
            var r = 2d * EquatorialRadius * ScaleFactor * t
                  / Math.Sqrt(Math.Pow(1d + e, 1d + e) * Math.Pow(1d - e, 1d - e)); // p.161 (21-33)

            var m = Math.Cos(lat) / Math.Sqrt(1d - eSinLat * eSinLat); // p.160 (14-15)
            var k = r / (EquatorialRadius * m); // p.161 (21-32)

            return new Point(k, k);
        }

        public override Point? LocationToMap(Location location)
        {
            var lat = location.Latitude * Math.PI / 180d;
            var lon = location.Longitude * Math.PI / 180d;

            if (!IsNorth)
            {
                lat = -lat;
                lon = -lon;
            }

            var e = Math.Sqrt((2d - Flattening) * Flattening);
            var eSinLat = e * Math.Sin(lat);

            var t = Math.Tan(Math.PI / 4d - lat / 2d)
                  / Math.Pow((1d - eSinLat) / (1d + eSinLat), e / 2d); // p.161 (15-9)
            var r = 2d * EquatorialRadius * ScaleFactor * t
                  / Math.Sqrt(Math.Pow(1d + e, 1d + e) * Math.Pow(1d - e, 1d - e)); // p.161 (21-33)

            var x = r * Math.Sin(lon); // p.161 (21-30)
            var y = -r * Math.Cos(lon); // p.161 (21-31)

            if (!IsNorth)
            {
                x = -x;
                y = -y;
            }

            return new Point(x + FalseEasting, y + FalseNorthing);
        }

        public override Location MapToLocation(Point point)
        {
            var x = point.X - FalseEasting;
            var y = point.Y - FalseNorthing;

            if (!IsNorth)
            {
                x = -x;
                y = -y;
            }

            var e = Math.Sqrt((2d - Flattening) * Flattening);
            var r = Math.Sqrt(x * x + y * y); // p.162 (20-18)
            var t = r * Math.Sqrt(Math.Pow(1d + e, 1d + e) * Math.Pow(1d - e, 1d - e))
                  / (2d * EquatorialRadius * ScaleFactor); // p.162 (21-39)

            var lat = WorldMercatorProjection.ApproximateLatitude(e, t); // p.162 (3-5)
            var lon = Math.Atan2(x, -y); // p.162 (20-16)

            if (!IsNorth)
            {
                lat = -lat;
                lon = -lon;
            }

            return new Location(lat * 180d / Math.PI, lon * 180d / Math.PI);
        }
    }

    /// <summary>
    /// Universal Polar Stereographic North Projection - EPSG:32661.
    /// </summary>
    public class UpsNorthProjection : PolarStereographicProjection
    {
        public const string DefaultCrsId = "EPSG:32661";

        public UpsNorthProjection()
            : this(DefaultCrsId)
        {
            // XAML needs parameterless constructor
        }

        public UpsNorthProjection(string crsId)
        {
            CrsId = crsId;
            IsNorth = true;
        }
    }

    /// <summary>
    /// Universal Polar Stereographic South Projection - EPSG:32761.
    /// </summary>
    public class UpsSouthProjection : PolarStereographicProjection
    {
        public const string DefaultCrsId = "EPSG:32761";

        public UpsSouthProjection()
            : this(DefaultCrsId)
        {
            // XAML needs parameterless constructor
        }

        public UpsSouthProjection(string crsId)
        {
            CrsId = crsId;
            IsNorth = false;
        }
    }
}

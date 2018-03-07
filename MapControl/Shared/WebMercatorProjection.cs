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
    /// Transforms map coordinates according to the Web (or Pseudo) Mercator Projection, EPSG:3857.
    /// Longitude values are transformed linearly to X values in meters, by multiplying with TrueScale.
    /// Latitude values in the interval [-MaxLatitude .. MaxLatitude] are transformed to Y values in meters
    /// in the interval [-R*pi .. R*pi], R=Wgs84EquatorialRadius.
    /// </summary>
    public class WebMercatorProjection : MapProjection
    {
        public WebMercatorProjection()
            : this("EPSG:3857")
        {
        }

        public WebMercatorProjection(string crsId)
        {
            CrsId = crsId;
            IsWebMercator = true;
            MaxLatitude = YToLatitude(180d);
        }

        public override Vector GetMapScale(Location location)
        {
            var scale = ViewportScale / Math.Cos(location.Latitude * Math.PI / 180d);

            return new Vector(scale, scale);
        }

        public override Point LocationToPoint(Location location)
        {
            return new Point(
                TrueScale * location.Longitude,
                TrueScale * LatitudeToY(location.Latitude));
        }

        public override Location PointToLocation(Point point)
        {
            return new Location(
                YToLatitude(point.Y / TrueScale),
                point.X / TrueScale);
        }

        public static double LatitudeToY(double latitude)
        {
            if (latitude <= -90d)
            {
                return double.NegativeInfinity;
            }

            if (latitude >= 90d)
            {
                return double.PositiveInfinity;
            }

            return Math.Log(Math.Tan((latitude + 90d) * Math.PI / 360d)) / Math.PI * 180d;
        }

        public static double YToLatitude(double y)
        {
            return 90d - Math.Atan(Math.Exp(-y * Math.PI / 180d)) / Math.PI * 360d;
        }
    }
}

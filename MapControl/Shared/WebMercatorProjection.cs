// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2017 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if WINDOWS_UWP
using Windows.Foundation;
#else
using System.Windows;
#endif

namespace MapControl
{
    /// <summary>
    /// Transforms map coordinates according to the Web (or Pseudo) Mercator Projection, EPSG:3857.
    /// Longitude values are transformed linearly to X values in meters, by multiplying with MetersPerDegree.
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
            LongitudeScale = MetersPerDegree;
            MaxLatitude = YToLatitude(180d);
        }

        public override double GetViewportScale(double zoomLevel)
        {
            return DegreesToViewportScale(zoomLevel) / MetersPerDegree;
        }

        public override Point GetMapScale(Location location)
        {
            var scale = ViewportScale / Math.Cos(location.Latitude * Math.PI / 180d);

            return new Point(scale, scale);
        }

        public override Point LocationToPoint(Location location)
        {
            return new Point(
                MetersPerDegree * location.Longitude,
                MetersPerDegree * LatitudeToY(location.Latitude));
        }

        public override Location PointToLocation(Point point)
        {
            return new Location(
                YToLatitude(point.Y / MetersPerDegree),
                point.X / MetersPerDegree);
        }

        public override Location TranslateLocation(Location location, Point translation)
        {
            var scaleX = MetersPerDegree * ViewportScale;
            var scaleY = scaleX / Math.Cos(location.Latitude * Math.PI / 180d);

            return new Location(
                location.Latitude - translation.Y / scaleY,
                location.Longitude + translation.X / scaleX);
        }

        public static double LatitudeToY(double latitude)
        {
            return latitude <= -90d ? double.NegativeInfinity
                : latitude >= 90d ? double.PositiveInfinity
                : Math.Log(Math.Tan((latitude + 90d) * Math.PI / 360d)) / Math.PI * 180d;
        }

        public static double YToLatitude(double y)
        {
            return 90d - Math.Atan(Math.Exp(-y * Math.PI / 180d)) / Math.PI * 360d;
        }
    }
}

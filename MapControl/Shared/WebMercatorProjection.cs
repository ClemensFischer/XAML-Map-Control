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
    /// Spherical Mercator Projection, EPSG:3857.
    /// See "Map Projections - A Working Manual" (https://pubs.usgs.gov/pp/1395/report.pdf), p.41-44.
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
            IsNormalCylindrical = true;
            IsWebMercator = true;
            MaxLatitude = YToLatitude(180d);
        }

        public override Vector GetMapScale(Location location)
        {
            var k = 1d / Math.Cos(location.Latitude * Math.PI / 180d); // p.44 (7-3)

            return new Vector(ViewportScale * k, ViewportScale * k);
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

            return Math.Log(Math.Tan((latitude + 90d) * Math.PI / 360d)) * 180d / Math.PI;
        }

        public static double YToLatitude(double y)
        {
            return 90d - Math.Atan(Math.Exp(-y * Math.PI / 180d)) * 360d / Math.PI;
        }
    }
}

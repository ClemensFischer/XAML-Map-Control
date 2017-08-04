// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2017 Clemens Fischer
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
    /// Base class for azimuthal map projections.
    /// </summary>
    public abstract class AzimuthalProjection : MapProjection
    {
        protected Location projectionCenter = new Location();

        public AzimuthalProjection()
        {
            IsAzimuthal = true;
            LongitudeScale = double.NaN;
        }

        public override double GetViewportScale(double zoomLevel)
        {
            return DegreesToViewportScale(zoomLevel) / MetersPerDegree;
        }

        public override Point GetMapScale(Location location)
        {
            return new Point(ViewportScale, ViewportScale);
        }

        public override Location TranslateLocation(Location location, Point translation)
        {
            var scaleY = ViewportScale * MetersPerDegree;
            var scaleX = scaleY * Math.Cos(location.Latitude * Math.PI / 180d);

            return new Location(
                location.Latitude - translation.Y / scaleY,
                location.Longitude + translation.X / scaleX);
        }

        public override Rect BoundingBoxToRect(BoundingBox boundingBox)
        {
            var cbbox = boundingBox as CenteredBoundingBox;

            if (cbbox != null)
            {
                var center = LocationToPoint(cbbox.Center);

                return new Rect(
                     center.X - cbbox.Width / 2d, center.Y - cbbox.Height / 2d,
                     cbbox.Width, cbbox.Height);
            }

            return base.BoundingBoxToRect(boundingBox);
        }

        public override BoundingBox RectToBoundingBox(Rect rect)
        {
            var center = PointToLocation(new Point(rect.X + rect.Width / 2d, rect.Y + rect.Height / 2d));

            return new CenteredBoundingBox(center, rect.Width, rect.Height); // width and height in meters
        }

        public override void SetViewportTransform(Location projectionCenter, Location mapCenter, Point viewportCenter, double zoomLevel, double heading)
        {
            this.projectionCenter = projectionCenter;

            base.SetViewportTransform(projectionCenter, mapCenter, viewportCenter, zoomLevel, heading);
        }

        public override string WmsQueryParameters(BoundingBox boundingBox, string version)
        {
            if (string.IsNullOrEmpty(CrsId))
            {
                return null;
            }

            var rect = BoundingBoxToRect(boundingBox);
            var width = (int)Math.Round(ViewportScale * rect.Width);
            var height = (int)Math.Round(ViewportScale * rect.Height);
            var crs = version.StartsWith("1.1.") ? "SRS" : "CRS";

            return string.Format(CultureInfo.InvariantCulture,
                "{0}={1},1,{2},{3}&BBOX={4},{5},{6},{7}&WIDTH={8}&HEIGHT={9}",
                crs, CrsId, projectionCenter.Longitude, projectionCenter.Latitude,
                rect.X, rect.Y, (rect.X + rect.Width), (rect.Y + rect.Height), width, height);
        }

        /// <summary>
        /// Calculates azimuth and distance in radians from location1 to location2.
        /// The returned distance has to be multiplied with an appropriate earth radius.
        /// </summary>
        public static void GetAzimuthDistance(Location location1, Location location2, out double azimuth, out double distance)
        {
            var lat1 = location1.Latitude * Math.PI / 180d;
            var lon1 = location1.Longitude * Math.PI / 180d;
            var lat2 = location2.Latitude * Math.PI / 180d;
            var lon2 = location2.Longitude * Math.PI / 180d;
            var cosLat1 = Math.Cos(lat1);
            var sinLat1 = Math.Sin(lat1);
            var cosLat2 = Math.Cos(lat2);
            var sinLat2 = Math.Sin(lat2);
            var cosLon12 = Math.Cos(lon2 - lon1);
            var sinLon12 = Math.Sin(lon2 - lon1);
            var cosDistance = sinLat1 * sinLat2 + cosLat1 * cosLat2 * cosLon12;

            azimuth = Math.Atan2(sinLon12, cosLat1 * sinLat2 / cosLat2 - sinLat1 * cosLon12);
            distance = Math.Acos(Math.Max(Math.Min(cosDistance, 1d), -1d));
        }

        /// <summary>
        /// Calculates the Location of the point given by azimuth and distance in radians from location.
        /// </summary>
        public static Location GetLocation(Location location, double azimuth, double distance)
        {
            var lat1 = location.Latitude * Math.PI / 180d;
            var sinDistance = Math.Sin(distance);
            var cosDistance = Math.Cos(distance);
            var cosAzimuth = Math.Cos(azimuth);
            var sinAzimuth = Math.Sin(azimuth);
            var cosLat1 = Math.Cos(lat1);
            var sinLat1 = Math.Sin(lat1);
            var sinLat2 = sinLat1 * cosDistance + cosLat1 * sinDistance * cosAzimuth;
            var lat2 = Math.Asin(Math.Max(Math.Min(sinLat2, 1d), -1d));
            var dLon = Math.Atan2(sinDistance * sinAzimuth, cosLat1 * cosDistance - sinLat1 * sinDistance * cosAzimuth);

            return new Location(180d / Math.PI * lat2, location.Longitude + 180d / Math.PI * dLon);
        }
    }
}

// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2017 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Globalization;
#if NETFX_CORE
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
        protected Location centerLocation = new Location();
        protected double centerRadius = Wgs84EquatorialRadius;

        public override bool IsAzimuthal { get; } = true;

        public override double LongitudeScale { get; } = double.NaN;

        public override double GetViewportScale(double zoomLevel)
        {
            return base.GetViewportScale(zoomLevel) / MetersPerDegree;
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

            if (cbbox == null)
            {
                return base.BoundingBoxToRect(boundingBox);
            }

            var center = LocationToPoint(cbbox.Center);

            return new Rect(
                 center.X - cbbox.Width / 2d, center.Y - cbbox.Height / 2d,
                 cbbox.Width, cbbox.Height);
        }

        public override BoundingBox RectToBoundingBox(Rect rect)
        {
            var center = PointToLocation(new Point(rect.X + rect.Width / 2d, rect.Y + rect.Height / 2d));

            return new CenteredBoundingBox(center, rect.Width, rect.Height); // width and height in meters
        }

        public override void SetViewportTransform(Location center, Point viewportCenter, double zoomLevel, double heading)
        {
            centerLocation = center;
            centerRadius = GeocentricRadius(center);
            ViewportScale = GetViewportScale(zoomLevel);

            ViewportTransform.Matrix = MatrixEx.TranslateScaleRotateTranslate(
                new Point(), ViewportScale, -ViewportScale, heading, viewportCenter);
        }

        public override string WmsQueryParameters(BoundingBox boundingBox, string version)
        {
            var rect = BoundingBoxToRect(boundingBox);
            var width = (int)Math.Round(ViewportScale * rect.Width);
            var height = (int)Math.Round(ViewportScale * rect.Height);
            var crs = version.StartsWith("1.1.") ? "SRS" : "CRS";

            return string.Format(CultureInfo.InvariantCulture,
                "{0}={1},1,{2},{3}&BBOX={4},{5},{6},{7}&WIDTH={8}&HEIGHT={9}",
                crs, CrsId, centerLocation.Longitude, centerLocation.Latitude,
                rect.X, rect.Y, (rect.X + rect.Width), (rect.Y + rect.Height), width, height);
        }

        /// <summary>
        /// Calculates the geocentric earth radius at the specified location,
        /// based on the specified ellipsoid equatorial radius and flattening values.
        /// </summary>
        public static double GeocentricRadius(Location location,
            double equatorialRadius = Wgs84EquatorialRadius, double flattening = Wgs84Flattening)
        {
            var a = equatorialRadius;
            var b = a * (1d - flattening);
            var aCosLat = a * Math.Cos(location.Latitude * Math.PI / 180);
            var bSinLat = b * Math.Sin(location.Latitude * Math.PI / 180);
            var aCosLat2 = aCosLat * aCosLat;
            var bSinLat2 = bSinLat * bSinLat;

            return Math.Sqrt((a * a * aCosLat2 + b * b * bSinLat2) / (aCosLat2 + bSinLat2));
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

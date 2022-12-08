// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;

namespace MapControl
{
    /// <summary>
    /// Base class for azimuthal map projections.
    /// </summary>
    public abstract class AzimuthalProjection : MapProjection
    {
        protected AzimuthalProjection()
        {
            Type = MapProjectionType.Azimuthal;
        }

        public override BoundingBox MapRectToBoundingBox(MapRect mapRect)
        {
            var center = MapToLocation(mapRect.Center);

            return center != null
                ? new CenteredBoundingBox(center, mapRect.Width / Wgs84MeterPerDegree, mapRect.Height / Wgs84MeterPerDegree)
                : null;
        }

        /// <summary>
        /// Calculates azimuth and spherical distance in radians from location1 to location2.
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
            distance = Math.Acos(Math.Min(Math.Max(cosDistance, -1d), 1d));
        }

        /// <summary>
        /// Calculates the Location of the point given by azimuth and spherical distance in radians from location.
        /// </summary>
        public static Location GetLocation(Location location, double azimuth, double distance)
        {
            var lat = location.Latitude;
            var lon = location.Longitude;

            if (distance > 0d)
            {
                var lat1 = lat * Math.PI / 180d;
                var sinDistance = Math.Sin(distance);
                var cosDistance = Math.Cos(distance);
                var cosAzimuth = Math.Cos(azimuth);
                var sinAzimuth = Math.Sin(azimuth);
                var cosLat1 = Math.Cos(lat1);
                var sinLat1 = Math.Sin(lat1);
                var sinLat2 = sinLat1 * cosDistance + cosLat1 * sinDistance * cosAzimuth;
                var lat2 = Math.Asin(Math.Min(Math.Max(sinLat2, -1d), 1d));
                var dLon = Math.Atan2(sinDistance * sinAzimuth, cosLat1 * cosDistance - sinLat1 * sinDistance * cosAzimuth);

                lat = lat2 * 180d / Math.PI;
                lon += dLon * 180d / Math.PI;
            }

            return new Location(lat, lon);
        }
    }
}

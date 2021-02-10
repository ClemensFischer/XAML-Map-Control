// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;

namespace MapControl
{
    /// <summary>
    /// Provides helper methods for geodetic calculations on a sphere.
    /// </summary>
    public static class LocationEx
    {
        /// <summary>
        /// see https://en.wikipedia.org/wiki/Great-circle_navigation
        /// </summary>
        public static double GreatCircleDistance(
            this Location location1, Location location2, double earthRadius = MapProjection.Wgs84EquatorialRadius)
        {
            var lat1 = location1.Latitude * Math.PI / 180d;
            var lon1 = location1.Longitude * Math.PI / 180d;
            var lat2 = location2.Latitude * Math.PI / 180d;
            var lon2 = location2.Longitude * Math.PI / 180d;
            var sinLat1 = Math.Sin(lat1);
            var cosLat1 = Math.Cos(lat1);
            var sinLat2 = Math.Sin(lat2);
            var cosLat2 = Math.Cos(lat2);
            var sinLon12 = Math.Sin(lon2 - lon1);
            var cosLon12 = Math.Cos(lon2 - lon1);
            var a = cosLat1 * sinLat2 - sinLat1 * cosLat2 * cosLon12;
            var b = cosLat2 * sinLon12;
            var s12 = Math.Atan2(Math.Sqrt(a * a + b * b), sinLat1 * sinLat2 + cosLat1 * cosLat2 * cosLon12);

            return earthRadius * s12;
        }

        /// <summary>
        /// see https://en.wikipedia.org/wiki/Great-circle_navigation
        /// </summary>
        public static Location GreatCircleLocation(
            this Location location, double azimuth, double distance, double earthRadius = MapProjection.Wgs84EquatorialRadius)
        {
            var s12 = distance / earthRadius;
            var az1 = azimuth * Math.PI / 180d;
            var lat1 = location.Latitude * Math.PI / 180d;
            var lon1 = location.Longitude * Math.PI / 180d;
            var sinS12 = Math.Sin(s12);
            var cosS12 = Math.Cos(s12);
            var sinAz1 = Math.Sin(az1);
            var cosAz1 = Math.Cos(az1);
            var sinLat1 = Math.Sin(lat1);
            var cosLat1 = Math.Cos(lat1);
            var lat2 = Math.Asin(sinLat1 * cosS12 + cosLat1 * sinS12 * cosAz1);
            var lon2 = lon1 + Math.Atan2(sinS12 * sinAz1, (cosLat1 * cosS12 - sinLat1 * sinS12 * cosAz1));

            return new Location(lat2 * 180d / Math.PI, lon2 * 180d / Math.PI);
        }
    }
}

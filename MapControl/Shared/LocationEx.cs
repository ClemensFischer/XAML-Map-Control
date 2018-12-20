// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Linq;

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
        public static double GreatCircleDistance(this Location location1, Location location2, double earthRadius = MapProjection.Wgs84EquatorialRadius)
        {
            var lat1 = location1.Latitude * Math.PI / 180d;
            var lon1 = location1.Longitude * Math.PI / 180d;
            var lat2 = location2.Latitude * Math.PI / 180d;
            var lon2 = location2.Longitude * Math.PI / 180d;
            var sinLat1 = Math.Sin(lat1);
            var cosLat1 = Math.Cos(lat1);
            var sinLat2 = Math.Sin(lat2);
            var cosLat2 = Math.Cos(lat2);
            var cosLon12 = Math.Cos(lon2 - lon1);
            var cosS12 = sinLat1 * sinLat2 + cosLat1 * cosLat2 * cosLon12;
            var s12 = 0d;

            if (Math.Abs(cosS12) < 0.99999999)
            {
                s12 = Math.Acos(Math.Min(Math.Max(cosS12, -1d), 1d));
            }
            else
            {
                var sinLon12 = Math.Sin(lon2 - lon1);
                var a = cosLat1 * sinLat2 - sinLat1 * cosLat2 * cosLon12;
                var b = cosLat2 * sinLon12;
                s12 = Math.Atan2(Math.Sqrt(a * a + b * b), cosS12);
            }

            return earthRadius * s12;
        }

        /// <summary>
        /// see https://en.wikipedia.org/wiki/Great-circle_navigation
        /// </summary>
        public static Location GreatCircleLocation(this Location location, double azimuth, double distance, double earthRadius = MapProjection.Wgs84EquatorialRadius)
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

        public static LocationCollection CalculateMeridianLocations(this Location location, double latitude2, double resolution = 1d)
        {
            if (resolution <= 0d)
            {
                throw new ArgumentOutOfRangeException("The parameter resolution must be greater than zero.");
            }

            var locations = new LocationCollection();
            var s = latitude2 - location.Latitude;
            var n = (int)Math.Ceiling(Math.Abs(s) / resolution);

            for (int i = 0; i <= n; i++)
            {
                locations.Add(new Location(location.Latitude + i * s / n, location.Longitude));
            }

            return locations;
        }

        /// <summary>
        /// see https://en.wikipedia.org/wiki/Great-circle_navigation
        /// </summary>
        public static LocationCollection CalculateGreatCircleLocations(this Location location1, Location location2, double resolution = 1d)
        {
            if (resolution <= 0d)
            {
                throw new ArgumentOutOfRangeException("The parameter resolution must be greater than zero.");
            }

            if (location1.Longitude == location2.Longitude ||
                location1.Latitude <= -90d || location1.Latitude >= 90d ||
                location2.Latitude <= -90d || location2.Latitude >= 90d)
            {
                return CalculateMeridianLocations(location1, location2.Latitude);
            }

            var locations = new LocationCollection(new Location(location1.Latitude, location1.Longitude));

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

            var cosS12 = sinLat1 * sinLat2 + cosLat1 * cosLat2 * cosLon12;
            var s12 = Math.Acos(Math.Min(Math.Max(cosS12, -1d), 1d));
            var n = (int)Math.Ceiling(s12 / resolution * 180d / Math.PI);

            if (n > 1)
            {
                var az1 = Math.Atan2(sinLon12, cosLat1 * sinLat2 / cosLat2 - sinLat1 * cosLon12);
                var cosAz1 = Math.Cos(az1);
                var sinAz1 = Math.Sin(az1);

                var az0 = Math.Atan2(sinAz1 * cosLat1, Math.Sqrt(cosAz1 * cosAz1 + sinAz1 * sinAz1 * sinLat1 * sinLat1));
                var sinAz0 = Math.Sin(az0);
                var cosAz0 = Math.Cos(az0);

                var s01 = Math.Atan2(sinLat1, cosLat1 * cosAz1);
                var lon0 = lon1 - Math.Atan2(sinAz0 * Math.Sin(s01), Math.Cos(s01));

                for (int i = 1; i < n; i++)
                {
                    double s = s01 + i * s12 / n;
                    double sinS = Math.Sin(s);
                    double cosS = Math.Cos(s);
                    double lat = Math.Atan2(cosAz0 * sinS, Math.Sqrt(cosS * cosS + sinAz0 * sinAz0 * sinS * sinS));
                    double lon = Math.Atan2(sinAz0 * sinS, cosS) + lon0;

                    locations.Add(lat * 180d / Math.PI, lon * 180d / Math.PI);
                }
            }

            locations.Add(location2.Latitude, location2.Longitude);
            return locations;
        }

        /// <summary>
        /// see https://en.wikipedia.org/wiki/Rhumb_line
        /// </summary>
        public static LocationCollection CalculateRhumbLineLocations(this Location location1, Location location2, double resolution = 1d)
        {
            if (resolution <= 0d)
            {
                throw new ArgumentOutOfRangeException("The parameter resolution must be greater than zero.");
            }

            var y1 = WebMercatorProjection.LatitudeToY(location1.Latitude);

            if (double.IsInfinity(y1))
            {
                throw new ArgumentOutOfRangeException("The parameter location1 must have an absolute latitude value of less than 90 degrees.");
            }

            var y2 = WebMercatorProjection.LatitudeToY(location2.Latitude);

            if (double.IsInfinity(y2))
            {
                throw new ArgumentOutOfRangeException("The parameter location2 must have an absolute latitude value of less than 90 degrees.");
            }

            var x1 = location1.Longitude;
            var x2 = location2.Longitude;
            var dx = x2 - x1;
            var dy = y2 - y1;
            var s = Math.Sqrt(dx * dx + dy * dy);
            var n = (int)Math.Ceiling(s / resolution);

            var locations = new LocationCollection(new Location(location1.Latitude, location1.Longitude));

            for (int i = 1; i < n; i++)
            {
                double x = x1 + i * dx / n;
                double y = y1 + i * dy / n;

                locations.Add(WebMercatorProjection.YToLatitude(y), x);
            }

            locations.Add(location2.Latitude, location2.Longitude);
            return locations;
        }

        public static void Add(this LocationCollection locations, double latitude, double longitude)
        {
            if (locations.Count > 0)
            {
                var deltaLon = longitude - locations.Last().Longitude;

                if (deltaLon < -180d)
                {
                    longitude += 360d;
                }
                else if (deltaLon > 180)
                {
                    longitude -= 360;
                }
            }

            locations.Add(new Location(latitude, longitude));
        }
    }
}

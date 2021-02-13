// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Globalization;

namespace MapControl
{
    /// <summary>
    /// A geographic location with latitude and longitude values in degrees.
    /// </summary>
#if !WINDOWS_UWP
    [System.ComponentModel.TypeConverter(typeof(LocationConverter))]
#endif
    public class Location : IEquatable<Location>
    {
        private double latitude;
        private double longitude;

        public Location()
        {
        }

        public Location(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public double Latitude
        {
            get { return latitude; }
            set { latitude = Math.Min(Math.Max(value, -90d), 90d); }
        }

        public double Longitude
        {
            get { return longitude; }
            set { longitude = value; }
        }

        public bool Equals(Location location)
        {
            return location != null
                && Math.Abs(location.latitude - latitude) < 1e-9
                && Math.Abs(location.longitude - longitude) < 1e-9;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Location);
        }

        public override int GetHashCode()
        {
            return latitude.GetHashCode() ^ longitude.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:F5},{1:F5}", latitude, longitude);
        }

        public static Location Parse(string locationString)
        {
            Location location = null;

            if (!string.IsNullOrEmpty(locationString))
            {
                var values = locationString.Split(new char[] { ',' });

                if (values.Length != 2)
                {
                    throw new FormatException("Location string must be a comma-separated pair of double values.");
                }

                location = new Location(
                    double.Parse(values[0], NumberStyles.Float, CultureInfo.InvariantCulture),
                    double.Parse(values[1], NumberStyles.Float, CultureInfo.InvariantCulture));
            }

            return location;
        }

        /// <summary>
        /// Normalizes a longitude to a value in the interval [-180 .. 180].
        /// </summary>
        public static double NormalizeLongitude(double longitude)
        {
            if (longitude < -180d)
            {
                longitude = ((longitude + 180d) % 360d) + 180d;
            }
            else if (longitude > 180d)
            {
                longitude = ((longitude - 180d) % 360d) - 180d;
            }

            return longitude;
        }

        /// <summary>
        /// Calculates the great circle distance between this and the specified Location.
        /// https://en.wikipedia.org/wiki/Great_circle
        /// https://en.wikipedia.org/wiki/Great-circle_distance
        /// https://en.wikipedia.org/wiki/Great-circle_navigation
        /// </summary>
        public double GetDistance(
            Location location, double earthRadius = MapProjection.Wgs84EquatorialRadius)
        {
            var lat1 = latitude * Math.PI / 180d;
            var lon1 = longitude * Math.PI / 180d;
            var lat2 = location.latitude * Math.PI / 180d;
            var lon2 = location.longitude * Math.PI / 180d;
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
        /// Calculates the Location on a great circle at the specified azimuth angle and distance from this Location.
        /// https://en.wikipedia.org/wiki/Great_circle
        /// https://en.wikipedia.org/wiki/Great-circle_navigation
        /// </summary>
        public Location GetLocation(
            double azimuth, double distance, double earthRadius = MapProjection.Wgs84EquatorialRadius)
        {
            var s12 = distance / earthRadius;
            var az1 = azimuth * Math.PI / 180d;
            var lat1 = latitude * Math.PI / 180d;
            var lon1 = longitude * Math.PI / 180d;
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

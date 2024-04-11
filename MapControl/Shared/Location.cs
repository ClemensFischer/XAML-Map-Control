// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Globalization;

namespace MapControl
{
    /// <summary>
    /// A geographic location with latitude and longitude values in degrees.
    /// </summary>
#if WINUI || UWP
    [Windows.Foundation.Metadata.CreateFromString(MethodName = "Parse")]
#else
    [System.ComponentModel.TypeConverter(typeof(LocationConverter))]
#endif
    public class Location : IEquatable<Location>
    {
        public Location()
        {
        }

        public Location(double latitude, double longitude)
        {
            Latitude = Math.Min(Math.Max(latitude, -90d), 90d);
            Longitude = longitude;
        }

        public double Latitude { get; }
        public double Longitude { get; }

        public bool Equals(Location location)
        {
            return location != null
                && Math.Abs(location.Latitude - Latitude) < 1e-9
                && Math.Abs(location.Longitude - Longitude) < 1e-9;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Location);
        }

        public override int GetHashCode()
        {
            return Latitude.GetHashCode() ^ Longitude.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:F5},{1:F5}", Latitude, Longitude);
        }

        /// <summary>
        /// Creates a Location instance from a string containing a comma-separated pair of floating point numbers.
        /// </summary>
        public static Location Parse(string location)
        {
            string[] values = null;

            if (!string.IsNullOrEmpty(location))
            {
                values = location.Split(new char[] { ',' });
            }

            if (values?.Length != 2)
            {
                throw new FormatException("Location string must contain a comma-separated pair of floating point numbers.");
            }

            return new Location(
                double.Parse(values[0], NumberStyles.Float, CultureInfo.InvariantCulture),
                double.Parse(values[1], NumberStyles.Float, CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Normalizes a longitude to a value in the interval [-180 .. 180).
        /// </summary>
        public static double NormalizeLongitude(double longitude)
        {
            var x = (longitude + 180d) % 360d;

            return x < 0d ? x + 180d : x - 180d;
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
            var lat1 = Latitude * Math.PI / 180d;
            var lon1 = Longitude * Math.PI / 180d;
            var lat2 = location.Latitude * Math.PI / 180d;
            var lon2 = location.Longitude * Math.PI / 180d;
            var sinLat1 = Math.Sin(lat1);
            var cosLat1 = Math.Cos(lat1);
            var sinLat2 = Math.Sin(lat2);
            var cosLat2 = Math.Cos(lat2);
            var sinLon12 = Math.Sin(lon2 - lon1);
            var cosLon12 = Math.Cos(lon2 - lon1);
            var a = cosLat1 * sinLat2 - sinLat1 * cosLat2 * cosLon12;
            var b = cosLat2 * sinLon12;
            var c = sinLat1 * sinLat2 + cosLat1 * cosLat2 * cosLon12;
            var s12 = Math.Atan2(Math.Sqrt(a * a + b * b), c);

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
            var lat1 = Latitude * Math.PI / 180d;
            var lon1 = Longitude * Math.PI / 180d;
            var sinS12 = Math.Sin(s12);
            var cosS12 = Math.Cos(s12);
            var sinAz1 = Math.Sin(az1);
            var cosAz1 = Math.Cos(az1);
            var sinLat1 = Math.Sin(lat1);
            var cosLat1 = Math.Cos(lat1);
            var lat2 = Math.Asin(sinLat1 * cosS12 + cosLat1 * sinS12 * cosAz1);
            var lon2 = lon1 + Math.Atan2(sinS12 * sinAz1, cosLat1 * cosS12 - sinLat1 * sinS12 * cosAz1);

            return new Location(lat2 * 180d / Math.PI, lon2 * 180d / Math.PI);
        }
    }
}

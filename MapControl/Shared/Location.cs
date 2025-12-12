using System;
using System.Globalization;

namespace MapControl
{
    /// <summary>
    /// A geographic location with latitude and longitude values in degrees.
    /// For calculations with azimuth and distance on great circles, see
    /// https://en.wikipedia.org/wiki/Great_circle
    /// https://en.wikipedia.org/wiki/Great-circle_distance
    /// https://en.wikipedia.org/wiki/Great-circle_navigation
    /// </summary>
#if UWP || WINUI
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
            return location != null &&
                   Equals(Latitude, location.Latitude) &&
                   Equals(Longitude, location.Longitude);
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
            return string.Format(CultureInfo.InvariantCulture, "{0},{1}", Latitude, Longitude);
        }

        public static bool CoordinateEquals(double coordinate1, double coordinate2)
        {
            return Math.Abs(coordinate1 - coordinate2) < 1e-9;
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
                throw new FormatException($"{nameof(Location)} string must contain a comma-separated pair of floating point numbers.");
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
        /// Calculates great circle azimuth and distance in radians between this and the specified Location.
        /// </summary>
        public void GetAzimuthDistance(double latitude, double longitude, out double azimuth, out double distance)
        {
            var lat1 = Latitude * Math.PI / 180d;
            var lon1 = Longitude * Math.PI / 180d;
            var lat2 = latitude * Math.PI / 180d;
            var lon2 = longitude * Math.PI / 180d;
            var cosLat1 = Math.Cos(lat1);
            var sinLat1 = Math.Sin(lat1);
            var cosLat2 = Math.Cos(lat2);
            var sinLat2 = Math.Sin(lat2);
            var cosLon12 = Math.Cos(lon2 - lon1);
            var sinLon12 = Math.Sin(lon2 - lon1);
            var a = cosLat2 * sinLon12;
            var b = cosLat1 * sinLat2 - sinLat1 * cosLat2 * cosLon12;
            // α1
            azimuth = Math.Atan2(a, b);
            // σ12
            distance = Math.Atan2(Math.Sqrt(a * a + b * b), sinLat1 * sinLat2 + cosLat1 * cosLat2 * cosLon12);
        }

        /// <summary>
        /// Calculates the great circle distance in meters between this and the specified Location.
        /// </summary>
        public double GetDistance(Location location, double earthRadius = MapProjection.Wgs84EquatorialRadius)
        {
            GetAzimuthDistance(location.Latitude, location.Longitude, out _, out double distance);

            return earthRadius * distance;
        }

        /// <summary>
        /// Calculates the Location on a great circle at the specified azimuth and distance in radians from this Location.
        /// </summary>
        public Location GetLocation(double azimuth, double distance)
        {
            var lat1 = Latitude * Math.PI / 180d;
            var lon1 = Longitude * Math.PI / 180d;
            var cosD = Math.Cos(distance);
            var sinD = Math.Sin(distance);
            var cosA = Math.Cos(azimuth);
            var sinA = Math.Sin(azimuth);
            var cosLat1 = Math.Cos(lat1);
            var sinLat1 = Math.Sin(lat1);
            var lat2 = Math.Asin(sinLat1 * cosD + cosLat1 * sinD * cosA);
            var lon2 = lon1 + Math.Atan2(sinD * sinA, cosLat1 * cosD - sinLat1 * sinD * cosA);

            return new Location(lat2 * 180d / Math.PI, lon2 * 180d / Math.PI);
        }

        /// <summary>
        /// Calculates the Location on a great circle at the specified azimuth in degrees and distance in meters from this Location.
        /// </summary>
        public Location GetLocation(double azimuth, double distance, double earthRadius = MapProjection.Wgs84EquatorialRadius)
        {
            return GetLocation(azimuth * Math.PI / 180d, distance / earthRadius);
        }
    }
}

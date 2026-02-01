using System;
using System.Globalization;

namespace MapControl
{
    /// <summary>
    /// A geographic location with latitude and longitude values in degrees.
    /// </summary>
#if UWP || WINUI
    [Windows.Foundation.Metadata.CreateFromString(MethodName = "Parse")]
#else
    [System.ComponentModel.TypeConverter(typeof(LocationConverter))]
#endif
    public class Location(double latitude, double longitude) : IEquatable<Location>
    {
        public double Latitude { get; } = Math.Min(Math.Max(latitude, -90d), 90d);
        public double Longitude => longitude;

        public bool LatitudeEquals(double latitude) => Math.Abs(Latitude - latitude) < 1e-9;

        public bool LongitudeEquals(double longitude) => Math.Abs(Longitude - longitude) < 1e-9;

        public bool Equals(double latitude, double longitude) => LatitudeEquals(latitude) && LongitudeEquals(longitude);

        public bool Equals(Location location) => location != null && Equals(location.Latitude, location.Longitude);

        public override bool Equals(object obj) => Equals(obj as Location);

        public override int GetHashCode() => Latitude.GetHashCode() ^ Longitude.GetHashCode();

        public override string ToString() => string.Format(CultureInfo.InvariantCulture, "{0},{1}", Latitude, Longitude);

        /// <summary>
        /// Creates a Location instance from a string containing a comma-separated pair of floating point numbers.
        /// </summary>
        public static Location Parse(string location)
        {
            string[] values = null;

            if (!string.IsNullOrEmpty(location))
            {
                values = location.Split(',');
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

        // Arithmetic mean radius (2*a + b) / 3 == (1 - f/3) * a.
        // See https://en.wikipedia.org/wiki/Earth_radius#Arithmetic_mean_radius.
        //
        public const double Wgs84MeanRadius = (1d - MapProjection.Wgs84Flattening / 3d) * MapProjection.Wgs84EquatorialRadius;

        /// <summary>
        /// Calculates great circle azimuth in degrees and distance in meters between this and the specified Location.
        /// See https://en.wikipedia.org/wiki/Great-circle_navigation#Course.
        /// </summary>
        public (double, double) GetAzimuthDistance(Location location, double earthRadius = Wgs84MeanRadius)
        {
            var lat1 = Latitude * Math.PI / 180d;
            var lon1 = Longitude * Math.PI / 180d;
            var lat2 = location.Latitude * Math.PI / 180d;
            var lon2 = location.Longitude * Math.PI / 180d;
            var cosLat1 = Math.Cos(lat1);
            var sinLat1 = Math.Sin(lat1);
            var cosLat2 = Math.Cos(lat2);
            var sinLat2 = Math.Sin(lat2);
            var cosLon12 = Math.Cos(lon2 - lon1);
            var sinLon12 = Math.Sin(lon2 - lon1);
            var a = cosLat2 * sinLon12;
            var b = cosLat1 * sinLat2 - sinLat1 * cosLat2 * cosLon12;
            // α1
            var azimuth = Math.Atan2(a, b);
            // σ12
            var distance = Math.Atan2(Math.Sqrt(a * a + b * b), sinLat1 * sinLat2 + cosLat1 * cosLat2 * cosLon12);

            return (azimuth * 180d / Math.PI, distance * earthRadius);
        }

        /// <summary>
        /// Calculates great distance in meters between this and the specified Location.
        /// See https://en.wikipedia.org/wiki/Great-circle_navigation#Course.
        /// </summary>
        public double GetDistance(Location location, double earthRadius = Wgs84MeanRadius)
        {
            (var _, var distance) = GetAzimuthDistance(location, earthRadius);

            return distance;
        }

        /// <summary>
        /// Calculates the Location on a great circle at the specified azimuth in degrees and distance in meters from this Location.
        /// See https://en.wikipedia.org/wiki/Great-circle_navigation#Finding_way-points.
        /// </summary>
        public Location GetLocation(double azimuth, double distance, double earthRadius = Wgs84MeanRadius)
        {
            var lat1 = Latitude * Math.PI / 180d;
            var lon1 = Longitude * Math.PI / 180d;
            var a = azimuth * Math.PI / 180d;
            var d = distance / earthRadius;
            var cosLat1 = Math.Cos(lat1);
            var sinLat1 = Math.Sin(lat1);
            var cosA = Math.Cos(a);
            var sinA = Math.Sin(a);
            var cosD = Math.Cos(d);
            var sinD = Math.Sin(d);
            var lat2 = Math.Asin(sinLat1 * cosD + cosLat1 * sinD * cosA);
            var lon2 = lon1 + Math.Atan2(sinD * sinA, cosLat1 * cosD - sinLat1 * sinD * cosA);

            return new Location(lat2 * 180d / Math.PI, lon2 * 180d / Math.PI);
        }
    }
}

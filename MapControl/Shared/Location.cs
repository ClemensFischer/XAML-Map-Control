// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
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

        public static Location Parse(string s)
        {
            var values = s.Split(new char[] { ',' });

            if (values.Length != 2)
            {
                throw new FormatException("Location string must be a comma-separated pair of double values.");
            }

            return new Location(
                double.Parse(values[0], NumberStyles.Float, CultureInfo.InvariantCulture),
                double.Parse(values[1], NumberStyles.Float, CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Normalizes a longitude to a value in the interval [-180..180].
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

        internal static double NearestLongitude(double longitude, double referenceLongitude)
        {
            longitude = NormalizeLongitude(longitude);

            if (longitude > referenceLongitude + 180d)
            {
                longitude -= 360d;
            }
            else if (longitude < referenceLongitude - 180d)
            {
                longitude += 360d;
            }

            return longitude;
        }
    }
}

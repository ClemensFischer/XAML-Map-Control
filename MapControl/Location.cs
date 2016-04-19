// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2016 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Globalization;

namespace MapControl
{
    /// <summary>
    /// A geographic location with latitude and longitude values in degrees.
    /// </summary>
    public partial class Location : IEquatable<Location>
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
            return ReferenceEquals(this, location)
                || (location != null
                && location.latitude == latitude
                && location.longitude == longitude);
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
            var pair = s.Split(new char[] { ',' });

            if (pair.Length != 2)
            {
                throw new FormatException("Location string must be a comma-separated pair of double values");
            }

            return new Location(
                double.Parse(pair[0], NumberStyles.Float, CultureInfo.InvariantCulture),
                double.Parse(pair[1], NumberStyles.Float, CultureInfo.InvariantCulture));
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

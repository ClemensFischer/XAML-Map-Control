// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © Clemens Fischer 2012-2013
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Globalization;

namespace MapControl
{
    /// <summary>
    /// A geographic location given as latitude and longitude.
    /// </summary>
    public partial class Location : IEquatable<Location>
    {
        private double latitude;
        private double longitude;
        internal double Y;

        public Location()
        {
        }

        public Location(double lat, double lon)
        {
            Latitude = lat;
            Longitude = lon;
        }

        public double Latitude
        {
            get { return latitude; }
            set
            {
                latitude = Math.Min(Math.Max(value, -90d), 90d);
                Y = double.NaN;
            }
        }

        public double Longitude
        {
            get { return longitude; }
            set { longitude = value; }
        }

        public bool Equals(Location other)
        {
            return other != null && other.latitude == latitude && other.longitude == longitude;
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
            var tokens = s.Split(new char[] { ',' });
            if (tokens.Length != 2)
            {
                throw new FormatException("Location string must be a comma-separated pair of double values");
            }

            return new Location(
                double.Parse(tokens[0], NumberStyles.Float, CultureInfo.InvariantCulture),
                double.Parse(tokens[1], NumberStyles.Float, CultureInfo.InvariantCulture));
        }

        public static double NormalizeLongitude(double longitude)
        {
            return (longitude >= -180d && longitude <= 180d) ? longitude : ((longitude + 180d) % 360d + 360d) % 360d - 180d;
        }
    }
}

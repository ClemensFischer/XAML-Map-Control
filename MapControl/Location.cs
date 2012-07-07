// WPF MapControl - http://wpfmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;

namespace MapControl
{
    /// <summary>
    /// A geographic location given as latitude and longitude.
    /// </summary>
    [TypeConverter(typeof(LocationConverter))]
    public class Location
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

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:0.00000},{1:0.00000}", latitude, longitude);
        }

        public static Location Parse(string source)
        {
            Point p = Point.Parse(source);
            return new Location(p.X, p.Y);
        }

        public static double NormalizeLongitude(double longitude)
        {
            return ((longitude + 180d) % 360d + 360d) % 360d - 180d;
        }
    }

    public class LocationConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return Location.Parse((string)value);
        }
    }
}

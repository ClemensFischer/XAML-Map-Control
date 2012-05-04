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
        public Location()
        {
        }

        public Location(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public static Location Parse(string source)
        {
            Point p = Point.Parse(source);
            return new Location(p.X, p.Y);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:0.00000},{1:0.00000}", Latitude, Longitude);
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

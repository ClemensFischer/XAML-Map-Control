// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2016 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.ComponentModel;
using System.Globalization;

namespace MapControl
{
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

    [TypeConverter(typeof(LocationConverter))]
#if !SILVERLIGHT
    [Serializable]
#endif
    public partial class Location
    {
    }
}

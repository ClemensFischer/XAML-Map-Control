// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
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
    [Serializable]
    public partial class Location
    {
    }

    public class LocationCollectionConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return LocationCollection.Parse((string)value);
        }
    }

    [TypeConverter(typeof(LocationCollectionConverter))]
    public partial class LocationCollection
    {
    }

    public class BoundingBoxConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return BoundingBox.Parse((string)value);
        }
    }

    [TypeConverter(typeof(BoundingBoxConverter))]
    [Serializable]
    public partial class BoundingBox
    {
    }

    public class TileSourceConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return new TileSource { UriFormat = value as string };
        }
    }

    [TypeConverter(typeof(TileSourceConverter))]
    public partial class TileSource
    {
    }
}

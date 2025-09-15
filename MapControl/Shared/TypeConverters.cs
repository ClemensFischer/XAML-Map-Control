using System;
using System.ComponentModel;
using System.Globalization;
#if WPF
using System.Windows.Data;
#elif UWP
using Windows.UI.Xaml.Data;
#elif WINUI
using Microsoft.UI.Xaml.Data;
#elif AVALONIA
using Avalonia.Data.Converters;
#endif
#if UWP || WINUI
using ConverterCulture = System.String;
#else
using ConverterCulture = System.Globalization.CultureInfo;
#endif

namespace MapControl
{
    public partial class LocationConverter : TypeConverter, IValueConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return Location.Parse(value.ToString());
        }

        public object Convert(object value, Type targetType, object parameter, ConverterCulture culture)
        {
            return ConvertFrom(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, ConverterCulture culture)
        {
            return value.ToString();
        }
    }

    public partial class LocationCollectionConverter : TypeConverter, IValueConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return LocationCollection.Parse(value.ToString());
        }

        public object Convert(object value, Type targetType, object parameter, ConverterCulture culture)
        {
            return ConvertFrom(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, ConverterCulture culture)
        {
            return value.ToString();
        }
    }

    public partial class BoundingBoxConverter : TypeConverter, IValueConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return BoundingBox.Parse(value.ToString());
        }

        public object Convert(object value, Type targetType, object parameter, ConverterCulture culture)
        {
            return ConvertFrom(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, ConverterCulture culture)
        {
            return value.ToString();
        }
    }

    public partial class TileSourceConverter : TypeConverter, IValueConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return TileSource.Parse(value.ToString());
        }

        public object Convert(object value, Type targetType, object parameter, ConverterCulture culture)
        {
            return ConvertFrom(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, ConverterCulture culture)
        {
            return value.ToString();
        }
    }

    public class MapProjectionConverter : TypeConverter, IValueConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return MapProjection.Parse(value.ToString());
        }

        public object Convert(object value, Type targetType, object parameter, ConverterCulture culture)
        {
            return ConvertFrom(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, ConverterCulture culture)
        {
            return value.ToString();
        }
    }
}

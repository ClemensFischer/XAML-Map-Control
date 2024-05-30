using System;
using System.ComponentModel;
using System.Globalization;
#if WINUI
using Microsoft.UI.Xaml.Data;
#elif UWP
using Windows.UI.Xaml.Data;
#elif AVALONIA
using Avalonia.Data.Converters;
#endif

namespace SampleApplication
{
    public class DoubleTriggerConverter : IValueConverter
    {
        public double Trigger { get; set; }
        public object TriggerValue { get; set; }
        public object DefaultValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var converter = TypeDescriptor.GetConverter(targetType);

            return (double)value == Trigger ? converter.ConvertFrom(TriggerValue) : converter.ConvertFrom(DefaultValue);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value, targetType, parameter, "");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ConvertBack(value, targetType, parameter, "");
        }
    }

    public class MapHeadingToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (double)value != 0d;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value, targetType, parameter, "");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ConvertBack(value, targetType, parameter, "");
        }
    }
}
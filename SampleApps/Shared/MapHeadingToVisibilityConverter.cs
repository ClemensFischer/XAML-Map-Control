using System;
using System.Globalization;
#if WPF
using System.Windows;
using System.Windows.Data;
#elif UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
#elif WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
#else
using Avalonia.Data.Converters;
#endif

namespace SampleApplication
{
    public class MapHeadingToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
#if AVALONIA
            return (double)value != 0d;
#else
            return (double)value != 0d ? Visibility.Visible : Visibility.Collapsed;
#endif
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value, targetType, parameter, culture.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ConvertBack(value, targetType, parameter, culture.ToString());
        }
    }
}
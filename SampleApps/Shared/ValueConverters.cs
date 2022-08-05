using System;
using System.Globalization;
#if WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
#elif UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
#else
using System.Windows;
using System.Windows.Data;
#endif

namespace SampleApplication
{
    public class DoubleToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(parameter is double p))
            {
                p = double.Parse(parameter.ToString());
            }

            //System.Diagnostics.Debug.WriteLine((double)value);

            return (double)value != p ? Visibility.Visible : Visibility.Collapsed;
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
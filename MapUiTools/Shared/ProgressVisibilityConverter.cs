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

namespace MapControl.UiTools
{
    internal partial class ProgressVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var visible = (double)value < 1d;
#if AVALONIA
            return visible;
#else
            return visible ? Visibility.Visible : Visibility.Collapsed;
#endif
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value, targetType, parameter, culture.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

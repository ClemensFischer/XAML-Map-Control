using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using MapControl;

namespace WpfApplication
{
    public class LocationToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var visibility = Visibility.Hidden;

            if (values.Length == 2 && values[0] is Map && values[1] is Transform)
            {
                var parentMap = (Map)values[0];
                var transform = ((Transform)values[1]).Value;

                if (transform.OffsetX >= 0d && transform.OffsetX <= parentMap.ActualWidth &&
                    transform.OffsetY >= 0d && transform.OffsetY <= parentMap.ActualHeight)
                {
                    visibility = Visibility.Visible;
                }
            }

            return visibility;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using MapControl;

namespace SampleApplication
{
    public class ViewportPositionToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility visibility = Visibility.Hidden;

            if (values.Length == 2 && values[0] is Map && values[1] is Point? && ((Point?)values[1]).HasValue)
            {
                Map parentMap = (Map)values[0];
                Point position = ((Point?)values[1]).Value;

                if (position.X >= 0d && position.X <= parentMap.ActualWidth &&
                    position.Y >= 0d && position.Y <= parentMap.ActualHeight)
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

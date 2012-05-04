using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using MapControl;

namespace MapControlTestApp
{
    class MapBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            MapBackground mapBackground = (MapBackground)value;

            if (parameter as string == "Foreground")
            {
                return mapBackground == MapBackground.Light ? Brushes.Black : Brushes.White;
            }

            return mapBackground == MapBackground.Light ? Brushes.White : Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
#if UWP
using Windows.UI.Xaml.Data;
#elif WINUI
using Microsoft.UI.Xaml.Data;
#endif

namespace MapControl
{
    public class TileSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return TileSource.Parse(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}

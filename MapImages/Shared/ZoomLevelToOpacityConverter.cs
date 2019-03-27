// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2019 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Globalization;
#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
#else
using System.Windows;
using System.Windows.Data;
#endif

namespace MapControl.Images
{
    public class ZoomLevelToOpacityConverter : IValueConverter
    {
        public double MinZoomLevel { get; set; } = 0d;
        public double MaxZoomLevel { get; set; } = 22d;
        public double FadeZoomRange { get; set; } = 1d;
        public double MaxOpacity { get; set; } = 1d;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is double))
            {
                return DependencyProperty.UnsetValue;
            }

            var zoomLevel = (double)value;
            var opacity = 0d;

            if (zoomLevel > MinZoomLevel && zoomLevel < MaxZoomLevel)
            {
                opacity = MaxOpacity;

                if (FadeZoomRange > 0d)
                {
                    opacity = Math.Min(opacity, (zoomLevel - MinZoomLevel) / FadeZoomRange);
                    opacity = Math.Min(opacity, (MaxZoomLevel - zoomLevel) / FadeZoomRange);
                }
            }

            return opacity;
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return Convert(value, targetType, parameter, CultureInfo.InvariantCulture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}

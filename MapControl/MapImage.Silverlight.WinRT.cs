// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2013 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if NETFX_CORE
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
#else
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
#endif

namespace MapControl
{
    public partial class MapImage
    {
        private void BeginOpacityAnimation(ImageSource image)
        {
            var bitmapImage = image as BitmapImage;

            if (bitmapImage != null)
            {
                bitmapImage.ImageOpened += BitmapImageOpened;
                bitmapImage.ImageFailed += BitmapImageFailed;
            }
            else
            {
                BeginOpacityAnimation();
            }
        }

        private void BitmapImageOpened(object sender, RoutedEventArgs e)
        {
            ((BitmapImage)sender).ImageOpened -= BitmapImageOpened;
            ((BitmapImage)sender).ImageFailed -= BitmapImageFailed;
            BeginOpacityAnimation();
        }

        private void BitmapImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            ((BitmapImage)sender).ImageOpened -= BitmapImageOpened;
            ((BitmapImage)sender).ImageFailed -= BitmapImageFailed;
            ((ImageBrush)Fill).ImageSource = null;
        }
    }
}

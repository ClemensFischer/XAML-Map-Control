// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2023 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
#endif

namespace MapControl
{
    public partial class Tile
    {
        private void AnimateImageOpacity()
        {
            if (Image.Source is BitmapImage bitmap && bitmap.UriSource != null)
            {
                bitmap.ImageOpened += BitmapImageOpened;
                bitmap.ImageFailed += BitmapImageFailed;
            }
            else
            {
                BeginOpacityAnimation();
            }
        }

        private void BitmapImageOpened(object sender, RoutedEventArgs e)
        {
            var bitmap = (BitmapImage)sender;

            bitmap.ImageOpened -= BitmapImageOpened;
            bitmap.ImageFailed -= BitmapImageFailed;

            BeginOpacityAnimation();
        }

        private void BitmapImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            var bitmap = (BitmapImage)sender;

            bitmap.ImageOpened -= BitmapImageOpened;
            bitmap.ImageFailed -= BitmapImageFailed;

            Image.Source = null;
        }
    }
}

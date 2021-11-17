// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
#endif

namespace MapControl
{
    public partial class Tile
    {
        public void SetImage(ImageSource image, bool fadeIn = true)
        {
            Pending = false;

            if (image != null && fadeIn && MapBase.ImageFadeDuration > TimeSpan.Zero)
            {
                if (image is BitmapImage bitmap && bitmap.UriSource != null)
                {
                    bitmap.ImageOpened += BitmapImageOpened;
                    bitmap.ImageFailed += BitmapImageFailed;
                }
                else
                {
                    FadeIn();
                }
            }
            else
            {
                Image.Opacity = 1d;
            }

            Image.Source = image;
        }

        private void BitmapImageOpened(object sender, RoutedEventArgs e)
        {
            var bitmap = (BitmapImage)sender;

            bitmap.ImageOpened -= BitmapImageOpened;
            bitmap.ImageFailed -= BitmapImageFailed;

            FadeIn();
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

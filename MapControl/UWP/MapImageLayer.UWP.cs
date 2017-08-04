// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2017 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace MapControl
{
    public partial class MapImageLayer
    {
        protected void UpdateImage(Uri uri)
        {
            UpdateImage(uri != null ? new BitmapImage(uri) : null);
        }

        protected void UpdateImage(BitmapSource bitmapSource)
        {
            SetTopImage(bitmapSource);

            var bitmapImage = bitmapSource as BitmapImage;

            if (bitmapImage != null)
            {
                bitmapImage.ImageOpened += BitmapImageOpened;
                bitmapImage.ImageFailed += BitmapImageFailed;
            }
            else
            {
                SwapImages();
            }
        }

        private void BitmapImageOpened(object sender, RoutedEventArgs e)
        {
            var bitmapImage = (BitmapImage)sender;

            bitmapImage.ImageOpened -= BitmapImageOpened;
            bitmapImage.ImageFailed -= BitmapImageFailed;

            SwapImages();
        }

        private void BitmapImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            var bitmapImage = (BitmapImage)sender;

            bitmapImage.ImageOpened -= BitmapImageOpened;
            bitmapImage.ImageFailed -= BitmapImageFailed;

            ((Image)Children[topImageIndex]).Source = null;
            SwapImages();
        }
    }
}

// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2016 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if NETFX_CORE
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
#else
using System.Windows;
using System.Windows.Media.Imaging;
#endif

namespace MapControl
{
    public partial class MapImageLayer
    {
        protected virtual void UpdateImage(BoundingBox boundingBox, Uri uri)
        {
            UpdateImage(boundingBox, new BitmapImage(uri));
        }

        protected void UpdateImage(BoundingBox boundingBox, BitmapSource bitmap)
        {
            SetTopImage(boundingBox, bitmap);

            var bitmapImage = bitmap as BitmapImage;

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
            var bitmap = (BitmapImage)sender;
            bitmap.ImageOpened -= BitmapImageOpened;
            bitmap.ImageFailed -= BitmapImageFailed;

            SwapImages();
        }

        private void BitmapImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            var bitmap = (BitmapImage)sender;
            bitmap.ImageOpened -= BitmapImageOpened;
            bitmap.ImageFailed -= BitmapImageFailed;

            var mapImage = (MapImage)Children[currentImageIndex];
            mapImage.Source = null;

            SwapImages();
        }
    }
}

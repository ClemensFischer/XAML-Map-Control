// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2015 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if WINDOWS_RUNTIME
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
        private void ImageUpdated(BitmapSource bitmap)
        {
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

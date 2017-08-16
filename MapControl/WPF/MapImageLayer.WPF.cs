// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2017 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MapControl
{
    public partial class MapImageLayer
    {
        protected void UpdateImage(Uri uri)
        {
            UpdateImage(uri != null
                ? BitmapFrame.Create(uri, BitmapCreateOptions.IgnoreImageCache, BitmapCacheOption.OnDemand)
                : null);
        }

        protected void UpdateImage(ImageSource imageSource)
        {
            SetTopImage(imageSource);

            var bitmapSource = imageSource as BitmapSource;

            if (bitmapSource != null && !bitmapSource.IsFrozen && bitmapSource.IsDownloading)
            {
                bitmapSource.DownloadCompleted += BitmapDownloadCompleted;
                bitmapSource.DownloadFailed += BitmapDownloadFailed;
            }
            else
            {
                SwapImages();
            }
        }

        private void BitmapDownloadCompleted(object sender, EventArgs e)
        {
            var bitmapSource = (BitmapSource)sender;

            bitmapSource.DownloadCompleted -= BitmapDownloadCompleted;
            bitmapSource.DownloadFailed -= BitmapDownloadFailed;

            SwapImages();
        }

        private void BitmapDownloadFailed(object sender, ExceptionEventArgs e)
        {
            var bitmapSource = (BitmapSource)sender;

            bitmapSource.DownloadCompleted -= BitmapDownloadCompleted;
            bitmapSource.DownloadFailed -= BitmapDownloadFailed;

            ((Image)Children[topImageIndex]).Source = null;
            SwapImages();
        }
    }
}

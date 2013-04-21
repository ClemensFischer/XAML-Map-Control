// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2013 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MapControl
{
    public partial class MapImage
    {
        private void BeginOpacityAnimation(ImageSource image)
        {
            var bitmapImage = image as BitmapImage;

            if (bitmapImage != null && bitmapImage.IsDownloading)
            {
                bitmapImage.DownloadCompleted += BitmapDownloadCompleted;
                bitmapImage.DownloadFailed += BitmapDownloadFailed;
            }
            else
            {
                BeginOpacityAnimation();
            }
        }

        private void BitmapDownloadCompleted(object sender, EventArgs e)
        {
            ((BitmapImage)sender).DownloadCompleted -= BitmapDownloadCompleted;
            ((BitmapImage)sender).DownloadFailed -= BitmapDownloadFailed;
            BeginOpacityAnimation();
        }

        private void BitmapDownloadFailed(object sender, ExceptionEventArgs e)
        {
            ((BitmapImage)sender).DownloadCompleted -= BitmapDownloadCompleted;
            ((BitmapImage)sender).DownloadFailed -= BitmapDownloadFailed;
            ((ImageBrush)Fill).ImageSource = null;
        }
    }
}

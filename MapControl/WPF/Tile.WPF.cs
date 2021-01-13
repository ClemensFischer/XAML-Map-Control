// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MapControl
{
    public partial class Tile
    {
        public void SetImage(ImageSource image, bool fadeIn = true)
        {
            Pending = false;

            if (image != null && fadeIn && MapBase.ImageFadeDuration > TimeSpan.Zero)
            {
                if (image is BitmapSource bitmap && !bitmap.IsFrozen && bitmap.IsDownloading)
                {
                    bitmap.DownloadCompleted += BitmapDownloadCompleted;
                    bitmap.DownloadFailed += BitmapDownloadFailed;
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

        private void BitmapDownloadCompleted(object sender, EventArgs e)
        {
            var bitmap = (BitmapSource)sender;

            bitmap.DownloadCompleted -= BitmapDownloadCompleted;
            bitmap.DownloadFailed -= BitmapDownloadFailed;

            FadeIn();
        }

        private void BitmapDownloadFailed(object sender, ExceptionEventArgs e)
        {
            var bitmap = (BitmapSource)sender;

            bitmap.DownloadCompleted -= BitmapDownloadCompleted;
            bitmap.DownloadFailed -= BitmapDownloadFailed;

            Image.Source = null;
        }
    }
}

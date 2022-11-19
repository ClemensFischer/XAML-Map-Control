// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MapControl
{
    public partial class Tile
    {
        private void AnimateImageOpacity()
        {
            if (Image.Source is BitmapSource bitmap && !bitmap.IsFrozen && bitmap.IsDownloading)
            {
                bitmap.DownloadCompleted += BitmapDownloadCompleted;
                bitmap.DownloadFailed += BitmapDownloadFailed;
            }
            else
            {
                BeginOpacityAnimation();
            }
        }

        private void BitmapDownloadCompleted(object sender, EventArgs e)
        {
            var bitmap = (BitmapSource)sender;

            bitmap.DownloadCompleted -= BitmapDownloadCompleted;
            bitmap.DownloadFailed -= BitmapDownloadFailed;

            BeginOpacityAnimation();
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

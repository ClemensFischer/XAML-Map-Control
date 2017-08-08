// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2017 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace MapControl
{
    public partial class Tile
    {
        public void SetImage(ImageSource imageSource, bool fadeIn = true)
        {
            Pending = false;

            if (fadeIn && FadeDuration > TimeSpan.Zero)
            {
                var bitmapSource = imageSource as BitmapSource;

                if (bitmapSource != null && !bitmapSource.IsFrozen && bitmapSource.IsDownloading)
                {
                    bitmapSource.DownloadCompleted += BitmapDownloadCompleted;
                    bitmapSource.DownloadFailed += BitmapDownloadFailed;
                }
                else
                {
                    Image.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(1d, FadeDuration));
                }
            }
            else
            {
                Image.Opacity = 1d;
            }

            Image.Source = imageSource;
        }

        private void BitmapDownloadCompleted(object sender, EventArgs e)
        {
            var bitmapSource = (BitmapSource)sender;

            bitmapSource.DownloadCompleted -= BitmapDownloadCompleted;
            bitmapSource.DownloadFailed -= BitmapDownloadFailed;

            Image.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(1d, FadeDuration));
        }

        private void BitmapDownloadFailed(object sender, ExceptionEventArgs e)
        {
            var bitmapSource = (BitmapSource)sender;

            bitmapSource.DownloadCompleted -= BitmapDownloadCompleted;
            bitmapSource.DownloadFailed -= BitmapDownloadFailed;

            Image.Source = null;
        }
    }
}

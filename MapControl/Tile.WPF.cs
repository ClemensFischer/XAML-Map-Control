// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2016 Clemens Fischer
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
        public void SetImage(ImageSource image, bool fadeIn = true)
        {
            Pending = false;

            if (image != null)
            {
                if (fadeIn && FadeDuration > TimeSpan.Zero)
                {
                    var bitmap = image as BitmapSource;

                    if (bitmap != null && !bitmap.IsFrozen && bitmap.IsDownloading)
                    {
                        bitmap.DownloadCompleted += BitmapDownloadCompleted;
                        bitmap.DownloadFailed += BitmapDownloadFailed;
                    }
                    else
                    {
                        Image.BeginAnimation(UIElement.OpacityProperty,
                            new DoubleAnimation(0d, 1d, FadeDuration));
                    }
                }
                else
                {
                    Image.Opacity = 1d;
                }

                Image.Source = image;
            }
        }

        private void BitmapDownloadCompleted(object sender, EventArgs e)
        {
            var bitmap = (BitmapSource)sender;

            bitmap.DownloadCompleted -= BitmapDownloadCompleted;
            bitmap.DownloadFailed -= BitmapDownloadFailed;

            Image.BeginAnimation(UIElement.OpacityProperty,
                new DoubleAnimation(0d, 1d, FadeDuration));
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

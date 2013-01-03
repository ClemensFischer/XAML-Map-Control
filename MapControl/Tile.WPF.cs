// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MapControl
{
    public partial class Tile
    {
        public readonly ImageBrush Brush = new ImageBrush { Opacity = 0d };

        public ImageSource ImageSource
        {
            get { return Brush.ImageSource; }
        }

        public void SetImageSource(ImageSource image, bool animateOpacity)
        {
            if (Brush.ImageSource == null)
            {
                if (animateOpacity)
                {
                    var bitmapImage = image as BitmapImage;

                    if (bitmapImage != null && bitmapImage.IsDownloading)
                    {
                        bitmapImage.DownloadCompleted += BitmapDownloadCompleted;
                        bitmapImage.DownloadFailed += BitmapDownloadFailed;
                    }
                    else
                    {
                        Brush.BeginAnimation(ImageBrush.OpacityProperty, OpacityAnimation);
                    }
                }
                else
                {
                    Brush.Opacity = 1d;
                }
            }

            Brush.ImageSource = image;
        }

        private void BitmapDownloadCompleted(object sender, EventArgs e)
        {
            ((BitmapImage)sender).DownloadCompleted -= BitmapDownloadCompleted;
            ((BitmapImage)sender).DownloadFailed -= BitmapDownloadFailed;
            Brush.BeginAnimation(ImageBrush.OpacityProperty, OpacityAnimation);
        }

        private void BitmapDownloadFailed(object sender, ExceptionEventArgs e)
        {
            ((BitmapImage)sender).DownloadCompleted -= BitmapDownloadCompleted;
            ((BitmapImage)sender).DownloadFailed -= BitmapDownloadFailed;
            Brush.ImageSource = null;
        }
    }
}

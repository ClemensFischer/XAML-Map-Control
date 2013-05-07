// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © Clemens Fischer 2012-2013
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

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
            if (image != null && Brush.ImageSource == null)
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
                        Brush.BeginAnimation(ImageBrush.OpacityProperty, new DoubleAnimation(1d, AnimationDuration));
                    }
                }
                else
                {
                    Brush.Opacity = 1d;
                }
            }

            Brush.ImageSource = image;
            HasImage = true;
        }

        private void BitmapDownloadCompleted(object sender, EventArgs e)
        {
            ((BitmapImage)sender).DownloadCompleted -= BitmapDownloadCompleted;
            ((BitmapImage)sender).DownloadFailed -= BitmapDownloadFailed;
            Brush.BeginAnimation(ImageBrush.OpacityProperty, new DoubleAnimation(1d, AnimationDuration));
        }

        private void BitmapDownloadFailed(object sender, ExceptionEventArgs e)
        {
            ((BitmapImage)sender).DownloadCompleted -= BitmapDownloadCompleted;
            ((BitmapImage)sender).DownloadFailed -= BitmapDownloadFailed;
            Brush.ImageSource = null;
        }
    }
}

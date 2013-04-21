// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2013 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Windows.Media;
using System.Windows.Media.Animation;
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
                        BeginOpacityAnimation();
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
            BeginOpacityAnimation();
        }

        private void BitmapDownloadFailed(object sender, ExceptionEventArgs e)
        {
            ((BitmapImage)sender).DownloadCompleted -= BitmapDownloadCompleted;
            ((BitmapImage)sender).DownloadFailed -= BitmapDownloadFailed;
            Brush.ImageSource = null;
        }

        private void BeginOpacityAnimation()
        {
            Brush.BeginAnimation(ImageBrush.OpacityProperty,
                new DoubleAnimation
                {
                    To = 1d,
                    Duration = AnimationDuration,
                    FillBehavior = FillBehavior.HoldEnd
                });
        }
    }
}

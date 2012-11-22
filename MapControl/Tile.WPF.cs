// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MapControl
{
    internal partial class Tile
    {
        public readonly ImageBrush Brush = new ImageBrush { Opacity = 0d };

        public ImageSource ImageSource
        {
            get { return Brush.ImageSource; }
            private set { Brush.ImageSource = value; }
        }

        public void SetImageSource(ImageSource source, bool animateOpacity)
        {
            if (ImageSource == null)
            {
                if (animateOpacity)
                {
                    var bitmap = source as BitmapImage;

                    if (bitmap != null && bitmap.IsDownloading)
                    {
                        bitmap.DownloadCompleted += BitmapDownloadCompleted;
                        bitmap.DownloadFailed += BitmapDownloadFailed;
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

            ImageSource = source;
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
            ImageSource = null;
        }
    }
}

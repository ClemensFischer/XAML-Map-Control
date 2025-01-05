// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © Clemens Fischer
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
        private void BeginFadeInAnimation()
        {
            var fadeInAnimation = new DoubleAnimation
            {
                From = 0d,
                Duration = MapBase.ImageFadeDuration,
                FillBehavior = FillBehavior.Stop
            };

            Image.BeginAnimation(UIElement.OpacityProperty, fadeInAnimation);
        }

        private void FadeIn()
        {
            if (Image.Source is BitmapSource bitmap && bitmap.IsDownloading && !bitmap.IsFrozen)
            {
                bitmap.DownloadCompleted += BitmapDownloadCompleted;
                bitmap.DownloadFailed += BitmapDownloadFailed;
            }
            else
            {
                BeginFadeInAnimation();
            }
        }

        private void BitmapDownloadCompleted(object sender, EventArgs e)
        {
            var bitmap = (BitmapSource)sender;

            bitmap.DownloadCompleted -= BitmapDownloadCompleted;
            bitmap.DownloadFailed -= BitmapDownloadFailed;

            BeginFadeInAnimation();
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

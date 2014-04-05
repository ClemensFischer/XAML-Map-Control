// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2014 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MapControl
{
    public partial class MapImageLayer
    {
        private readonly DispatcherTimer updateTimer = new DispatcherTimer(DispatcherPriority.Background);

        private void AddDownloadEventHandlers(BitmapSource bitmap)
        {
            if (bitmap.IsDownloading)
            {
                bitmap.DownloadCompleted += BitmapDownloadCompleted;
                bitmap.DownloadFailed += BitmapDownloadFailed;
            }
            else
            {
                BlendImages();
            }
        }

        private void BitmapDownloadCompleted(object sender, EventArgs e)
        {
            var bitmap = (BitmapSource)sender;

            bitmap.DownloadCompleted -= BitmapDownloadCompleted;
            bitmap.DownloadFailed -= BitmapDownloadFailed;

            BlendImages();
        }

        private void BitmapDownloadFailed(object sender, ExceptionEventArgs e)
        {
            var bitmap = (BitmapSource)sender;

            bitmap.DownloadCompleted -= BitmapDownloadCompleted;
            bitmap.DownloadFailed -= BitmapDownloadFailed;

            ((MapImage)Children[currentImageIndex]).Source = null;
            BlendImages();
        }
    }
}

// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2017 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MapControl
{
    public partial class MapImageLayer
    {
        protected void UpdateImage(Uri uri)
        {
            Task.Run(() =>
            {
                var image = BitmapSourceHelper.FromUri(uri);
                Dispatcher.BeginInvoke(new Action(() => UpdateImage(image)));
            });
        }

        protected void UpdateImage(BitmapSource bitmap)
        {
            SetTopImage(bitmap);

            if (bitmap != null && !bitmap.IsFrozen && bitmap.IsDownloading)
            {
                bitmap.DownloadCompleted += BitmapDownloadCompleted;
                bitmap.DownloadFailed += BitmapDownloadFailed;
            }
            else
            {
                SwapImages();
            }
        }

        private void BitmapDownloadCompleted(object sender, EventArgs e)
        {
            var bitmap = (BitmapSource)sender;
            bitmap.DownloadCompleted -= BitmapDownloadCompleted;
            bitmap.DownloadFailed -= BitmapDownloadFailed;

            SwapImages();
        }

        private void BitmapDownloadFailed(object sender, ExceptionEventArgs e)
        {
            var bitmap = (BitmapSource)sender;
            bitmap.DownloadCompleted -= BitmapDownloadCompleted;
            bitmap.DownloadFailed -= BitmapDownloadFailed;

            ((Image)Children[topImageIndex]).Source = null;
            SwapImages();
        }
    }
}

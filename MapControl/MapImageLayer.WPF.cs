// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2016 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MapControl
{
    public partial class MapImageLayer
    {
        protected virtual void UpdateImage(Uri uri)
        {
            Task.Run(() =>
            {
                var image = ImageLoader.FromUri(uri);
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

            var mapImage = (MapImage)Children[currentImageIndex];
            mapImage.Source = null;

            SwapImages();
        }
    }
}

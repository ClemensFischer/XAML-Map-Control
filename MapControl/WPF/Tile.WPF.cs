using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace MapControl
{
    public partial class Tile
    {
        public async Task LoadImageAsync(Func<Task<ImageSource>> loadImageFunc)
        {
            var image = await loadImageFunc().ConfigureAwait(false);

            await Image.Dispatcher.InvokeAsync(
                () =>
                {
                    Image.Source = image;

                    if (image != null && MapBase.ImageFadeDuration > TimeSpan.Zero)
                    {
                        if (image is BitmapSource bitmap && !bitmap.IsFrozen && bitmap.IsDownloading)
                        {
                            bitmap.DownloadCompleted += BitmapDownloadCompleted;
                            bitmap.DownloadFailed += BitmapDownloadFailed;
                        }
                        else
                        {
                            BeginFadeInAnimation();
                        }
                    }
                });
        }

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

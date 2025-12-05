using System;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace MapControl
{
    public class DrawingTile : Tile
    {
        public DrawingTile(int zoomLevel, int x, int y, int columnCount)
            : base(zoomLevel, x, y, columnCount)
        {
            Drawing.Children.Add(ImageDrawing);
        }

        public DrawingGroup Drawing { get; } = new DrawingGroup();

        public ImageDrawing ImageDrawing { get; } = new ImageDrawing();

        public override async Task LoadImageAsync(Func<Task<ImageSource>> loadImageFunc)
        {
            var image = await loadImageFunc().ConfigureAwait(false);

            void SetImageSource()
            {
                ImageDrawing.ImageSource = image;

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
            }

            await Drawing.Dispatcher.InvokeAsync(SetImageSource);
        }

        private void BeginFadeInAnimation()
        {
            var fadeInAnimation = new DoubleAnimation
            {
                From = 0d,
                Duration = MapBase.ImageFadeDuration,
                FillBehavior = FillBehavior.Stop
            };

            Drawing.BeginAnimation(DrawingGroup.OpacityProperty, fadeInAnimation);
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

            ImageDrawing.ImageSource = null;
        }
    }
}

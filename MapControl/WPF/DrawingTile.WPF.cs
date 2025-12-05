using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace MapControl
{
    public class DrawingTile : Tile
    {
        private readonly ImageDrawing imageDrawing = new ImageDrawing();

        public DrawingTile(int zoomLevel, int x, int y, int columnCount)
            : base(zoomLevel, x, y, columnCount)
        {
            Drawing.Children.Add(imageDrawing);
        }

        public DrawingGroup Drawing { get; } = new DrawingGroup();

        public ImageSource ImageSource
        {
            get => imageDrawing.ImageSource;
            set => imageDrawing.ImageSource = value;
        }

        public void SetRect(int xMin, int yMin, int tileWidth, int tileHeight)
        {
            imageDrawing.Rect = new Rect(tileWidth * (X - xMin), tileHeight * (Y - yMin), tileWidth, tileHeight);
        }

        public override async Task LoadImageAsync(Func<Task<ImageSource>> loadImageFunc)
        {
            var image = await loadImageFunc().ConfigureAwait(false);

            void SetImageSource()
            {
                imageDrawing.ImageSource = image;

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

            imageDrawing.ImageSource = null;
        }
    }
}

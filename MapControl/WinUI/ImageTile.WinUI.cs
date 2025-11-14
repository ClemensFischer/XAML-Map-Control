using System;
using System.Threading.Tasks;
#if UWP
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
#else
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
#endif

namespace MapControl
{
    public class ImageTile(int zoomLevel, int x, int y, int columnCount)
        : Tile(zoomLevel, x, y, columnCount)
    {
        public Image Image { get; } = new() { Stretch = Stretch.Fill };

        public override async Task LoadImageAsync(Func<Task<ImageSource>> loadImageFunc)
        {
            var tcs = new TaskCompletionSource<object>();

            async void LoadAndSetImageSource()
            {
                try
                {
                    var image = await loadImageFunc();

                    Image.Source = image;

                    if (image != null && MapBase.ImageFadeDuration > TimeSpan.Zero)
                    {
                        if (image is BitmapImage bitmap && bitmap.UriSource != null)
                        {
                            bitmap.ImageOpened += BitmapImageOpened;
                            bitmap.ImageFailed += BitmapImageFailed;
                        }
                        else
                        {
                            BeginFadeInAnimation();
                        }
                    }

                    tcs.TrySetResult(null);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            }
#if UWP
            if (!await Image.Dispatcher.TryRunAsync(CoreDispatcherPriority.Low, LoadAndSetImageSource))
#else
            if (!Image.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Low, LoadAndSetImageSource))
#endif
            {
                tcs.TrySetCanceled();
            }

            await tcs.Task;
        }

        private void BeginFadeInAnimation()
        {
            var fadeInAnimation = new DoubleAnimation
            {
                From = 0d,
                Duration = MapBase.ImageFadeDuration,
                FillBehavior = FillBehavior.Stop
            };

            Storyboard.SetTarget(fadeInAnimation, Image);
            Storyboard.SetTargetProperty(fadeInAnimation, nameof(UIElement.Opacity));

            var storyboard = new Storyboard();
            storyboard.Children.Add(fadeInAnimation);
            storyboard.Begin();
        }

        private void BitmapImageOpened(object sender, RoutedEventArgs e)
        {
            var bitmap = (BitmapImage)sender;

            bitmap.ImageOpened -= BitmapImageOpened;
            bitmap.ImageFailed -= BitmapImageFailed;

            BeginFadeInAnimation();
        }

        private void BitmapImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            var bitmap = (BitmapImage)sender;

            bitmap.ImageOpened -= BitmapImageOpened;
            bitmap.ImageFailed -= BitmapImageFailed;

            Image.Source = null;
        }
    }
}

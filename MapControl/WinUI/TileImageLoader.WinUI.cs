using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media;
using System;
using System.Threading.Tasks;

namespace MapControl
{
    public partial class TileImageLoader
    {
        private static Task LoadTileImage(Tile tile, Func<Task<ImageSource>> loadImageFunc)
        {
            var tcs = new TaskCompletionSource();

            async void LoadTileImage()
            {
                try
                {
                    var image = await loadImageFunc();

                    tcs.TrySetResult(); // tcs.Task has completed when image is loaded

                    tile.SetImageSource(image);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            }

            if (!tile.Image.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Low, LoadTileImage))
            {
                tcs.TrySetCanceled();
            }

            return tcs.Task;
        }
    }
}

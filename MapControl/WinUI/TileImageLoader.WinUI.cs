using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media;
using System;
using System.Threading;
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
                    tile.SetImageSource(await loadImageFunc());
                    tcs.TrySetResult();
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

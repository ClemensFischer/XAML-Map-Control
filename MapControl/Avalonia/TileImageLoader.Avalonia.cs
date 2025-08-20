using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MapControl
{
    public partial class TileImageLoader
    {
        private static async Task LoadTileImage(Tile tile, Func<Task<IImage>> loadImageFunc, CancellationToken cancellationToken)
        {
            var image = await loadImageFunc().ConfigureAwait(false);

            if (cancellationToken.IsCancellationRequested)
            {
                tile.IsPending = true;
            }
            else
            {
                _ = Dispatcher.UIThread.InvokeAsync(() => tile.SetImageSource(image)); // no need to await InvokeAsync
            }
        }
    }
}

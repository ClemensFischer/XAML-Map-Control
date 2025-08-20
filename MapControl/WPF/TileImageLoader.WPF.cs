using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MapControl
{
    public partial class TileImageLoader
    {
        private static async Task LoadTileImage(Tile tile, Func<Task<ImageSource>> loadImageFunc, CancellationToken cancellationToken)
        {
            var image = await loadImageFunc().ConfigureAwait(false);

            if (cancellationToken.IsCancellationRequested)
            {
                tile.IsPending = true;
            }
            else
            {
                _ = tile.Image.Dispatcher.InvokeAsync(() => tile.SetImageSource(image)); // no need to await InvokeAsync
            }
        }
    }
}

using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MapControl
{
    public partial class TileImageLoader
    {
        private static async Task LoadTileImage(Tile tile, Func<Task<IImage>> loadImageFunc)
        {
            var image = await loadImageFunc().ConfigureAwait(false);

            await Dispatcher.UIThread.InvokeAsync(() => tile.SetImageSource(image));
        }
    }
}

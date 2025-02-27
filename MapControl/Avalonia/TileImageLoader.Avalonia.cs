using System;
using System.Threading.Tasks;

namespace MapControl
{
    public partial class TileImageLoader
    {
        private static async Task LoadTileImage(Tile tile, Func<Task<IImage>> loadImageFunc)
        {
            var image = await loadImageFunc().ConfigureAwait(false);

            _ = Dispatcher.UIThread.InvokeAsync(() => tile.SetImageSource(image)); // no need to await InvokeAsync
        }
    }
}

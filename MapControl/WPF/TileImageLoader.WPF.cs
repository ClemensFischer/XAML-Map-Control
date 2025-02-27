using System;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MapControl
{
    public partial class TileImageLoader
    {
        private static async Task LoadTileImage(Tile tile, Func<Task<ImageSource>> loadImageFunc)
        {
            var image = await loadImageFunc().ConfigureAwait(false);

            _ = tile.Image.Dispatcher.InvokeAsync(() => tile.SetImageSource(image)); // no need to await InvokeAsync
        }
    }
}

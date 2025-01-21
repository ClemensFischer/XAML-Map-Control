// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MapControl
{
    public partial class TileImageLoader
    {
        private static async Task LoadTileAsync(Tile tile, Func<Task<ImageSource>> loadImageFunc)
        {
            var image = await loadImageFunc().ConfigureAwait(false);

            _ = tile.Image.Dispatcher.InvokeAsync(() => tile.SetImageSource(image)); // no need to await InvokeAsync
        }
    }
}

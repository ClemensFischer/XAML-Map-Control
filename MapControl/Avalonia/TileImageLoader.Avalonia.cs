// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Threading.Tasks;

namespace MapControl
{
    public partial class TileImageLoader
    {
        private static async Task LoadTileAsync(Tile tile, Func<Task<IImage>> loadImageFunc)
        {
            var image = await loadImageFunc().ConfigureAwait(false);

            _ = Dispatcher.UIThread.InvokeAsync(() => tile.SetImageSource(image)); // no need to await InvokeAsync
        }
    }
}

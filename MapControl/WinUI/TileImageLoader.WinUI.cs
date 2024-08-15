// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media;
using System;
using System.Threading.Tasks;

namespace MapControl
{
    public partial class TileImageLoader
    {
        private static Task LoadTileAsync(Tile tile, Func<Task<ImageSource>> loadImageFunc)
        {
            var tcs = new TaskCompletionSource();

            async void callback()
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

            tile.Image.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Low, callback);

            return tcs.Task;
        }
    }
}

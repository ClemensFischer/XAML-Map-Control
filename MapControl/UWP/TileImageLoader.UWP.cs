// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;

namespace MapControl
{
    public partial class TileImageLoader
    {
        private static async Task LoadTileImage(Tile tile, Func<Task<ImageSource>> loadImageFunc)
        {
            var tcs = new TaskCompletionSource<object>();

            async void LoadTileImage()
            {
                try
                {
                    var image = await loadImageFunc();
        
                    tcs.TrySetResult(null); // tcs.Task has completed when image is loaded

                    tile.SetImageSource(image);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            }

            if (!await tile.Image.Dispatcher.TryRunAsync(CoreDispatcherPriority.Low, LoadTileImage))
            {
                tcs.TrySetCanceled();
            }

            await tcs.Task;
        }
    }
}

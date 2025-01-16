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
        private static Task<object> LoadTileAsync(Tile tile, Func<Task<ImageSource>> loadImageFunc)
        {
            var tcs = new TaskCompletionSource<object>();

            async void LoadTileImage()
            {
                try
                {
                    var image = await loadImageFunc();

                    tile.SetImageSource(image);
                    tcs.TrySetResult(null);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            }

            _ = tile.Image.Dispatcher.RunAsync(CoreDispatcherPriority.Low, LoadTileImage);

            return tcs.Task;
        }
    }
}

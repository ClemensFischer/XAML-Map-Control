// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;

namespace MapControl
{
    public partial class TileImageLoader
    {
        /// <summary>
        /// Default folder path where an IImageCache instance may save cached data,
        /// i.e. Windows.Storage.ApplicationData.Current.TemporaryFolder.Path.
        /// </summary>
        public static string DefaultCacheFolder => Windows.Storage.ApplicationData.Current.TemporaryFolder.Path;


        private static async Task LoadTileAsync(Tile tile, Func<Task<ImageSource>> loadImageFunc)
        {
            var tcs = new TaskCompletionSource<object>();

            async void callback()
            {
                try
                {
                    tile.SetImageSource(await loadImageFunc());
                    tcs.TrySetResult(null);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            }

            await tile.Image.Dispatcher.RunAsync(CoreDispatcherPriority.Low, callback);

            await tcs.Task.ConfigureAwait(false);
        }
    }
}

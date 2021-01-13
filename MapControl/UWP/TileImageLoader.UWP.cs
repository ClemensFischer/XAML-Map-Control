// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;

namespace MapControl
{
    public partial class TileImageLoader
    {
        /// <summary>
        /// Default StorageFolder where an IImageCache instance may save cached data,
        /// i.e. ApplicationData.Current.TemporaryFolder.
        /// </summary>
        public static StorageFolder DefaultCacheFolder
        {
            get { return ApplicationData.Current.TemporaryFolder; }
        }

        /// <summary>
        /// The IImageCache implementation used to cache tile images. The default is null.
        /// </summary>
        public static Caching.IImageCache Cache { get; set; }


        private static async Task LoadCachedTileAsync(Tile tile, Uri uri, string cacheKey)
        {
            var cacheItem = await Cache.GetAsync(cacheKey).ConfigureAwait(false);
            var buffer = cacheItem?.Buffer;

            if (cacheItem == null || cacheItem.Expiration < DateTime.UtcNow)
            {
                var response = await ImageLoader.GetHttpResponseAsync(uri).ConfigureAwait(false);

                if (response != null) // download succeeded
                {
                    buffer = response.Buffer?.AsBuffer(); // may be null or empty when no tile available, but still be cached

                    await Cache.SetAsync(cacheKey, buffer, GetExpiration(response.MaxAge)).ConfigureAwait(false);
                }
            }

            if (buffer != null && buffer.Length > 0)
            {
                await SetTileImageAsync(tile, () => ImageLoader.LoadImageAsync(buffer)).ConfigureAwait(false);
            }
        }

        private static Task LoadTileAsync(Tile tile, TileSource tileSource)
        {
            return SetTileImageAsync(tile, () => tileSource.LoadImageAsync(tile.XIndex, tile.Y, tile.ZoomLevel));
        }

        private static async Task SetTileImageAsync(Tile tile, Func<Task<ImageSource>> loadImage)
        {
            var tcs = new TaskCompletionSource<object>();

            await tile.Image.Dispatcher.RunAsync(CoreDispatcherPriority.Low, async () =>
            {
                try
                {
                    tile.SetImage(await loadImage());
                    tcs.SetResult(null);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            await tcs.Task.ConfigureAwait(false); // wait until image loading in the UI thread is completed
        }
    }
}

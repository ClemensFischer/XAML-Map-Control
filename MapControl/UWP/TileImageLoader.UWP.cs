// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2020 Clemens Fischer
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


        private static async Task LoadCachedTileImageAsync(Tile tile, Uri uri, string cacheKey)
        {
            var cacheItem = await Cache.GetAsync(cacheKey).ConfigureAwait(false);
            var buffer = cacheItem?.Buffer;

            if (cacheItem == null || cacheItem.Expiration < DateTime.UtcNow)
            {
                var response = await ImageLoader.GetHttpResponseAsync(uri, false).ConfigureAwait(false);

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

        private static Task LoadTileImageAsync(Tile tile, TileSource tileSource)
        {
            return SetTileImageAsync(tile, () => tileSource.LoadImageAsync(tile.XIndex, tile.Y, tile.ZoomLevel));
        }

        private static async Task SetTileImageAsync(Tile tile, Func<Task<ImageSource>> loadImageFunc)
        {
            var tcs = new TaskCompletionSource<object>();

            await tile.Image.Dispatcher.RunAsync(CoreDispatcherPriority.Low, async () =>
            {
                try
                {
                    var image = await loadImageFunc();

                    if (image != null)
                    {
                        tile.SetImage(image);
                    }

                    tcs.SetResult(null);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            await tcs.Task.ConfigureAwait(false);
        }
    }
}

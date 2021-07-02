// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Threading.Tasks;
using MapControl.Caching;
#if WINDOWS_UWP
using Windows.UI.Core;
using Windows.UI.Xaml.Media;
#else
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media;
#endif

namespace MapControl
{
    namespace Caching
    {
        public interface IImageCache
        {
            Task<ImageCacheItem> GetAsync(string key);

            Task SetAsync(string key, ImageCacheItem cacheItem);
        }
    }

    public partial class TileImageLoader
    {
        /// <summary>
        /// Default folder path where an IImageCache instance may save cached data,
        /// i.e. Windows.Storage.ApplicationData.Current.TemporaryFolder.Path.
        /// </summary>
        public static string DefaultCacheFolder
        {
            get { return Windows.Storage.ApplicationData.Current.TemporaryFolder.Path; }
        }

        /// <summary>
        /// The IImageCache implementation used to cache tile images. The default is null.
        /// </summary>
        public static IImageCache Cache { get; set; }


        private static async Task LoadCachedTileAsync(Tile tile, Uri uri, string cacheKey)
        {
            var cacheItem = await Cache.GetAsync(cacheKey).ConfigureAwait(false);
            var buffer = cacheItem?.Buffer;

            if (cacheItem == null || cacheItem.Expiration < DateTime.UtcNow)
            {
                var response = await ImageLoader.GetHttpResponseAsync(uri).ConfigureAwait(false);

                if (response != null) // download succeeded
                {
                    buffer = response.Buffer; // may be null or empty when no tile available, but still be cached

                    cacheItem = new ImageCacheItem
                    {
                        Buffer = buffer,
                        Expiration = GetExpiration(response.MaxAge)
                    };

                    await Cache.SetAsync(cacheKey, cacheItem).ConfigureAwait(false);
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

#if WINDOWS_UWP
        public static async Task SetTileImageAsync(Tile tile, Func<Task<ImageSource>> loadImageFunc)
        {
            var tcs = new TaskCompletionSource<object>();

            await tile.Image.Dispatcher.RunAsync(CoreDispatcherPriority.Low, async () =>
            {
                try
                {
                    tile.SetImage(await loadImageFunc());
                    tcs.TrySetResult(null);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });

            await tcs.Task.ConfigureAwait(false);
        }
#else
        public static Task SetTileImageAsync(Tile tile, Func<Task<ImageSource>> loadImageFunc)
        {
            var tcs = new TaskCompletionSource();

            tile.Image.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Low, async () =>
            {
                try
                {
                    tile.SetImage(await loadImageFunc());
                    tcs.TrySetResult();
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });

            return tcs.Task;
        }
#endif
    }
}

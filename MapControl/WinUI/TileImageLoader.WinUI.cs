// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MapControl
{
    namespace Caching
    {
        public interface IImageCache
        {
            Task<Tuple<byte[], DateTime>> GetAsync(string key);

            Task SetAsync(string key, byte[] buffer, DateTime expiration);
        }
    }

    public partial class TileImageLoader
    {
        /// <summary>
        /// Default folder path where an IImageCache instance may save cached data, i.e. C:\ProgramData\MapControl\TileCache
        /// </summary>
        public static string DefaultCacheFolder
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MapControl", "TileCache"); }
        }

        /// <summary>
        /// An IImageCache implementation used to cache tile images.
        /// </summary>
        public static Caching.IImageCache Cache { get; set; }


        private static async Task LoadCachedTileAsync(Tile tile, Uri uri, string cacheKey)
        {
            var cacheItem = await Cache.GetAsync(cacheKey).ConfigureAwait(false);
            var buffer = cacheItem?.Item1;

            if (cacheItem == null || cacheItem.Item2 < DateTime.UtcNow)
            {
                var response = await ImageLoader.GetHttpResponseAsync(uri).ConfigureAwait(false);

                if (response != null) // download succeeded
                {
                    buffer = response.Buffer; // may be null or empty when no tile available, but still be cached

                    await Cache.SetAsync(cacheKey, buffer, GetExpiration(response.MaxAge)).ConfigureAwait(false);
                }
            }
            //else System.Diagnostics.Debug.WriteLine($"Cached: {cacheKey}");

            if (buffer != null && buffer.Length > 0)
            {
                await SetTileImageAsync(tile, () => ImageLoader.LoadImageAsync(buffer)).ConfigureAwait(false);
            }
        }

        private static Task LoadTileAsync(Tile tile, TileSource tileSource)
        {
            return SetTileImageAsync(tile, () => tileSource.LoadImageAsync(tile.XIndex, tile.Y, tile.ZoomLevel));
        }

        public static Task SetTileImageAsync(Tile tile, Func<Task<ImageSource>> loadImageFunc)
        {
            var tcs = new TaskCompletionSource();

            async void callback()
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
            }

            if (!tile.Image.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Low, callback))
            {
                tile.Pending = true;
                tcs.TrySetResult();
            }

            return tcs.Task;
        }
    }
}

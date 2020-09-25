// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2020 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.IO;
using System.Runtime.Caching;
using System.Threading.Tasks;
using System.Windows.Media;
using MapControl.Caching;

namespace MapControl
{
    namespace Caching
    {
        public class ImageCacheItem
        {
            public byte[] Buffer { get; set; }
            public DateTime Expiration { get; set; }
        }
    }

    public partial class TileImageLoader
    {
        /// <summary>
        /// Default folder path where an ObjectCache instance may save cached data, i.e. C:\ProgramData\MapControl\TileCache
        /// </summary>
        public static string DefaultCacheFolder
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MapControl", "TileCache"); }
        }

        /// <summary>
        /// An ObjectCache instance used to cache tile image data (i.e. ImageCacheItem objects).
        /// The default ObjectCache value is MemoryCache.Default.
        /// </summary>
        public static ObjectCache Cache { get; set; } = MemoryCache.Default;


        private static async Task LoadCachedTileImageAsync(Tile tile, Uri uri, string cacheKey)
        {
            var cacheItem = await GetCacheAsync(cacheKey).ConfigureAwait(false);
            var buffer = cacheItem?.Buffer;

            if (cacheItem == null || cacheItem.Expiration < DateTime.UtcNow)
            {
                var response = await ImageLoader.GetHttpResponseAsync(uri).ConfigureAwait(false);

                if (response != null) // download succeeded
                {
                    buffer = response.Buffer; // may be null or empty when no tile available, but still be cached

                    await SetCacheAsync(cacheKey, buffer, GetExpiration(response.MaxAge)).ConfigureAwait(false);
                }
            }

            if (buffer != null && buffer.Length > 0)
            {
                var image = await ImageLoader.LoadImageAsync(buffer).ConfigureAwait(false);

                await tile.Image.Dispatcher.InvokeAsync(() => tile.SetImage(image));
            }
        }

        private static async Task LoadTileImageAsync(Tile tile, TileSource tileSource)
        {
            var image = await tileSource.LoadImageAsync(tile.XIndex, tile.Y, tile.ZoomLevel).ConfigureAwait(false);

            await tile.Image.Dispatcher.InvokeAsync(() => tile.SetImage(image));
        }

        private static Task<ImageCacheItem> GetCacheAsync(string cacheKey)
        {
            return Task.Run(() => Cache.Get(cacheKey) as ImageCacheItem);
        }

        private static Task SetCacheAsync(string cacheKey, byte[] buffer, DateTime expiration)
        {
            var imageCacheItem = new ImageCacheItem
            {
                Buffer = buffer,
                Expiration = expiration
            };

            var cacheItemPolicy = new CacheItemPolicy
            {
                AbsoluteExpiration = expiration
            };

            return Task.Run(() => Cache.Set(cacheKey, imageCacheItem, cacheItemPolicy));
        }
    }
}

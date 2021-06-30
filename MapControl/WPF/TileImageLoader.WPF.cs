// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using MapControl.Caching;
using System;
using System.IO;
using System.Runtime.Caching;
using System.Threading.Tasks;

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


        private static async Task LoadCachedTileAsync(Tile tile, Uri uri, string cacheKey)
        {
            var cacheItem = Cache.Get(cacheKey) as ImageCacheItem;
            var buffer = cacheItem?.Buffer;

            if (cacheItem == null || cacheItem.Expiration < DateTime.UtcNow)
            {
                var response = await ImageLoader.GetHttpResponseAsync(uri).ConfigureAwait(false);

                if (response != null) // download succeeded
                {
                    buffer = response.Buffer;

                    cacheItem = new ImageCacheItem
                    {
                        Buffer = buffer, // may be null or empty when no tile available, but still be cached
                        Expiration = GetExpiration(response.MaxAge)
                    };

                    Cache.Set(cacheKey, cacheItem, new CacheItemPolicy { AbsoluteExpiration = cacheItem.Expiration });
                }
            }

            if (buffer != null && buffer.Length > 0)
            {
                var image = await ImageLoader.LoadImageAsync(buffer).ConfigureAwait(false);

                await tile.Image.Dispatcher.InvokeAsync(() => tile.SetImage(image));
            }
        }

        private static async Task LoadTileAsync(Tile tile, TileSource tileSource)
        {
            var image = await tileSource.LoadImageAsync(tile.XIndex, tile.Y, tile.ZoomLevel).ConfigureAwait(false);

            await tile.Image.Dispatcher.InvokeAsync(() => tile.SetImage(image));
        }
    }
}

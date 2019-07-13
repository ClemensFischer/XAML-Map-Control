// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2019 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.IO;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MapControl
{
    public partial class TileImageLoader
    {
        /// <summary>
        /// Default folder path where an ObjectCache instance may save cached data,
        /// i.e. C:\ProgramData\MapControl\TileCache
        /// </summary>
        public static string DefaultCacheFolder
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MapControl", "TileCache"); }
        }

        /// <summary>
        /// The ObjectCache used to cache tile images. The default is MemoryCache.Default.
        /// </summary>
        public static ObjectCache Cache { get; set; } = MemoryCache.Default;


        private static async Task LoadCachedTileImageAsync(Tile tile, Uri uri, string cacheKey)
        {
            DateTime expiration;
            var buffer = GetCachedImage(cacheKey, out expiration);

            if (buffer == null || expiration < DateTime.UtcNow)
            {
                var response = await ImageLoader.GetHttpResponseAsync(uri, false).ConfigureAwait(false);

                if (response != null) // download succeeded
                {
                    buffer = response.Buffer;

                    if (buffer != null) // tile image available
                    {
                        await SetCachedImage(cacheKey, buffer, GetExpiration(response.MaxAge)).ConfigureAwait(false);
                    }
                }
            }

            if (buffer != null)
            {
                SetTileImageAsync(tile, await ImageLoader.LoadImageAsync(buffer).ConfigureAwait(false));
            }
        }

        private static async Task LoadTileImageAsync(Tile tile, TileSource tileSource)
        {
            var image = await tileSource.LoadImageAsync(tile.XIndex, tile.Y, tile.ZoomLevel).ConfigureAwait(false);

            if (image != null)
            {
                SetTileImageAsync(tile, image);
            }
        }

        private static void SetTileImageAsync(Tile tile, ImageSource image)
        {
            tile.Image.Dispatcher.InvokeAsync(() => tile.SetImage(image));
        }

        private static byte[] GetCachedImage(string cacheKey, out DateTime expiration)
        {
            var buffer = Cache.Get(cacheKey) as byte[];

            if (buffer != null && buffer.Length >= 16 &&
                Encoding.ASCII.GetString(buffer, buffer.Length - 16, 8) == "EXPIRES:")
            {
                expiration = new DateTime(BitConverter.ToInt64(buffer, buffer.Length - 8), DateTimeKind.Utc);
            }
            else
            {
                expiration = DateTime.MinValue;
            }

            return buffer;
        }

        private static async Task SetCachedImage(string cacheKey, byte[] buffer, DateTime expiration)
        {
            using (var stream = new MemoryStream(buffer.Length + 16))
            {
                await stream.WriteAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                await stream.WriteAsync(Encoding.ASCII.GetBytes("EXPIRES:"), 0, 8).ConfigureAwait(false);
                await stream.WriteAsync(BitConverter.GetBytes(expiration.Ticks), 0, 8).ConfigureAwait(false);

                Cache.Set(cacheKey, stream.ToArray(), new CacheItemPolicy { AbsoluteExpiration = expiration });
            }
        }
    }
}

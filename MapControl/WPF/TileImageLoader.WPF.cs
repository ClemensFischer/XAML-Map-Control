// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2017 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MapControl
{
    public partial class TileImageLoader : ITileImageLoader
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

        private static int DefaultConnectionLimit
        {
            get { return ServicePointManager.DefaultConnectionLimit; }
        }

        private async Task LoadTileImageAsync(Tile tile, Uri uri, string cacheKey)
        {
            DateTime expiration;
            var buffer = GetCachedImage(cacheKey, out expiration);
            var loaded = false;

            if (buffer == null || expiration < DateTime.UtcNow)
            {
                try
                {
                    loaded = await ImageLoader.LoadHttpTileImageAsync(uri, async (stream, maxAge) =>
                    {
                        await SetTileImageAsync(tile, stream); // create BitmapFrame before caching

                        SetCachedImage(cacheKey, stream, GetExpiration(maxAge));
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("TileImageLoader: {0}: {1}", uri, ex.Message);
                }
            }

            if (!loaded && buffer != null) // keep expired image if download failed
            {
                using (var stream = new MemoryStream(buffer))
                {
                    await SetTileImageAsync(tile, stream);
                }
            }
        }

        private async Task SetTileImageAsync(Tile tile, MemoryStream stream)
        {
            var imageSource = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);

            await tile.Image.Dispatcher.InvokeAsync(() => tile.SetImage(imageSource));
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

        private static void SetCachedImage(string cacheKey, MemoryStream stream, DateTime expiration)
        {
            stream.Seek(0, SeekOrigin.End);
            stream.Write(Encoding.ASCII.GetBytes("EXPIRES:"), 0, 8);
            stream.Write(BitConverter.GetBytes(expiration.Ticks), 0, 8);

            Cache.Set(cacheKey, stream.ToArray(), new CacheItemPolicy { AbsoluteExpiration = expiration });
        }
    }
}

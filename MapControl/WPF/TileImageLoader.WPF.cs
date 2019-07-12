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
            ImageSource image = null;
            DateTime expiration;
            byte[] cacheBuffer;

            GetCachedImage(cacheKey, out cacheBuffer, out expiration);

            if (cacheBuffer == null || expiration < DateTime.UtcNow)
            {
                using (var stream = await ImageLoader.LoadImageStreamAsync(uri).ConfigureAwait(false))
                {
                    if (stream != null) // download succeeded
                    {
                        cacheBuffer = null; // discard cached image

                        if (stream.Length > 0) // tile image available
                        {
                            image = ImageLoader.LoadImage(stream);

                            SetCachedImage(cacheKey, stream, GetExpiration(stream.MaxAge));
                        }
                    }
                }
            }

            if (cacheBuffer != null) // cached image not expired or download failed
            {
                image = ImageLoader.LoadImage(cacheBuffer);
            }

            if (image != null)
            {
                SetTileImage(tile, image);
            }
        }

        private static async Task LoadTileImageAsync(Tile tile, TileSource tileSource)
        {
            var image = await tileSource.LoadImageAsync(tile.XIndex, tile.Y, tile.ZoomLevel).ConfigureAwait(false);

            if (image != null)
            {
                SetTileImage(tile, image);
            }
        }

        private static void SetTileImage(Tile tile, ImageSource image)
        {
            tile.Image.Dispatcher.InvokeAsync(() => tile.SetImage(image));
        }

        private static void GetCachedImage(string cacheKey, out byte[] buffer, out DateTime expiration)
        {
            buffer = Cache.Get(cacheKey) as byte[];

            if (buffer != null && buffer.Length >= 16 &&
                Encoding.ASCII.GetString(buffer, buffer.Length - 16, 8) == "EXPIRES:")
            {
                expiration = new DateTime(BitConverter.ToInt64(buffer, buffer.Length - 8), DateTimeKind.Utc);
            }
            else
            {
                expiration = DateTime.MinValue;
            }
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

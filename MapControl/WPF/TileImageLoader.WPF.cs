// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.IO;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;

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

        private async Task LoadCachedTileImageAsync(Tile tile, Uri uri, string cacheKey)
        {
            DateTime expiration;
            var cacheBuffer = GetCachedImage(cacheKey, out expiration);

            if (cacheBuffer == null || expiration < DateTime.UtcNow)
            {
                var result = await ImageLoader.LoadHttpStreamAsync(uri);

                if (result != null) // download succeeded
                {
                    cacheBuffer = null; // discard cached image

                    if (result.Item1 != null) // tile image available
                    {
                        using (var stream = result.Item1)
                        {
                            LoadTileImage(tile, stream);
                            SetCachedImage(cacheKey, stream, GetExpiration(result.Item2));
                        }
                    }
                }
            }

            if (cacheBuffer != null)
            {
                using (var stream = new MemoryStream(cacheBuffer))
                {
                    LoadTileImage(tile, stream);
                }
            }
        }

        private async Task LoadTileImageAsync(Tile tile, TileSource tileSource)
        {
            SetTileImage(tile, await tileSource.LoadImageAsync(tile.XIndex, tile.Y, tile.ZoomLevel));
        }

        private void LoadTileImage(Tile tile, Stream stream)
        {
            SetTileImage(tile, ImageLoader.LoadImage(stream));
        }

        private void SetTileImage(Tile tile, ImageSource imageSource)
        {
            tile.Image.Dispatcher.InvokeAsync(() => tile.SetImage(imageSource));
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

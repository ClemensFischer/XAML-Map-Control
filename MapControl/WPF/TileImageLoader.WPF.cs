// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2017 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MapControl
{
    public partial class TileImageLoader : ITileImageLoader
    {
        /// <summary>
        /// Default folder path where an ObjectCache instance may save cached data.
        /// </summary>
        public static readonly string DefaultCacheFolder =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MapControl", "TileCache");

        /// <summary>
        /// The ObjectCache used to cache tile images. The default is MemoryCache.Default.
        /// </summary>
        public static ObjectCache Cache { get; set; } = MemoryCache.Default;

        private async Task LoadTileImageAsync(Tile tile, Uri uri, string cacheKey)
        {
            DateTime expiration;
            var buffer = GetCachedImage(cacheKey, out expiration);
            var loaded = false;

            if (buffer == null || expiration < DateTime.UtcNow)
            {
                loaded = await DownloadTileImageAsync(tile, uri, cacheKey);
            }

            if (!loaded && buffer != null) // keep expired image if download failed
            {
                using (var stream = new MemoryStream(buffer))
                {
                    await SetTileImageAsync(tile, stream);
                }
            }
        }

        private async Task<bool> DownloadTileImageAsync(Tile tile, Uri uri, string cacheKey)
        {
            var success = false;

            try
            {
                using (var response = await TileSource.HttpClient.GetAsync(uri))
                {
                    success = response.IsSuccessStatusCode;

                    if (!success)
                    {
                        Debug.WriteLine("TileImageLoader: {0}: {1} {2}", uri, (int)response.StatusCode, response.ReasonPhrase);
                    }
                    else if (TileSource.TileAvailable(response.Headers))
                    {
                        using (var stream = new MemoryStream())
                        {
                            await response.Content.CopyToAsync(stream);
                            stream.Seek(0, SeekOrigin.Begin);

                            await SetTileImageAsync(tile, stream); // create BitmapFrame before caching

                            SetCachedImage(cacheKey, stream, GetExpiration(response));
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("TileImageLoader: {0}: {1}", uri, ex.Message);
            }

            return success;
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

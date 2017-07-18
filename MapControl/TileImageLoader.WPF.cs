// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2017 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MapControl
{
    /// <summary>
    /// Loads map tile images and optionally caches them in a System.Runtime.Caching.ObjectCache.
    /// </summary>
    public class TileImageLoader : ITileImageLoader
    {
        /// <summary>
        /// Default name of an ObjectCache instance that is assigned to the Cache property.
        /// </summary>
        public const string DefaultCacheName = "TileCache";

        /// <summary>
        /// Default folder path where an ObjectCache instance may save cached data.
        /// </summary>
        public static readonly string DefaultCacheFolder =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MapControl");

        /// <summary>
        /// Default expiration time for cached tile images. Used when no expiration time
        /// was transmitted on download. The default value is one day.
        /// </summary>
        public static TimeSpan DefaultCacheExpiration { get; set; }

        /// <summary>
        /// Minimum expiration time for cached tile images. Used when an unnecessarily small expiration time
        /// was transmitted on download (e.g. Cache-Control: max-age=0). The default value is one hour.
        /// </summary>
        public static TimeSpan MinimumCacheExpiration { get; set; }

        /// <summary>
        /// The ObjectCache used to cache tile images. The default is MemoryCache.Default.
        /// </summary>
        public static ObjectCache Cache { get; set; }

        /// <summary>
        /// Optional value to be used for the HttpWebRequest.UserAgent property. The default is null.
        /// </summary>
        public static string HttpUserAgent { get; set; }

        static TileImageLoader()
        {
            DefaultCacheExpiration = TimeSpan.FromDays(1);
            MinimumCacheExpiration = TimeSpan.FromHours(1);
            Cache = MemoryCache.Default;
        }

        private readonly ConcurrentStack<Tile> pendingTiles = new ConcurrentStack<Tile>();
        private int taskCount;

        public void LoadTiles(MapTileLayer tileLayer)
        {
            pendingTiles.Clear();

            var tiles = tileLayer.Tiles.Where(t => t.Pending);

            if (tiles.Any())
            {
                pendingTiles.PushRange(tiles.Reverse().ToArray());

                var tileSource = tileLayer.TileSource;
                var sourceName = tileLayer.SourceName;
                var maxDownloads = tileLayer.MaxParallelDownloads;

                while (taskCount < Math.Min(pendingTiles.Count, maxDownloads))
                {
                    Interlocked.Increment(ref taskCount);

                    Task.Run(() =>
                    {
                        LoadPendingTiles(tileSource, sourceName);

                        Interlocked.Decrement(ref taskCount);
                    });
                }
            }
        }

        private void LoadPendingTiles(TileSource tileSource, string sourceName)
        {
            var imageTileSource = tileSource as ImageTileSource;
            Tile tile;

            while (pendingTiles.TryPop(out tile))
            {
                tile.Pending = false;

                try
                {
                    ImageSource image = null;
                    Uri uri;

                    if (imageTileSource != null)
                    {
                        image = imageTileSource.LoadImage(tile.XIndex, tile.Y, tile.ZoomLevel);
                    }
                    else if ((uri = tileSource.GetUri(tile.XIndex, tile.Y, tile.ZoomLevel)) != null)
                    {
                        image = LoadImage(uri, sourceName, tile.XIndex, tile.Y, tile.ZoomLevel);
                    }

                    if (image != null)
                    {
                        tile.SetImage(image);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("{0}/{1}/{2}: {3}", tile.ZoomLevel, tile.XIndex, tile.Y, ex.Message);
                }
            }
        }

        private ImageSource LoadImage(Uri uri, string sourceName, int x, int y, int zoomLevel)
        {
            ImageSource image = null;

            try
            {
                if (!uri.IsAbsoluteUri)
                {
                    image = BitmapSourceHelper.FromFile(uri.OriginalString);
                }
                else if (uri.Scheme == "file")
                {
                    image = BitmapSourceHelper.FromFile(uri.LocalPath);
                }
                else if (Cache == null || string.IsNullOrEmpty(sourceName))
                {
                    image = DownloadImage(uri, null);
                }
                else
                {
                    var cacheKey = string.Format("{0}/{1}/{2}/{3}", sourceName, zoomLevel, x, y);

                    if (!GetCachedImage(cacheKey, ref image))
                    {
                        // Either no cached image was found or expiration time has expired.
                        // If download fails use possibly cached but expired image anyway.
                        image = DownloadImage(uri, cacheKey);
                    }
                }
            }
            catch (WebException ex)
            {
                Debug.WriteLine("{0}: {1}: {2}", uri, ex.Status, ex.Message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("{0}: {1}", uri, ex.Message);
            }

            return image;
        }

        private static ImageSource DownloadImage(Uri uri, string cacheKey)
        {
            ImageSource image = null;
            var request = WebRequest.CreateHttp(uri);

            if (HttpUserAgent != null)
            {
                request.UserAgent = HttpUserAgent;
            }

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                if (response.Headers["X-VE-Tile-Info"] != "no-tile") // set by Bing Maps
                {
                    using (var responseStream = response.GetResponseStream())
                    using (var memoryStream = new MemoryStream())
                    {
                        responseStream.CopyTo(memoryStream);
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        image = BitmapSourceHelper.FromStream(memoryStream);

                        if (cacheKey != null)
                        {
                            SetCachedImage(cacheKey, memoryStream, GetExpiration(response.Headers));
                        }
                    }
                }
            }

            return image;
        }

        private static bool GetCachedImage(string cacheKey, ref ImageSource image)
        {
            var result = false;
            var buffer = Cache.Get(cacheKey) as byte[];

            if (buffer != null)
            {
                try
                {
                    using (var memoryStream = new MemoryStream(buffer))
                    {
                        image = BitmapSourceHelper.FromStream(memoryStream);
                    }

                    DateTime expiration = DateTime.MinValue;

                    if (buffer.Length >= 16 && Encoding.ASCII.GetString(buffer, buffer.Length - 16, 8) == "EXPIRES:")
                    {
                        expiration = new DateTime(BitConverter.ToInt64(buffer, buffer.Length - 8), DateTimeKind.Utc);
                    }

                    result = expiration > DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("{0}: {1}", cacheKey, ex.Message);
                }
            }

            return result;
        }

        private static void SetCachedImage(string cacheKey, MemoryStream memoryStream, DateTime expiration)
        {
            memoryStream.Seek(0, SeekOrigin.End);
            memoryStream.Write(Encoding.ASCII.GetBytes("EXPIRES:"), 0, 8);
            memoryStream.Write(BitConverter.GetBytes(expiration.Ticks), 0, 8);

            Cache.Set(cacheKey, memoryStream.ToArray(), new CacheItemPolicy { AbsoluteExpiration = expiration });
        }

        private static DateTime GetExpiration(WebHeaderCollection headers)
        {
            var expiration = DefaultCacheExpiration;
            var cacheControl = headers["Cache-Control"];

            if (cacheControl != null)
            {
                int maxAgeValue;
                var maxAgeDirective = cacheControl
                    .Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .FirstOrDefault(s => s.StartsWith("max-age="));

                if (maxAgeDirective != null &&
                    int.TryParse(maxAgeDirective.Substring(8), out maxAgeValue))
                {
                    expiration = TimeSpan.FromSeconds(maxAgeValue);

                    if (expiration < MinimumCacheExpiration)
                    {
                        expiration = MinimumCacheExpiration;
                    }
                }
            }

            return DateTime.UtcNow.Add(expiration);
        }
    }
}

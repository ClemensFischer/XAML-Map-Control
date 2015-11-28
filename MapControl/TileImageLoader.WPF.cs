// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2015 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

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
        /// was transmitted on download. The default and recommended minimum value is seven days.
        /// See OpenStreetMap tile usage policy: http://wiki.openstreetmap.org/wiki/Tile_usage_policy
        /// </summary>
        public static TimeSpan DefaultCacheExpiration { get; set; }

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
            DefaultCacheExpiration = TimeSpan.FromDays(7);
            Cache = MemoryCache.Default;
        }

        private class PendingTile
        {
            public readonly Tile Tile;
            public readonly ImageSource CachedImage;

            public PendingTile(Tile tile, ImageSource cachedImage)
            {
                Tile = tile;
                CachedImage = cachedImage;
            }
        }

        private readonly ConcurrentQueue<PendingTile> pendingTiles = new ConcurrentQueue<PendingTile>();
        private int taskCount;

        public void BeginLoadTiles(TileLayer tileLayer, IEnumerable<Tile> tiles)
        {
            if (tiles.Any())
            {
                // get current TileLayer property values in UI thread
                var dispatcher = tileLayer.Dispatcher;
                var tileSource = tileLayer.TileSource;
                var imageTileSource = tileSource as ImageTileSource;

                if (imageTileSource != null && !imageTileSource.IsAsync) // call LoadImage in UI thread with low priority
                {
                    foreach (var tile in tiles)
                    {
                        dispatcher.BeginInvoke(new Action<Tile>(t => t.SetImage(LoadImage(imageTileSource, t))), DispatcherPriority.Background, tile);
                    }
                }
                else
                {
                    var tileList = tiles.ToList(); // evaluate immediately
                    var sourceName = tileLayer.SourceName;
                    var maxDownloads = tileLayer.MaxParallelDownloads;

                    Task.Run(() => GetTiles(tileList, dispatcher, tileSource, sourceName, maxDownloads));
                }
            }
        }

        public void CancelLoadTiles(TileLayer tileLayer)
        {
            PendingTile pendingTile;

            while (pendingTiles.TryDequeue(out pendingTile)) ; // no Clear method
        }

        private void GetTiles(IEnumerable<Tile> tiles, Dispatcher dispatcher, TileSource tileSource, string sourceName, int maxDownloads)
        {
            var useCache = Cache != null
                && !string.IsNullOrEmpty(sourceName)
                && !(tileSource is ImageTileSource)
                && !tileSource.UriFormat.StartsWith("file:");

            foreach (var tile in tiles)
            {
                BitmapSource cachedImage = null;

                if (useCache && GetCachedImage(CacheKey(sourceName, tile), out cachedImage))
                {
                    dispatcher.BeginInvoke(new Action<Tile, ImageSource>((t, i) => t.SetImage(i)), tile, cachedImage);
                }
                else
                {
                    pendingTiles.Enqueue(new PendingTile(tile, cachedImage));
                }
            }

            var newTaskCount = Math.Min(pendingTiles.Count, maxDownloads) - taskCount;

            while (newTaskCount-- > 0)
            {
                Interlocked.Increment(ref taskCount);

                Task.Run(() => LoadPendingTiles(dispatcher, tileSource, sourceName));
            }
        }

        private void LoadPendingTiles(Dispatcher dispatcher, TileSource tileSource, string sourceName)
        {
            var imageTileSource = tileSource as ImageTileSource;
            PendingTile pendingTile;

            while (pendingTiles.TryDequeue(out pendingTile))
            {
                var tile = pendingTile.Tile;
                ImageSource image = null;

                if (imageTileSource != null)
                {
                    image = LoadImage(imageTileSource, tile);
                }
                else
                {
                    var uri = tileSource.GetUri(tile.XIndex, tile.Y, tile.ZoomLevel);

                    if (uri != null)
                    {
                        if (!uri.IsAbsoluteUri)
                        {
                            image = LoadImage(uri.OriginalString);
                        }
                        else if (uri.Scheme == "file")
                        {
                            image = LoadImage(uri.LocalPath);
                        }
                        else
                        {
                            image = DownloadImage(uri, CacheKey(sourceName, tile))
                                ?? pendingTile.CachedImage; // use possibly cached image if download failed
                        }
                    }
                }

                if (image != null)
                {
                    dispatcher.BeginInvoke(new Action<Tile, ImageSource>((t, i) => t.SetImage(i)), tile, image);
                }
                else
                {
                    tile.SetImage(null);
                }
            }

            Interlocked.Decrement(ref taskCount);
        }

        private static ImageSource LoadImage(ImageTileSource tileSource, Tile tile)
        {
            ImageSource image = null;

            try
            {
                image = tileSource.LoadImage(tile.XIndex, tile.Y, tile.ZoomLevel);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ImageTileSource.LoadImage: " + ex.Message);
            }

            return image;
        }

        private static ImageSource LoadImage(string path)
        {
            ImageSource image = null;

            if (File.Exists(path))
            {
                try
                {
                    using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        image = BitmapFrame.Create(fileStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("{0}: {1}", path, ex.Message);
                }
            }

            return image;
        }

        private static ImageSource DownloadImage(Uri uri, string cacheKey)
        {
            BitmapSource image = null;

            try
            {
                var request = HttpWebRequest.CreateHttp(uri);

                if (HttpUserAgent != null)
                {
                    request.UserAgent = HttpUserAgent;
                }

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    using (var responseStream = response.GetResponseStream())
                    using (var memoryStream = new MemoryStream())
                    {
                        responseStream.CopyTo(memoryStream);
                        memoryStream.Seek(0, SeekOrigin.Begin);

                        image = BitmapFrame.Create(memoryStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);

                        if (cacheKey != null)
                        {
                            SetCachedImage(cacheKey, memoryStream, GetExpiration(response.Headers));
                        }
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

        private static string TileKey(TileSource tileSource, Tile tile)
        {
            return string.Format("{0:X}/{1:X}/{2:X}/{3:X}", tileSource.GetHashCode(), tile.ZoomLevel, tile.XIndex, tile.Y);
        }

        private static string CacheKey(string sourceName, Tile tile)
        {
            return string.IsNullOrEmpty(sourceName) ? null : string.Format("{0}/{1}/{2}/{3}", sourceName, tile.ZoomLevel, tile.XIndex, tile.Y);
        }

        private static bool GetCachedImage(string cacheKey, out BitmapSource image)
        {
            image = null;

            var buffer = Cache.Get(cacheKey) as byte[];

            if (buffer != null)
            {
                try
                {
                    using (var memoryStream = new MemoryStream(buffer))
                    {
                        image = BitmapFrame.Create(memoryStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    }

                    DateTime expiration = DateTime.MinValue;

                    if (buffer.Length >= 16 && Encoding.ASCII.GetString(buffer, buffer.Length - 16, 8) == "EXPIRES:")
                    {
                        expiration = new DateTime(BitConverter.ToInt64(buffer, buffer.Length - 8), DateTimeKind.Utc);
                    }

                    return expiration > DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("{0}: {1}", cacheKey, ex.Message);
                }
            }

            return false;
        }

        private static void SetCachedImage(string cacheKey, MemoryStream memoryStream, DateTime expiration)
        {
            memoryStream.Seek(0, SeekOrigin.End);
            memoryStream.Write(Encoding.ASCII.GetBytes("EXPIRES:"), 0, 8);
            memoryStream.Write(BitConverter.GetBytes(expiration.Ticks), 0, 8);

            Cache.Set(cacheKey, memoryStream.ToArray(), new CacheItemPolicy { AbsoluteExpiration = expiration });

            //Debug.WriteLine("Cached {0}, Expires {1}", cacheKey, expiration);
        }

        private static DateTime GetExpiration(WebHeaderCollection headers)
        {
            var cacheControl = headers["Cache-Control"];
            int maxAge;
            DateTime expiration;

            if (cacheControl != null &&
                cacheControl.StartsWith("max-age=") &&
                int.TryParse(cacheControl.Substring(8), out maxAge))
            {
                maxAge = Math.Min(maxAge, (int)DefaultCacheExpiration.TotalSeconds);
                expiration = DateTime.UtcNow.AddSeconds(maxAge);
            }
            else
            {
                var expires = headers["Expires"];
                var maxExpiration = DateTime.UtcNow.Add(DefaultCacheExpiration);

                if (expires == null ||
                    !DateTime.TryParse(expires, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out expiration) ||
                    expiration > maxExpiration)
                {
                    expiration = maxExpiration;
                }
            }

            return expiration;
        }
    }
}

// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Caching;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MapControl
{
    /// <summary>
    /// Loads map tile images by their URIs and optionally caches the images in an ObjectCache.
    /// </summary>
    public class TileImageLoader
    {
        private readonly TileLayer tileLayer;
        private readonly ConcurrentQueue<Tile> pendingTiles = new ConcurrentQueue<Tile>();
        private int downloadThreadCount;

        /// <summary>
        /// Default Name of an ObjectCache instance that is assigned to the Cache property.
        /// </summary>
        public static readonly string DefaultCacheName = "TileCache";

        /// <summary>
        /// Default value for the directory where an ObjectCache instance may save cached data.
        /// </summary>
        public static readonly string DefaultCacheDirectory =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MapControl");

        /// <summary>
        /// The ObjectCache used to cache tile images.
        /// The default is System.Runtime.Caching.MemoryCache.Default.
        /// </summary>
        public static ObjectCache Cache { get; set; }

        /// <summary>
        /// The time interval after which cached images expire. The default value is 30 days.
        /// When an image is not retrieved from the cache during this interval it is considered
        /// as expired and will be removed from the cache. If an image is retrieved from the
        /// cache and the CacheUpdateAge time interval has expired, the image is downloaded
        /// again and rewritten to the cache with a new expiration time.
        /// </summary>
        public static TimeSpan CacheExpiration { get; set; }

        /// <summary>
        /// The time interval after which a cached image is updated and rewritten to the cache.
        /// The default value is one day. This time interval should be shorter than the value
        /// of the CacheExpiration property.
        /// </summary>
        public static TimeSpan CacheUpdateAge { get; set; }

        static TileImageLoader()
        {
            Cache = MemoryCache.Default;
            CacheExpiration = TimeSpan.FromDays(30d);
            CacheUpdateAge = TimeSpan.FromDays(1d);
        }

        internal TileImageLoader(TileLayer tileLayer)
        {
            this.tileLayer = tileLayer;
        }

        internal void BeginGetTiles(IEnumerable<Tile> tiles)
        {
            ThreadPool.QueueUserWorkItem(BeginGetTilesAsync, new List<Tile>(tiles.Where(t => t.ImageSource == null && t.Uri == null)));
        }

        internal void CancelGetTiles()
        {
            Tile tile;
            while (pendingTiles.TryDequeue(out tile)) ; // no Clear method
        }

        private void BeginGetTilesAsync(object newTilesList)
        {
            var newTiles = (List<Tile>)newTilesList;
            var imageTileSource = tileLayer.TileSource as ImageTileSource;

            if (imageTileSource == null)
            {
                if (Cache == null || string.IsNullOrWhiteSpace(tileLayer.SourceName))
                {
                    foreach (var tile in newTiles)
                    {
                        pendingTiles.Enqueue(tile);
                    }
                }
                else
                {
                    var outdatedTiles = new List<Tile>(newTiles.Count);

                    foreach (var tile in newTiles)
                    {
                        var key = CacheKey(tile);
                        var buffer = Cache.Get(key) as byte[];

                        if (buffer == null)
                        {
                            pendingTiles.Enqueue(tile);
                        }
                        else if (!CreateTileImage(tile, buffer))
                        {
                            // got corrupted buffer from cache
                            Cache.Remove(key);
                            pendingTiles.Enqueue(tile);
                        }
                        else if (IsCacheOutdated(buffer))
                        {
                            // update cached image
                            outdatedTiles.Add(tile);
                        }
                    }

                    foreach (var tile in outdatedTiles)
                    {
                        pendingTiles.Enqueue(tile);
                    }
                }

                while (downloadThreadCount < Math.Min(pendingTiles.Count, tileLayer.MaxParallelDownloads))
                {
                    Interlocked.Increment(ref downloadThreadCount);

                    ThreadPool.QueueUserWorkItem(DownloadTiles);
                }
            }
            else if (imageTileSource.CanLoadAsync)
            {
                foreach (var tile in newTiles)
                {
                    tileLayer.Dispatcher.BeginInvoke(
                        (Action<Tile, ImageSource>)((t, s) => t.SetImageSource(s, true)),
                        DispatcherPriority.Background,
                        tile, imageTileSource.LoadImage(tile.XIndex, tile.Y, tile.ZoomLevel));
                }
            }
            else
            {
                foreach (var tile in newTiles)
                {
                    tileLayer.Dispatcher.BeginInvoke(
                        (Action<Tile>)(t => t.SetImageSource(imageTileSource.LoadImage(t.XIndex, t.Y, t.ZoomLevel), true)),
                        DispatcherPriority.Background,
                        tile);
                }
            }
        }

        private void DownloadTiles(object o)
        {
            Tile tile;
            while (pendingTiles.TryDequeue(out tile))
            {
                tile.Uri = tileLayer.TileSource.GetUri(tile.XIndex, tile.Y, tile.ZoomLevel);
                var buffer = DownloadImage(tile.Uri);

                if (buffer != null && CreateTileImage(tile, buffer) && Cache != null)
                {
                    Cache.Set(CacheKey(tile), buffer, new CacheItemPolicy { SlidingExpiration = CacheExpiration });
                }
            }

            Interlocked.Decrement(ref downloadThreadCount);
        }

        private string CacheKey(Tile tile)
        {
            return string.Format("{0}/{1}/{2}/{3}", tileLayer.SourceName, tile.ZoomLevel, tile.XIndex, tile.Y);
        }

        private bool CreateTileImage(Tile tile, byte[] buffer)
        {
            var bitmap = new BitmapImage();

            try
            {
                using (var stream = new MemoryStream(buffer, 8, buffer.Length - 8, false))
                {
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                    bitmap.Freeze();
                }
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("Creating tile image failed: {0}", ex.Message);
                return false;
            }

            tileLayer.Dispatcher.BeginInvoke(
                (Action<Tile>)(t => t.SetImageSource(bitmap, true)),
                DispatcherPriority.Background,
                tile);

            return true;
        }

        private static byte[] DownloadImage(Uri uri)
        {
            byte[] buffer = null;

            try
            {
                var request = (HttpWebRequest)WebRequest.Create(uri);
                request.UserAgent = typeof(TileImageLoader).ToString();

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream responseStream = response.GetResponseStream())
                {
                    var length = response.ContentLength;
                    var creationTime = DateTime.UtcNow.ToBinary();

                    using (var memoryStream = length > 0 ? new MemoryStream((int)length + 8) : new MemoryStream())
                    {
                        memoryStream.Write(BitConverter.GetBytes(creationTime), 0, 8);
                        responseStream.CopyTo(memoryStream);

                        buffer = length > 0 ? memoryStream.GetBuffer() : memoryStream.ToArray();
                    }
                }

                Trace.TraceInformation("Downloaded {0}", uri);
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    var statusCode = ((HttpWebResponse)ex.Response).StatusCode;
                    if (statusCode != HttpStatusCode.NotFound)
                    {
                        Trace.TraceInformation("Downloading {0} failed: {1}", uri, ex.Message);
                    }
                }
                else
                {
                    Trace.TraceWarning("Downloading {0} failed with {1}: {2}", uri, ex.Status, ex.Message);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("Downloading {0} failed: {1}", uri, ex.Message);
            }

            return buffer;
        }

        private static bool IsCacheOutdated(byte[] buffer)
        {
            var creationTime = BitConverter.ToInt64(buffer, 0);

            return DateTime.FromBinary(creationTime) + CacheUpdateAge < DateTime.UtcNow;
        }
    }
}

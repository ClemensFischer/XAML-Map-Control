// WPF MapControl - http://wpfmapcontrol.codeplex.com/
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
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MapControl
{
    /// <summary>
    /// Loads map tile images by their URIs and optionally caches the images in an ObjectCache.
    /// </summary>
    public class TileImageLoader : DispatcherObject
    {
        private readonly TileLayer tileLayer;
        private readonly ConcurrentQueue<Tile> pendingTiles = new ConcurrentQueue<Tile>();

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

        internal void BeginGetTiles(ICollection<Tile> tiles)
        {
            ThreadPool.QueueUserWorkItem(BeginGetTilesAsync, new List<Tile>(tiles.Where(t => t.Image == null && t.Uri == null)));
        }

        internal void CancelGetTiles()
        {
            Tile tile;
            while (pendingTiles.TryDequeue(out tile)) ; // no Clear method
        }

        private void BeginGetTilesAsync(object newTilesList)
        {
            List<Tile> newTiles = (List<Tile>)newTilesList;

            if (tileLayer.TileSource is ImageTileSource)
            {
                ImageTileSource imageTileSource = (ImageTileSource)tileLayer.TileSource;

                newTiles.ForEach(tile =>
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)(() => tile.Image = imageTileSource.GetImage(tile.XIndex, tile.Y, tile.ZoomLevel)));
                });
            }
            else
            {
                if (Cache == null)
                {
                    newTiles.ForEach(tile => pendingTiles.Enqueue(tile));
                }
                else
                {
                    List<Tile> outdatedTiles = new List<Tile>(newTiles.Count);

                    newTiles.ForEach(tile =>
                    {
                        string key = CacheKey(tile);
                        byte[] imageBuffer = Cache.Get(key) as byte[];

                        if (imageBuffer == null)
                        {
                            pendingTiles.Enqueue(tile);
                        }
                        else if (!CreateTileImage(tile, imageBuffer))
                        {
                            // got corrupted buffer from cache
                            Cache.Remove(key);
                            pendingTiles.Enqueue(tile);
                        }
                        else if (IsCacheOutdated(imageBuffer))
                        {
                            // update cached image
                            outdatedTiles.Add(tile);
                        }
                    });

                    outdatedTiles.ForEach(tile => pendingTiles.Enqueue(tile));
                }

                int numDownloads = Math.Min(pendingTiles.Count, tileLayer.MaxParallelDownloads);

                while (--numDownloads >= 0)
                {
                    ThreadPool.QueueUserWorkItem(DownloadTiles);
                }
            }
        }

        private void DownloadTiles(object o)
        {
            Tile tile;
            while (pendingTiles.TryDequeue(out tile))
            {
                tile.Uri = tileLayer.TileSource.GetUri(tile.XIndex, tile.Y, tile.ZoomLevel);
                byte[] imageBuffer = DownloadImage(tile);

                if (imageBuffer != null && CreateTileImage(tile, imageBuffer) && Cache != null)
                {
                    Cache.Set(CacheKey(tile), imageBuffer, new CacheItemPolicy { SlidingExpiration = CacheExpiration });
                }
            }
        }

        private string CacheKey(Tile tile)
        {
            return string.Format("{0}/{1}/{2}/{3}", tileLayer.Name, tile.ZoomLevel, tile.XIndex, tile.Y);
        }

        private byte[] DownloadImage(Tile tile)
        {
            HttpWebRequest request = null;
            byte[] buffer = null;

            try
            {
                TraceInformation("{0} - Requesting", tile.Uri);

                request = (HttpWebRequest)WebRequest.Create(tile.Uri);
                request.UserAgent = typeof(TileImageLoader).ToString();
                request.KeepAlive = true;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream responseStream = response.GetResponseStream())
                {
                    long length = response.ContentLength;
                    long creationTime = DateTime.UtcNow.ToBinary();

                    using (MemoryStream memoryStream = length > 0 ? new MemoryStream((int)length + 8) : new MemoryStream())
                    {
                        memoryStream.Write(BitConverter.GetBytes(creationTime), 0, 8);
                        responseStream.CopyTo(memoryStream);

                        buffer = length > 0 ? memoryStream.GetBuffer() : memoryStream.ToArray();
                    }
                }

                TraceInformation("{0} - Completed", tile.Uri);
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    TraceInformation("{0} - {1}", tile.Uri, ((HttpWebResponse)ex.Response).StatusCode);
                }
                else
                {
                    TraceWarning("{0} - {1}", tile.Uri, ex.Status);
                }
            }
            catch (Exception ex)
            {
                TraceWarning("{0} - {1}", tile.Uri, ex.Message);
            }

            return buffer;
        }

        private bool IsCacheOutdated(byte[] imageBuffer)
        {
            long creationTime = BitConverter.ToInt64(imageBuffer, 0);

            return DateTime.FromBinary(creationTime) + CacheUpdateAge < DateTime.UtcNow;
        }

        private bool CreateTileImage(Tile tile, byte[] imageBuffer)
        {
            BitmapImage bitmap = new BitmapImage();

            try
            {
                using (Stream stream = new MemoryStream(imageBuffer, 8, imageBuffer.Length - 8, false))
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
                TraceWarning("Creating tile image failed: {0}", ex.Message);
                return false;
            }

            Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)(() => tile.Image = bitmap));
            return true;
        }

        private static void TraceWarning(string format, params object[] args)
        {
            Trace.TraceWarning("[{0:00}] {1}", Thread.CurrentThread.ManagedThreadId, string.Format(format, args));
        }

        private static void TraceInformation(string format, params object[] args)
        {
            //Trace.TraceInformation("[{0:00}] {1}", Thread.CurrentThread.ManagedThreadId, string.Format(format, args));
        }
    }
}

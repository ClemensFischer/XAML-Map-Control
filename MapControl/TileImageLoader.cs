// WPF MapControl - http://wpfmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Caching;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MapControl
{
    /// <summary>
    /// Loads map tile images by their URIs and optionally caches the images in an ObjectCache.
    /// </summary>
    public class TileImageLoader : DispatcherObject
    {
        [Serializable]
        private class CachedImage
        {
            public readonly DateTime CreationTime = DateTime.UtcNow;
            public readonly byte[] ImageBuffer;

            public CachedImage(byte[] imageBuffer)
            {
                ImageBuffer = imageBuffer;
            }
        }

        private readonly TileLayer tileLayer;
        private readonly ConcurrentQueue<Tile> pendingTiles = new ConcurrentQueue<Tile>();

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

        /// <summary>
        /// Creates an instance of the ObjectCache-derived type T and sets the static Cache
        /// property to this instance. Class T must (like System.Runtime.Caching.MemoryCache)
        /// provide a constructor with two parameters, first a string that gets the name of
        /// the cache instance, second a NameValueCollection that gets the config parameter.
        /// If config is null, a new NameValueCollection is created. If config does not already
        /// contain an entry with key "directory", a new entry is added with this key and a
        /// value that specifies the path to an application data directory where the cache
        /// implementation may store persistent cache data files.
        /// </summary>
        public static void CreateCache<T>(NameValueCollection config = null) where T : ObjectCache
        {
            if (config == null)
            {
                config = new NameValueCollection(1);
            }

            if (config["directory"] == null)
            {
                config["directory"] = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MapControl");
            }

            try
            {
                Cache = (ObjectCache)Activator.CreateInstance(typeof(T), "TileCache", config);
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("Could not create instance of type {0} with String and NameValueCollection constructor parameters: {1}", typeof(T), ex.Message);
                throw;
            }
        }

        static TileImageLoader()
        {
            Cache = MemoryCache.Default;
            CacheExpiration = TimeSpan.FromDays(30d);
            CacheUpdateAge = TimeSpan.FromDays(1d);

            Application.Current.Exit += (o, e) =>
            {
                IDisposable disposableCache = Cache as IDisposable;
                if (disposableCache != null)
                {
                    disposableCache.Dispose();
                }
            };
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
                        CachedImage cachedImage = Cache.Get(key) as CachedImage;

                        if (cachedImage == null)
                        {
                            pendingTiles.Enqueue(tile);
                        }
                        else if (!CreateTileImage(tile, cachedImage.ImageBuffer))
                        {
                            // got corrupted buffer from cache
                            Cache.Remove(key);
                            pendingTiles.Enqueue(tile);
                        }
                        else if (cachedImage.CreationTime + CacheUpdateAge < DateTime.UtcNow)
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

                if (imageBuffer != null &&
                    CreateTileImage(tile, imageBuffer) &&
                    Cache != null)
                {
                    Cache.Set(CacheKey(tile), new CachedImage(imageBuffer), new CacheItemPolicy { SlidingExpiration = CacheExpiration });
                }
            }
        }

        private string CacheKey(Tile tile)
        {
            return string.Format("{0}-{1}-{2}-{3}", tileLayer.Name, tile.ZoomLevel, tile.XIndex, tile.Y);
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
                    if (response.ContentLength > 0)
                    {
                        using (MemoryStream memoryStream = new MemoryStream((int)response.ContentLength))
                        {
                            responseStream.CopyTo(memoryStream);
                            buffer = memoryStream.GetBuffer();
                        }
                    }
                    else
                    {
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            responseStream.CopyTo(memoryStream);
                            buffer = memoryStream.ToArray();
                        }
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

        private bool CreateTileImage(Tile tile, byte[] buffer)
        {
            BitmapImage bitmap = new BitmapImage();

            try
            {
                using (Stream stream = new MemoryStream(buffer))
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

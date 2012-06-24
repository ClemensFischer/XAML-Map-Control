// WPF MapControl - http://wpfmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
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
    /// Loads map tiles by their URIs and optionally caches their image files in a folder
    /// defined by the static TileCacheFolder property.
    /// </summary>
    public class TileImageLoader : DispatcherObject
    {
        public static string TileCacheFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MapControl Cache");
        public static TimeSpan TileCacheExpiryAge = TimeSpan.FromDays(1d);

        private readonly TileLayer tileLayer;
        private readonly Queue<Tile> pendingTiles = new Queue<Tile>();
        private int numDownloads;

        public TileImageLoader(TileLayer tileLayer)
        {
            this.tileLayer = tileLayer;
        }

        private bool IsCached
        {
            get { return tileLayer.IsCached && !string.IsNullOrEmpty(TileCacheFolder); }
        }

        internal void StartDownloadTiles(ICollection<Tile> tiles)
        {
            ThreadPool.QueueUserWorkItem(StartDownloadTilesAsync, new List<Tile>(tiles.Where(t => t.Image == null && t.Uri == null)));
        }

        internal void StopDownloadTiles()
        {
            lock (pendingTiles)
            {
                pendingTiles.Clear();
            }
        }

        private void StartDownloadTilesAsync(object newTilesList)
        {
            List<Tile> newTiles = (List<Tile>)newTilesList;
            List<Tile> expiredTiles = new List<Tile>(newTiles.Count);

            lock (pendingTiles)
            {
                newTiles.ForEach(tile =>
                {
                    ImageSource image = GetMemoryCachedImage(tile);

                    if (image == null && IsCached)
                    {
                        bool fileCacheExpired;
                        image = GetFileCachedImage(tile, out fileCacheExpired);

                        if (image != null)
                        {
                            SetMemoryCachedImage(tile, image);

                            if (fileCacheExpired)
                            {
                                expiredTiles.Add(tile); // enqueue later
                            }
                        }
                    }

                    if (image != null)
                    {
                        Dispatcher.BeginInvoke((Action)(() => tile.Image = image));
                    }
                    else
                    {
                        pendingTiles.Enqueue(tile);
                    }
                });

                expiredTiles.ForEach(tile => pendingTiles.Enqueue(tile));

                DownloadNextTiles(null);
            }
        }

        private void DownloadNextTiles(object o)
        {
            while (pendingTiles.Count > 0 && numDownloads < tileLayer.MaxDownloads)
            {
                Tile tile = pendingTiles.Dequeue();
                tile.Uri = tileLayer.TileSource.GetUri(tile.XIndex, tile.Y, tile.ZoomLevel);
                numDownloads++;

                ThreadPool.QueueUserWorkItem(DownloadTileAsync, tile);
            }
        }

        private void DownloadTileAsync(object t)
        {
            Tile tile = (Tile)t;
            ImageSource image = DownloadImage(tile);

            if (image != null)
            {
                SetMemoryCachedImage(tile, image);

                Dispatcher.BeginInvoke((Action)(() => tile.Image = image));
            }

            lock (pendingTiles)
            {
                numDownloads--;
                DownloadNextTiles(null);
            }
        }

        private string MemoryCacheKey(Tile tile)
        {
            return string.Format("{0}/{1}/{2}/{3}", tileLayer.Name, tile.ZoomLevel, tile.XIndex, tile.Y);
        }

        private string CacheFilePath(Tile tile)
        {
            return string.Format("{0}.{1}",
                Path.Combine(TileCacheFolder, tileLayer.Name, tile.ZoomLevel.ToString(), tile.XIndex.ToString(), tile.Y.ToString()),
                tileLayer.ImageType);
        }

        private ImageSource GetMemoryCachedImage(Tile tile)
        {
            string key = MemoryCacheKey(tile);
            ImageSource image = MemoryCache.Default.Get(key) as ImageSource;

            if (image != null)
            {
                TraceInformation("{0} - Memory Cached", key);
            }

            return image;
        }

        private void SetMemoryCachedImage(Tile tile, ImageSource image)
        {
            MemoryCache.Default.Set(MemoryCacheKey(tile), image,
                new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMinutes(10d) });
        }

        private ImageSource GetFileCachedImage(Tile tile, out bool expired)
        {
            string path = CacheFilePath(tile);
            ImageSource image = null;
            expired = false;

            if (File.Exists(path))
            {
                try
                {
                    using (Stream fileStream = File.OpenRead(path))
                    {
                        image = BitmapFrame.Create(fileStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    }

                    expired = File.GetLastWriteTime(path) + TileCacheExpiryAge <= DateTime.Now;
                    TraceInformation(expired ? "{0} - File Cache Expired" : "{0} - File Cached", path);
                }
                catch (Exception exc)
                {
                    TraceWarning("{0} - {1}", path, exc.Message);
                    File.Delete(path);
                }
            }

            return image;
        }

        private ImageSource DownloadImage(Tile tile)
        {
            ImageSource image = null;

            try
            {
                TraceInformation("{0} - Requesting", tile.Uri);

                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(tile.Uri);
                webRequest.UserAgent = typeof(TileImageLoader).ToString();
                webRequest.KeepAlive = true;

                using (HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        using (Stream memoryStream = new MemoryStream((int)response.ContentLength))
                        {
                            responseStream.CopyTo(memoryStream);
                            memoryStream.Position = 0;
                            image = BitmapFrame.Create(memoryStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);

                            if (IsCached)
                            {
                                string path = CacheFilePath(tile);
                                Directory.CreateDirectory(Path.GetDirectoryName(path));

                                using (Stream fileStream = File.OpenWrite(path))
                                {
                                    memoryStream.Position = 0;
                                    memoryStream.CopyTo(fileStream);
                                }
                            }
                        }
                    }
                }

                TraceInformation("{0} - Completed", tile.Uri);
            }
            catch (WebException exc)
            {
                if (exc.Status == WebExceptionStatus.ProtocolError)
                {
                    TraceInformation("{0} - {1}", tile.Uri, ((HttpWebResponse)exc.Response).StatusCode);
                }
                else
                {
                    TraceWarning("{0} - {1}", tile.Uri, exc.Status);
                }
            }
            catch (Exception exc)
            {
                TraceWarning("{0} - {1}", tile.Uri, exc.Message);
            }

            return image;
        }

        private static void TraceWarning(string format, params object[] args)
        {
            System.Diagnostics.Trace.TraceWarning("[{0:00}] {1}", Thread.CurrentThread.ManagedThreadId, string.Format(format, args));
        }

        private static void TraceInformation(string format, params object[] args)
        {
            //System.Diagnostics.Trace.TraceInformation("[{0:00}] {1}", Thread.CurrentThread.ManagedThreadId, string.Format(format, args));
        }
    }
}

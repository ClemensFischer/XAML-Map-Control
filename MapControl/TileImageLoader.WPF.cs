// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © Clemens Fischer 2012-2013
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
            ThreadPool.QueueUserWorkItem(BeginGetTilesAsync, new List<Tile>(tiles.Where(t => !t.HasImage)));
        }

        internal void CancelGetTiles()
        {
            Tile tile;
            while (pendingTiles.TryDequeue(out tile)) ; // no Clear method
        }

        private string GetCacheKey(Tile tile)
        {
            return string.Format("{0}/{1}/{2}/{3}", tileLayer.SourceName, tile.ZoomLevel, tile.XIndex, tile.Y);
        }

        private void BeginGetTilesAsync(object newTilesList)
        {
            var newTiles = (List<Tile>)newTilesList;
            var imageTileSource = tileLayer.TileSource as ImageTileSource;
            var animateOpacity = tileLayer.AnimateTileOpacity;

            if (imageTileSource != null && !imageTileSource.CanLoadAsync)
            {
                foreach (var tile in newTiles)
                {
                    tileLayer.Dispatcher.BeginInvoke(
                        (Action<Tile, ImageTileSource>)((t, ts) => t.SetImageSource(ts.LoadImage(t.XIndex, t.Y, t.ZoomLevel), animateOpacity)),
                        DispatcherPriority.Background, tile, imageTileSource);
                }
            }
            else
            {
                if (imageTileSource == null && Cache != null &&
                    !tileLayer.TileSource.UriFormat.StartsWith("file://") &&
                    !string.IsNullOrWhiteSpace(tileLayer.SourceName))
                {
                    var outdatedTiles = new List<Tile>(newTiles.Count);

                    foreach (var tile in newTiles)
                    {
                        var key = GetCacheKey(tile);
                        var buffer = Cache.Get(key) as byte[];
                        var image = CreateImage(buffer);

                        if (image != null)
                        {
                            tileLayer.Dispatcher.BeginInvoke(
                                (Action<Tile, ImageSource>)((t, i) => t.SetImageSource(i, animateOpacity)),
                                DispatcherPriority.Background, tile, image);

                            long creationTime = BitConverter.ToInt64(buffer, 0);

                            if (DateTime.FromBinary(creationTime) + CacheUpdateAge < DateTime.UtcNow)
                            {
                                // update outdated cache
                                outdatedTiles.Add(tile);
                            }
                        }
                        else
                        {
                            pendingTiles.Enqueue(tile);
                        }
                    }

                    newTiles = outdatedTiles; // enqueue outdated tiles at last
                }

                foreach (var tile in newTiles)
                {
                    pendingTiles.Enqueue(tile);
                }

                while (downloadThreadCount < Math.Min(pendingTiles.Count, tileLayer.MaxParallelDownloads))
                {
                    Interlocked.Increment(ref downloadThreadCount);

                    ThreadPool.QueueUserWorkItem(o => LoadTiles(imageTileSource, animateOpacity));
                }
            }
        }

        private void LoadTiles(ImageTileSource imageTileSource, bool animateOpacity)
        {
            Tile tile;

            while (pendingTiles.TryDequeue(out tile))
            {
                byte[] buffer = null;
                ImageSource image = null;

                if (imageTileSource != null)
                {
                    try
                    {
                        image = imageTileSource.LoadImage(tile.XIndex, tile.Y, tile.ZoomLevel);
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceWarning("Loading tile image failed: {0}", ex.Message);
                    }
                }
                else
                {
                    var uri = tileLayer.TileSource.GetUri(tile.XIndex, tile.Y, tile.ZoomLevel);

                    if (uri != null)
                    {
                        if (uri.Scheme == "http")
                        {
                            buffer = DownloadImage(uri);
                            image = CreateImage(buffer);
                        }
                        else
                        {
                            image = CreateImage(uri);
                        }
                    }
                }

                if (image != null)
                {
                    tileLayer.Dispatcher.BeginInvoke(
                        (Action<Tile, ImageSource>)((t, i) => t.SetImageSource(i, animateOpacity)),
                        DispatcherPriority.Background, tile, image);

                    if (buffer != null && Cache != null)
                    {
                        Cache.Set(GetCacheKey(tile), buffer, new CacheItemPolicy { SlidingExpiration = CacheExpiration });
                    }
                }
            }

            Interlocked.Decrement(ref downloadThreadCount);
        }

        private static ImageSource CreateImage(Uri uri)
        {
            var image = new BitmapImage();

            try
            {
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = uri;
                image.EndInit();
                image.Freeze();
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("Creating tile image failed: {0}", ex.Message);
                image = null;
            }

            return image;
        }

        private static ImageSource CreateImage(byte[] buffer)
        {
            BitmapImage image = null;

            if (buffer != null && buffer.Length > sizeof(long))
            {
                try
                {
                    using (var stream = new MemoryStream(buffer, sizeof(long), buffer.Length - sizeof(long), false))
                    {
                        image = new BitmapImage();
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.StreamSource = stream;
                        image.EndInit();
                        image.Freeze();
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceWarning("Creating tile image failed: {0}", ex.Message);
                    image = null;
                }
            }

            return image;
        }

        private static byte[] DownloadImage(Uri uri)
        {
            byte[] buffer = null;

            try
            {
                var request = (HttpWebRequest)WebRequest.Create(uri);
                request.UserAgent = "XAML Map Control";

                using (var response = (HttpWebResponse)request.GetResponse())
                using (var responseStream = response.GetResponseStream())
                {
                    var length = (int)response.ContentLength;

                    using (var memoryStream = length > 0 ? new MemoryStream(length + sizeof(long)) : new MemoryStream())
                    {
                        long creationTime = DateTime.UtcNow.ToBinary();

                        memoryStream.Write(BitConverter.GetBytes(creationTime), 0, sizeof(long));
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
    }
}

// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2014 Clemens Fischer
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
using System.Threading;
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
        /// Default Name of an ObjectCache instance that is assigned to the Cache property.
        /// </summary>
        public const string DefaultCacheName = "TileCache";

        /// <summary>
        /// Default value for the directory where an ObjectCache instance may save cached data.
        /// </summary>
        public static readonly string DefaultCacheDirectory =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MapControl");

        /// <summary>
        /// Default expiration time span for cached images. Used when no expiration date
        /// was transmitted on download. The default value is seven days.
        /// </summary>
        public static TimeSpan DefaultCacheExpiration { get; set; }

        /// <summary>
        /// The ObjectCache used to cache tile images. The default is MemoryCache.Default.
        /// </summary>
        public static ObjectCache Cache { get; set; }

        static TileImageLoader()
        {
            DefaultCacheExpiration = TimeSpan.FromDays(7);
            Cache = MemoryCache.Default;
        }

        private readonly ConcurrentQueue<Tile> pendingTiles = new ConcurrentQueue<Tile>();
        private int threadCount;

        public void BeginLoadTiles(TileLayer tileLayer, IEnumerable<Tile> tiles)
        {
            if (tiles.Any())
            {
                // get current TileLayer property values in UI thread
                var tileSource = tileLayer.TileSource;
                var imageTileSource = tileSource as ImageTileSource;
                var animateOpacity = tileLayer.AnimateTileOpacity;
                var dispatcher = tileLayer.Dispatcher;

                if (imageTileSource != null && !imageTileSource.IsAsync) // call LoadImage in UI thread
                {
                    var setImageAction = new Action<Tile>(t => t.SetImageSource(LoadImage(imageTileSource, t), animateOpacity));

                    foreach (var tile in tiles)
                    {
                        dispatcher.BeginInvoke(setImageAction, DispatcherPriority.Background, tile); // with low priority
                    }
                }
                else
                {
                    var tileList = tiles.ToList();
                    var sourceName = tileLayer.SourceName;
                    var maxDownloads = tileLayer.MaxParallelDownloads;

                    ThreadPool.QueueUserWorkItem(o =>
                        GetTiles(tileList, dispatcher, tileSource, sourceName, animateOpacity, maxDownloads));
                }
            }
        }

        public void CancelLoadTiles(TileLayer tileLayer)
        {
            Tile tile;
            while (pendingTiles.TryDequeue(out tile)) ; // no Clear method
        }

        private void GetTiles(List<Tile> tiles, Dispatcher dispatcher, TileSource tileSource, string sourceName, bool animateOpacity, int maxDownloads)
        {
            if (Cache != null && !string.IsNullOrWhiteSpace(sourceName) &&
                !(tileSource is ImageTileSource) && !tileSource.UriFormat.StartsWith("file:"))
            {
                var setImageAction = new Action<Tile, ImageSource>((t, i) => t.SetImageSource(i, animateOpacity));
                var outdatedTiles = new List<Tile>(tiles.Count);

                foreach (var tile in tiles)
                {
                    var buffer = Cache.Get(TileCache.Key(sourceName, tile)) as byte[];
                    var image = CreateImage(buffer);

                    if (image == null)
                    {
                        pendingTiles.Enqueue(tile); // not yet cached
                    }
                    else if (TileCache.IsExpired(buffer))
                    {
                        dispatcher.Invoke(setImageAction, tile, image); // synchronously before enqueuing
                        outdatedTiles.Add(tile); // update outdated cache
                    }
                    else
                    {
                        dispatcher.BeginInvoke(setImageAction, tile, image);
                    }
                }

                tiles = outdatedTiles; // enqueue outdated tiles after current tiles
            }

            foreach (var tile in tiles)
            {
                pendingTiles.Enqueue(tile);
            }

            while (threadCount < Math.Min(pendingTiles.Count, maxDownloads))
            {
                Interlocked.Increment(ref threadCount);

                ThreadPool.QueueUserWorkItem(o => LoadPendingTiles(dispatcher, tileSource, sourceName, animateOpacity));
            }
        }

        private void LoadPendingTiles(Dispatcher dispatcher, TileSource tileSource, string sourceName, bool animateOpacity)
        {
            var setImageAction = new Action<Tile, ImageSource>((t, i) => t.SetImageSource(i, animateOpacity));
            var imageTileSource = tileSource as ImageTileSource;
            Tile tile;

            while (pendingTiles.TryDequeue(out tile))
            {
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
                        if (uri.Scheme == "file") // create from FileStream because creating from Uri leaves the file open
                        {
                            image = CreateImage(uri.LocalPath);
                        }
                        else
                        {
                            DateTime expirationTime;
                            var buffer = DownloadImage(uri, out expirationTime);

                            image = CreateImage(buffer);

                            if (image != null &&
                                Cache != null &&
                                !string.IsNullOrWhiteSpace(sourceName) &&
                                expirationTime > DateTime.UtcNow)
                            {
                                Cache.Set(TileCache.Key(sourceName, tile), buffer, new CacheItemPolicy { AbsoluteExpiration = expirationTime });
                            }
                        }
                    }
                }

                if (image != null || !tile.HasImageSource) // set null image if tile does not yet have an ImageSource
                {
                    dispatcher.BeginInvoke(setImageAction, tile, image);
                }
            }

            Interlocked.Decrement(ref threadCount);
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
                Trace.TraceWarning("Loading tile image failed: {0}", ex.Message);
            }

            return image;
        }

        private static ImageSource CreateImage(string path)
        {
            ImageSource image = null;

            if (File.Exists(path))
            {
                try
                {
                    using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        image = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceWarning("Creating tile image failed: {0}", ex.Message);
                }
            }

            return image;
        }

        private static ImageSource CreateImage(byte[] buffer)
        {
            ImageSource image = null;

            if (buffer != null)
            {
                try
                {
                    using (var stream = TileCache.ImageStream(buffer))
                    {
                        image = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceWarning("Creating tile image failed: {0}", ex.Message);
                }
            }

            return image;
        }

        private static byte[] DownloadImage(Uri uri, out DateTime expirationTime)
        {
            expirationTime = DateTime.UtcNow + DefaultCacheExpiration;

            byte[] buffer = null;

            try
            {
                var request = HttpWebRequest.CreateHttp(uri);

                using (var response = (HttpWebResponse)request.GetResponse())
                using (var responseStream = response.GetResponseStream())
                {
                    var expiresHeader = response.Headers["Expires"];
                    DateTime expires;

                    if (expiresHeader != null &&
                        DateTime.TryParse(expiresHeader, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out expires) &&
                        expirationTime > expires)
                    {
                        expirationTime = expires;
                    }

                    buffer = TileCache.CreateBuffer(responseStream, (int)response.ContentLength, expirationTime);
                }

                //Trace.TraceInformation("Downloaded {0}, expires {1}", uri, expirationTime);
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    var statusCode = ((HttpWebResponse)ex.Response).StatusCode;
                    if (statusCode != HttpStatusCode.NotFound)
                    {
                        Trace.TraceWarning("Downloading {0} failed: {1}", uri, ex.Message);
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

        private static class TileCache
        {
            private const int imageBufferOffset = sizeof(Int64);

            public static string Key(string sourceName, Tile tile)
            {
                return string.Format("{0}/{1}/{2}/{3}", sourceName, tile.ZoomLevel, tile.XIndex, tile.Y);
            }

            public static MemoryStream ImageStream(byte[] cacheBuffer)
            {
                return new MemoryStream(cacheBuffer, imageBufferOffset, cacheBuffer.Length - imageBufferOffset, false);
            }

            public static bool IsExpired(byte[] cacheBuffer)
            {
                return DateTime.FromBinary(BitConverter.ToInt64(cacheBuffer, 0)) < DateTime.UtcNow;
            }

            public static byte[] CreateBuffer(Stream imageStream, int length, DateTime expirationTime)
            {
                using (var memoryStream = length > 0 ? new MemoryStream(length + imageBufferOffset) : new MemoryStream())
                {
                    memoryStream.Write(BitConverter.GetBytes(expirationTime.ToBinary()), 0, imageBufferOffset);
                    imageStream.CopyTo(memoryStream);

                    return length > 0 ? memoryStream.GetBuffer() : memoryStream.ToArray();
                }
            }
        }
    }
}

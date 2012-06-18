// WPF MapControl - http://wpfmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
        public static TimeSpan TileCacheExpiryAge = TimeSpan.FromDays(1);

        private readonly Queue<Tile> pendingTiles = new Queue<Tile>();
        private int numDownloads;

        internal int MaxDownloads;
        internal string TileLayerName;
        internal TileSource TileSource;

        private bool IsCached
        {
            get { return !string.IsNullOrEmpty(TileCacheFolder) && !string.IsNullOrEmpty(TileLayerName); }
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

            lock (pendingTiles)
            {
                if (IsCached)
                {
                    List<Tile> expiredTiles = new List<Tile>(newTiles.Count);

                    newTiles.ForEach(tile =>
                    {
                        bool cacheExpired;
                        ImageSource image = GetCachedImage(tile, out cacheExpired);

                        if (image != null)
                        {
                            Dispatcher.BeginInvoke((Action)(() => tile.Image = image));

                            if (cacheExpired)
                            {
                                expiredTiles.Add(tile); // enqueue later
                            }
                        }
                        else
                        {
                            pendingTiles.Enqueue(tile);
                        }
                    });

                    expiredTiles.ForEach(tile => pendingTiles.Enqueue(tile));
                }
                else
                {
                    newTiles.ForEach(tile => pendingTiles.Enqueue(tile));
                }

                DownloadNextTiles(null);
            }
        }

        private void DownloadNextTiles(object o)
        {
            while (pendingTiles.Count > 0 && numDownloads < MaxDownloads)
            {
                Tile tile = pendingTiles.Dequeue();
                tile.Uri = TileSource.GetUri(tile.XIndex, tile.Y, tile.ZoomLevel);
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
                Dispatcher.BeginInvoke((Action)(() => tile.Image = image));
            }

            lock (pendingTiles)
            {
                tile.Uri = null;
                numDownloads--;
                DownloadNextTiles(null);
            }
        }

        private ImageSource GetCachedImage(Tile tile, out bool expired)
        {
            string tileDir = TileDirectory(tile);
            ImageSource image = null;
            expired = false;

            try
            {
                if (Directory.Exists(tileDir))
                {
                    string tilePath = Directory.GetFiles(tileDir, string.Format("{0}.*", tile.Y)).FirstOrDefault();

                    if (tilePath != null)
                    {
                        try
                        {
                            using (Stream fileStream = File.OpenRead(tilePath))
                            {
                                image = BitmapFrame.Create(fileStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                            }

                            expired = File.GetLastWriteTime(tilePath) + TileCacheExpiryAge <= DateTime.Now;

                            TraceInformation(expired ? "{0} - Cache Expired" : "{0} - Cached", tilePath);
                        }
                        catch (Exception exc)
                        {
                            TraceWarning("{0} - {1}", tilePath, exc.Message);
                            File.Delete(tilePath);
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                TraceWarning("{0} - {1}", tileDir, exc.Message);
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

                            BitmapDecoder decoder = BitmapDecoder.Create(memoryStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                            image = decoder.Frames[0];

                            string tilePath;

                            if (IsCached && (tilePath = TilePath(tile, decoder)) != null)
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(tilePath));

                                using (Stream fileStream = File.OpenWrite(tilePath))
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

        private string TileDirectory(Tile tile)
        {
            return Path.Combine(TileCacheFolder, TileLayerName, tile.ZoomLevel.ToString(), tile.XIndex.ToString());
        }

        private string TilePath(Tile tile, BitmapDecoder decoder)
        {
            string extension;

            if (decoder is PngBitmapDecoder)
            {
                extension = "png";
            }
            else if (decoder is JpegBitmapDecoder)
            {
                extension = "jpg";
            }
            else if (decoder is BmpBitmapDecoder)
            {
                extension = "bmp";
            }
            else if (decoder is GifBitmapDecoder)
            {
                extension = "gif";
            }
            else if (decoder is TiffBitmapDecoder)
            {
                extension = "tif";
            }
            else
            {
                return null;
            }

            return Path.Combine(TileDirectory(tile), string.Format("{0}.{1}", tile.Y, extension));
        }

        private static void TraceWarning(string format, params object[] args)
        {
            System.Diagnostics.Trace.TraceWarning("[{0:00}] {1}", Thread.CurrentThread.ManagedThreadId, string.Format(format, args));
        }

        private static void TraceInformation(string format, params object[] args)
        {
            System.Diagnostics.Trace.TraceInformation("[{0:00}] {1}", Thread.CurrentThread.ManagedThreadId, string.Format(format, args));
        }
    }
}

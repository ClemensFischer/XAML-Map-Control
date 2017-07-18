// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2017 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace MapControl
{
    /// <summary>
    /// Loads map tile images and optionally caches them in a IImageCache.
    /// </summary>
    public class TileImageLoader : ITileImageLoader
    {
        /// <summary>
        /// Default name of an IImageCache instance that is assigned to the Cache property.
        /// </summary>
        public const string DefaultCacheName = "TileCache";

        /// <summary>
        /// Default StorageFolder where an IImageCache instance may save cached data.
        /// </summary>
        public static readonly StorageFolder DefaultCacheFolder = ApplicationData.Current.TemporaryFolder;

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
        /// The IImageCache implementation used to cache tile images. The default is null.
        /// </summary>
        public static Caching.IImageCache Cache;

        static TileImageLoader()
        {
            DefaultCacheExpiration = TimeSpan.FromDays(1);
            MinimumCacheExpiration = TimeSpan.FromHours(1);
        }

        private readonly ConcurrentStack<Tile> pendingTiles = new ConcurrentStack<Tile>();
        private int taskCount;

        public void LoadTiles(MapTileLayer tileLayer)
        {
            pendingTiles.Clear();

            var tiles = tileLayer.Tiles.Where(t => t.Pending);

            if (tiles.Any())
            {
                var tileSource = tileLayer.TileSource;
                var imageTileSource = tileSource as ImageTileSource;

                if (imageTileSource != null)
                {
                    LoadTiles(tiles, imageTileSource);
                }
                else
                {
                    pendingTiles.PushRange(tiles.Reverse().ToArray());

                    var sourceName = tileLayer.SourceName;
                    var maxDownloads = tileLayer.MaxParallelDownloads;

                    while (taskCount < Math.Min(pendingTiles.Count, maxDownloads))
                    {
                        Interlocked.Increment(ref taskCount);

                        Task.Run(async () =>
                        {
                            await LoadPendingTiles(tileSource, sourceName);

                            Interlocked.Decrement(ref taskCount);
                        });
                    }
                }
            }
        }

        private void LoadTiles(IEnumerable<Tile> tiles, ImageTileSource tileSource)
        {
            foreach (var tile in tiles)
            {
                tile.Pending = false;

                try
                {
                    var image = tileSource.LoadImage(tile.XIndex, tile.Y, tile.ZoomLevel);

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

        private async Task LoadPendingTiles(TileSource tileSource, string sourceName)
        {
            Tile tile;

            while (pendingTiles.TryPop(out tile))
            {
                tile.Pending = false;

                try
                {
                    var uri = tileSource.GetUri(tile.XIndex, tile.Y, tile.ZoomLevel);

                    if (uri != null)
                    {
                        if (!uri.IsAbsoluteUri)
                        {
                            await LoadImageFromFile(tile, uri.OriginalString);
                        }
                        else if (uri.Scheme == "file")
                        {
                            await LoadImageFromFile(tile, uri.LocalPath);
                        }
                        else if (Cache == null || sourceName == null)
                        {
                            await DownloadImage(tile, uri, null);
                        }
                        else
                        {
                            var extension = Path.GetExtension(uri.LocalPath);

                            if (string.IsNullOrEmpty(extension) || extension == ".jpeg")
                            {
                                extension = ".jpg";
                            }

                            var cacheKey = string.Format(@"{0}\{1}\{2}\{3}{4}", sourceName, tile.ZoomLevel, tile.XIndex, tile.Y, extension);
                            var cacheItem = await Cache.GetAsync(cacheKey);
                            var loaded = false;

                            if (cacheItem == null || cacheItem.Expiration <= DateTime.UtcNow)
                            {
                                loaded = await DownloadImage(tile, uri, cacheKey);
                            }

                            if (!loaded && cacheItem != null && cacheItem.Buffer != null)
                            {
                                using (var stream = new InMemoryRandomAccessStream())
                                {
                                    await stream.WriteAsync(cacheItem.Buffer);
                                    await stream.FlushAsync();
                                    stream.Seek(0);

                                    await LoadImageFromStream(tile, stream);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("{0}/{1}/{2}: {3}", tile.ZoomLevel, tile.XIndex, tile.Y, ex.Message);
                }
            }
        }

        private async Task<bool> DownloadImage(Tile tile, Uri uri, string cacheKey)
        {
            try
            {
                using (var httpClient = new HttpClient(new HttpBaseProtocolFilter { AllowAutoRedirect = false }))
                using (var response = await httpClient.GetAsync(uri))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string tileInfo;

                        if (!response.Headers.TryGetValue("X-VE-Tile-Info", out tileInfo) || tileInfo != "no-tile") // set by Bing Maps
                        {
                            await LoadImageFromHttpResponse(response, tile, cacheKey);
                        }

                        return true;
                    }

                    Debug.WriteLine("{0}: {1} {2}", uri, (int)response.StatusCode, response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("{0}: {1}", uri, ex.Message);
            }

            return false;
        }

        private async Task LoadImageFromHttpResponse(HttpResponseMessage response, Tile tile, string cacheKey)
        {
            using (var stream = new InMemoryRandomAccessStream())
            {
                using (var content = response.Content)
                {
                    await content.WriteToStreamAsync(stream);
                }

                await stream.FlushAsync();
                stream.Seek(0);

                await LoadImageFromStream(tile, stream);

                if (cacheKey != null)
                {
                    var buffer = new Windows.Storage.Streams.Buffer((uint)stream.Size);

                    stream.Seek(0);
                    await stream.ReadAsync(buffer, buffer.Capacity, InputStreamOptions.None);

                    var expiration = DefaultCacheExpiration;

                    if (response.Headers.CacheControl.MaxAge.HasValue)
                    {
                        expiration = response.Headers.CacheControl.MaxAge.Value;

                        if (expiration < MinimumCacheExpiration)
                        {
                            expiration = MinimumCacheExpiration;
                        }
                    }

                    await Cache.SetAsync(cacheKey, buffer, DateTime.UtcNow.Add(expiration));
                }
            }
        }

        private async Task LoadImageFromFile(Tile tile, string path)
        {
            try
            {
                var file = await StorageFile.GetFileFromPathAsync(path);

                using (var stream = await file.OpenReadAsync())
                {
                    await LoadImageFromStream(tile, stream);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("{0}: {1}", path, ex.Message);
            }
        }

        private async Task LoadImageFromStream(Tile tile, IRandomAccessStream stream)
        {
            var tcs = new TaskCompletionSource<object>();

            await tile.Image.Dispatcher.RunAsync(CoreDispatcherPriority.Low, async () =>
            {
                try
                {
                    var image = new BitmapImage();
                    await image.SetSourceAsync(stream);
                    tile.SetImage(image, true, false);

                    tcs.SetResult(null);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            await tcs.Task;
        }
    }
}

// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2015 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        /// was transmitted on download. The default and recommended minimum value is seven days.
        /// See OpenStreetMap tile usage policy: http://wiki.openstreetmap.org/wiki/Tile_usage_policy
        /// </summary>
        public static TimeSpan DefaultCacheExpiration = TimeSpan.FromDays(7);

        /// <summary>
        /// The IImageCache implementation used to cache tile images. The default is null.
        /// </summary>
        public static Caching.IImageCache Cache;

        private class PendingTile
        {
            public readonly Tile Tile;
            public readonly Uri Uri;
            public readonly BitmapSource Image;

            public PendingTile(Tile tile, Uri uri)
            {
                Tile = tile;
                Uri = uri;
                Image = new BitmapImage();
            }
        }

        private readonly ConcurrentQueue<PendingTile> pendingTiles = new ConcurrentQueue<PendingTile>();
        private int taskCount;

        public void BeginLoadTiles(TileLayer tileLayer, IEnumerable<Tile> tiles)
        {
            var tileSource = tileLayer.TileSource;
            var imageTileSource = tileSource as ImageTileSource;
            var sourceName = tileLayer.SourceName;
            var useCache = Cache != null && !string.IsNullOrEmpty(sourceName);

            foreach (var tile in tiles)
            {
                try
                {
                    if (imageTileSource != null)
                    {
                        tile.SetImage(imageTileSource.LoadImage(tile.XIndex, tile.Y, tile.ZoomLevel));
                    }
                    else
                    {
                        var uri = tileSource.GetUri(tile.XIndex, tile.Y, tile.ZoomLevel);

                        if (uri == null)
                        {
                            tile.SetImage(null);
                        }
                        else if (!useCache)
                        {
                            tile.SetImage(new BitmapImage(uri));
                        }
                        else
                        {
                            pendingTiles.Enqueue(new PendingTile(tile, uri));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Loading tile image failed: {0}", ex.Message);
                }

                var newTaskCount = Math.Min(pendingTiles.Count, tileLayer.MaxParallelDownloads) - taskCount;

                while (newTaskCount-- > 0)
                {
                    Interlocked.Increment(ref taskCount);

                    Task.Run(async () => await LoadPendingTiles(tileSource, sourceName));
                }
            }
        }

        public void CancelLoadTiles(TileLayer tileLayer)
        {
            PendingTile pendingTile;

            while (pendingTiles.TryDequeue(out pendingTile)) ; // no Clear method
        }

        private async Task LoadPendingTiles(TileSource tileSource, string sourceName)
        {
            PendingTile pendingTile;

            while (pendingTiles.TryDequeue(out pendingTile))
            {
                var tile = pendingTile.Tile;
                var uri = pendingTile.Uri;
                var image = pendingTile.Image;
                var extension = Path.GetExtension(uri.LocalPath);

                if (string.IsNullOrEmpty(extension) || extension == ".jpeg")
                {
                    extension = ".jpg";
                }

                var cacheKey = string.Format(@"{0}\{1}\{2}\{3}{4}", sourceName, tile.ZoomLevel, tile.XIndex, tile.Y, extension);
                var cacheItem = await Cache.GetAsync(cacheKey);
                var loaded = false;

                if (cacheItem == null || cacheItem.Expires <= DateTime.UtcNow)
                {
                    loaded = await DownloadImage(tile, image, uri, cacheKey);
                }

                if (!loaded && cacheItem != null && cacheItem.Buffer != null)
                {
                    using (var stream = new InMemoryRandomAccessStream())
                    {
                        await stream.WriteAsync(cacheItem.Buffer);
                        await stream.FlushAsync();
                        stream.Seek(0);

                        await LoadImageFromStream(tile, image, stream);
                    }
                }
            }

            Interlocked.Decrement(ref taskCount);
        }

        private async Task<bool> DownloadImage(Tile tile, BitmapSource image, Uri uri, string cacheKey)
        {
            try
            {
                using (var httpClient = new HttpClient(new HttpBaseProtocolFilter { AllowAutoRedirect = false }))
                using (var response = await httpClient.GetAsync(uri))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        return await LoadImageFromHttpResponse(response, tile, image, cacheKey);
                    }

                    Debug.WriteLine("{0}: ({1}) {2}", uri, (int)response.StatusCode, response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("{0}: {1}", uri, ex.Message);
            }

            return false;
        }

        private async Task<bool> LoadImageFromHttpResponse(HttpResponseMessage response, Tile tile, BitmapSource image, string cacheKey)
        {
            using (var stream = new InMemoryRandomAccessStream())
            {
                using (var content = response.Content)
                {
                    await content.WriteToStreamAsync(stream);
                }

                await stream.FlushAsync();
                stream.Seek(0);

                var loaded = await LoadImageFromStream(tile, image, stream);

                if (loaded && cacheKey != null)
                {
                    var buffer = new Windows.Storage.Streams.Buffer((uint)stream.Size);

                    stream.Seek(0);
                    await stream.ReadAsync(buffer, buffer.Capacity, InputStreamOptions.None);

                    var maxAge = DefaultCacheExpiration;

                    if (response.Headers.CacheControl.MaxAge.HasValue &&
                        response.Headers.CacheControl.MaxAge.Value < maxAge)
                    {
                        maxAge = response.Headers.CacheControl.MaxAge.Value;
                    }

                    await Cache.SetAsync(cacheKey, buffer, DateTime.UtcNow.Add(maxAge));
                }

                return loaded;
            }
        }

        private async Task<bool> LoadImageFromStream(Tile tile, BitmapSource image, IRandomAccessStream stream)
        {
            var completion = new TaskCompletionSource<bool>();

            var action = image.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                async () =>
                {
                    try
                    {
                        await image.SetSourceAsync(stream);
                        tile.SetImage(image, true, false);

                        completion.SetResult(true);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        tile.SetImage(null);

                        completion.SetResult(false);
                    }
                });

            return await completion.Task;
        }
    }
}

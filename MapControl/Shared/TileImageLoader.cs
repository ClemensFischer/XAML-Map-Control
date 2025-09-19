using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MapControl
{
    /// <summary>
    /// Loads and optionally caches map tile images for a MapTileLayer.
    /// </summary>
    public interface ITileImageLoader
    {
        void BeginLoadTiles(IEnumerable<Tile> tiles, TileSource tileSource, string cacheName, IProgress<double> progress);

        void CancelLoadTiles();
    }

    public class TileImageLoader : ITileImageLoader
    {
        private static ILogger logger;
        private static ILogger Logger => logger ??= ImageLoader.LoggerFactory?.CreateLogger(typeof(TileImageLoader));

        /// <summary>
        /// Default folder path where a persistent cache implementation may save data, i.e. "C:\ProgramData\MapControl\TileCache".
        /// </summary>
        public static string DefaultCacheFolder =>
#if UWP
            Path.Combine(Windows.Storage.ApplicationData.Current.LocalCacheFolder.Path, "TileCache");
#else
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MapControl", "TileCache");
#endif
        /// <summary>
        /// An IDistributedCache implementation used to cache tile images.
        /// The default value is a MemoryDistributedCache instance.
        /// </summary>
        public static IDistributedCache Cache { get; set; } = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));

        /// <summary>
        /// Default expiration time for cached tile images. Used when no expiration time
        /// was transmitted on download. The default value is one day.
        /// </summary>
        public static TimeSpan DefaultCacheExpiration { get; set; } = TimeSpan.FromDays(1);

        /// <summary>
        /// Minimum expiration time for cached tile images. A transmitted expiration time
        /// that falls below this value is ignored. The default value is TimeSpan.Zero.
        /// </summary>
        public static TimeSpan MinCacheExpiration { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// Maximum expiration time for cached tile images. A transmitted expiration time
        /// that exceeds this value is ignored. The default value is ten days.
        /// </summary>
        public static TimeSpan MaxCacheExpiration { get; set; } = TimeSpan.FromDays(10);

        /// <summary>
        /// Maximum number of parallel tile loading tasks. The default value is 4.
        /// </summary>
        public static int MaxLoadTasks { get; set; } = 4;

        private readonly Queue<Tile> tileQueue = new Queue<Tile>();
        private int tileCount;
        private int taskCount;

        /// <summary>
        /// Loads all pending tiles from the tiles collection. Tile image caching is enabled when the Cache
        /// property is not null and tileSource.UriFormat starts with "http" and cacheName is a non-empty string.
        /// </summary>
        public void BeginLoadTiles(IEnumerable<Tile> tiles, TileSource tileSource, string cacheName, IProgress<double> progress)
        {
            if (Cache == null || tileSource.UriTemplate == null || !tileSource.UriTemplate.StartsWith("http"))
            {
                cacheName = null; // disable caching
            }

            lock (tileQueue)
            {
                tileQueue.Clear();

                foreach (var tile in tiles.Where(tile => tile.IsPending))
                {
                    tileQueue.Enqueue(tile);
                }

                tileCount = tileQueue.Count;

                var maxTasks = Math.Min(tileCount, MaxLoadTasks);

                while (taskCount < maxTasks)
                {
                    taskCount++;
                    Logger?.LogDebug("Task count: {count}", taskCount);

                    _ = Task.Run(() => LoadTilesFromQueue(tileSource, cacheName, progress));
                }
            }
        }

        public void CancelLoadTiles()
        {
            lock (tileQueue)
            {
                tileQueue.Clear();
                tileCount = 0;
            }
        }

        private async Task LoadTilesFromQueue(TileSource tileSource, string cacheName, IProgress<double> progress)
        {
            while (true)
            {
                Tile tile;
                int tileNumber;

                lock (tileQueue)
                {
                    if (!tileQueue.TryDequeue(out tile))
                    {
                        taskCount--;
                        Logger?.LogDebug("Task count: {count}", taskCount);
                        break;
                    }

                    tileNumber = tileCount - tileQueue.Count;
                }

                tile.IsPending = false;

                progress?.Report((double)tileNumber / tileCount);

                Logger?.LogDebug("Loading tile {number} of {count} ({zoom}/{column}/{row}) in thread {thread}",
                    tileNumber, tileCount, tile.ZoomLevel, tile.Column, tile.Row, Environment.CurrentManagedThreadId);

                try
                {
                    // Pass tileSource.LoadImageAsync calls to platform-specific method
                    // tile.LoadImageAsync(Func<Task<ImageSource>>) for completion in the UI thread.

                    if (string.IsNullOrEmpty(cacheName))
                    {
                        await tile.LoadImageAsync(() => tileSource.LoadImageAsync(tile.ZoomLevel, tile.Column, tile.Row)).ConfigureAwait(false);
                    }
                    else
                    {
                        var uri = tileSource.GetUri(tile.ZoomLevel, tile.Column, tile.Row);

                        if (uri != null)
                        {
                            var buffer = await LoadCachedBuffer(tile, uri, cacheName).ConfigureAwait(false);

                            if (buffer?.Length > 0)
                            {
                                await tile.LoadImageAsync(() => tileSource.LoadImageAsync(buffer)).ConfigureAwait(false);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger?.LogError(ex, "Failed loading tile {zoom}/{column}/{row}", tile.ZoomLevel, tile.Column, tile.Row);
                }
            }
        }

        private static async Task<byte[]> LoadCachedBuffer(Tile tile, Uri uri, string cacheName)
        {
            var extension = Path.GetExtension(uri.LocalPath);

            if (string.IsNullOrEmpty(extension) || extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase))
            {
                extension = ".jpg";
            }

            var cacheKey = $"{cacheName}/{tile.ZoomLevel}/{tile.Column}/{tile.Row}{extension}";

            try
            {
                var cachedBuffer = await Cache.GetAsync(cacheKey).ConfigureAwait(false);

                if (cachedBuffer != null)
                {
                    return cachedBuffer;
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Cache.GetAsync({cacheKey})", cacheKey);
            }

            (var buffer, var maxAge) = await ImageLoader.GetHttpResponseAsync(uri).ConfigureAwait(false);

            if (buffer != null)
            {
                try
                {
                    var options = new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow =
                            !maxAge.HasValue ? DefaultCacheExpiration
                            : maxAge.Value < MinCacheExpiration ? MinCacheExpiration
                            : maxAge.Value > MaxCacheExpiration ? MaxCacheExpiration
                            : maxAge.Value
                    };

                    await Cache.SetAsync(cacheKey, buffer, options).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Logger?.LogError(ex, "Cache.SetAsync({cacheKey})", cacheKey);
                }
            }

            return buffer;
        }
    }

#if NETFRAMEWORK
    internal static class QueueExtension
    {
        public static bool TryDequeue<T>(this Queue<T> queue, out T item) where T : class
        {
            item = queue.Count > 0 ? queue.Dequeue() : null;
            return item != null;
        }
    }
#endif
}

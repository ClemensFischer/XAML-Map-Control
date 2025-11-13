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
    public interface ITileImageLoader
    {
        /// <summary>
        /// Loads all pending tiles from the tiles collection. Tile image caching is enabled
        /// when cacheName is a non-empty string and tiles are loaded from http or https Uris.
        /// </summary>
        void BeginLoadTiles(IEnumerable<Tile> tiles, TileSource tileSource, string cacheName, IProgress<double> progress);

        /// <summary>
        /// Terminates all running tile loading tasks.
        /// </summary>
        void CancelLoadTiles();
    }

    /// <summary>
    /// Loads and optionally caches map tile images for a MapTilePyramidLayer.
    /// </summary>
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

        private readonly Queue<Tile> tileQueue = new();
        private int tileCount;
        private int taskCount;

        public void BeginLoadTiles(IEnumerable<Tile> tiles, TileSource tileSource, string cacheName, IProgress<double> progress)
        {
            if (Cache == null)
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
            bool TryDequeueTile(out Tile tile)
            {
                lock (tileQueue)
                {
                    if (tileQueue.Count > 0)
                    {
                        tile = tileQueue.Dequeue();
                        return true;
                    }

                    taskCount--;
                    Logger?.LogDebug("Task count: {count}", taskCount);
                }

                tile = null;
                return false;
            }

            while (TryDequeueTile(out Tile tile))
            {
                tile.IsPending = false;

                Logger?.LogDebug("Thread {thread,2}: Loading tile ({zoom}/{column}/{row})",
                    Environment.CurrentManagedThreadId, tile.ZoomLevel, tile.Column, tile.Row);

                try
                {
                    // Pass image loading callbacks to platform-specific method
                    // tile.LoadImageAsync(Func<Task<ImageSource>>) for completion in the UI thread.

                    var uri = tileSource.GetUri(tile.ZoomLevel, tile.Column, tile.Row);

                    if (uri == null)
                    {
                        await tile.LoadImageAsync(() => tileSource.LoadImageAsync(tile.ZoomLevel, tile.Column, tile.Row)).ConfigureAwait(false);
                    }
                    else if (uri.Scheme != "http" && uri.Scheme != "https" || string.IsNullOrEmpty(cacheName))
                    {
                        await tile.LoadImageAsync(() => ImageLoader.LoadImageAsync(uri)).ConfigureAwait(false);
                    }
                    else
                    {
                        var buffer = await LoadCachedBuffer(tile, uri, cacheName).ConfigureAwait(false);

                        if (buffer?.Length > 0)
                        {
                            await tile.LoadImageAsync(() => ImageLoader.LoadImageAsync(buffer)).ConfigureAwait(false);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger?.LogError(ex, "Failed loading tile {zoom}/{column}/{row}", tile.ZoomLevel, tile.Column, tile.Row);
                }

                progress?.Report(1d - (double)tileQueue.Count / tileCount);
            }
        }

        private static async Task<byte[]> LoadCachedBuffer(Tile tile, Uri uri, string cacheName)
        {
            var extension = Path.GetExtension(uri.LocalPath).ToLower();

            if (string.IsNullOrEmpty(extension) || extension.Equals(".jpeg"))
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
}

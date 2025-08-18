using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
#if WPF
using System.Windows.Media;
#elif UWP
using Windows.UI.Xaml.Media;
#elif WINUI
using Microsoft.UI.Xaml.Media;
#endif

namespace MapControl
{
    /// <summary>
    /// Loads and optionally caches map tile images for a MapTileLayer.
    /// </summary>
    public interface ITileImageLoader
    {
        Task LoadTilesAsync(IEnumerable<Tile> tiles, TileSource tileSource, string cacheName, IProgress<double> progress);
    }

    public partial class TileImageLoader : ITileImageLoader
    {
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

        private static ILogger logger;
        private static ILogger Logger => logger ?? (logger = ImageLoader.LoggerFactory?.CreateLogger<TileImageLoader>());

        private ConcurrentStack<Tile> pendingTiles;

        /// <summary>
        /// Loads all pending tiles from the tiles collection. Tile image caching is enabled when the Cache
        /// property is non-null and tileSource.UriFormat starts with "http" and cacheName is a non-empty string.
        /// </summary>
        public async Task LoadTilesAsync(IEnumerable<Tile> tiles, TileSource tileSource, string cacheName, IProgress<double> progress)
        {
            pendingTiles?.Clear();

            if (tileSource != null && tiles != null && (tiles = tiles.Where(tile => tile.IsPending)).Any())
            {
                pendingTiles = new ConcurrentStack<Tile>(tiles.Reverse());

                var tileCount = pendingTiles.Count;
                var taskCount = Math.Min(tileCount, MaxLoadTasks);

                if (taskCount > 0)
                {
                    if (Cache == null || tileSource.UriTemplate == null || !tileSource.UriTemplate.StartsWith("http"))
                    {
                        cacheName = null; // disable tile image caching
                    }

                    progress?.Report(0d);

                    var tasks = new Task[taskCount];
                    var tileStack = pendingTiles; // pendingTiles member may change while tasks are running

                    async Task LoadTilesFromQueueAsync()
                    {
                        while (tileStack.TryPop(out var tile)) // use captured tileStack variable in local function
                        {
                            tile.IsPending = false;

                            progress?.Report((double)(tileCount - tileStack.Count) / tileCount);

                            try
                            {
                                await LoadTileImage(tile, tileSource, cacheName).ConfigureAwait(false);
                            }
                            catch (Exception ex)
                            {
                                Logger?.LogError(ex, "Failed loading tile image {zoom}/{column}/{row}", tile.ZoomLevel, tile.Column, tile.Row);
                            }
                        }
                    }

                    for (int i = 0; i < taskCount; i++)
                    {
                        tasks[i] = Task.Run(LoadTilesFromQueueAsync);
                    }

                    await Task.WhenAll(tasks);
                }
            }
        }

        private static async Task LoadTileImage(Tile tile, TileSource tileSource, string cacheName)
        {
            // Pass tileSource.LoadImageAsync calls to platform-specific method
            // LoadTileImage(Tile, Func<Task<ImageSource>>) for execution on the UI thread in WinUI and UWP.

            if (string.IsNullOrEmpty(cacheName))
            {
                Task<ImageSource> LoadImage() => tileSource.LoadImageAsync(tile.Column, tile.Row, tile.ZoomLevel);

                await LoadTileImage(tile, LoadImage).ConfigureAwait(false);
            }
            else
            {
                var uri = tileSource.GetUri(tile.Column, tile.Row, tile.ZoomLevel);

                if (uri != null)
                {
                    var buffer = await LoadCachedBuffer(tile, uri, cacheName).ConfigureAwait(false);

                    if (buffer != null && buffer.Length > 0)
                    {
                        Task<ImageSource> LoadImage() => tileSource.LoadImageAsync(buffer);

                        await LoadTileImage(tile, LoadImage).ConfigureAwait(false);
                    }
                }
            }
        }

        private static async Task<byte[]> LoadCachedBuffer(Tile tile, Uri uri, string cacheName)
        {
            byte[] buffer = null;

            var extension = Path.GetExtension(uri.LocalPath);

            if (string.IsNullOrEmpty(extension) || extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase))
            {
                extension = ".jpg";
            }

            var cacheKey = $"{cacheName}/{tile.ZoomLevel}/{tile.Column}/{tile.Row}{extension}";

            try
            {
                buffer = await Cache.GetAsync(cacheKey).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Cache.GetAsync: {cacheKey}", cacheKey);
            }

            if (buffer == null)
            {
                var response = await ImageLoader.GetHttpResponseAsync(uri).ConfigureAwait(false);

                if (response != null)
                {
                    buffer = response.Buffer ?? Array.Empty<byte>(); // cache even if null, when no tile available

                    try
                    {
                        var options = new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow =
                                !response.MaxAge.HasValue ? DefaultCacheExpiration
                                : response.MaxAge.Value < MinCacheExpiration ? MinCacheExpiration
                                : response.MaxAge.Value > MaxCacheExpiration ? MaxCacheExpiration
                                : response.MaxAge.Value
                        };

                        await Cache.SetAsync(cacheKey, buffer, options).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        Logger?.LogError(ex, "Cache.SetAsync: {cacheKey}", cacheKey);
                    }
                }
            }

            return buffer;
        }
    }
}

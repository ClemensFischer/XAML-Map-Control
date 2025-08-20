using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Threading;
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
        Task LoadTilesAsync(IEnumerable<Tile> tiles, TileSource tileSource, string cacheName, IProgress<double> progress, CancellationToken cancellationToken);
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

        /// <summary>
        /// Indicates whether HTTP requests are cancelled when the LoadTilesAsync method is cancelled.
        /// If the property value is false, cancellation only stops dequeuing entries from the tile queue,
        /// but lets currently running requests run to completion.
        /// </summary>
        public static bool RequestCancellationEnabled { get; set; }

        private static ILogger logger;
        private static ILogger Logger => logger ?? (logger = ImageLoader.LoggerFactory?.CreateLogger<TileImageLoader>());

        /// <summary>
        /// Loads all pending tiles from the tiles collection. Tile image caching is enabled when the Cache
        /// property is not null and tileSource.UriFormat starts with "http" and cacheName is a non-empty string.
        /// </summary>
        public async Task LoadTilesAsync(IEnumerable<Tile> tiles, TileSource tileSource, string cacheName, IProgress<double> progress, CancellationToken cancellationToken)
        {
            var pendingTiles = new ConcurrentStack<Tile>(tiles.Where(tile => tile.IsPending).Reverse());
            var tileCount = pendingTiles.Count;
            var taskCount = Math.Min(tileCount, MaxLoadTasks);

            if (taskCount > 0)
            {
                if (Cache == null || tileSource.UriTemplate == null || !tileSource.UriTemplate.StartsWith("http"))
                {
                    cacheName = null; // disable tile image caching
                }

                async Task LoadTilesFromQueueAsync()
                {
                    while (!cancellationToken.IsCancellationRequested && pendingTiles.TryPop(out var tile))
                    {
                        tile.IsPending = false;

                        progress?.Report((double)(tileCount - pendingTiles.Count) / tileCount);

                        Logger?.LogTrace("[{thread}] Loading tile image {zoom}/{column}/{row}", Environment.CurrentManagedThreadId, tile.ZoomLevel, tile.Column, tile.Row);

                        try
                        {
                            var requestCancellationToken = RequestCancellationEnabled ? cancellationToken : CancellationToken.None;

                            await LoadTileImage(tile, tileSource, cacheName, requestCancellationToken).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            Logger?.LogError(ex, "Failed loading tile image {zoom}/{column}/{row}", tile.ZoomLevel, tile.Column, tile.Row);
                        }
                    }
                }

                try
                {
                    var tasks = new Task[taskCount];

                    for (int i = 0; i < taskCount; i++)
                    {
                        tasks[i] = Task.Run(LoadTilesFromQueueAsync, cancellationToken);
                    }

                    await Task.WhenAll(tasks);
                }
                catch (OperationCanceledException)
                {
                    // no action
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    Logger?.LogTrace("Cancelled LoadTilesAsync with {count} pending tiles", pendingTiles.Count);
                }
            }
        }

        private static async Task LoadTileImage(Tile tile, TileSource tileSource, string cacheName, CancellationToken cancellationToken)
        {
            // Pass tileSource.LoadImageAsync calls to platform-specific method
            // LoadTileImage(Tile, Func<Task<ImageSource>>) for execution on the UI thread in WinUI and UWP.

            if (string.IsNullOrEmpty(cacheName))
            {
                Task<ImageSource> LoadImage() => tileSource.LoadImageAsync(tile.Column, tile.Row, tile.ZoomLevel, cancellationToken);

                await LoadTileImage(tile, LoadImage, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var uri = tileSource.GetUri(tile.Column, tile.Row, tile.ZoomLevel);

                if (uri != null)
                {
                    var buffer = await LoadCachedBuffer(tile, uri, cacheName, cancellationToken).ConfigureAwait(false);

                    if (buffer != null && buffer.Length > 0)
                    {
                        Task<ImageSource> LoadImage() => tileSource.LoadImageAsync(buffer);

                        await LoadTileImage(tile, LoadImage, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
        }

        private static async Task<byte[]> LoadCachedBuffer(Tile tile, Uri uri, string cacheName, CancellationToken cancellationToken)
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
                buffer = await Cache.GetAsync(cacheKey, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                Logger?.LogTrace("Cancelled Cache.GetAsync({cacheKey})", cacheKey);
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Cache.GetAsync({cacheKey})", cacheKey);
            }

            if (buffer == null)
            {
                var response = await ImageLoader.GetHttpResponseAsync(uri, null, cancellationToken).ConfigureAwait(false);

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

                        await Cache.SetAsync(cacheKey, buffer, options, cancellationToken).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        Logger?.LogTrace("Cancelled Cache.SetAsync({cacheKey})", cacheKey);
                    }
                    catch (Exception ex)
                    {
                        Logger?.LogError(ex, "Cache.SetAsync({cacheKey})", cacheKey);
                    }
                }
            }

            return buffer;
        }
    }
}

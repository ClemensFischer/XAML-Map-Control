// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MapControl
{
    /// <summary>
    /// Loads and optionally caches map tile images for a MapTileLayer.
    /// </summary>
    public partial class TileImageLoader : ITileImageLoader
    {
        /// <summary>
        /// Maximum number of parallel tile loading tasks. The default value is 4.
        /// </summary>
        public static int MaxLoadTasks { get; set; } = 4;

        /// <summary>
        /// Minimum expiration time for cached tile images. The default value is one hour.
        /// </summary>
        public static TimeSpan MinCacheExpiration { get; set; } = TimeSpan.FromHours(1);

        /// <summary>
        /// Maximum expiration time for cached tile images. The default value is one week.
        /// </summary>
        public static TimeSpan MaxCacheExpiration { get; set; } = TimeSpan.FromDays(7);

        /// <summary>
        /// Default expiration time for cached tile images. Used when no expiration time
        /// was transmitted on download. The default value is one day.
        /// </summary>
        public static TimeSpan DefaultCacheExpiration { get; set; } = TimeSpan.FromDays(1);

        /// <summary>
        /// Format string for creating cache keys from the SourceName property of a TileSource,
        /// the ZoomLevel, XIndex, and Y properties of a Tile, and the image file extension.
        /// The default value is "{0};{1};{2};{3}{4}".
        /// </summary>
        public static string CacheKeyFormat { get; set; } = "{0};{1};{2};{3}{4}";

        private readonly ConcurrentStack<Tile> pendingTiles = new ConcurrentStack<Tile>();
        private int taskCount;

        /// <summary>
        /// Loads all pending tiles from the Tiles collection of a MapTileLayer by running up to MaxLoadTasks parallel Tasks.
        /// If the TileSource's SourceName is non-empty and its UriFormat starts with "http", tile images are cached in the
        /// TileImageLoader's Cache.
        /// </summary>
        public void LoadTilesAsync(MapTileLayer tileLayer)
        {
            pendingTiles.Clear();

            var tileSource = tileLayer.TileSource;
            var sourceName = tileLayer.SourceName;
            var tiles = tileLayer.Tiles.Where(t => t.Pending);

            if (tileSource != null && tiles.Any())
            {
                pendingTiles.PushRange(tiles.Reverse().ToArray());

                Func<Tile, Task> loadFunc;

                if (Cache != null && !string.IsNullOrEmpty(sourceName) &&
                    tileSource.UriFormat != null && tileSource.UriFormat.StartsWith("http"))
                {
                    loadFunc = tile => LoadCachedTileImageAsync(tile, tileSource, sourceName);
                }
                else
                {
                    loadFunc = tile => LoadTileImageAsync(tile, tileSource);
                }

                var newTasks = Math.Min(pendingTiles.Count, MaxLoadTasks) - taskCount;

                while (--newTasks >= 0)
                {
                    Interlocked.Increment(ref taskCount);

                    var task = Task.Run(() => LoadTilesAsync(loadFunc)); // do not await
                }

                //Debug.WriteLine("{0}: {1} tasks", Environment.CurrentManagedThreadId, taskCount);
            }
        }

        private async Task LoadTilesAsync(Func<Tile, Task> loadTileImageFunc)
        {
            Tile tile;

            while (pendingTiles.TryPop(out tile))
            {
                tile.Pending = false;

                try
                {
                    await loadTileImageFunc(tile);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("TileImageLoader: {0}/{1}/{2}: {3}", tile.ZoomLevel, tile.XIndex, tile.Y, ex.Message);
                }
            }

            Interlocked.Decrement(ref taskCount);
            //Debug.WriteLine("{0}: {1} tasks", Environment.CurrentManagedThreadId, taskCount);
        }

        private async Task LoadCachedTileImageAsync(Tile tile, TileSource tileSource, string sourceName)
        {
            var uri = tileSource.GetUri(tile.XIndex, tile.Y, tile.ZoomLevel);

            if (uri != null)
            {
                var extension = Path.GetExtension(uri.LocalPath);

                if (string.IsNullOrEmpty(extension) || extension == ".jpeg")
                {
                    extension = ".jpg";
                }

                var cacheKey = string.Format(CacheKeyFormat, sourceName, tile.ZoomLevel, tile.XIndex, tile.Y, extension);

                await LoadCachedTileImageAsync(tile, uri, cacheKey);
            }
        }

        private static DateTime GetExpiration(TimeSpan? maxAge)
        {
            var expiration = DefaultCacheExpiration;

            if (maxAge.HasValue)
            {
                expiration = maxAge.Value;

                if (expiration < MinCacheExpiration)
                {
                    expiration = MinCacheExpiration;
                }
                else if (expiration > MaxCacheExpiration)
                {
                    expiration = MaxCacheExpiration;
                }
            }

            return DateTime.UtcNow.Add(expiration);
        }
    }
}

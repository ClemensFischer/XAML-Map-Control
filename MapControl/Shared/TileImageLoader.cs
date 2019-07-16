// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2019 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        /// Default expiration time for cached tile images. Used when no expiration time
        /// was transmitted on download. The default value is one day.
        /// </summary>
        public static TimeSpan DefaultCacheExpiration { get; set; } = TimeSpan.FromDays(1);

        /// <summary>
        /// Minimum expiration time for cached tile images. The default value is one day.
        /// </summary>
        public static TimeSpan MinCacheExpiration { get; set; } = TimeSpan.FromDays(1);

        /// <summary>
        /// Maximum expiration time for cached tile images. The default value is one week.
        /// </summary>
        public static TimeSpan MaxCacheExpiration { get; set; } = TimeSpan.FromDays(7);

        /// <summary>
        /// Format string for creating cache keys from the sourceName argument passed to LoadTilesAsync,
        /// the ZoomLevel, XIndex, and Y properties of a Tile, and the image file extension.
        /// The default value is "{0}/{1}/{2}/{3}{4}".
        /// </summary>
        public static string CacheKeyFormat { get; set; } = "{0}/{1}/{2}/{3}{4}";


        public class TileQueue : ConcurrentStack<Tile>
        {
            public void Enqueue(IEnumerable<Tile> tiles)
            {
                PushRange(tiles.Reverse().ToArray());
            }

            public bool TryDequeue(out Tile tile)
            {
                return TryPop(out tile);
            }
        }

        private readonly TileQueue tileQueue = new TileQueue();
        private Func<Tile, Task> loadTileImage;
        private int taskCount;

        /// <summary>
        /// Loads all pending tiles from the tiles collection.
        /// If tileSource.UriFormat starts with "http" and sourceName is a non-empty string,
        /// tile images will be cached in the TileImageLoader's Cache (if it's not null).
        /// The method is async void because it implements void ITileImageLoader.LoadTilesAsync
        /// and is not awaited when it is called in MapTileLayer.UpdateTiles().
        /// </summary>
        public async void LoadTilesAsync(IEnumerable<Tile> tiles, TileSource tileSource, string sourceName)
        {
            tileQueue.Clear();

            tiles = tiles.Where(tile => tile.Pending);

            if (tiles.Any() && tileSource != null)
            {
                if (Cache != null &&
                    tileSource.UriFormat != null &&
                    tileSource.UriFormat.StartsWith("http") &&
                    !string.IsNullOrEmpty(sourceName))
                {
                    loadTileImage = tile => LoadCachedTileImageAsync(tile, tileSource, sourceName);
                }
                else
                {
                    loadTileImage = tile => LoadTileImageAsync(tile, tileSource);
                }

                tileQueue.Enqueue(tiles);

                var newTasks = Math.Min(tileQueue.Count, MaxLoadTasks) - taskCount;

                if (newTasks > 0)
                {
                    Interlocked.Add(ref taskCount, newTasks);

                    await Task.WhenAll(Enumerable.Range(0, newTasks).Select(n => LoadTilesFromQueueAsync())).ConfigureAwait(false);
                }
            }
        }

        private async Task LoadTilesFromQueueAsync()
        {
            Tile tile;

            while (tileQueue.TryDequeue(out tile))
            {
                tile.Pending = false;

                try
                {
                    await loadTileImage(tile).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("TileImageLoader: {0}/{1}/{2}: {3}", tile.ZoomLevel, tile.XIndex, tile.Y, ex.Message);
                }
            }

            Interlocked.Decrement(ref taskCount);
        }

        private static async Task LoadCachedTileImageAsync(Tile tile, TileSource tileSource, string sourceName)
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

                await LoadCachedTileImageAsync(tile, uri, cacheKey).ConfigureAwait(false);
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

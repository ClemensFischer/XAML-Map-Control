// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2020 Clemens Fischer
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
        /// Format string for creating cache keys from the cacheName argument passed to LoadTilesAsync,
        /// the ZoomLevel, XIndex, and Y properties of a Tile, and the image file extension.
        /// The default value is "{0}/{1}/{2}/{3}{4}".
        /// </summary>
        public static string CacheKeyFormat { get; set; } = "{0}/{1}/{2}/{3}{4}";


        private class TileQueue : ConcurrentStack<Tile>
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
        private Func<Tile, Task> loadTile;
        private int taskCount;

        /// <summary>
        /// Loads all pending tiles from the tiles collection.
        /// If tileSource.UriFormat starts with "http" and cacheName is a non-empty string,
        /// tile images will be cached in the TileImageLoader's Cache (if that is not null).
        /// </summary>
        public void LoadTiles(IEnumerable<Tile> tiles, TileSource tileSource, string cacheName)
        {
            tileQueue.Clear();

            tiles = tiles.Where(tile => tile.Pending);

            if (tiles.Any() && tileSource != null)
            {
                if (Cache != null &&
                    tileSource.UriFormat != null &&
                    tileSource.UriFormat.StartsWith("http") &&
                    !string.IsNullOrEmpty(cacheName))
                {
                    loadTile = tile => LoadCachedTileAsync(tile, tileSource, cacheName);
                }
                else
                {
                    loadTile = tile => LoadTileAsync(tile, tileSource);
                }

                tileQueue.Enqueue(tiles);

                while (taskCount < Math.Min(tileQueue.Count, MaxLoadTasks))
                {
                    Interlocked.Increment(ref taskCount);

                    Task.Run(LoadTilesFromQueueAsync);
                }
            }
        }

        private async Task LoadTilesFromQueueAsync()
        {
            while (tileQueue.TryDequeue(out Tile tile))
            {
                tile.Pending = false;

                try
                {
                     await loadTile(tile).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("TileImageLoader: {0}/{1}/{2}: {3}", tile.ZoomLevel, tile.XIndex, tile.Y, ex.Message);
                }
            }

            Interlocked.Decrement(ref taskCount);
        }

        private static async Task LoadCachedTileAsync(Tile tile, TileSource tileSource, string cacheName)
        {
            var uri = tileSource.GetUri(tile.XIndex, tile.Y, tile.ZoomLevel);

            if (uri != null)
            {
                var extension = Path.GetExtension(uri.LocalPath);

                if (string.IsNullOrEmpty(extension) || extension == ".jpeg")
                {
                    extension = ".jpg";
                }

                var cacheKey = string.Format(CacheKeyFormat, cacheName, tile.ZoomLevel, tile.XIndex, tile.Y, extension);

                await LoadCachedTileAsync(tile, uri, cacheKey).ConfigureAwait(false);
            }
        }

        private static DateTime GetExpiration(TimeSpan? maxAge)
        {
            return DateTime.UtcNow.Add(maxAge ?? DefaultCacheExpiration);
        }
    }
}

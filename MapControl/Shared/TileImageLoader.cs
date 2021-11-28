// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
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
        /// Maximum expiration time for cached tile images. A transmitted expiration time
        /// that exceeds this value is ignored. The default value is ten days.
        /// </summary>
        public static TimeSpan MaxCacheExpiration { get; set; } = TimeSpan.FromDays(10);

        /// <summary>
        /// The current TileSource, passed to the most recent LoadTiles call.
        /// </summary>
        public TileSource TileSource { get; private set; }

        private ConcurrentStack<Tile> pendingTiles;

        /// <summary>
        /// Loads all pending tiles from the tiles collection.
        /// If tileSource.UriFormat starts with "http" and cacheName is a non-empty string,
        /// tile images will be cached in the TileImageLoader's Cache - if that is not null.
        /// </summary>
        public Task LoadTiles(IEnumerable<Tile> tiles, TileSource tileSource, string cacheName)
        {
            pendingTiles?.Clear(); // stop processing the current queue

            TileSource = tileSource;

            if (tileSource != null)
            {
                pendingTiles = new ConcurrentStack<Tile>(tiles.Where(tile => tile.Pending).Reverse());

                var numTasks = Math.Min(pendingTiles.Count, MaxLoadTasks);

                if (numTasks > 0)
                {
                    if (Cache == null || tileSource.UriFormat == null || !tileSource.UriFormat.StartsWith("http"))
                    {
                        cacheName = null; // no tile caching
                    }

                    var tasks = Enumerable.Range(0, numTasks)
                        .Select(_ => Task.Run(() => LoadPendingTiles(pendingTiles, tileSource, cacheName)));

                    return Task.WhenAll(tasks);
                }
            }

            return Task.CompletedTask;
        }

        private static async Task LoadPendingTiles(ConcurrentStack<Tile> pendingTiles, TileSource tileSource, string cacheName)
        {
            while (pendingTiles.TryPop(out var tile))
            {
                tile.Pending = false;

                try
                {
                    await LoadTile(tile, tileSource, cacheName).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"TileImageLoader: {tile.ZoomLevel}/{tile.XIndex}/{tile.Y}: {ex.Message}");
                }
            }
        }

        private static Task LoadTile(Tile tile, TileSource tileSource, string cacheName)
        {
            if (string.IsNullOrEmpty(cacheName))
            {
                return LoadTile(tile, tileSource);
            }

            var uri = tileSource.GetUri(tile.XIndex, tile.Y, tile.ZoomLevel);

            if (uri != null)
            {
                var extension = Path.GetExtension(uri.LocalPath);

                if (string.IsNullOrEmpty(extension) || extension == ".jpeg")
                {
                    extension = ".jpg";
                }

                var cacheKey = string.Format(CultureInfo.InvariantCulture,
                    "{0}/{1}/{2}/{3}{4}", cacheName, tile.ZoomLevel, tile.XIndex, tile.Y, extension);

                return LoadCachedTile(tile, uri, cacheKey);
            }

            return Task.CompletedTask;
        }

        private static DateTime GetExpiration(TimeSpan? maxAge)
        {
            if (!maxAge.HasValue)
            {
                maxAge = DefaultCacheExpiration;
            }
            else if (maxAge.Value > MaxCacheExpiration)
            {
                maxAge = MaxCacheExpiration;
            }

            return DateTime.UtcNow.Add(maxAge.Value);
        }
    }
}

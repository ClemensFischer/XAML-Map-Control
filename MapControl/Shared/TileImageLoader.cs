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
            pendingTiles?.Clear(); // stop download from current stack

            pendingTiles = new ConcurrentStack<Tile>(tiles.Where(tile => tile.Pending).Reverse());

            TileSource = tileSource;

            if (tileSource == null || pendingTiles.IsEmpty)
            {
                return Task.CompletedTask;
            }

            if (Cache == null || tileSource.UriFormat == null || !tileSource.UriFormat.StartsWith("http"))
            {
                cacheName = null; // no tile caching
            }

            var tasks = Enumerable
                .Range(0, Math.Min(pendingTiles.Count, MaxLoadTasks))
                .Select(_ => Task.Run(() => LoadPendingTilesAsync(pendingTiles, tileSource, cacheName)));

            return Task.WhenAll(tasks);
        }

        private static async Task LoadPendingTilesAsync(ConcurrentStack<Tile> pendingTiles, TileSource tileSource, string cacheName)
        {
            while (pendingTiles.TryPop(out var tile))
            {
                tile.Pending = false;

                try
                {
                    await LoadTileAsync(tile, tileSource, cacheName).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"TileImageLoader: {tile.ZoomLevel}/{tile.XIndex}/{tile.Y}: {ex.Message}");
                }
            }
        }

        private static Task LoadTileAsync(Tile tile, TileSource tileSource, string cacheName)
        {
            if (string.IsNullOrEmpty(cacheName))
            {
                return LoadTileAsync(tile, tileSource);
            }

            var uri = tileSource.GetUri(tile.XIndex, tile.Y, tile.ZoomLevel);

            if (uri == null)
            {
                return Task.CompletedTask;
            }

            var extension = Path.GetExtension(uri.LocalPath);

            if (string.IsNullOrEmpty(extension) || extension == ".jpeg")
            {
                extension = ".jpg";
            }

            var cacheKey = string.Format(CultureInfo.InvariantCulture,
                "{0}/{1}/{2}/{3}{4}", cacheName, tile.ZoomLevel, tile.XIndex, tile.Y, extension);

            return LoadCachedTileAsync(tile, uri, cacheKey);
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

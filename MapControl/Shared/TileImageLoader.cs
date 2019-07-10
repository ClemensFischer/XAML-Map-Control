// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2019 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        /// Format string for creating cache keys from the SourceName property and the
        /// ZoomLevel, XIndex, and Y properties of a Tile and the image file extension.
        /// The default value is "{0}/{1}/{2}/{3}{4}".
        /// </summary>
        public static string CacheKeyFormat { get; set; } = "{0}/{1}/{2}/{3}{4}";


        private readonly TileQueue tileQueue = new TileQueue();
        private int taskCount;

        /// <summary>
        /// Loads all pending tiles from the tiles collection in up to MaxLoadTasks parallel Tasks.
        /// If the UriFormat of the TileSource starts with "http" and sourceName is a non-empty string,
        /// tile images will be cached in the TileImageLoader's Cache.
        /// </summary>
        public void LoadTilesAsync(IEnumerable<Tile> tiles, TileSource tileSource, string sourceName)
        {
            tileQueue.Clear();

            if (tileSource != null)
            {
                tileQueue.Enqueue(tiles);

                var newTasks = Math.Min(tileQueue.Count, MaxLoadTasks) - taskCount;

                if (newTasks > 0)
                {
                    Interlocked.Add(ref taskCount, newTasks);

                    while (--newTasks >= 0)
                    {
                        Task.Run(() => LoadTilesFromQueueAsync(tileSource, sourceName));
                    }
                }
            }
        }

        private async Task LoadTilesFromQueueAsync(TileSource tileSource, string sourceName)
        {
            Tile tile;

            while (tileQueue.TryDequeue(out tile))
            {
                try
                {
                    await LoadTileImageAsync(tile, tileSource, sourceName).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("TileImageLoader: {0}/{1}/{2}: {3}", tile.ZoomLevel, tile.XIndex, tile.Y, ex.Message);
                }
            }

            Interlocked.Decrement(ref taskCount);
        }

        private async Task LoadTileImageAsync(Tile tile, TileSource tileSource, string sourceName)
        {
            if (Cache != null &&
                tileSource.UriFormat != null &&
                tileSource.UriFormat.StartsWith("http") &&
                !string.IsNullOrEmpty(sourceName))
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
            else
            {
                await LoadTileImageAsync(tile, tileSource).ConfigureAwait(false);
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

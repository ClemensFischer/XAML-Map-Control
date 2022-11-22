// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
        private class TileQueue : ConcurrentStack<Tile>
        {
            public TileQueue(IEnumerable<Tile> tiles)
                : base(tiles.Where(tile => !tile.IsLoaded).Reverse())
            {
            }

            public bool IsCanceled { get; private set; }

            public bool TryDequeue(out Tile tile)
            {
                tile = null;

                if (IsCanceled || !TryPop(out tile))
                {
                    return false;
                }

                tile.IsLoaded = true;
                return true;
            }

            public void Cancel()
            {
                IsCanceled = true;
                Clear();
            }
        }

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
        /// Reports tile loading process as double value between 0 and 1. 
        /// </summary>
        public IProgress<double> Progress { get; set; }

        /// <summary>
        /// The current TileSource, passed to the most recent LoadTiles call.
        /// </summary>
        public TileSource TileSource { get; private set; }

        private TileQueue pendingTiles;
        private int progressTotal;
        private int progressLoaded;

        /// <summary>
        /// Loads all pending tiles from the tiles collection.
        /// If tileSource.UriFormat starts with "http" and cacheName is a non-empty string,
        /// tile images will be cached in the TileImageLoader's Cache - if that is not null.
        /// </summary>
        public Task LoadTiles(IEnumerable<Tile> tiles, TileSource tileSource, string cacheName)
        {
            pendingTiles?.Cancel();

            TileSource = tileSource;

            if (tileSource != null)
            {
                pendingTiles = new TileQueue(tiles);

                var numTasks = Math.Min(pendingTiles.Count, MaxLoadTasks);

                if (numTasks > 0)
                {
                    if (Progress != null)
                    {
                        progressTotal = pendingTiles.Count;
                        progressLoaded = 0;
                        Progress.Report(0d);
                    }

                    if (Cache == null || tileSource.UriTemplate == null || !tileSource.UriTemplate.StartsWith("http"))
                    {
                        cacheName = null; // no tile caching
                    }

                    return Task.WhenAll(Enumerable.Range(0, numTasks).Select(
                        _ => Task.Run(() => LoadPendingTiles(pendingTiles, tileSource, cacheName))));
                }
            }

            if (Progress != null && progressLoaded < progressTotal)
            {
                Progress.Report(1d);
            }

            return Task.CompletedTask;
        }

        private async Task LoadPendingTiles(TileQueue tileQueue, TileSource tileSource, string cacheName)
        {
            while (tileQueue.TryDequeue(out var tile))
            {
                try
                {
                    await LoadTile(tile, tileSource, cacheName).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"TileImageLoader: {tile.ZoomLevel}/{tile.Column}/{tile.Row}: {ex.Message}");
                }

                if (Progress != null && !tileQueue.IsCanceled)
                {
                    Interlocked.Increment(ref progressLoaded);

                    Progress.Report((double)progressLoaded / progressTotal);
                }
            }
        }

        private static Task LoadTile(Tile tile, TileSource tileSource, string cacheName)
        {
            if (string.IsNullOrEmpty(cacheName))
            {
                return LoadTile(tile, () => tileSource.LoadImageAsync(tile.Column, tile.Row, tile.ZoomLevel));
            }

            var uri = tileSource.GetUri(tile.Column, tile.Row, tile.ZoomLevel);

            if (uri != null)
            {
                var extension = Path.GetExtension(uri.LocalPath);

                if (string.IsNullOrEmpty(extension) || extension == ".jpeg")
                {
                    extension = ".jpg";
                }

                var cacheKey = string.Format(CultureInfo.InvariantCulture,
                    "{0}/{1}/{2}/{3}{4}", cacheName, tile.ZoomLevel, tile.Column, tile.Row, extension);

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

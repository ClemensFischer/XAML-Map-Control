// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
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
                : base(tiles.Where(tile => tile.IsPending).Reverse())
            {
            }

            public bool TryDequeue(out Tile tile)
            {
                if (TryPop(out tile))
                {
                    tile.IsPending = false;
                    return true;
                }

                tile = null;
                return false;
            }
        }

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
        /// Maximum expiration time for cached tile images. A transmitted expiration time
        /// that exceeds this value is ignored. The default value is ten days.
        /// </summary>
        public static TimeSpan MaxCacheExpiration { get; set; } = TimeSpan.FromDays(10);

        /// <summary>
        /// Maximum number of parallel tile loading tasks. The default value is 4.
        /// </summary>
        public static int MaxLoadTasks { get; set; } = 4;


        private TileQueue pendingTiles;

        /// <summary>
        /// Loads all pending tiles from the tiles collection.
        /// If tileSource.UriFormat starts with "http" and cacheName is a non-empty string,
        /// tile images will be cached in the TileImageLoader's Cache - if that is not null.
        /// </summary>
        public Task LoadTilesAsync(IEnumerable<Tile> tiles, TileSource tileSource, string cacheName, IProgress<double> progress)
        {
            pendingTiles?.Clear();

            if (tiles != null && tileSource != null)
            {
                pendingTiles = new TileQueue(tiles);

                var tileCount = pendingTiles.Count;
                var taskCount = Math.Min(tileCount, MaxLoadTasks);

                if (taskCount > 0)
                {
                    if (Cache == null || tileSource.UriTemplate == null || !tileSource.UriTemplate.StartsWith("http"))
                    {
                        cacheName = null; // no tile caching
                    }

                    progress?.Report(0d);

                    var tileQueue = pendingTiles; // pendingTiles may change while tasks are running
                    var tasks = new Task[taskCount];

                    async Task LoadTilesFromQueueAsync()
                    {
                        while (tileQueue.TryDequeue(out var tile))
                        {
                            progress?.Report((double)(tileCount - tileQueue.Count) / tileCount);

                            await LoadTileAsync(tile, tileSource, cacheName).ConfigureAwait(false);
                        }
                    }

                    for (int i = 0; i < taskCount; i++)
                    {
                        tasks[i] = Task.Run(LoadTilesFromQueueAsync);
                    }

                    return Task.WhenAll(tasks);
                }
            }

            return Task.CompletedTask;
        }

        private static Task LoadTileAsync(Tile tile, TileSource tileSource, string cacheName)
        {
            try
            {
                if (string.IsNullOrEmpty(cacheName))
                {
                    return LoadTileAsync(tile, () => tileSource.LoadImageAsync(tile.Column, tile.Row, tile.ZoomLevel));
                }

                var uri = tileSource.GetUri(tile.Column, tile.Row, tile.ZoomLevel);

                if (uri != null)
                {
                    return LoadCachedTileAsync(tile, uri, cacheName);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"TileImageLoader: {tile.ZoomLevel}/{tile.Column}/{tile.Row}: {ex.Message}");
            }

            return Task.CompletedTask;
        }

        private static async Task LoadCachedTileAsync(Tile tile, Uri uri, string cacheName)
        {
            var extension = Path.GetExtension(uri.LocalPath);

            if (string.IsNullOrEmpty(extension) || extension == ".jpeg")
            {
                extension = ".jpg";
            }

            var cacheKey = string.Format(CultureInfo.InvariantCulture,
                "{0}/{1}/{2}/{3}{4}", cacheName, tile.ZoomLevel, tile.Column, tile.Row, extension);

            byte[] buffer = null;

            try
            {
                buffer = await Cache.GetAsync(cacheKey).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"TileImageLoader: {cacheKey}: {ex.Message}");
            }

            if (buffer == null)
            {
                var response = await ImageLoader.GetHttpResponseAsync(uri).ConfigureAwait(false);

                if (response != null)
                {
                    buffer = response.Buffer ?? Array.Empty<byte>(); // cache even if null, when no tile available

                    var maxAge = response.MaxAge ?? DefaultCacheExpiration;

                    if (maxAge > MaxCacheExpiration)
                    {
                        maxAge = MaxCacheExpiration;
                    }

                    var cacheOptions = new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = maxAge
                    };

                    await Cache.SetAsync(cacheKey, buffer, cacheOptions).ConfigureAwait(false);
                }
            }
            //else Debug.WriteLine($"Cached: {cacheKey} - {buffer.Length} bytes");

            if (buffer != null && buffer.Length > 0)
            {
                await LoadTileAsync(tile, () => ImageLoader.LoadImageAsync(buffer)).ConfigureAwait(false);
            }
        }
    }
}

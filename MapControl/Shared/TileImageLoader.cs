// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2017 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#if WINDOWS_UWP
using Windows.Web.Http;
#else
using System.Net.Http;
#endif

namespace MapControl
{
    /// <summary>
    /// Loads and optionally caches map tile images for a MapTileLayer.
    /// </summary>
    public partial class TileImageLoader : ITileImageLoader
    {
        /// <summary>
        /// The HttpClient instance used when image data is downloaded from a web resource.
        /// </summary>
        public static HttpClient HttpClient { get; set; } = new HttpClient();

        /// <summary>
        /// Default expiration time for cached tile images. Used when no expiration time
        /// was transmitted on download. The default value is one day.
        /// </summary>
        public static TimeSpan DefaultCacheExpiration { get; set; } = TimeSpan.FromDays(1);

        /// <summary>
        /// Minimum expiration time for cached tile images. The default value is one hour.
        /// </summary>
        public static TimeSpan MinimumCacheExpiration { get; set; } = TimeSpan.FromHours(1);

        /// <summary>
        /// Maximum expiration time for cached tile images. The default value is one week.
        /// </summary>
        public static TimeSpan MaximumCacheExpiration { get; set; } = TimeSpan.FromDays(7);

        /// <summary>
        /// Format string for creating cache keys from the SourceName property of a TileSource,
        /// the ZoomLevel, XIndex, and Y properties of a Tile, and the image file extension.
        /// The default value is "{0};{1};{2};{3}{4}".
        /// </summary>
        public static string CacheKeyFormat { get; set; } = "{0};{1};{2};{3}{4}";

        private const string bingMapsTileInfo = "X-VE-Tile-Info";
        private const string bingMapsNoTile = "no-tile";

        private readonly ConcurrentStack<Tile> pendingTiles = new ConcurrentStack<Tile>();
        private int taskCount;

        public void LoadTiles(MapTileLayer tileLayer)
        {
            pendingTiles.Clear();

            var tileSource = tileLayer.TileSource;
            var sourceName = tileLayer.SourceName;
            var tiles = tileLayer.Tiles.Where(t => t.Pending);

            if (tileSource != null && tiles.Any())
            {
                if (Cache == null || string.IsNullOrEmpty(sourceName) ||
                    tileSource.UriFormat == null || !tileSource.UriFormat.StartsWith("http"))
                {
                    // no caching, load tile images in UI thread

                    foreach (var tile in tiles)
                    {
                        LoadTileImage(tileSource, tile);
                    }
                }
                else
                {
                    pendingTiles.PushRange(tiles.Reverse().ToArray());

                    while (taskCount < Math.Min(pendingTiles.Count, tileLayer.MaxParallelDownloads))
                    {
                        Interlocked.Increment(ref taskCount);

                        var task = Task.Run(async () => // do not await
                        {
                            await LoadPendingTilesAsync(tileSource, sourceName); // run multiple times in parallel

                            Interlocked.Decrement(ref taskCount);
                        });
                    }
                }
            }
        }

        private void LoadTileImage(TileSource tileSource, Tile tile)
        {
            tile.Pending = false;

            try
            {
                var imageSource = tileSource.LoadImage(tile.XIndex, tile.Y, tile.ZoomLevel);

                if (imageSource != null)
                {
                    tile.SetImage(imageSource);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("TileImageLoader: {0}/{1}/{2}: {3}", tile.ZoomLevel, tile.XIndex, tile.Y, ex.Message);
            }
        }

        private async Task LoadPendingTilesAsync(TileSource tileSource, string sourceName)
        {
            Tile tile;

            while (pendingTiles.TryPop(out tile))
            {
                tile.Pending = false;

                try
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

                        await LoadTileImageAsync(tile, uri, cacheKey);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("TileImageLoader: {0}/{1}/{2}: {3}", tile.ZoomLevel, tile.XIndex, tile.Y, ex.Message);
                }
            }
        }

        private static DateTime GetExpiration(HttpResponseMessage response)
        {
            var expiration = DefaultCacheExpiration;
            var headers = response.Headers;

            if (headers.CacheControl != null && headers.CacheControl.MaxAge.HasValue)
            {
                expiration = headers.CacheControl.MaxAge.Value;

                if (expiration < MinimumCacheExpiration)
                {
                    expiration = MinimumCacheExpiration;
                }
                else if (expiration > MaximumCacheExpiration)
                {
                    expiration = MaximumCacheExpiration;
                }
            }

            return DateTime.UtcNow.Add(expiration);
        }
    }
}

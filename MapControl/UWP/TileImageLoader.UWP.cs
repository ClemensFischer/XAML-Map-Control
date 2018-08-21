// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;

namespace MapControl
{
    public partial class TileImageLoader
    {
        /// <summary>
        /// Default StorageFolder where an IImageCache instance may save cached data,
        /// i.e. ApplicationData.Current.TemporaryFolder.
        /// </summary>
        public static StorageFolder DefaultCacheFolder
        {
            get { return ApplicationData.Current.TemporaryFolder; }
        }

        /// <summary>
        /// The IImageCache implementation used to cache tile images. The default is null.
        /// </summary>
        public static Caching.IImageCache Cache { get; set; }

        private async Task LoadCachedTileImageAsync(Tile tile, Uri uri, string cacheKey)
        {
            var cacheItem = await Cache.GetAsync(cacheKey);
            var cacheBuffer = cacheItem?.Buffer;

            if (cacheBuffer == null || cacheItem.Expiration < DateTime.UtcNow)
            {
                var result = await ImageLoader.LoadHttpBufferAsync(uri);

                if (result != null) // download succeeded
                {
                    cacheBuffer = null; // discard cached image

                    if (result.Item1 != null) // tile image available
                    {
                        await LoadTileImageAsync(tile, result.Item1);
                        await Cache.SetAsync(cacheKey, result.Item1, GetExpiration(result.Item2));
                    }
                }
            }

            if (cacheBuffer != null)
            {
                await LoadTileImageAsync(tile, cacheBuffer);
            }
        }

        private async Task LoadTileImageAsync(Tile tile, IBuffer buffer)
        {
            var tcs = new TaskCompletionSource<object>();

            using (var stream = new InMemoryRandomAccessStream())
            {
                await stream.WriteAsync(buffer);
                stream.Seek(0);

                await tile.Image.Dispatcher.RunAsync(CoreDispatcherPriority.Low, async () =>
                {
                    try
                    {
                        tile.SetImage(await ImageLoader.LoadImageAsync(stream));
                        tcs.SetResult(null);
                    }
                    catch (Exception ex)
                    {
                        tcs.SetException(ex);
                    }
                });
            }

            await tcs.Task;
        }

        private async Task LoadTileImageAsync(Tile tile, TileSource tileSource)
        {
            var tcs = new TaskCompletionSource<object>();

            await tile.Image.Dispatcher.RunAsync(CoreDispatcherPriority.Low, async () =>
            {
                try
                {
                    tile.SetImage(await tileSource.LoadImageAsync(tile.XIndex, tile.Y, tile.ZoomLevel));
                    tcs.SetResult(null);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            await tcs.Task;
        }
    }
}

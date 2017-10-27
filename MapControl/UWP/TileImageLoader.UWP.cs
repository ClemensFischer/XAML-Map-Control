// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2017 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;

namespace MapControl
{
    public partial class TileImageLoader : ITileImageLoader
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

        /// <summary>
        /// Gets or sets the maximum number of concurrent connections. The default value is 2.
        /// </summary>
        public static int DefaultConnectionLimit { get; set; } = 2;

        private async Task LoadTileImageAsync(Tile tile, Uri uri, string cacheKey)
        {
            var cacheItem = await Cache.GetAsync(cacheKey);
            var cacheBuffer = cacheItem?.Buffer;
            var loaded = false;

            if (cacheBuffer == null || cacheItem.Expiration < DateTime.UtcNow)
            {
                try
                {
                    loaded = await ImageLoader.LoadHttpTileImageAsync(uri, async (buffer, maxAge) =>
                    {
                        await SetTileImageAsync(tile, buffer); // create BitmapImage before caching

                        await Cache.SetAsync(cacheKey, buffer, GetExpiration(maxAge));
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("TileImageLoader: {0}: {1}", uri, ex.Message);
                }
            }

            if (!loaded && cacheBuffer != null) // keep expired image if download failed
            {
                await SetTileImageAsync(tile, cacheBuffer);
            }
        }

        private async Task SetTileImageAsync(Tile tile, IBuffer buffer)
        {
            var tcs = new TaskCompletionSource<object>();

            using (var stream = new InMemoryRandomAccessStream())
            {
                await stream.WriteAsync(buffer);
                await stream.FlushAsync(); // necessary?
                stream.Seek(0);

                await tile.Image.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    try
                    {
                        var bitmapImage = new BitmapImage();
                        await bitmapImage.SetSourceAsync(stream);

                        tile.SetImage(bitmapImage);
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
    }
}

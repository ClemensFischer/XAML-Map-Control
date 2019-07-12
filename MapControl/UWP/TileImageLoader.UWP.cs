// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2019 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;

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


        private static async Task LoadCachedTileImageAsync(Tile tile, Uri uri, string cacheKey)
        {
            var cacheItem = await Cache.GetAsync(cacheKey).ConfigureAwait(false);
            var cacheBuffer = cacheItem?.Buffer;

            if (cacheBuffer == null || cacheItem.Expiration < DateTime.UtcNow)
            {
                using (var stream = await ImageLoader.LoadImageStreamAsync(uri).ConfigureAwait(false))
                {
                    if (stream != null) // download succeeded
                    {
                        cacheBuffer = null; // discard cached image

                        if (stream.Length > 0) // tile image available
                        {
                            await SetTileImageAsync(tile, () => ImageLoader.LoadImageAsync(stream)).ConfigureAwait(false);

                            await Cache.SetAsync(cacheKey, stream.ToArray().AsBuffer(), GetExpiration(stream.MaxAge)).ConfigureAwait(false);
                        }
                    }
                }
            }

            if (cacheBuffer != null) // cached image not expired or download failed
            {
                using (var stream = new InMemoryRandomAccessStream())
                {
                    await stream.WriteAsync(cacheBuffer);
                    stream.Seek(0);

                    await SetTileImageAsync(tile, () => ImageLoader.LoadImageAsync(stream)).ConfigureAwait(false);
                }
            }
        }

        private static Task LoadTileImageAsync(Tile tile, TileSource tileSource)
        {
            return SetTileImageAsync(tile, () => tileSource.LoadImageAsync(tile.XIndex, tile.Y, tile.ZoomLevel));
        }

        private static async Task SetTileImageAsync(Tile tile, Func<Task<ImageSource>> loadImageFunc)
        {
            var tcs = new TaskCompletionSource<object>();

            await tile.Image.Dispatcher.RunAsync(CoreDispatcherPriority.Low, async () =>
            {
                try
                {
                    var image = await loadImageFunc();

                    if (image != null)
                    {
                        tile.SetImage(image);
                    }

                    tcs.SetResult(null);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            await tcs.Task.ConfigureAwait(false);
        }
    }
}

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
using Windows.Web.Http;

namespace MapControl
{
    public partial class TileImageLoader : ITileImageLoader
    {
        /// <summary>
        /// Default StorageFolder where an IImageCache instance may save cached data.
        /// </summary>
        public static readonly StorageFolder DefaultCacheFolder = ApplicationData.Current.TemporaryFolder;

        /// <summary>
        /// The IImageCache implementation used to cache tile images. The default is null.
        /// </summary>
        public static Caching.IImageCache Cache { get; set; }

        private async Task LoadTileImageAsync(Tile tile, Uri uri, string cacheKey)
        {
            var cacheItem = await Cache.GetAsync(cacheKey);
            var buffer = cacheItem?.Buffer;
            var loaded = false;

            //if (buffer != null)
            //{
            //    Debug.WriteLine("TileImageLoader: {0}: expire{1} {2}", cacheKey, cacheItem.Expiration < DateTime.UtcNow ? "d" : "s", cacheItem.Expiration);
            //}

            if (buffer == null || cacheItem.Expiration < DateTime.UtcNow)
            {
                loaded = await DownloadTileImageAsync(tile, uri, cacheKey);
            }

            if (!loaded && buffer != null) // keep expired image if download failed
            {
                await SetTileImageAsync(tile, buffer);
            }
        }

        private async Task<bool> DownloadTileImageAsync(Tile tile, Uri uri, string cacheKey)
        {
            try
            {
                using (var response = await HttpClient.GetAsync(uri))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string tileInfo;

                        if (!response.Headers.TryGetValue(bingMapsTileInfo, out tileInfo) ||
                            tileInfo != bingMapsNoTile)
                        {
                            var buffer = await response.Content.ReadAsBufferAsync();

                            await SetTileImageAsync(tile, buffer); // create BitmapImage in UI thread before caching

                            await Cache.SetAsync(cacheKey, buffer, GetExpiration(response));
                        }

                        return true;
                    }

                    Debug.WriteLine("TileImageLoader: {0}: {1} {2}", uri, (int)response.StatusCode, response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("TileImageLoader: {0}: {1}", uri, ex.Message);
            }

            return false;
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

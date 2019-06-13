﻿// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2019 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
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

        private async Task LoadCachedTileImageAsync(Tile tile, Uri uri, string cacheKey)
        {
            var cacheItem = await Cache.GetAsync(cacheKey).ConfigureAwait(false);
            var cacheBuffer = cacheItem?.Buffer;

            if (cacheBuffer == null || cacheItem.Expiration < DateTime.UtcNow)
            {
                var response = await ImageLoader.LoadHttpBufferAsync(uri).ConfigureAwait(false);

                if (response != null) // download succeeded
                {
                    cacheBuffer = null; // discard cached image

                    if (response.Buffer != null) // tile image available
                    {
                        await LoadTileImageAsync(tile, response.Buffer).ConfigureAwait(false);
                        await Cache.SetAsync(cacheKey, response.Buffer, GetExpiration(response.MaxAge)).ConfigureAwait(false);
                    }
                }
            }

            if (cacheBuffer != null) // cached image not expired or download failed
            {
                await LoadTileImageAsync(tile, cacheBuffer).ConfigureAwait(false);
            }
        }

        private async Task LoadTileImageAsync(Tile tile, IBuffer buffer)
        {
            using (var stream = new InMemoryRandomAccessStream())
            {
                await stream.WriteAsync(buffer);
                stream.Seek(0);

                await SetTileImageAsync(tile, () => ImageLoader.LoadImageAsync(stream)).ConfigureAwait(false);
            }
        }

        private Task LoadTileImageAsync(Tile tile, TileSource tileSource)
        {
            return SetTileImageAsync(tile, () => tileSource.LoadImageAsync(tile.XIndex, tile.Y, tile.ZoomLevel));
        }

        private async Task SetTileImageAsync(Tile tile, Func<Task<ImageSource>> loadImageFunc)
        {
            var tcs = new TaskCompletionSource<object>();

            await tile.Image.Dispatcher.RunAsync(CoreDispatcherPriority.Low, async () =>
            {
                try
                {
                    tile.SetImage(await loadImageFunc());
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

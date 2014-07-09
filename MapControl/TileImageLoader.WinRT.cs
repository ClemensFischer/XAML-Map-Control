// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2014 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace MapControl
{
    /// <summary>
    /// Loads map tile images.
    /// </summary>
    public class TileImageLoader
    {
        public static IObjectCache Cache { get; set; }

        private HttpClient httpClient;

        internal void BeginGetTiles(TileLayer tileLayer, IEnumerable<Tile> tiles)
        {
            var imageTileSource = tileLayer.TileSource as ImageTileSource;

            foreach (var tile in tiles)
            {
                try
                {
                    ImageSource image = null;

                    if (imageTileSource != null)
                    {
                        image = imageTileSource.LoadImage(tile.XIndex, tile.Y, tile.ZoomLevel);
                    }
                    else
                    {
                        var uri = tileLayer.TileSource.GetUri(tile.XIndex, tile.Y, tile.ZoomLevel);

                        if (uri != null)
                        {
                            if (Cache == null || string.IsNullOrEmpty(tileLayer.SourceName))
                            {
                                image = new BitmapImage(uri);
                            }
                            else
                            {
                                var bitmap = new BitmapImage();
                                image = bitmap;

                                Task.Run(async () => await LoadCachedImage(tileLayer, tile, uri, bitmap));
                            }
                        }
                    }

                    tile.SetImageSource(image, tileLayer.AnimateTileOpacity);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Loading tile image failed: {0}", ex.Message);
                }
            }
        }

        internal void CancelGetTiles()
        {
        }

        private async Task LoadCachedImage(TileLayer tileLayer, Tile tile, Uri uri, BitmapImage bitmap)
        {
            var cacheKey = string.Format(@"{0}\{1}\{2}\{3}{4}",
                tileLayer.SourceName, tile.ZoomLevel, tile.XIndex, tile.Y, Path.GetExtension(uri.LocalPath));

            var buffer = await Cache.GetAsync(cacheKey) as IBuffer;

            if (buffer != null)
            {
                await LoadImageFromBuffer(buffer, bitmap);
                //Debug.WriteLine("Loaded cached image {0}", cacheKey);
            }
            else
            {
                DownloadAndCacheImage(uri, bitmap, cacheKey);
            }
        }

        private async Task LoadImageFromBuffer(IBuffer buffer, BitmapImage bitmap)
        {
            using (var stream = new InMemoryRandomAccessStream())
            {
                await stream.WriteAsync(buffer);
                await stream.FlushAsync();
                stream.Seek(0);

                await bitmap.Dispatcher.RunAsync(CoreDispatcherPriority.Low, async () =>
                {
                    try
                    {
                        await bitmap.SetSourceAsync(stream);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                });
            }
        }

        private void DownloadAndCacheImage(Uri uri, BitmapImage bitmap, string cacheKey)
        {
            try
            {
                if (httpClient == null)
                {
                    var filter = new HttpBaseProtocolFilter();
                    filter.AllowAutoRedirect = false;
                    filter.CacheControl.ReadBehavior = HttpCacheReadBehavior.Default;
                    filter.CacheControl.WriteBehavior = HttpCacheWriteBehavior.NoCache;

                    httpClient = new HttpClient(filter);
                }

                httpClient.GetAsync(uri).Completed = async (request, status) =>
                {
                    if (status == AsyncStatus.Completed)
                    {
                        using (var response = request.GetResults())
                        {
                            await LoadImageFromHttpResponse(response, bitmap, cacheKey);
                        }
                    }
                    else
                    {
                        Debug.WriteLine("{0}: {1}", uri, request.ErrorCode != null ? request.ErrorCode.Message : status.ToString());
                    }
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine("{0}: {1}", uri, ex.Message);
            }
        }

        private async Task LoadImageFromHttpResponse(HttpResponseMessage response, BitmapImage bitmap, string cacheKey)
        {
            if (response.IsSuccessStatusCode)
            {
                var stream = new InMemoryRandomAccessStream();

                using (var content = response.Content)
                {
                    await content.WriteToStreamAsync(stream);
                }

                await stream.FlushAsync();
                stream.Seek(0);

                await bitmap.Dispatcher.RunAsync(CoreDispatcherPriority.Low, async () =>
                {
                    try
                    {
                        await bitmap.SetSourceAsync(stream);

                        // cache image asynchronously, after successful decoding
                        var task = Task.Run(async () =>
                        {
                            var buffer = new Windows.Storage.Streams.Buffer((uint)stream.Size);

                            stream.Seek(0);
                            await stream.ReadAsync(buffer, buffer.Capacity, InputStreamOptions.None);
                            stream.Dispose();

                            await Cache.SetAsync(cacheKey, buffer);
                        });
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("{0}: {1}", response.RequestMessage.RequestUri, ex.Message);
                        stream.Dispose();
                    }
                });
            }
            else
            {
                Debug.WriteLine("{0}: {1}", response.RequestMessage.RequestUri, response.StatusCode);
            }
        }
    }
}

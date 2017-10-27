// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2017 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Http;
using Windows.Web.Http.Headers;

namespace MapControl
{
    public static class ImageLoader
    {
        /// <summary>
        /// The HttpClient instance used when image data is downloaded from a web resource.
        /// </summary>
        public static HttpClient HttpClient { get; set; } = new HttpClient();

        public static async Task<ImageSource> LoadImageAsync(Uri uri, bool isTileImage)
        {
            if (!uri.IsAbsoluteUri || uri.Scheme == "file")
            {
                return await LoadLocalImageAsync(uri);
            }

            if (uri.Scheme == "http")
            {
                return await LoadHttpImageAsync(uri, isTileImage);
            }

            return new BitmapImage(uri);
        }

        public static async Task<ImageSource> LoadLocalImageAsync(Uri uri)
        {
            var path = uri.IsAbsoluteUri ? uri.LocalPath : uri.OriginalString;

            if (!await Task.Run(() => File.Exists(path)))
            {
                return null;
            }

            var file = await StorageFile.GetFileFromPathAsync(path);

            using (var stream = await file.OpenReadAsync())
            {
                var bitmapImage = new BitmapImage();
                await bitmapImage.SetSourceAsync(stream);

                return bitmapImage;
            }
        }

        public static async Task<ImageSource> LoadHttpImageAsync(Uri uri, bool isTileImage)
        {
            using (var response = await HttpClient.GetAsync(uri))
            {
                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine("ImageLoader: {0}: {1} {2}", uri, (int)response.StatusCode, response.ReasonPhrase);
                }
                else if (!isTileImage || IsTileAvailable(response.Headers))
                {
                    using (var stream = new InMemoryRandomAccessStream())
                    {
                        await response.Content.WriteToStreamAsync(stream);
                        stream.Seek(0);

                        var bitmapImage = new BitmapImage();
                        await bitmapImage.SetSourceAsync(stream);

                        return bitmapImage;
                    }
                }

                return null;
            }
        }

        public static async Task<bool> LoadHttpTileImageAsync(Uri uri, Func<IBuffer, TimeSpan?, Task> tileCallback)
        {
            using (var response = await HttpClient.GetAsync(uri))
            {
                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine("ImageLoader: {0}: {1} {2}", uri, (int)response.StatusCode, response.ReasonPhrase);
                }
                else if (IsTileAvailable(response.Headers))
                {
                    var buffer = await response.Content.ReadAsBufferAsync();

                    await tileCallback(buffer, response.Headers.CacheControl?.MaxAge);
                }

                return response.IsSuccessStatusCode;
            }
        }

        private static bool IsTileAvailable(HttpResponseHeaderCollection responseHeaders)
        {
            return !responseHeaders.TryGetValue("X-VE-Tile-Info", out string tileInfo) || tileInfo != "no-tile";
        }
    }
}

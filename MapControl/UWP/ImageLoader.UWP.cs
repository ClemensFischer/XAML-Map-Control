// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Http;
using Windows.Web.Http.Headers;

namespace MapControl
{
    public static partial class ImageLoader
    {
        public static async Task<BitmapSource> LoadImageAsync(IRandomAccessStream stream)
        {
            var image = new BitmapImage();
            await image.SetSourceAsync(stream);
            return image;
        }

        public static async Task<BitmapSource> LoadImageAsync(byte[] buffer)
        {
            using (var stream = new InMemoryRandomAccessStream())
            {
                await stream.WriteAsync(buffer.AsBuffer());
                stream.Seek(0);
                return await LoadImageAsync(stream);
            }
        }

        private static async Task<BitmapSource> LoadImageAsync(IHttpContent content)
        {
            using (var stream = new InMemoryRandomAccessStream())
            {
                await content.WriteToStreamAsync(stream);
                stream.Seek(0);
                return await LoadImageAsync(stream);
            }
        }

        private static async Task<BitmapSource> LoadLocalImageAsync(Uri uri)
        {
            BitmapSource image = null;
            var path = uri.IsAbsoluteUri ? uri.LocalPath : uri.OriginalString;

            if (File.Exists(path))
            {
                var file = await StorageFile.GetFileFromPathAsync(path);

                using (var stream = await file.OpenReadAsync())
                {
                    image = await LoadImageAsync(stream);
                }
            }

            return image;
        }

        internal static async Task<Tuple<IBuffer, TimeSpan?>> LoadHttpBufferAsync(Uri uri)
        {
            Tuple<IBuffer, TimeSpan?> result = null;

            try
            {
                using (var response = await HttpClient.GetAsync(uri))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        Debug.WriteLine("ImageLoader: {0}: {1} {2}", uri, (int)response.StatusCode, response.ReasonPhrase);
                    }
                    else
                    {
                        IBuffer buffer = null;
                        TimeSpan? maxAge = null;

                        if (IsTileAvailable(response.Headers))
                        {
                            buffer = await response.Content.ReadAsBufferAsync();
                            maxAge = response.Headers.CacheControl?.MaxAge;
                        }

                        result = new Tuple<IBuffer, TimeSpan?>(buffer, maxAge);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ImageLoader: {0}: {1}", uri, ex.Message);
            }

            return result;
        }

        private static bool IsTileAvailable(HttpResponseHeaderCollection responseHeaders)
        {
            return !responseHeaders.TryGetValue("X-VE-Tile-Info", out string tileInfo) || tileInfo != "no-tile";
        }
    }
}

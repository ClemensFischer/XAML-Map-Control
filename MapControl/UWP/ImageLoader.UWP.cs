// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2019 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Http;
using Windows.Web.Http.Headers;

namespace MapControl
{
    public static partial class ImageLoader
    {
        public static async Task<ImageSource> LoadImageAsync(IRandomAccessStream stream)
        {
            var image = new BitmapImage();
            await image.SetSourceAsync(stream);
            return image;
        }

        public static async Task<ImageSource> LoadImageAsync(byte[] buffer)
        {
            using (var stream = new InMemoryRandomAccessStream())
            {
                await stream.WriteAsync(buffer.AsBuffer());
                stream.Seek(0);

                return await LoadImageAsync(stream);
            }
        }

        public static async Task<ImageSource> LoadImageAsync(string path)
        {
            ImageSource image = null;

            if (File.Exists(path))
            {
                var file = await StorageFile.GetFileFromPathAsync(Path.GetFullPath(path));

                using (var stream = await file.OpenReadAsync())
                {
                    image = await LoadImageAsync(stream);
                }
            }

            return image;
        }

        private static async Task<ImageSource> LoadImageAsync(IHttpContent content)
        {
            using (var stream = new InMemoryRandomAccessStream())
            {
                await content.WriteToStreamAsync(stream);
                stream.Seek(0);

                return await LoadImageAsync(stream);
            }
        }

        internal class HttpBufferResponse
        {
            public readonly IBuffer Buffer;
            public readonly TimeSpan? MaxAge;

            public HttpBufferResponse(IBuffer buffer, TimeSpan? maxAge)
            {
                Buffer = buffer;
                MaxAge = maxAge;
            }
        }

        internal static async Task<HttpBufferResponse> LoadHttpBufferAsync(Uri uri)
        {
            HttpBufferResponse response = null;

            try
            {
                using (var responseMessage = await HttpClient.GetAsync(uri))
                {
                    if (responseMessage.IsSuccessStatusCode)
                    {
                        IBuffer buffer = null;
                        TimeSpan? maxAge = null;

                        if (ImageAvailable(responseMessage.Headers))
                        {
                            buffer = await responseMessage.Content.ReadAsBufferAsync();
                            maxAge = responseMessage.Headers.CacheControl?.MaxAge;
                        }

                        response = new HttpBufferResponse(buffer, maxAge);
                    }
                    else
                    {
                        Debug.WriteLine("ImageLoader: {0}: {1} {2}", uri, (int)responseMessage.StatusCode, responseMessage.ReasonPhrase);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ImageLoader: {0}: {1}", uri, ex.Message);
            }

            return response;
        }

        private static bool ImageAvailable(HttpResponseHeaderCollection responseHeaders)
        {
            return !responseHeaders.TryGetValue("X-VE-Tile-Info", out string tileInfo) || tileInfo != "no-tile";
        }
    }
}

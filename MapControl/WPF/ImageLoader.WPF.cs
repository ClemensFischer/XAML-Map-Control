// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2019 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MapControl
{
    public static partial class ImageLoader
    {
        public static ImageSource LoadImage(Stream stream)
        {
            var bitmapImage = new BitmapImage();

            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = stream;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
        }

        public static ImageSource LoadImage(byte[] buffer)
        {
            using (var stream = new MemoryStream(buffer))
            {
                return LoadImage(stream);
            }
        }

        public static ImageSource LoadImage(string path)
        {
            ImageSource image = null;

            if (File.Exists(path))
            {
                using (var stream = File.OpenRead(path))
                {
                    image = LoadImage(stream);
                }
            }

            return image;
        }

        public static Task<ImageSource> LoadImageAsync(Stream stream)
        {
            return Task.Run(() => LoadImage(stream));
        }

        public static Task<ImageSource> LoadImageAsync(byte[] buffer)
        {
            return Task.Run(() => LoadImage(buffer));
        }

        public static Task<ImageSource> LoadImageAsync(string path)
        {
            return Task.Run(() => LoadImage(path));
        }

        private static async Task<ImageSource> LoadImageAsync(HttpContent content)
        {
            using (var stream = new MemoryStream())
            {
                await content.CopyToAsync(stream);
                stream.Seek(0, SeekOrigin.Begin);

                return LoadImage(stream);
            }
        }

        internal class HttpStreamResponse
        {
            public readonly MemoryStream Stream;
            public readonly TimeSpan? MaxAge;

            public HttpStreamResponse(MemoryStream stream, TimeSpan? maxAge)
            {
                Stream = stream;
                MaxAge = maxAge;
            }
        }

        internal static async Task<HttpStreamResponse> LoadHttpStreamAsync(Uri uri)
        {
            HttpStreamResponse response = null;

            try
            {
                using (var responseMessage = await HttpClient.GetAsync(uri).ConfigureAwait(false))
                {
                    if (responseMessage.IsSuccessStatusCode)
                    {
                        MemoryStream stream = null;
                        TimeSpan? maxAge = null;

                        if (ImageAvailable(responseMessage.Headers))
                        {
                            stream = new MemoryStream();
                            await responseMessage.Content.CopyToAsync(stream).ConfigureAwait(false);
                            stream.Seek(0, SeekOrigin.Begin);

                            maxAge = responseMessage.Headers.CacheControl?.MaxAge;
                        }

                        response = new HttpStreamResponse(stream, maxAge);
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

        private static bool ImageAvailable(HttpResponseHeaders responseHeaders)
        {
            IEnumerable<string> tileInfo;
            return !responseHeaders.TryGetValues("X-VE-Tile-Info", out tileInfo) || !tileInfo.Contains("no-tile");
        }
    }
}

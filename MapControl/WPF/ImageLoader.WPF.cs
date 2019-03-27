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
using System.Windows.Media.Imaging;

namespace MapControl
{
    public static partial class ImageLoader
    {
        public static BitmapSource LoadImage(Stream stream)
        {
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = stream;
            bitmapImage.EndInit();
            bitmapImage.Freeze();
            return bitmapImage;
        }

        public static Task<BitmapSource> LoadImageAsync(Stream stream)
        {
            return Task.Run(() => LoadImage(stream));
        }

        public static BitmapSource LoadImage(byte[] buffer)
        {
            using (var stream = new MemoryStream(buffer))
            {
                return LoadImage(stream);
            }
        }

        public static Task<BitmapSource> LoadImageAsync(byte[] buffer)
        {
            return Task.Run(() => LoadImage(buffer));
        }

        private static async Task<BitmapSource> LoadImageAsync(HttpContent content)
        {
            using (var stream = new MemoryStream())
            {
                await content.CopyToAsync(stream);
                stream.Seek(0, SeekOrigin.Begin);
                return await LoadImageAsync(stream);
            }
        }

        private static BitmapSource LoadLocalImage(Uri uri)
        {
            BitmapSource image = null;
            var path = uri.IsAbsoluteUri ? uri.LocalPath : uri.OriginalString;

            if (File.Exists(path))
            {
                using (var stream = File.OpenRead(path))
                {
                    image = LoadImage(stream);
                }
            }

            return image;
        }

        private static Task<BitmapSource> LoadLocalImageAsync(Uri uri)
        {
            return Task.Run(() => LoadLocalImage(uri));
        }

        internal static async Task<Tuple<MemoryStream, TimeSpan?>> LoadHttpStreamAsync(Uri uri)
        {
            Tuple<MemoryStream, TimeSpan?> result = null;

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
                        MemoryStream stream = null;
                        TimeSpan? maxAge = null;

                        if (IsTileAvailable(response.Headers))
                        {
                            stream = new MemoryStream();
                            await response.Content.CopyToAsync(stream);
                            stream.Seek(0, SeekOrigin.Begin);
                            maxAge = response.Headers.CacheControl?.MaxAge;
                        }

                        result = new Tuple<MemoryStream, TimeSpan?>(stream, maxAge);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ImageLoader: {0}: {1}", uri, ex.Message);
            }

            return result;
        }

        private static bool IsTileAvailable(HttpResponseHeaders responseHeaders)
        {
            IEnumerable<string> tileInfo;
            return !responseHeaders.TryGetValues("X-VE-Tile-Info", out tileInfo) || !tileInfo.Contains("no-tile");
        }
    }
}

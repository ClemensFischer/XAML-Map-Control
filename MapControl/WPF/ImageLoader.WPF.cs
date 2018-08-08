// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
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
        public static Task<ImageSource> LoadLocalImageAsync(Uri uri)
        {
            return Task.Run(() =>
            {
                ImageSource imageSource = null;
                var path = uri.IsAbsoluteUri ? uri.LocalPath : uri.OriginalString;

                if (File.Exists(path))
                {
                    using (var stream = File.OpenRead(path))
                    {
                        imageSource = CreateImageSource(stream);
                    }
                }

                return imageSource;
            });
        }

        public static async Task<bool> LoadHttpTileImageAsync(Uri uri, Func<MemoryStream, TimeSpan?, Task> tileCallback)
        {
            using (var response = await HttpClient.GetAsync(uri))
            {
                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine("ImageLoader: {0}: {1} {2}", uri, (int)response.StatusCode, response.ReasonPhrase);
                }
                else if (IsTileAvailable(response.Headers))
                {
                    using (var stream = new MemoryStream())
                    {
                        await response.Content.CopyToAsync(stream);
                        stream.Seek(0, SeekOrigin.Begin);

                        await tileCallback(stream, response.Headers.CacheControl?.MaxAge);
                    }
                }

                return response.IsSuccessStatusCode;
            }
        }

        public static ImageSource CreateImageSource(Stream stream)
        {
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = stream;
            bitmapImage.EndInit();
            bitmapImage.Freeze();
            return bitmapImage;
        }

        public static Task<ImageSource> CreateImageSourceAsync(Stream stream)
        {
            return Task.Run(() => CreateImageSource(stream));
        }

        private static async Task<Stream> GetResponseStreamAsync(HttpContent content)
        {
            var stream = new MemoryStream();
            await content.CopyToAsync(stream);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        private static bool IsTileAvailable(HttpResponseHeaders responseHeaders)
        {
            IEnumerable<string> tileInfo;
            return !responseHeaders.TryGetValues("X-VE-Tile-Info", out tileInfo) || !tileInfo.Contains("no-tile");
        }
    }
}

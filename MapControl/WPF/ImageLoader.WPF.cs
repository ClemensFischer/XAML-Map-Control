// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2017 Clemens Fischer
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

        public static Task<ImageSource> LoadLocalImageAsync(Uri uri)
        {
            return Task.Run(() =>
            {
                var path = uri.IsAbsoluteUri ? uri.LocalPath : uri.OriginalString;

                if (!File.Exists(path))
                {
                    return null;
                }

                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    return (ImageSource)BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                }
            });
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
                    using (var stream = new MemoryStream())
                    {
                        await response.Content.CopyToAsync(stream);
                        stream.Seek(0, SeekOrigin.Begin);

                        return BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    }
                }

                return null;
            }
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
                    var stream = new MemoryStream();

                    await response.Content.CopyToAsync(stream);
                    stream.Seek(0, SeekOrigin.Begin);

                    await tileCallback(stream, response.Headers.CacheControl?.MaxAge);
                }

                return response.IsSuccessStatusCode;
            }
        }

        private static bool IsTileAvailable(HttpResponseHeaders responseHeaders)
        {
            IEnumerable<string> tileInfo;

            return !responseHeaders.TryGetValues("X-VE-Tile-Info", out tileInfo) || !tileInfo.Contains("no-tile");
        }
    }
}

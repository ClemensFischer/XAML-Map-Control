// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
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
    public static partial class ImageLoader
    {
        public static async Task<ImageSource> LoadLocalImageAsync(Uri uri)
        {
            ImageSource imageSource = null;
            var path = uri.IsAbsoluteUri ? uri.LocalPath : uri.OriginalString;

            if (File.Exists(path))
            {
                var file = await StorageFile.GetFileFromPathAsync(path);

                using (var stream = await file.OpenReadAsync())
                {
                    imageSource = await CreateImageSourceAsync(stream);
                }
            }

            return imageSource;
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

        public static async Task<ImageSource> CreateImageSourceAsync(IRandomAccessStream stream)
        {
            var bitmapImage = new BitmapImage();
            await bitmapImage.SetSourceAsync(stream);
            return bitmapImage;
        }

        private static async Task<InMemoryRandomAccessStream> GetResponseStreamAsync(IHttpContent content)
        {
            var stream = new InMemoryRandomAccessStream();
            await content.WriteToStreamAsync(stream);
            stream.Seek(0);
            return stream;
        }

        private static bool IsTileAvailable(HttpResponseHeaderCollection responseHeaders)
        {
            return !responseHeaders.TryGetValue("X-VE-Tile-Info", out string tileInfo) || tileInfo != "no-tile";
        }
    }
}

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

            try
            {
                var path = uri.IsAbsoluteUri ? uri.LocalPath : uri.OriginalString;

                if (File.Exists(path))
                {
                    var file = await StorageFile.GetFileFromPathAsync(path);

                    using (var stream = await file.OpenReadAsync())
                    {
                        imageSource = await CreateImageSourceAsync(stream);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ImageLoader: {0}: {1}", uri, ex.Message);
            }

            return imageSource;
        }

        public static async Task<Tuple<IBuffer, TimeSpan?>> LoadHttpBufferAsync(Uri uri)
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

        public static async Task<ImageSource> CreateImageSourceAsync(IRandomAccessStream stream)
        {
            var bitmapImage = new BitmapImage();
            await bitmapImage.SetSourceAsync(stream);
            return bitmapImage;
        }

        private static async Task<ImageSource> CreateImageSourceAsync(IHttpContent content)
        {
            using (var stream = new InMemoryRandomAccessStream())
            {
                await content.WriteToStreamAsync(stream);
                stream.Seek(0);
                return await CreateImageSourceAsync(stream);
            }
        }

        private static bool IsTileAvailable(HttpResponseHeaderCollection responseHeaders)
        {
            return !responseHeaders.TryGetValue("X-VE-Tile-Info", out string tileInfo) || tileInfo != "no-tile";
        }
    }
}

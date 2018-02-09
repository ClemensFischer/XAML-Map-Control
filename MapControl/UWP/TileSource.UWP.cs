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
    public partial class TileSource
    {
        /// <summary>
        /// The HttpClient instance used when image data is downloaded from a web resource.
        /// </summary>
        public static HttpClient HttpClient { get; set; } = new HttpClient();

        /// <summary>
        /// Check HTTP response headers for tile availability, e.g. X-VE-Tile-Info=no-tile
        /// </summary>
        public static bool IsTileAvailable(HttpResponseHeaderCollection responseHeaders)
        {
            string tileInfo;

            return !responseHeaders.TryGetValue("X-VE-Tile-Info", out tileInfo) || tileInfo != "no-tile";
        }

        protected static async Task<ImageSource> LoadLocalImageAsync(Uri uri)
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

        protected static async Task<ImageSource> LoadHttpImageAsync(Uri uri)
        {
            using (var response = await HttpClient.GetAsync(uri))
            {
                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine("TileSource: {0}: {1} {2}", uri, (int)response.StatusCode, response.ReasonPhrase);
                }
                else if (IsTileAvailable(response.Headers))
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
            }

            return null;
        }
    }
}

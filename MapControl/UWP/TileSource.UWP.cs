// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2017 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Diagnostics;
using System.Threading.Tasks;
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
        /// Check HTTP response headers for tile unavailability, e.g. X-VE-Tile-Info=no-tile
        /// </summary>
        public static bool TileAvailable(HttpResponseHeaderCollection responseHeaders)
        {
            string tileInfo;

            return !responseHeaders.TryGetValue("X-VE-Tile-Info", out tileInfo) || tileInfo != "no-tile";
        }

        /// <summary>
        /// Load a tile ImageSource asynchronously from GetUri(x, y, zoomLevel)
        /// </summary>
        public virtual async Task<ImageSource> LoadImageAsync(int x, int y, int zoomLevel)
        {
            ImageSource imageSource = null;

            var uri = GetUri(x, y, zoomLevel);

            if (uri != null)
            {
                try
                {
                    if (uri.Scheme == "http")
                    {
                        using (var response = await HttpClient.GetAsync(uri))
                        {
                            if (!response.IsSuccessStatusCode)
                            {
                                Debug.WriteLine("TileSource: {0}: {1} {2}", uri, (int)response.StatusCode, response.ReasonPhrase);
                            }
                            else if (TileAvailable(response.Headers))
                            {
                                using (var stream = new InMemoryRandomAccessStream())
                                {
                                    await response.Content.WriteToStreamAsync(stream);
                                    stream.Seek(0);

                                    var bitmapImage = new BitmapImage();
                                    await bitmapImage.SetSourceAsync(stream);

                                    imageSource = bitmapImage;
                                }
                            }
                        }
                    }
                    else
                    {
                        imageSource = new BitmapImage(uri);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("TileSource: {0}: {1}", uri, ex.Message);
                }
            }

            return imageSource;
        }
    }
}

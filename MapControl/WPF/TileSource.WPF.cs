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
    public partial class TileSource
    {
        /// <summary>
        /// The HttpClient instance used when image data is downloaded from a web resource.
        /// </summary>
        public static HttpClient HttpClient { get; set; } = new HttpClient();

        /// <summary>
        /// Check HTTP response headers for tile unavailability, e.g. X-VE-Tile-Info=no-tile
        /// </summary>
        public static bool TileAvailable(HttpResponseHeaders responseHeaders)
        {
            IEnumerable<string> tileInfo;

            return !responseHeaders.TryGetValues("X-VE-Tile-Info", out tileInfo) || !tileInfo.Contains("no-tile");
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
                                using (var stream = new MemoryStream())
                                {
                                    await response.Content.CopyToAsync(stream);
                                    stream.Seek(0, SeekOrigin.Begin);

                                    imageSource = await Task.Run(() => BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad));
                                }
                            }
                        }
                    }
                    else
                    {
                        imageSource = BitmapFrame.Create(uri, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
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

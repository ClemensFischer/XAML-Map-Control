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
        /// Check HTTP response headers for tile availability, e.g. X-VE-Tile-Info=no-tile
        /// </summary>
        public static bool TileAvailable(HttpResponseHeaders responseHeaders)
        {
            IEnumerable<string> tileInfo;

            return !responseHeaders.TryGetValues("X-VE-Tile-Info", out tileInfo) || !tileInfo.Contains("no-tile");
        }

        protected Task<ImageSource> LoadLocalImageAsync(Uri uri)
        {
            return Task.Run(() =>
            {
                var path = uri.IsAbsoluteUri ? uri.LocalPath : uri.OriginalString;

                if (File.Exists(path))
                {
                    using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                    {
                        return (ImageSource)BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    }
                }

                return null;
            });
        }

        protected async Task<ImageSource> LoadHttpImageAsync(Uri uri)
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

                        return await Task.Run(() => BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad));
                    }
                }
            }

            return null;
        }
    }
}

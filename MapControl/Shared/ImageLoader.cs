// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2019 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
#if WINDOWS_UWP
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
#else
using System.Windows.Media;
using System.Windows.Media.Imaging;
#endif

namespace MapControl
{
    public static partial class ImageLoader
    {
        /// <summary>
        /// The HttpClient instance used when image data is downloaded from a web resource.
        /// </summary>
        public static HttpClient HttpClient { get; set; } = new HttpClient();

        public static async Task<ImageSource> LoadImageAsync(Uri uri)
        {
            ImageSource image = null;

            try
            {
                if (!uri.IsAbsoluteUri || uri.Scheme == "file")
                {
                    image = await LoadImageAsync(uri.IsAbsoluteUri ? uri.LocalPath : uri.OriginalString);
                }
                else if (uri.Scheme == "http" || uri.Scheme == "https")
                {
                    using (var response = await HttpClient.GetAsync(uri))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            if (ImageAvailable(response.Headers))
                            {
                                using (var stream = new MemoryStream())
                                {
                                    await response.Content.CopyToAsync(stream);
                                    stream.Seek(0, SeekOrigin.Begin);

                                    image = await LoadImageAsync(stream);
                                }
                            }
                        }
                        else
                        {
                            Debug.WriteLine("ImageLoader: {0}: {1} {2}", uri, (int)response.StatusCode, response.ReasonPhrase);
                        }
                    }
                }
                else
                {
                    image = new BitmapImage(uri);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ImageLoader: {0}: {1}", uri, ex.Message);
            }

            return image;
        }

        internal class ImageStream : MemoryStream
        {
            public TimeSpan? MaxAge { get; set; }
        }

        internal static async Task<ImageStream> LoadImageStreamAsync(Uri uri)
        {
            ImageStream stream = null;

            try
            {
                using (var response = await HttpClient.GetAsync(uri).ConfigureAwait(false))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        stream = new ImageStream();

                        if (ImageAvailable(response.Headers))
                        {
                            await response.Content.CopyToAsync(stream).ConfigureAwait(false);
                            stream.Seek(0, SeekOrigin.Begin);

                            stream.MaxAge = response.Headers.CacheControl?.MaxAge;
                        }
                    }
                    else
                    {
                        Debug.WriteLine("ImageLoader: {0}: {1} {2}", uri, (int)response.StatusCode, response.ReasonPhrase);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ImageLoader: {0}: {1}", uri, ex.Message);
            }

            return stream;
        }

        private static bool ImageAvailable(HttpResponseHeaders responseHeaders)
        {
            IEnumerable<string> tileInfo;

            return !responseHeaders.TryGetValues("X-VE-Tile-Info", out tileInfo) || !tileInfo.Contains("no-tile");
        }
    }
}
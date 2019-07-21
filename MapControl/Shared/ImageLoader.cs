// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2019 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
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
        /// The System.Net.Http.HttpClient instance used to download images via a http or https Uri.
        /// </summary>
        public static HttpClient HttpClient { get; set; } = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };


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
                    var response = await GetHttpResponseAsync(uri);

                    if (response != null && response.Buffer != null)
                    {
                        image = await LoadImageAsync(response.Buffer);
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

        internal static async Task<HttpResponse> GetHttpResponseAsync(Uri uri, bool continueOnCapturedContext = true)
        {
            HttpResponse response = null;

            try
            {
                using (var responseMessage = await HttpClient
                    .GetAsync(uri, HttpCompletionOption.ResponseHeadersRead)
                    .ConfigureAwait(continueOnCapturedContext))
                {
                    if (responseMessage.IsSuccessStatusCode)
                    {
                        IEnumerable<string> tileInfo;

                        if (responseMessage.Headers.TryGetValues("X-VE-Tile-Info", out tileInfo) &&
                            tileInfo.Contains("no-tile"))
                        {
                            response = new HttpResponse(null, null); // no tile image
                        }
                        else
                        {
                            response = new HttpResponse(
                                await responseMessage.Content.ReadAsByteArrayAsync().ConfigureAwait(continueOnCapturedContext),
                                responseMessage.Headers.CacheControl?.MaxAge);
                        }
                    }
                    else
                    {
                        Debug.WriteLine("ImageLoader: {0}: {1} {2}", uri, (int)responseMessage.StatusCode, responseMessage.ReasonPhrase);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ImageLoader: {0}: {1}", uri, ex.Message);
            }

            return response;
        }

        internal class HttpResponse
        {
            public byte[] Buffer { get; }
            public TimeSpan? MaxAge { get; }

            public HttpResponse(byte[] buffer, TimeSpan? maxAge)
            {
                Buffer = buffer;
                MaxAge = maxAge;
            }
        }
    }
}
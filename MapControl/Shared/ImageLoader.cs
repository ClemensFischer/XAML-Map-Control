// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2023 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
#if WINUI
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
#elif UWP
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
        public static HttpClient HttpClient { get; set; } = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };


        public static async Task<ImageSource> LoadImageAsync(Uri uri, IProgress<double> progress = null)
        {
            ImageSource image = null;

            progress?.Report(0d);

            try
            {
                if (!uri.IsAbsoluteUri || uri.IsFile)
                {
                    image = await LoadImageAsync(uri.IsAbsoluteUri ? uri.LocalPath : uri.OriginalString);
                }
                else if (uri.Scheme == "http" || uri.Scheme == "https")
                {
                    var response = await GetHttpResponseAsync(uri, progress);

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
                Debug.WriteLine($"ImageLoader: {uri}: {ex.Message}");
            }

            progress?.Report(1d);

            return image;
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

        internal static async Task<HttpResponse> GetHttpResponseAsync(Uri uri, IProgress<double> progress = null)
        {
            HttpResponse response = null;

            try
            {
                using (var responseMessage = await HttpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
                {
                    if (responseMessage.IsSuccessStatusCode)
                    {
                        byte[] buffer = null;

                        // Check for possibly unavailable Bing Maps tile.
                        //
                        if (!responseMessage.Headers.TryGetValues("X-VE-Tile-Info", out IEnumerable<string> tileInfo) ||
                            !tileInfo.Contains("no-tile"))
                        {
                            buffer = await ReadAsByteArrayAsync(responseMessage.Content, progress).ConfigureAwait(false);
                        }

                        response = new HttpResponse(buffer, responseMessage.Headers.CacheControl?.MaxAge);
                    }
                    else
                    {
                        Debug.WriteLine($"ImageLoader: {uri}: {(int)responseMessage.StatusCode} {responseMessage.ReasonPhrase}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ImageLoader: {uri}: {ex.Message}");
            }

            return response;
        }

        private static async Task<byte[]> ReadAsByteArrayAsync(HttpContent content, IProgress<double> progress)
        {
            if (progress == null || !content.Headers.ContentLength.HasValue)
            {
                return await content.ReadAsByteArrayAsync().ConfigureAwait(false);
            }

            var length = (int)content.Headers.ContentLength.Value;
            var buffer = new byte[length];

            using (var stream = await content.ReadAsStreamAsync().ConfigureAwait(false))
            {
                int offset = 0;
                int read;

                while (offset < length &&
                    (read = await stream.ReadAsync(buffer, offset, length - offset).ConfigureAwait(false)) > 0)
                {
                    offset += read;

                    if (offset < length) // 1.0 reported by caller
                    {
                        progress.Report((double)offset / length);
                    }
                }
            }

            return buffer;
        }
    }
}
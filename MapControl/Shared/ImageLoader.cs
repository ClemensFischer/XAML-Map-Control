﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
#if WPF
using System.Windows.Media;
#elif UWP
using Windows.UI.Xaml.Media;
#elif WINUI
using Microsoft.UI.Xaml.Media;
#endif

namespace MapControl
{
    public static partial class ImageLoader
    {
        private static ILogger logger;
        private static ILogger Logger => logger ?? (logger = LoggerFactory?.CreateLogger(typeof(ImageLoader)));

        public static ILoggerFactory LoggerFactory { get; set; }

        /// <summary>
        /// The System.Net.Http.HttpClient instance used to download images via a http or https Uri.
        /// </summary>
        public static HttpClient HttpClient { get; set; }

        static ImageLoader()
        {
            HttpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            HttpClient.DefaultRequestHeaders.Add("User-Agent", $"XAML-Map-Control/{typeof(ImageLoader).Assembly.GetName().Version}");
        }

        public static async Task<ImageSource> LoadImageAsync(Uri uri, IProgress<double> progress = null)
        {
            ImageSource image = null;

            progress?.Report(0d);

            try
            {
                if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
                {
                    var response = await GetHttpResponseAsync(uri, progress);

                    if (response?.Buffer != null)
                    {
                        image = await LoadImageAsync(response.Buffer);
                    }
                }
                else if (uri.IsFile || !uri.IsAbsoluteUri)
                {
                    image = await LoadImageAsync(uri.IsAbsoluteUri ? uri.LocalPath : uri.OriginalString);
                }
                else
                {
                    image = LoadImage(uri);
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Failed loading image from {uri}", uri);
            }

            progress?.Report(1d);

            return image;
        }

        public static async Task<ImageSource> LoadImageAsync(byte[] buffer)
        {
            using (var stream = new MemoryStream(buffer))
            {
                return await LoadImageAsync(stream);
            }
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
                            var content = responseMessage.Content;

                            if (progress != null && content.Headers.ContentLength.HasValue)
                            {
                                buffer = await ReadAsByteArray(content, progress).ConfigureAwait(false);
                            }
                            else
                            {
                                buffer = await content.ReadAsByteArrayAsync().ConfigureAwait(false);
                            }
                        }

                        response = new HttpResponse(buffer, responseMessage.Headers.CacheControl?.MaxAge);
                    }
                    else
                    {
                        Logger?.LogWarning("{uri}: {status} {reason}", uri, (int)responseMessage.StatusCode, responseMessage.ReasonPhrase);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Failed loading image from {uri}", uri);
            }

            return response;
        }

        private static async Task<byte[]> ReadAsByteArray(HttpContent content, IProgress<double> progress)
        {
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
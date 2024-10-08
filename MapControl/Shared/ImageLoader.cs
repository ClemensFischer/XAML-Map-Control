﻿// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        /// <summary>
        /// The System.Net.Http.HttpClient instance used to download images via a http or https Uri.
        /// </summary>
        public static HttpClient HttpClient { get; set; } = new HttpClient();

        static ImageLoader()
        {
            HttpClient.Timeout = TimeSpan.FromSeconds(30);
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

                    if (response != null && response.Buffer != null)
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
                Debug.WriteLine($"{nameof(ImageLoader)}: {uri}: {ex.Message}");
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
                                buffer = await ReadAsByteArrayAsync(content, progress).ConfigureAwait(false);
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
                        Debug.WriteLine($"{nameof(ImageLoader)}: {uri}: {(int)responseMessage.StatusCode} {responseMessage.ReasonPhrase}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{nameof(ImageLoader)}: {uri}: {ex.Message}");
            }

            return response;
        }

        private static async Task<byte[]> ReadAsByteArrayAsync(HttpContent content, IProgress<double> progress)
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

        private static Task<ImageSource[]> LoadImagesAsync(Uri uri1, Uri uri2, IProgress<double> progress)
        {
            IProgress<double> progress1 = null;
            IProgress<double> progress2 = null;

            if (progress != null)
            {
                var p1 = 0d;
                var p2 = 0d;
                progress1 = new Progress<double>(p => { p1 = p; progress.Report((p1 + p2) / 2d); });
                progress2 = new Progress<double>(p => { p2 = p; progress.Report((p1 + p2) / 2d); });
            }

            return Task.WhenAll(LoadImageAsync(uri1, progress1), LoadImageAsync(uri2, progress2));
        }
    }
}
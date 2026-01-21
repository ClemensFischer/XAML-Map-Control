using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
#if WPF
using System.Windows.Media;
#elif UWP
using Windows.UI.Xaml.Media;
#elif WINUI
using Microsoft.UI.Xaml.Media;
#elif AVALONIA
using ImageSource = Avalonia.Media.IImage;
#endif

namespace MapControl
{
    public static partial class ImageLoader
    {
        private static ILogger Logger => field ??= LoggerFactory?.CreateLogger(typeof(ImageLoader));

        public static ILoggerFactory LoggerFactory { get; set; }

        /// <summary>
        /// The System.Net.Http.HttpClient instance used to download images.
        /// An application should add a unique User-Agent value to the DefaultRequestHeaders of this
        /// HttpClient instance (or the Headers of a HttpRequestMessage used in a HttpMessageHandler).
        /// Failing to set a unique User-Agent value is a violation of OpenStreetMap's tile usage policy
        /// (see https://operations.osmfoundation.org/policies/tiles/) and results in blocked access
        /// to their tile servers.
        /// </summary>
        public static HttpClient HttpClient
        {
            get => field ??= new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            set;
        }

        public static bool IsHttp(this Uri uri)
        {
            return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
        }

        public static async Task<ImageSource> LoadImageAsync(byte[] buffer)
        {
            using var stream = new MemoryStream(buffer);

            return await LoadImageAsync(stream);
        }

        public static async Task<ImageSource> LoadImageAsync(Uri uri, IProgress<double> progress = null)
        {
            ImageSource image = null;

            progress?.Report(0d);

            try
            {
                if (!uri.IsAbsoluteUri)
                {
                    image = await LoadImageAsync(uri.OriginalString);
                }
                else if (uri.IsHttp())
                {
                    var buffer = await GetHttpContent(uri, progress);

                    if (buffer != null)
                    {
                        image = await LoadImageAsync(buffer);
                    }
                }
                else if (uri.IsFile)
                {
                    image = await LoadImageAsync(uri.LocalPath);
                }
                else
                {
                    image = LoadResourceImage(uri);
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Failed loading image from {uri}", uri);
            }

            progress?.Report(1d);

            return image;
        }

        public static async Task<HttpResponseMessage> GetHttpResponseAsync(Uri uri, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
        {
            try
            {
                var response = await HttpClient.GetAsync(uri, completionOption).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    return response;
                }

                Logger?.LogWarning("{status} ({reason}) from {uri}", (int)response.StatusCode, response.ReasonPhrase, uri);
                response.Dispose();
            }
            catch (TaskCanceledException)
            {
                Logger?.LogWarning("Timeout from {uri}", uri);
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "{uri}", uri);
            }

            return null;
        }

        private static async Task<byte[]> GetHttpContent(Uri uri, IProgress<double> progress)
        {
            var completionOption = progress != null ? HttpCompletionOption.ResponseHeadersRead : HttpCompletionOption.ResponseContentRead;

            using var response = await GetHttpResponseAsync(uri, completionOption).ConfigureAwait(false);

            if (response == null)
            {
                return null;
            }

            var content = response.Content;
            var contentLength = content.Headers.ContentLength;

            if (progress == null || !contentLength.HasValue)
            {
                return await content.ReadAsByteArrayAsync().ConfigureAwait(false);
            }

            var length = (int)contentLength.Value;
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
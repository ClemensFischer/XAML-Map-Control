using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
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
            HttpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            HttpClient.DefaultRequestHeaders.Add("User-Agent", $"XAML-Map-Control/{typeof(ImageLoader).Assembly.GetName().Version}");
        }

        public static Task<ImageSource> LoadImageAsync(Uri uri, IProgress<double> progress = null)
        {
            return LoadImageAsync(uri, progress, CancellationToken.None);
        }

        public static async Task<ImageSource> LoadImageAsync(Uri uri, IProgress<double> progress, CancellationToken cancellationToken)
        {
            ImageSource image = null;

            progress?.Report(0d);

            try
            {
                if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
                {
                    var response = await GetHttpResponseAsync(uri, progress, cancellationToken);

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

        internal static async Task<HttpResponse> GetHttpResponseAsync(Uri uri, IProgress<double> progress, CancellationToken cancellationToken)
        {
            HttpResponse response = null;

            try
            {
                var completionOptions = progress != null ? HttpCompletionOption.ResponseHeadersRead : HttpCompletionOption.ResponseContentRead;

                using (var responseMessage = await HttpClient.GetAsync(uri, completionOptions, cancellationToken).ConfigureAwait(false))
                {
                    if (responseMessage.IsSuccessStatusCode)
                    {
                        byte[] buffer = await responseMessage.Content.ReadAsByteArrayAsync(progress, cancellationToken).ConfigureAwait(false);

                        response = new HttpResponse(buffer, responseMessage.Headers.CacheControl?.MaxAge);
                    }
                    else
                    {
                        Logger?.LogWarning("{uri}: {status} {reason}", uri, (int)responseMessage.StatusCode, responseMessage.ReasonPhrase);
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
                if (ex.CancellationToken.IsCancellationRequested)
                {
                    Logger?.LogTrace("Cancelled loading image from {uri}", uri);
                }
                else
                {
                    Logger?.LogError(ex, "Failed loading image from {uri}", uri);
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Failed loading image from {uri}", uri);
            }

            return response;
        }
    }

    internal static class HttpContentExtensions
    {
        public static async Task<byte[]> ReadAsByteArrayAsync(this HttpContent content, IProgress<double> progress, CancellationToken cancellationToken)
        {
            byte[] buffer;

            if (progress != null && content.Headers.ContentLength.HasValue)
            {
                var length = (int)content.Headers.ContentLength.Value;
                buffer = new byte[length];

                using (var stream = await content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false))
                {
                    int offset = 0;
                    int read;

                    while (offset < length &&
                        (read = await stream.ReadAsync(buffer, offset, length - offset, cancellationToken).ConfigureAwait(false)) > 0)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        offset += read;

                        if (offset < length) // 1.0 reported by caller
                        {
                            progress.Report((double)offset / length);
                        }
                    }
                }
            }
            else
            {
                buffer = await content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
            }

            return buffer;
        }

#if !NET
        public static Task<byte[]> ReadAsByteArrayAsync(this HttpContent content, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return content.ReadAsByteArrayAsync();
        }

        public static Task<Stream> ReadAsStreamAsync(this HttpContent content, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return content.ReadAsStreamAsync();
        }
#endif
    }
}
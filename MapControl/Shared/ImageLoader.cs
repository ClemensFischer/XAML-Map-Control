// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Diagnostics;
using System.Threading.Tasks;
#if WINDOWS_UWP
using Windows.Web.Http;
using Windows.UI.Xaml.Media.Imaging;
#else
using System.Net.Http;
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

        public static async Task<BitmapSource> LoadImageAsync(Uri uri)
        {
            BitmapSource image = null;

            try
            {
                if (!uri.IsAbsoluteUri || uri.Scheme == "file")
                {
                    image = await LoadLocalImageAsync(uri);
                }
                else if (uri.Scheme == "http" || uri.Scheme == "https")
                {
                    image = await LoadHttpImageAsync(uri);
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

        private static async Task<BitmapSource> LoadHttpImageAsync(Uri uri)
        {
            BitmapSource image = null;

            using (var response = await HttpClient.GetAsync(uri))
            {
                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine("ImageLoader: {0}: {1} {2}", uri, (int)response.StatusCode, response.ReasonPhrase);
                }
                else if (IsTileAvailable(response.Headers))
                {
                    image = await LoadImageAsync(response.Content);
                }
            }

            return image;
        }
    }
}
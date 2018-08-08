// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Diagnostics;
using System.Threading.Tasks;
#if WINDOWS_UWP
using Windows.Web.Http;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
#else
using System.Net.Http;
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

        public static async Task<ImageSource> LoadImageAsync(Uri uri, bool isTileImage)
        {
            ImageSource imageSource = null;

            if (!uri.IsAbsoluteUri || uri.Scheme == "file")
            {
                imageSource = await LoadLocalImageAsync(uri);
            }
            else if (uri.Scheme == "http")
            {
                imageSource = await LoadHttpImageAsync(uri, isTileImage);
            }
            else
            {
                imageSource = new BitmapImage(uri);
            }

            return imageSource;
        }

        public static async Task<ImageSource> LoadHttpImageAsync(Uri uri, bool isTileImage)
        {
            ImageSource imageSource = null;

            using (var response = await HttpClient.GetAsync(uri))
            {
                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine("ImageLoader: {0}: {1} {2}", uri, (int)response.StatusCode, response.ReasonPhrase);
                }
                else if (!isTileImage || IsTileAvailable(response.Headers))
                {
                    using (var stream = await GetResponseStreamAsync(response.Content))
                    {
                        imageSource = await CreateImageSourceAsync(stream);
                    }
                }

                return imageSource;
            }
        }
    }
}
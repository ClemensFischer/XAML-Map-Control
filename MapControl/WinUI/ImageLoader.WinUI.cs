// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
#if WINUI
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
#else
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
#endif

namespace MapControl
{
    public static partial class ImageLoader
    {
        public static async Task<ImageSource> LoadImageAsync(IRandomAccessStream stream)
        {
            var image = new BitmapImage();

            await image.SetSourceAsync(stream);

            return image;
        }

        public static Task<ImageSource> LoadImageAsync(Stream stream)
        {
            return LoadImageAsync(stream.AsRandomAccessStream());
        }

        public static async Task<ImageSource> LoadImageAsync(byte[] buffer)
        {
            using (var stream = new MemoryStream(buffer))
            {
                return await LoadImageAsync(stream); // await before closing stream
            }
        }

        public static async Task<ImageSource> LoadImageAsync(string path)
        {
            ImageSource image = null;

            if (File.Exists(path))
            {
                var file = await StorageFile.GetFileFromPathAsync(Path.GetFullPath(path));

                using (var stream = await file.OpenReadAsync())
                {
                    image = await LoadImageAsync(stream);
                }
            }

            return image;
        }

        public static async Task<WriteableBitmap> LoadWriteableBitmapAsync(Uri uri, IProgress<double> progress = null)
        {
            WriteableBitmap image = null;

            progress?.Report(0d);

            try
            {
                if (!uri.IsAbsoluteUri || uri.IsFile)
                {
                    var file = await StorageFile.GetFileFromPathAsync(uri.IsAbsoluteUri ? uri.LocalPath : uri.OriginalString);

                    using (var stream = await file.OpenReadAsync())
                    {
                        image = await LoadWriteableBitmapAsync(stream);
                    }
                }
                else if (uri.Scheme == "http" || uri.Scheme == "https")
                {
                    var response = await GetHttpResponseAsync(uri, progress);

                    if (response != null && response.Buffer != null)
                    {
                        using (var stream = new MemoryStream(response.Buffer))
                        {
                            image = await LoadWriteableBitmapAsync(stream.AsRandomAccessStream());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ImageLoader: {uri}: {ex.Message}");
            }

            progress?.Report(1d);

            return image;
        }

        public static async Task<WriteableBitmap> LoadWriteableBitmapAsync(IRandomAccessStream stream)
        {
            var decoder = await BitmapDecoder.CreateAsync(stream);
            var pixelData = await decoder.GetPixelDataAsync(
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Straight,
                new BitmapTransform(),
                ExifOrientationMode.IgnoreExifOrientation,
                ColorManagementMode.DoNotColorManage);

            var pixels = pixelData.DetachPixelData();
            var image = new WriteableBitmap((int)decoder.PixelWidth, (int)decoder.PixelHeight);

            using (var pixelStream = image.PixelBuffer.AsStream())
            {
                await pixelStream.WriteAsync(pixels, 0, pixels.Length);
            }

            return image;
        }

        internal static async Task<WriteableBitmap> LoadMergedImageAsync(Uri uri1, Uri uri2, IProgress<double> progress)
        {
            WriteableBitmap image = null;
            IProgress<double> progress1 = null;
            IProgress<double> progress2 = null;

            if (progress != null)
            {
                var p1 = 0d;
                var p2 = 0d;
                progress1 = new Progress<double>(p => { p1 = p; progress.Report((p1 + p2) / 2d); });
                progress2 = new Progress<double>(p => { p2 = p; progress.Report((p1 + p2) / 2d); });
            }

            var images = await Task.WhenAll(
                LoadWriteableBitmapAsync(uri1, progress1),
                LoadWriteableBitmapAsync(uri2, progress2));

            if (images.Length == 2 &&
                images[0] != null &&
                images[1] != null &&
                images[0].PixelHeight == images[1].PixelHeight)
            {
                var width = images[0].PixelWidth + images[1].PixelWidth;
                var height = images[1].PixelHeight;
                var stride1 = images[0].PixelWidth * 4;
                var stride2 = images[1].PixelWidth * 4;
                var buffer1 = images[0].PixelBuffer.ToArray();
                var buffer2 = images[1].PixelBuffer.ToArray();

                image = new WriteableBitmap(width, height);

                using (var pixelStream = image.PixelBuffer.AsStream())
                {
                    for (var i = 0; i < height; i++)
                    {
                        await pixelStream.WriteAsync(buffer1, i * stride1, stride1);
                        await pixelStream.WriteAsync(buffer2, i * stride2, stride2);
                    }
                }
            }

            return image;
        }
    }
}

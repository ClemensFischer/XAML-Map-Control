// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
#if UWP
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
#else
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
#endif

namespace MapControl
{
    public static partial class ImageLoader
    {
        public static ImageSource LoadImage(Uri uri)
        {
            return new BitmapImage(uri);
        }

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

        public static async Task<ImageSource> LoadImageAsync(string path)
        {
            ImageSource image = null;

            path = FilePath.GetFullPath(path);

            if (File.Exists(path))
            {
                var file = await StorageFile.GetFileFromPathAsync(path);

                using (var stream = await file.OpenReadAsync())
                {
                    image = await LoadImageAsync(stream);
                }
            }

            return image;
        }

        internal static async Task<WriteableBitmap> LoadWriteableBitmapAsync(BitmapDecoder decoder)
        {
            var image = new WriteableBitmap((int)decoder.PixelWidth, (int)decoder.PixelHeight);
            var pixelData = await decoder.GetPixelDataAsync(
                BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, new BitmapTransform(),
                ExifOrientationMode.IgnoreExifOrientation, ColorManagementMode.DoNotColorManage);

            pixelData.DetachPixelData().CopyTo(image.PixelBuffer);

            return image;
        }

        internal static async Task<WriteableBitmap> LoadWriteableBitmapAsync(Uri uri)
        {
            WriteableBitmap image = null;

            try
            {
                using (var stream = await RandomAccessStreamReference.CreateFromUri(uri).OpenReadAsync())
                {
                    image = await LoadWriteableBitmapAsync(await BitmapDecoder.CreateAsync(stream));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{nameof(ImageLoader)}: {uri}: {ex.Message}");
            }

            return image;
        }

        internal static async Task<ImageSource> LoadMergedImageAsync(Uri uri1, Uri uri2, IProgress<double> progress)
        {
            WriteableBitmap mergedImage = null;

            progress?.Report(0d);

            var images = await Task.WhenAll(LoadWriteableBitmapAsync(uri1), LoadWriteableBitmapAsync(uri2));

            if (images.Length == 2 &&
                images[0] != null &&
                images[1] != null &&
                images[0].PixelHeight == images[1].PixelHeight)
            {
                var buffer1 = images[0].PixelBuffer;
                var buffer2 = images[1].PixelBuffer;
                var stride1 = (uint)images[0].PixelWidth * 4;
                var stride2 = (uint)images[1].PixelWidth * 4;
                var stride = stride1 + stride2;
                var height = images[0].PixelHeight;

                mergedImage = new WriteableBitmap(images[0].PixelWidth + images[1].PixelWidth, height);

                var buffer = mergedImage.PixelBuffer;

                for (uint y = 0; y < height; y++)
                {
                    buffer1.CopyTo(y * stride1, buffer, y * stride, stride1);
                    buffer2.CopyTo(y * stride2, buffer, y * stride + stride1, stride2);
                }
            }

            progress?.Report(1d);

            return mergedImage;
        }
    }
}

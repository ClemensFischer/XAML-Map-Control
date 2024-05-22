// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
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

        public static async Task<WriteableBitmap> LoadImageAsync(BitmapDecoder decoder)
        {
            var image = new WriteableBitmap((int)decoder.PixelWidth, (int)decoder.PixelHeight);
            var pixelData = await decoder.GetPixelDataAsync(
                BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, new BitmapTransform(),
                ExifOrientationMode.IgnoreExifOrientation, ColorManagementMode.DoNotColorManage);

            pixelData.DetachPixelData().CopyTo(image.PixelBuffer);

            return image;
        }

        public static async Task<ImageSource> LoadImageAsync(IRandomAccessStream stream)
        {
            // WinUI BitmapImage produces visual artifacts with Bing Maps Aerial (or all JPEG?)
            // images in a tile raster, where thin white lines may appear as gaps between tiles.
            // Alternatives are SoftwareBitmapSource or WriteableBitmap.
            //
            // var image = new BitmapImage();
            // await image.SetSourceAsync(stream);
            // return image;

            return await LoadImageAsync(await BitmapDecoder.CreateAsync(stream));
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

        internal static async Task<ImageSource> LoadMergedImageAsync(Uri uri1, Uri uri2, IProgress<double> progress)
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

            var images = await Task.WhenAll(LoadImageAsync(uri1, progress1), LoadImageAsync(uri2, progress2));

            if (images.Length == 2 &&
                images[0] is WriteableBitmap image1 &&
                images[1] is WriteableBitmap image2 &&
                image1.PixelHeight == image2.PixelHeight)
            {
                var buffer1 = image1.PixelBuffer;
                var buffer2 = image2.PixelBuffer;
                var stride1 = (uint)image1.PixelWidth * 4;
                var stride2 = (uint)image2.PixelWidth * 4;
                var stride = stride1 + stride2;
                var height = image1.PixelHeight;

                image = new WriteableBitmap(image1.PixelWidth + image2.PixelWidth, height);

                var buffer = image.PixelBuffer;

                for (uint y = 0; y < height; y++)
                {
                    buffer1.CopyTo(y * stride1, buffer, y * stride, stride1);
                    buffer2.CopyTo(y * stride2, buffer, y * stride + stride1, stride2);
                }
            }

            return image;
        }
    }
}

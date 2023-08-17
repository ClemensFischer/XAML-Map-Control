// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2023 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
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
        public static async Task<WriteableBitmap> LoadImageAsync(BitmapDecoder decoder)
        {
            var image = new WriteableBitmap((int)decoder.PixelWidth, (int)decoder.PixelHeight);
            var pixelData = await decoder.GetPixelDataAsync(
                BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight, new BitmapTransform(),
                ExifOrientationMode.IgnoreExifOrientation, ColorManagementMode.DoNotColorManage);
            var pixels = pixelData.DetachPixelData();

            using (var pixelStream = image.PixelBuffer.AsStream())
            {
                await pixelStream.WriteAsync(pixels, 0, pixels.Length);
            }

            return image;
        }

        public static async Task<ImageSource> LoadImageAsync(IRandomAccessStream stream)
        {
            // WinUI BitmapImage produces visual artifacts with Bing Maps Aerial (or all JPEG?)
            // images in a tile raster, where thin white lines may appear as gaps between tiles.
            // Alternatives are SoftwareBitmapSource or WriteableBitmap.

            return await LoadImageAsync(await BitmapDecoder.CreateAsync(stream));
        }

        public static Task<ImageSource> LoadImageAsync(Stream stream)
        {
            return LoadImageAsync(stream.AsRandomAccessStream());
        }

        public static async Task<ImageSource> LoadImageAsync(byte[] buffer)
        {
            using (var stream = new MemoryStream(buffer))
            {
                return await LoadImageAsync(stream);
            }
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
            var image1 = (WriteableBitmap)images[0];
            var image2 = (WriteableBitmap)images[1];

            if (image1.PixelHeight == image2.PixelHeight)
            {
                var width = image1.PixelWidth + image2.PixelWidth;
                var height = image2.PixelHeight;
                var stride1 = image1.PixelWidth * 4;
                var stride2 = image2.PixelWidth * 4;
                var buffer1 = image1.PixelBuffer.ToArray();
                var buffer2 = image2.PixelBuffer.ToArray();

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

// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2023 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MapControl
{
    public static partial class ImageLoader
    {
        public static ImageSource LoadImage(Stream stream)
        {
            var image = new BitmapImage();

            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = stream;
            image.EndInit();
            image.Freeze();

            return image;
        }

        public static Task<ImageSource> LoadImageAsync(Stream stream)
        {
            return Task.FromResult(LoadImage(stream));
        }

        public static async Task<ImageSource> LoadImageAsync(byte[] buffer)
        {
            using (var stream = new MemoryStream(buffer))
            {
                return await LoadImageAsync(stream);
            }
        }

        public static Task<ImageSource> LoadImageAsync(string path)
        {
            return Task.Run(() =>
            {
                if (!File.Exists(path))
                {
                    return null;
                }

                using (var stream = File.OpenRead(path))
                {
                    return LoadImage(stream);
                }
            });
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
                images[0] is BitmapSource image1 &&
                images[1] is BitmapSource image2 &&
                image1.PixelHeight == image2.PixelHeight &&
                image1.Format == image2.Format &&
                image1.Format.BitsPerPixel % 8 == 0)
            {
                var format = image1.Format;
                var height = image1.PixelHeight;
                var width1 = image1.PixelWidth;
                var width2 = image2.PixelWidth;
                var stride1 = width1 * format.BitsPerPixel / 8;
                var stride2 = width2 * format.BitsPerPixel / 8;
                var buffer1 = new byte[stride1 * height];
                var buffer2 = new byte[stride2 * height];

                image1.CopyPixels(buffer1, stride1, 0);
                image2.CopyPixels(buffer2, stride2, 0);

                image = new WriteableBitmap(width1 + width2, height, 96, 96, format, null);
                image.WritePixels(new Int32Rect(0, 0, width1, height), buffer1, stride1, 0);
                image.WritePixels(new Int32Rect(width1, 0, width2, height), buffer2, stride2, 0);
                image.Freeze();
            }

            return image;
        }
    }
}

// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MapControl
{
    public static partial class ImageLoader
    {
        public static Task<ImageSource> LoadImageAsync(Stream stream)
        {
            return Task.Run(() => LoadImage(stream));
        }

        public static Task<ImageSource> LoadImageAsync(byte[] buffer)
        {
            return Task.Run(() =>
            {
                using (var stream = new MemoryStream(buffer))
                {
                    return LoadImage(stream);
                }
            });
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

        private static ImageSource LoadImage(Stream stream)
        {
            var bitmapImage = new BitmapImage();

            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = stream;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
        }

        internal static async Task<ImageSource> LoadMergedImageAsync(Uri uri1, Uri uri2, IProgress<double> progress)
        {
            ImageSource image = null;
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
                var width = image1.PixelWidth + image2.PixelWidth;
                var height = image1.PixelHeight;
                var stride1 = image1.PixelWidth * format.BitsPerPixel / 8;
                var stride2 = image2.PixelWidth * format.BitsPerPixel / 8;
                var buffer1 = new byte[stride1 * height];
                var buffer2 = new byte[stride2 * height];
                var stride = stride1 + stride2;
                var buffer = new byte[stride * height];

                image1.CopyPixels(buffer1, stride1, 0);
                image2.CopyPixels(buffer2, stride2, 0);

                for (var i = 0; i < height; i++)
                {
                    Array.Copy(buffer1, i * stride1, buffer, i * stride, stride1);
                    Array.Copy(buffer2, i * stride2, buffer, i * stride + stride1, stride2);
                }

                image = BitmapSource.Create(width, height, 96, 96, format, null, buffer, stride);
            }

            return image;
        }
    }
}

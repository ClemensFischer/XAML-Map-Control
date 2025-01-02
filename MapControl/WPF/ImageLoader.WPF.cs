// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MapControl
{
    public static partial class ImageLoader
    {
        public static ImageSource LoadImage(Uri uri)
        {
            return new BitmapImage(uri);
        }

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

        public static Task<ImageSource> LoadImageAsync(string path)
        {
            if (!File.Exists(path))
            {
                return Task.FromResult<ImageSource>(null);
            }

            return Task.Run(() =>
            {
                using (var stream = File.OpenRead(path))
                {
                    return LoadImage(stream);
                }
            });
        }

        internal static async Task<ImageSource> LoadMergedImageAsync(Uri uri1, Uri uri2, IProgress<double> progress)
        {
            WriteableBitmap mergedBitmap = null;
            var p1 = 0d;
            var p2 = 0d;

            var images = await Task.WhenAll(
                LoadImageAsync(uri1, new Progress<double>(p => { p1 = p; progress.Report((p1 + p2) / 2d); })),
                LoadImageAsync(uri2, new Progress<double>(p => { p2 = p; progress.Report((p1 + p2) / 2d); })));

            if (images.Length == 2 &&
                images[0] is BitmapSource bitmap1 &&
                images[1] is BitmapSource bitmap2 &&
                bitmap1.PixelHeight == bitmap2.PixelHeight &&
                bitmap1.Format == bitmap2.Format &&
                bitmap1.Format.BitsPerPixel % 8 == 0)
            {
                var format = bitmap1.Format;
                var height = bitmap1.PixelHeight;
                var width1 = bitmap1.PixelWidth;
                var width2 = bitmap2.PixelWidth;
                var stride1 = width1 * format.BitsPerPixel / 8;
                var stride2 = width2 * format.BitsPerPixel / 8;
                var buffer1 = new byte[stride1 * height];
                var buffer2 = new byte[stride2 * height];

                bitmap1.CopyPixels(buffer1, stride1, 0);
                bitmap2.CopyPixels(buffer2, stride2, 0);

                mergedBitmap = new WriteableBitmap(width1 + width2, height, 96, 96, format, null);
                mergedBitmap.WritePixels(new Int32Rect(0, 0, width1, height), buffer1, stride1, 0);
                mergedBitmap.WritePixels(new Int32Rect(width1, 0, width2, height), buffer2, stride2, 0);
                mergedBitmap.Freeze();
            }

            return mergedBitmap;
        }
    }
}

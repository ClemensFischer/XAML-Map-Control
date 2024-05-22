// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MapControl
{
    public static partial class ImageLoader
    {
        public static IImage LoadImage(Uri uri)
        {
            return null;
        }

        public static IImage LoadImage(Stream stream)
        {
            return new Bitmap(stream);
        }

        public static Task<IImage> LoadImageAsync(Stream stream)
        {
            return Task.FromResult(LoadImage(stream));
        }

        public static Task<IImage> LoadImageAsync(string path)
        {
            if (!File.Exists(path))
            {
                return Task.FromResult<IImage>(null);
            }

            return Task.Run(() =>
            {
                using var stream = File.OpenRead(path);

                return LoadImage(stream);
            });
        }

        internal static async Task<IImage> LoadMergedImageAsync(Uri uri1, Uri uri2, IProgress<double> progress)
        {
            var images = await LoadImagesAsync(uri1, uri2, progress);

            WriteableBitmap image = null;

            if (images.Length == 2 &&
                images[0] is Bitmap image1 &&
                images[1] is Bitmap image2 &&
                image1.PixelSize.Height == image2.PixelSize.Height &&
                image1.Format.HasValue &&
                image2.Format.HasValue &&
                image1.Format.Value == image2.Format.Value &&
                image1.AlphaFormat.HasValue &&
                image2.AlphaFormat.HasValue &&
                image1.AlphaFormat.Value == image2.AlphaFormat.Value)
            {
                var bpp = image1.Format.Value == PixelFormat.Rgb565 ? 2 : 4;
                var size = new PixelSize(image1.PixelSize.Width + image2.PixelSize.Width, image1.PixelSize.Height);
                var stride1 = image1.PixelSize.Width * bpp;
                var stride = size.Width * bpp;
                var buffer = new byte[stride * size.Height];

                unsafe
                {
                    fixed (byte* ptr = buffer)
                    {
                        var p = (nint)ptr;

                        image1.CopyPixels(new PixelRect(image1.PixelSize), p, buffer.Length, stride);
                        image2.CopyPixels(new PixelRect(image2.PixelSize), p + stride1, buffer.Length, stride);

                        image = new WriteableBitmap(image1.Format.Value, image1.AlphaFormat.Value, p, size, image1.Dpi, stride);
                    }
                }
            }

            return image;
        }
    }
}

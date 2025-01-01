// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

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
            WriteableBitmap mergedImage = null;

            var images = await LoadImagesAsync(uri1, uri2, progress);

            if (images.Length == 2 &&
                images[0] is Bitmap image1 &&
                images[1] is Bitmap image2 &&
                image1.PixelSize.Height == image2.PixelSize.Height &&
                image1.Format.HasValue &&
                image1.Format == image2.Format &&
                image1.AlphaFormat.HasValue &&
                image1.AlphaFormat == image2.AlphaFormat)
            {
                var bpp = image1.Format.Value == PixelFormat.Rgb565 ? 2 : 4;
                var pixelSize = new PixelSize(image1.PixelSize.Width + image2.PixelSize.Width, image1.PixelSize.Height);
                var stride1 = bpp * image1.PixelSize.Width;
                var stride = bpp * pixelSize.Width;
                var bufferSize = stride * pixelSize.Height;

                unsafe
                {
                    fixed (byte* ptr = new byte[stride * pixelSize.Height])
                    {
                        var buffer = (nint)ptr;

                        image1.CopyPixels(new PixelRect(image1.PixelSize), buffer, bufferSize, stride);
                        image2.CopyPixels(new PixelRect(image2.PixelSize), buffer + stride1, bufferSize, stride);

                        mergedImage = new WriteableBitmap(image1.Format.Value, image1.AlphaFormat.Value, buffer, pixelSize, image1.Dpi, stride);
                    }
                }
            }

            return mergedImage;
        }
    }
}

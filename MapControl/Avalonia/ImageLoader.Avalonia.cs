﻿// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
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
            WriteableBitmap mergedBitmap = null;

            var images = await LoadImagesAsync(uri1, uri2, progress);

            if (images.Length == 2 &&
                images[0] is Bitmap bitmap1 &&
                images[1] is Bitmap bitmap2 &&
                bitmap1.PixelSize.Height == bitmap2.PixelSize.Height &&
                bitmap1.Format.HasValue &&
                bitmap1.Format == bitmap2.Format &&
                bitmap1.AlphaFormat.HasValue &&
                bitmap1.AlphaFormat == bitmap2.AlphaFormat)
            {
                var bpp = bitmap1.Format.Value == PixelFormat.Rgb565 ? 2 : 4;
                var pixelSize = new PixelSize(bitmap1.PixelSize.Width + bitmap2.PixelSize.Width, bitmap1.PixelSize.Height);
                var stride1 = bpp * bitmap1.PixelSize.Width;
                var stride = bpp * pixelSize.Width;
                var bufferSize = stride * pixelSize.Height;

                unsafe
                {
                    fixed (byte* ptr = new byte[stride * pixelSize.Height])
                    {
                        var buffer = (nint)ptr;

                        bitmap1.CopyPixels(new PixelRect(bitmap1.PixelSize), buffer, bufferSize, stride);
                        bitmap2.CopyPixels(new PixelRect(bitmap2.PixelSize), buffer + stride1, bufferSize, stride);

                        mergedBitmap = new WriteableBitmap(bitmap1.Format.Value, bitmap1.AlphaFormat.Value, buffer, pixelSize, bitmap1.Dpi, stride);
                    }
                }
            }

            return mergedBitmap;
        }
    }
}

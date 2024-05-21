// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using Avalonia.Media;
using Avalonia.Media.Imaging;
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
            return Task.Run(() =>
            {
                if (!File.Exists(path))
                {
                    return null;
                }

                using var stream = File.OpenRead(path);

                return LoadImage(stream);
            });
        }

        internal static Task<IImage> LoadMergedImageAsync(Uri uri1, Uri uri2, IProgress<double> progress)
        {
            return Task.FromResult<IImage>(null);
        }
    }
}

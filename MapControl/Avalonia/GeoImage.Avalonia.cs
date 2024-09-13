// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Threading.Tasks;

namespace MapControl
{
    public static partial class GeoImage
    {
        private partial class GeoBitmap
        {
            public Point BitmapSize => new(BitmapSource.PixelSize.Width, BitmapSource.PixelSize.Height);

            public ImageBrush ImageBrush => new(BitmapSource);
        }

        private static Task<GeoBitmap> LoadGeoTiffAsync(string sourcePath)
        {
            throw new InvalidOperationException("GeoTIFF is not supported.");
        }
    }
}

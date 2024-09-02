// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Threading.Tasks;

namespace MapControl
{
    public partial class GeoImage
    {
        private Point BitmapSize => new(bitmapSource.PixelSize.Width, bitmapSource.PixelSize.Height);

        private Task LoadGeoTiffAsync(string sourcePath)
        {
            throw new InvalidOperationException("GeoTIFF is not supported.");
        }
    }
}

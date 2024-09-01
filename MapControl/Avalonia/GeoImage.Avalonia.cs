// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Threading.Tasks;

namespace MapControl
{
    public partial class GeoImage : Image
    {
        private void SetImage(ImageSource image)
        {
            Source = image;
            Stretch = Stretch.Fill;
        }

        private static Task<GeoBitmap> ReadGeoTiffAsync(string sourcePath)
        {
            throw new InvalidOperationException("GeoTIFF is not supported.");
        }
    }
}

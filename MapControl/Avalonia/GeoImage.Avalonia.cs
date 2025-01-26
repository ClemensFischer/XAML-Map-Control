// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Threading.Tasks;

namespace MapControl
{
    public static partial class GeoImage
    {
        private static Task<GeoBitmap> LoadGeoTiff(string sourcePath)
        {
            throw new InvalidOperationException("GeoTIFF is not supported.");
        }
    }
}

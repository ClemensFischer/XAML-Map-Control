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

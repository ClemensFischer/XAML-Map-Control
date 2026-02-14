using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;

namespace MapControl.MapsforgeTiles
{
    public partial class MapsforgeTileSource
    {
        private static Bitmap CreateImage(int[] pixels)
        {
            var size = (int)Math.Sqrt(pixels.Length);

            unsafe
            {
                fixed (int* ptr = pixels)
                {
                    return new Bitmap(PixelFormat.Bgra8888, AlphaFormat.Opaque, (nint)ptr,
                        new PixelSize(size, size), new Vector(96d, 96d), size * 4);
                }
            }
        }
    }
}

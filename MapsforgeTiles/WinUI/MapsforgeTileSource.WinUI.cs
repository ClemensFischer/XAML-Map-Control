using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
#if UWP
using Windows.UI.Xaml.Media.Imaging;
#elif WINUI
using Microsoft.UI.Xaml.Media.Imaging;
#endif

namespace MapControl.MapsforgeTiles
{
    public partial class MapsforgeTileSource
    {
        private static WriteableBitmap CreateImage(int[] pixels)
        {
            var size = (int)Math.Sqrt(pixels.Length);
            var bitmap = new WriteableBitmap(size, size);

            using var stream = bitmap.PixelBuffer.AsStream();
            using var writer = new BinaryWriter(stream);

            foreach (var pixel in pixels)
            {
                writer.Write(pixel);
            }

            return bitmap;
        }
    }
}

using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MapControl.MapsforgeTiles
{
    public partial class MapsforgeTileSource
    {
        private static BitmapSource CreateImage(int[] pixels)
        {
            var size = (int)Math.Sqrt(pixels.Length);
            var image = BitmapSource.Create(size, size, 96d, 96d, PixelFormats.Bgra32, null, pixels, size * 4);
            image.Freeze();
            return image;
        }
    }
}

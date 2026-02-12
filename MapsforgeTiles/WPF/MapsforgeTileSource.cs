using MapsforgeWrapper;
using System;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MapControl.MapsforgeTiles
{
    public class MapsforgeTileSource(string theme, int cacheCapacity = 200) : TileSource
    {
        private readonly TileRenderer renderer = new(theme, cacheCapacity);

        public static void Initialize(string mapFilePath, float dpiScale)
        {
            TileRenderer.Initialize(mapFilePath, dpiScale);
        }

        public override Task<ImageSource> LoadImageAsync(int zoomLevel, int column, int row)
        {
            var pixels = renderer.RenderTile(zoomLevel, column, row);
            ImageSource image = pixels != null ? CreateImage(pixels) : null;

            return Task.FromResult(image);
        }

        private static BitmapSource CreateImage(int[] pixels)
        {
            var size = (int)Math.Sqrt(pixels.Length);
            var image = BitmapSource.Create(size, size, 96d, 96d, PixelFormats.Bgra32, null, pixels, size * 4);
            image.Freeze();
            return image;
        }
    }
}

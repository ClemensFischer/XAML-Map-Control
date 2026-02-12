using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using MapsforgeWrapper;
#if UWP
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
#elif WINUI
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
#endif

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

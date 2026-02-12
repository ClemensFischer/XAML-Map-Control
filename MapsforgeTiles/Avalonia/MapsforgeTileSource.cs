using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using MapsforgeWrapper;
using System;
using System.Threading.Tasks;

namespace MapControl.MapsforgeTiles
{
    public class MapsforgeTileSource(string theme, int cacheCapacity = 200) : TileSource
    {
        private readonly TileRenderer renderer = new(theme, cacheCapacity);

        public static void Initialize(string mapFilePath, float dpiScale)
        {
            TileRenderer.Initialize(mapFilePath, dpiScale);
        }

        public override Task<IImage> LoadImageAsync(int zoomLevel, int column, int row)
        {
            var pixels = renderer.RenderTile(zoomLevel, column, row);
            IImage image = pixels != null ? CreateImage(pixels) : null;

            return Task.FromResult(image);
        }

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

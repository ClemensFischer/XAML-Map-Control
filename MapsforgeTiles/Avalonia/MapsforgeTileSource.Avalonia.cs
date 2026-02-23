using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace MapControl.MapsforgeTiles
{
    public partial class MapsforgeTileSource
    {
        public override async Task<IImage> LoadImageAsync(int zoomLevel, int column, int row)
        {
            Bitmap bitmap = null;

            try
            {
                var pixels = RenderTile(zoomLevel, column, row);

                if (pixels != null)
                {
                    var size = displayModel.getTileSize();

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
            catch (Exception ex)
            {
                Logger?.LogError(ex, "LoadImageAsync");
            }

            return bitmap;
        }
    }
}

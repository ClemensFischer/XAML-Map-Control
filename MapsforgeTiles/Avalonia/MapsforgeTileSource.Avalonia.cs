using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace MapControl.MapsforgeTiles;

public partial class MapsforgeTileSource
{
    public override async Task<IImage> LoadImageAsync(int zoomLevel, int column, int row)
    {
        IImage image = null;

        try
        {
            var pixels = RenderTile(zoomLevel, column, row);

            if (pixels != null)
            {
                unsafe
                {
                    fixed (int* ptr = pixels)
                    {
                        image = new Bitmap(PixelFormat.Bgra8888, AlphaFormat.Opaque, (nint)ptr,
                            new PixelSize(TileSize, TileSize), new Vector(96d, 96d), TileSize * 4);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "LoadImageAsync");
        }

        return image;
    }
}

using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MapControl.MapsforgeTiles;

public partial class MapsforgeTileSource
{
    public override async Task<ImageSource> LoadImageAsync(int zoomLevel, int column, int row)
    {
        ImageSource image = null;

        try
        {
            var pixels = RenderTile(zoomLevel, column, row);

            if (pixels != null)
            {
                image = BitmapSource.Create(TileSize, TileSize, 96d, 96d, PixelFormats.Bgra32, null, pixels, TileSize * 4);
                image.Freeze();
            }
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "LoadImageAsync");
        }

        return image;
    }
}

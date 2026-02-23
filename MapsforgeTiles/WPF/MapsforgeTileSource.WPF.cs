using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MapControl.MapsforgeTiles
{
    public partial class MapsforgeTileSource
    {
        public override async Task<ImageSource> LoadImageAsync(int zoomLevel, int column, int row)
        {
            BitmapSource bitmap = null;

            try
            {
                var pixels = RenderTile(zoomLevel, column, row);

                if (pixels != null)
                {
                    var size = displayModel.getTileSize();

                    bitmap = BitmapSource.Create(size, size, 96d, 96d, PixelFormats.Bgra32, null, pixels, size * 4);
                    bitmap.Freeze();
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

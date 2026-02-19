using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
#if UWP
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
#elif WINUI
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
#endif

namespace MapControl.MapsforgeTiles
{
    public partial class MapsforgeTileSource
    {
        public override async Task<ImageSource> LoadImageAsync(int zoomLevel, int column, int row)
        {
            ImageSource image = null;
            var size = TileRenderer.TileSize;
            var bitmap = new WriteableBitmap(size, size);
            using var stream = bitmap.PixelBuffer.AsStream();

            try
            {
                // Run a Task because in WinUI/UWP LoadImageAsync is called in the UI thread.
                //
                await Task.Run(() =>
                {
                    var pixels = tileRenderer.RenderTile(zoomLevel, column, row);

                    if (pixels != null)
                    {
                        stream.Write(MemoryMarshal.AsBytes(pixels.AsSpan()));
                        image = bitmap;
                    }
                });
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "LoadImageAsync");
            }

            return image;
        }
    }
}

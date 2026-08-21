using Microsoft.Extensions.Logging;
using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
#if UWP
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
#elif WINUI
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
#endif

namespace MapControl.MapsforgeTiles;

public partial class MapsforgeTileSource
{
    public override async Task<ImageSource> LoadImageAsync(int zoomLevel, int column, int row)
    {
        SoftwareBitmapSource image = null;

        // Run a Task because in WinUI/UWP LoadImageAsync is called in the UI thread.
        //
        var bitmap = await Task.Run(() =>
        {
            try
            {
                var pixels = RenderTile(zoomLevel, column, row);

                if (pixels != null)
                {
                    var buffer = new Windows.Storage.Streams.Buffer((uint)pixels.Length * 4);

                    MemoryMarshal.AsBytes(pixels.AsSpan()).CopyTo(buffer);

                    return SoftwareBitmap.CreateCopyFromBuffer(
                        buffer, BitmapPixelFormat.Bgra8, TileSize, TileSize, BitmapAlphaMode.Premultiplied);
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "LoadImageAsync");
            }

            return null;
        });

        if (bitmap != null)
        {
            image = new SoftwareBitmapSource();
            await image.SetBitmapAsync(bitmap);
        }

        return image;
    }
}

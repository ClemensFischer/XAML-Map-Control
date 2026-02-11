using System.Threading.Tasks;
using TileRenderer;
#if WPF
using System.Windows.Media;
#elif UWP
using Windows.UI.Xaml.Media;
#elif WINUI
using Microsoft.UI.Xaml.Media;
#elif AVALONIA
using ImageSource = Avalonia.Media.IImage;
#endif

namespace MapControl.MapsforgeTiles
{
    public partial class MapsforgeTileSource(string mapDirectory, string theme, int cacheCapacity = 200) : TileSource
    {
        private readonly MapsforgeTileRenderer renderer = new(mapDirectory, theme, cacheCapacity);

        public static void SetDpiScale(float scale)
        {
            MapsforgeTileRenderer.SetDpiScale(scale);
        }

        public override Task<ImageSource> LoadImageAsync(int zoomLevel, int column, int row)
        {
            var pixels = renderer.RenderTile(zoomLevel, column, row);
            ImageSource image = pixels != null ? CreateImage(pixels) : null;

            return Task.FromResult(image);
        }
    }
}

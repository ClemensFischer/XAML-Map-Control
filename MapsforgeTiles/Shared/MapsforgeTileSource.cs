using System.Threading.Tasks;
#if WPF
using System.Windows.Media;
#elif UWP
using Windows.UI.Xaml.Media;
#elif WINUI
using Microsoft.UI.Xaml.Media;
#elif AVALONIA
using ImageSource=Avalonia.Media.IImage;
#endif

namespace MapControl.MapsforgeTiles
{
    public partial class MapsforgeTileSource(string theme, int cacheCapacity = 200) : TileSource
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
    }
}

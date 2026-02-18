using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
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
    public partial class MapsforgeTileSource(string theme, int cacheCapacity = 200, float textScale = 1f) : TileSource
    {
        private static ILogger Logger => field ??= ImageLoader.LoggerFactory?.CreateLogger<MapsforgeTileSource>();

        private readonly TileRenderer renderer = new(theme, cacheCapacity, textScale);

        public static void Initialize(string mapFilePath, float dpiScale)
        {
            List<string> mapFiles;

            if (mapFilePath.EndsWith(".map"))
            {
                mapFiles = [mapFilePath];
            }
            else
            {
                mapFiles = [.. Directory.EnumerateFiles(mapFilePath, "*.map")];
            }

            foreach (var mapFile in mapFiles)
            {
                Logger?.LogInformation("Loading {mapFile}", mapFile);
            }

            TileRenderer.Initialize(mapFiles, dpiScale);
        }

        public override Task<ImageSource> LoadImageAsync(int zoomLevel, int column, int row)
        {
            ImageSource image = null;

            try
            {
                var pixels = renderer.RenderTile(zoomLevel, column, row);

                if (pixels != null)
                {
                    image = CreateImage(pixels);
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "LoadImageAsync");
            }

            return Task.FromResult(image);
        }
    }
}

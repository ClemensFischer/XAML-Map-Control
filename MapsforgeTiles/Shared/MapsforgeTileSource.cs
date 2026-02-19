using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;

namespace MapControl.MapsforgeTiles
{
    public partial class MapsforgeTileSource(string theme, int cacheCapacity = 200, float textScale = 1f) : TileSource
    {
        private static ILogger Logger => field ??= ImageLoader.LoggerFactory?.CreateLogger<MapsforgeTileSource>();

        private readonly TileRenderer tileRenderer = new TileRenderer(theme, cacheCapacity, textScale);

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
    }
}

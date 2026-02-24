using Microsoft.Extensions.Logging;
using org.mapsforge.map.awt.graphics;
using org.mapsforge.map.datastore;
using org.mapsforge.map.layer.cache;
using org.mapsforge.map.layer.renderer;
using org.mapsforge.map.model;
using org.mapsforge.map.reader;
using org.mapsforge.map.rendertheme;
using org.mapsforge.map.rendertheme.@internal;
using org.mapsforge.map.rendertheme.rule;
using System.Collections.Generic;
using System.IO;

namespace MapControl.MapsforgeTiles
{
    public partial class MapsforgeTileSource : TileSource
    {
        private static ILogger Logger => field ??= ImageLoader.LoggerFactory?.CreateLogger<MapsforgeTileSource>();

        private static MapDataStore mapDataStore;

        private readonly DisplayModel displayModel;
        private readonly InMemoryTileCache tileCache;
        private readonly DatabaseRenderer renderer;
        private readonly RenderThemeFuture renderThemeFuture;

        public static void LoadMaps(string mapFileOrDirectory)
        {
            List<string> mapFiles;

            if (mapFileOrDirectory.EndsWith(".map"))
            {
                mapFiles = [mapFileOrDirectory];
            }
            else
            {
                mapFiles = [.. Directory.EnumerateFiles(mapFileOrDirectory, "*.map")];
            }

            LoadMapFiles(mapFiles);
        }

        public static void LoadMapFiles(List<string> mapFiles)
        {
            if (mapFiles.Count == 1)
            {
                Logger?.LogInformation("Loading {mapFile}", mapFiles[0]);

                mapDataStore = new MapFile(mapFiles[0]);
            }
            else
            {
                var multiMapDataStore = new MultiMapDataStore(MultiMapDataStore.DataPolicy.DEDUPLICATE);
                mapDataStore = multiMapDataStore;

                foreach (var mapFile in mapFiles)
                {
                    Logger?.LogInformation("Loading {mapFile}", mapFile);

                    multiMapDataStore.addMapDataStore(new MapFile(mapFile), false, false);
                }
            }
        }

        public MapsforgeTileSource(string theme, int cacheCapacity = 200)
        {
            XmlRenderTheme renderTheme;

            if (theme.EndsWith(".xml"))
            {
                renderTheme = new ExternalRenderTheme(theme);
            }
            else
            {
                renderTheme = MapsforgeThemes.valueOf(theme.ToUpper());
            }

            displayModel = new DisplayModel();
            tileCache = new InMemoryTileCache(cacheCapacity);
            renderer = new DatabaseRenderer(mapDataStore, AwtGraphicFactory.INSTANCE, tileCache, null, true, false, null);
            renderThemeFuture = new RenderThemeFuture(AwtGraphicFactory.INSTANCE, renderTheme, displayModel);
        }

        public float TextScale { get; set; } = 1f;

        public float DpiScale
        {
            get => displayModel.getUserScaleFactor();
            set => displayModel.setUserScaleFactor(value);
        }

        public int TileSize => displayModel.getTileSize();

        public int[] RenderTile(int zoomLevel, int column, int row)
        {
            if (!renderThemeFuture.isDone())
            {
                lock (renderThemeFuture)
                {
                    if (!renderThemeFuture.isDone())
                    {
                        Logger?.LogInformation("Loading render theme...");
                        renderThemeFuture.run();
                        Logger?.LogInformation("Loading render theme done.");
                    }
                }
            }

            int[] imageBuffer = null;
            var tile = new org.mapsforge.core.model.Tile(column, row, (byte)zoomLevel, displayModel.getTileSize());
            var job = new RendererJob(tile, mapDataStore, renderThemeFuture, displayModel, TextScale, false, false);
            var bitmap = tileCache.get(job) ?? renderer.executeJob(job);

            if (bitmap != null)
            {
                var image = AwtGraphicFactory.getBitmap(bitmap);

                if (image != null)
                {
                    imageBuffer = image.getRGB(0, 0, image.getWidth(), image.getHeight(), null, 0, image.getWidth());
                }
            }

            return imageBuffer;
        }
    }
}

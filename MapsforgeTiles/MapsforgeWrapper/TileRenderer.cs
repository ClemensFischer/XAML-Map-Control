using org.mapsforge.core.model;
using org.mapsforge.map.awt.graphics;
using org.mapsforge.map.datastore;
using org.mapsforge.map.layer.cache;
using org.mapsforge.map.layer.renderer;
using org.mapsforge.map.model;
using org.mapsforge.map.reader;
using org.mapsforge.map.rendertheme.@internal;
using org.mapsforge.map.rendertheme.rule;
using System.IO;

namespace MapsforgeWrapper
{
    public class TileRenderer
    {
        private static DisplayModel displayModel;
        private static MapDataStore dataStore;

        public static void Initialize(string mapFilePath, float dpiScale)
        {
            DisplayModel.setDeviceScaleFactor(dpiScale);
            displayModel = new DisplayModel();

            if (mapFilePath.EndsWith(".map"))
            {
                dataStore = new MapFile(mapFilePath);
            }
            else
            {
                var multiMapDataStore = new MultiMapDataStore(MultiMapDataStore.DataPolicy.DEDUPLICATE);
                foreach (var file in Directory.EnumerateFiles(mapFilePath, "*.map"))
                {
                    multiMapDataStore.addMapDataStore(new MapFile(file), false, false);
                }
                dataStore = multiMapDataStore;
            }
        }

        private readonly InMemoryTileCache tileCache;
        private readonly DatabaseRenderer renderer;
        private readonly RenderThemeFuture renderThemeFuture;

        public TileRenderer(string theme, int cacheCapacity = 200)
        {
            tileCache = new InMemoryTileCache(cacheCapacity);
            renderer = new DatabaseRenderer(dataStore, AwtGraphicFactory.INSTANCE, tileCache, null, true, false, null);
            renderThemeFuture = new RenderThemeFuture(AwtGraphicFactory.INSTANCE, MapsforgeThemes.valueOf(theme.ToUpper()), displayModel);
        }

        public int[] RenderTile(int zoomLevel, int column, int row)
        {
            if (!renderThemeFuture.isDone())
            {
                renderThemeFuture.run();
            }

            int[] imageBuffer = null;
            var tile = new Tile(column, row, (byte)zoomLevel, displayModel.getTileSize());
            var job = new RendererJob(tile, dataStore, renderThemeFuture, displayModel, 1f, false, false);
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

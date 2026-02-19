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

namespace MapControl.MapsforgeTiles
{
    public class TileRenderer
    {
        private static DisplayModel displayModel;
        private static MapDataStore mapDataStore;

        public static int TileSize => displayModel.getTileSize();

        public static void Initialize(List<string> mapFiles, float dpiScale)
        {
            DisplayModel.setDeviceScaleFactor(dpiScale);
            displayModel = new DisplayModel();

            if (mapFiles.Count == 1)
            {
                mapDataStore = new MapFile(mapFiles[0]);
            }
            else
            {
                var multiMapDataStore = new MultiMapDataStore(MultiMapDataStore.DataPolicy.DEDUPLICATE);
                mapDataStore = multiMapDataStore;

                foreach (var mapFile in mapFiles)
                {
                    multiMapDataStore.addMapDataStore(new MapFile(mapFile), false, false);
                }
            }
        }

        private readonly InMemoryTileCache tileCache;
        private readonly DatabaseRenderer renderer;
        private readonly RenderThemeFuture renderThemeFuture;
        private readonly float textScale;

        public TileRenderer(string theme, int cacheCapacity, float renderTextScale)
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

            tileCache = new InMemoryTileCache(cacheCapacity);
            renderer = new DatabaseRenderer(mapDataStore, AwtGraphicFactory.INSTANCE, tileCache, null, true, false, null);
            renderThemeFuture = new RenderThemeFuture(AwtGraphicFactory.INSTANCE, renderTheme, displayModel);
            textScale = renderTextScale;
        }

        public int[] RenderTile(int zoomLevel, int column, int row)
        {
            if (!renderThemeFuture.isDone())
            {
                renderThemeFuture.run();
            }

            int[] imageBuffer = null;
            var tile = new org.mapsforge.core.model.Tile(column, row, (byte)zoomLevel, displayModel.getTileSize());
            var job = new RendererJob(tile, mapDataStore, renderThemeFuture, displayModel, textScale, false, false);
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

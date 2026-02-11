package TileRenderer;

import org.mapsforge.core.graphics.TileBitmap;
import org.mapsforge.core.model.Tile;
import org.mapsforge.map.awt.graphics.AwtGraphicFactory;
import org.mapsforge.map.datastore.MapDataStore;
import org.mapsforge.map.datastore.MultiMapDataStore;
import org.mapsforge.map.layer.cache.InMemoryTileCache;
import org.mapsforge.map.layer.renderer.DatabaseRenderer;
import org.mapsforge.map.layer.renderer.RendererJob;
import org.mapsforge.map.model.DisplayModel;
import org.mapsforge.map.reader.MapFile;
import org.mapsforge.map.rendertheme.internal.MapsforgeThemes;
import org.mapsforge.map.rendertheme.rule.RenderThemeFuture;

import java.awt.image.BufferedImage;
import java.io.File;
import java.io.IOException;

public class MapsforgeTileRenderer {

    public static void SetDpiScale(float scale) {
        DisplayModel.setDeviceScaleFactor(scale);
    }

    private final MapDataStore dataStore;
    private final DisplayModel displayModel;
    private final InMemoryTileCache tileCache;
    private final DatabaseRenderer renderer;
    private final RenderThemeFuture renderThemeFuture;

    public MapsforgeTileRenderer(String mapFilePath, String theme, int cacheCapacity) {
        if (mapFilePath.endsWith(".map")) {
            dataStore = new MapFile(mapFilePath);
        } else {
            MultiMapDataStore multiMapDataStore = new MultiMapDataStore(MultiMapDataStore.DataPolicy.DEDUPLICATE);
            File dir = new File(mapFilePath);
            for (File mapFile : dir.listFiles((file, name) -> name.endsWith(".map"))) {
                multiMapDataStore.addMapDataStore(new MapFile(mapFile), false, false);
            }
            dataStore = multiMapDataStore;
        }

        displayModel = new DisplayModel();
        tileCache = new InMemoryTileCache(cacheCapacity);
        renderer = new DatabaseRenderer(dataStore, AwtGraphicFactory.INSTANCE, tileCache, null, true, false, null);
        renderThemeFuture = new RenderThemeFuture(AwtGraphicFactory.INSTANCE, MapsforgeThemes.valueOf(theme.toUpperCase()), displayModel);
    }

    public int[] RenderTile(int zoomLevel, int x, int y) throws IOException {
        if (!renderThemeFuture.isDone()) {
            renderThemeFuture.run();
        }

        int[] imageBuffer = null;
        Tile tile = new Tile(x, y, (byte) zoomLevel, displayModel.getTileSize());
        RendererJob job = new RendererJob(tile, dataStore, renderThemeFuture, displayModel, 1f, false, false);
        TileBitmap bitmap = tileCache.get(job);

        if (bitmap == null) {
            bitmap = renderer.executeJob(job);
        }

        if (bitmap != null) {
            BufferedImage image = AwtGraphicFactory.getBitmap(bitmap);

            if (image != null) {
                imageBuffer = image.getRGB(0, 0, image.getWidth(), image.getHeight(), null, 0, image.getWidth());
            }
        }

        return imageBuffer;
    }
}

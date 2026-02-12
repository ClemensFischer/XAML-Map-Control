# MapsforgeTiles

Tile source libraries for loading map content from vector map files.

The `MapsforgeTileSource` classes in these libraries make use of a `TileRenderer` class in a separate platform-independent
.NET class library called MapsforgeWrapper. `TileRenderer` uses the [Mapsforge](https://github.com/mapsforge/mapsforge)
Java library, which is made accessible to .NET via [IKVM](https://github.com/ikvmnet/ikvm), a Java Virtual Machine for .NET.
[Mapsforge](https://github.com/mapsforge/mapsforge) is published under a simplified variant of the
[LGPL v3 license](https://www.gnu.org/licenses/lgpl-3.0). Copies of the LGPL and GPL are included here.

Map files can be downloaded from the [Mapsforge Download Server](https://download.mapsforge.org/).

`MapsforgeTileSource` is initialized by a static `Initialize` method that takes the file path to either a single map file
or a directory containing multiple map files, and a DPI scale factor that controls the size of the rendered map tiles.

The `MapsforgeTileSource` instance constructor takes a string parameter that specifies the Mapsforge theme used by its `TileRenderer`.
See [MapsforgeThemes.java](https://github.com/mapsforge/mapsforge/blob/master/mapsforge-themes/src/main/java/org/mapsforge/map/rendertheme/internal/MapsforgeThemes.java)
for available theme names. A second, optional constructor parameter specifies the size of the TileRenderer's internal tile cache.

Code sample:
```
MapControl.MapsforgeTiles.MapsforgeTileSource.Initialize(".\mapfiles", 1.5f);

map.MapLayer = new MapTileLayer
{
    TileSource = new MapControl.MapsforgeTiles.MapsforgeTileSource("Default")
};
```

---

Apparently, IKVM's `MavenReference` does not work with other Maven repositories than Maven Central.
Mapsforge however, is hosted by JitPack. So the currently only working way to utilize Mapsforge is
by creating a local JAR file with all dependencies required by `TileRenderer` and reference it via
`IkvmReference`.

This means that you need [Maven](https://maven.apache.org/) to build the MapsforgeWrapper library.
There is a custom `PreBuild` event in `MapsforgeWrapper.csproj` which executes the command
```
mvn package
```
in the project file's directory.
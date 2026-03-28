# MapsforgeTiles

Tile source libraries for loading map content from vector map files.

The `MapsforgeTileSource` class in these libraries uses parts of the [Mapsforge](https://github.com/mapsforge/mapsforge)
Java library, which is made accessible to .NET via [IKVM](https://github.com/ikvmnet/ikvm), a Java Virtual Machine for .NET.
[Mapsforge](https://github.com/mapsforge/mapsforge) is published under a simplified variant of the
[LGPL v3 license](https://www.gnu.org/licenses/lgpl-3.0).

Map files can be downloaded from the [Mapsforge Download Server](https://download.mapsforge.org/).

`MapsforgeTileSource` is initialized by a static `LoadMaps` method that takes the file path to either a single map file
or a directory containing multiple map files.

The class has a `Theme` property that specifies a Mapsforge theme. This is either the path of an XML `rendertheme` file,
the path of a ZIP file containing a render theme, or the name of one of the built-in themes (ignoring case), e.g. `Default`.
See [MapsforgeThemes.java](https://github.com/mapsforge/mapsforge/blob/master/mapsforge-themes/src/main/java/org/mapsforge/map/rendertheme/internal/MapsforgeThemes.java)
for available theme names.

Code sample:
```
MapControl.MapsforgeTiles.MapsforgeTileSource.LoadMaps(".\mapfiles");

var tileSource = new MapControl.MapsforgeTiles.MapsforgeTileSource { Theme = "Default" };

map.MapLayer = new MapTileLayer { TileSource = tileSource };
```

---

While building with IKVM's `MavenReference` succeeds, running a `RenderThemeFuture`
always fails with a `NoClassDefFoundError` exception for `org.xmlpull.v1.XmlPullParserFactory`.

An alternative approach is to import Mapsforge classes by an `IkvmReference` that references a
local JAR file with all required dependencies. This JAR is built from `pom.xml` in the MapsforgeTiles
directory, by a custom PreBuild event in the project files which executes the command
```
mvn package
```

So in order to build the MapsforgeTiles libraries, a [Maven](https://maven.apache.org/)
installation is required.

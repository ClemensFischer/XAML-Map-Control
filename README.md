# XAML Map Control

A set of controls for WPF, UWP, WinUI and Avalonia UI for rendering raster maps from different providers and various types of map overlays.

Map providers can easily be added by specifying a template string for their map tile URLs. 
Map overlay layers allow to draw and interact with graphical objects and pushpins on the map.
The Map Control API is similar to the Microsoft Bing Maps Control for WPF, except that no API key is required.

The project includes sample applications for all platforms, which demonstrate the features of XAML Map Control.

Map Control supports multiple map projections. However, the MapTileLayer class only works with WebMercatorProjection.
For other projections, an appropriate WmtsTileLayer or WmsImageLayer could be used.

---

Main classes are

- `MapBase`: The core map control. Provides properties like Center, ZoomLevel and Heading,
which define the currently displayed map viewport.

- `Map`: MapBase with basic mouse and touch input handling for zoom, pan, and rotation.

- `MapTileLayer`: Provides [tiled map content](https://wiki.openstreetmap.org/wiki/Raster_tile_providers) (e.g. from OpenStreetMap) by means of a `TileSource`.

- `WmtsTileLayer`: Provides tiled map content from a Web Map Tile Service ([WMTS](https://en.wikipedia.org/wiki/Web_Map_Tile_Service)).

- `MapImageLayer`, `WmsImageLayer`: Provides single image map content, e.g. from a Web Map Service ([WMS](https://en.wikipedia.org/wiki/Web_Map_Service)).

- `MapItemsControl`: Displays a collection of `MapItem` objects (with a geographic location).

---

In order to use OpenStreetMap tile servers in accordance with their [Tile Usage Policy](https://operations.osmfoundation.org/policies/tiles/),
your application must set a unique HTTP `User-Agent` request header, e.g. by adding it to the `DefaultRequestHeaders`
of the `HttpClient` instance returned by the static `ImageLoader.HttpClient` property:

    ImageLoader.HttpClient.DefaultRequestHeaders.Add("User-Agent", "Your-Application/1.0");

The tile usage policy also requires that an application caches map tiles locally.

The `TileImageLoader` class uses
[`IDistributedCache`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.caching.distributed.idistributedcache)
for optional caching of map tile bitmaps. By default, the cache is a
[`MemoryDistributedCache`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.caching.distributed.memorydistributedcache)
instance with all default options.

Map Control provides three minimal `IDistributedCache` implementations for persistent caching:
* `ImageFileCache`, an implementation that stores each cached map tile as a single image file,
in the original file format delivered by the map provider (typically PNG or JPG). ImageFileCache is part of the MapControl library.
* `FileDbCache`, based on [EzTools FileDb](https://github.com/eztools-software/FileDb),
a simple file based No-SQL database, in a separate library FileDbCache.
* `SQLiteCache`, based on [System.Data.SQLite](https://system.data.sqlite.org/index.html/doc/trunk/www/index.wiki),
in a separate library SQLiteCache.

You can of course also use a full-featured cache like
[`RedisCache`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.caching.stackexchangeredis.rediscache).

To try the sample applications with `FileDbCache` or `SQLiteCache`, add an appropriate package reference to their
Visual Studio projects and modify the `TileImageLoader.Cache` assignment accordingly in `MainWindow.xaml.cs`.

---

XAML Map Control is available on NuGet, as a set of framework-specific packages with Package Ids
* [XAML.MapControl.WPF](https://www.nuget.org/packages/XAML.MapControl.WPF/),
* [XAML.MapControl.UWP](https://www.nuget.org/packages/XAML.MapControl.UWP/),
* [XAML.MapControl.WinUI](https://www.nuget.org/packages/XAML.MapControl.WinUI/),
* [XAML.MapControl.Avalonia](https://www.nuget.org/packages/XAML.MapControl.Avalonia/).

There are also packages for an extension library with additional map projections, based on
[ProjNET4GeoAPI](https://github.com/NetTopologySuite/ProjNet4GeoAPI), with Package Ids
* [XAML.MapControl.MapProjections.WPF](https://www.nuget.org/packages/XAML.MapControl.MapProjections.WPF/),
* [XAML.MapControl.MapProjections.UWP](https://www.nuget.org/packages/XAML.MapControl.MapProjections.UWP/),
* [XAML.MapControl.MapProjections.WinUI](https://www.nuget.org/packages/XAML.MapControl.MapProjections.WinUI/),
* [XAML.MapControl.MapProjections.Avalonia](https://www.nuget.org/packages/XAML.MapControl.MapProjections.Avalonia/),

and a library for [MBTiles](https://wiki.openstreetmap.org/wiki/MBTiles) support, with Package Ids
* [XAML.MapControl.MBTiles.WPF](https://www.nuget.org/packages/XAML.MapControl.MBTiles.WPF/),
* [XAML.MapControl.MBTiles.UWP](https://www.nuget.org/packages/XAML.MapControl.MBTiles.UWP/),
* [XAML.MapControl.MBTiles.WinUI](https://www.nuget.org/packages/XAML.MapControl.MBTiles.WinUI/),
* [XAML.MapControl.MBTiles.Avalonia](https://www.nuget.org/packages/XAML.MapControl.MBTiles.Avalonia/),

FileDbCache and SQLiteCache are available with Package Ids
* [XAML.MapControl.FileDbCache](https://www.nuget.org/packages/XAML.MapControl.FileDbCache/),
* [XAML.MapControl.SQLiteCache](https://www.nuget.org/packages/XAML.MapControl.SQLiteCache/).

---

The project is not open for contributions. Pull requests will not be accepted.

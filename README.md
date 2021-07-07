# XAML Map Control

A set of controls for WPF, UWP and WinUI for rendering digital maps from different providers and various types of map overlays.

Map providers can easily be added by specifying a template string for their map tile URLs. 
Map overlay layers allow to draw and interact with graphical objects and pushpins on the map.
The Map Control API is similar to the Microsoft Bing Maps Control for WPF, except that no API key is required.

The project includes sample applications for both platforms, which demonstrate the features of XAML Map Control.

Map Control supports multiple map projections. However, the MapTileLayer class only works with WebMercatorProjection.
For other projections, an appropriate WmtsTileLayer or WmsImageLayer could be used.

---

Main classes are

- **MapBase**: The core map control. Provides properties like Center, ZoomLevel and Heading,
which define the currently displayed map viewport.

- **Map**: MapBase with basic mouse and touch input handling for zoom, pan, and rotation.

- **MapTileLayer**: Provides tiled map content (e.g. from OpenStreetMap) by means of a **TileSource**.

- **MapImageLayer**, **WmsImageLayer**: Provides single image map content, e.g. from a Web Map Service (WMS).

- **WmtsTileLayer**: Provides tiled map content from a Web Map Tile Service (WMTS).

- **MapItemsControl**: Displays a collection of **MapItem** objects (with a geographic **Location**).

---

The WPF version allows to use a System.Runtime.Caching.ObjectCache instance for caching map tile bitmaps.
The cache may be set to an instance of System.Runtime.Caching.MemoryCache (e.g. MemoryCache.Default),
but caching can also be done persistently by some specialized ObjectCache implementation.
Map Control comes with three such implementations:
* ImageFileCache, an ObjectCache implementation that stores each cached map tile as a single image file,
in the original file format delivered by the map provider (typically PNG or JPG). ImageFileCache is part of
the MapControl.WPF library. It does not support expiration, which means that cached tile image files will
not be deleted automatically. The cache may hence consume a considerable amount of disk space.
* FileDbCache, an ObjectCache implementation based on [EzTools FileDb](https://github.com/eztools-software/FileDb),
a simple, file based No-SQL database, in a separate library FileDbCache.WPF.
* SQLiteCache, an ObjectCache implementation based on [System.Data.SQLite](https://system.data.sqlite.org/index.html/doc/trunk/www/index.wiki),
in a separate library SQLiteCache.WPF.

If you want to try the sample application with persistent caching, uncomment the appropriate TileImageLoader.Cache
setting in the sample application's MainWindow.xaml.cs file. Please note that some map providers may not allow
persistent caching of their map data.

For UWP and WinUI, the cache functionality is defined by the interface IImageCache in the namespace MapControl.Caching,
and implemented by the classes ImageFileCache, FileDbCache (in library FileDbCache.UWP/.WinUI) and SQLiteCache
(in library SQLiteCache.UWP/.WinUI).
Local image files and database files are written to ApplicationData.Current.TemporaryFolder by default.

---

XAML Map Control is available on NuGet, with Package Id [XAML.MapControl](https://www.nuget.org/packages/XAML.MapControl/).

---

The project is not open for contributions. Pull requests will not be accepted.

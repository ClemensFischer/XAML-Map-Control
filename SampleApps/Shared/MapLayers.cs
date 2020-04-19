using MapControl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
#if WINDOWS_UWP
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Media;
#endif

namespace ViewModel
{
    public class MapLayers : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly Dictionary<string, UIElement> mapLayers = new Dictionary<string, UIElement>
        {
            {
                "OpenStreetMap",
                new MapTileLayer
                {
                    SourceName = "OpenStreetMap",
                    Description = "© [OpenStreetMap Contributors](http://www.openstreetmap.org/copyright)",
                    TileSource = new TileSource { UriFormat = "https://{c}.tile.openstreetmap.org/{z}/{x}/{y}.png" },
                    MaxZoomLevel = 19
                }
            },
            {
                "OpenStreetMap German",
                new MapTileLayer
                {
                    SourceName = "OpenStreetMap German",
                    Description = "© [OpenStreetMap contributors](http://www.openstreetmap.org/copyright)",
                    TileSource = new TileSource { UriFormat = "https://{c}.tile.openstreetmap.de/tiles/osmde/{z}/{x}/{y}.png" },
                    MaxZoomLevel = 19
                }
            },
            {
                "Stamen Terrain",
                new MapTileLayer
                {
                    SourceName = "Stamen Terrain",
                    Description = "Map tiles by [Stamen Design](http://stamen.com/), under [CC BY 3.0](http://creativecommons.org/licenses/by/3.0)\nData by [OpenStreetMap](http://openstreetmap.org/), under [ODbL](http://www.openstreetmap.org/copyright)",
                    TileSource = new TileSource { UriFormat = "http://tile.stamen.com/terrain/{z}/{x}/{y}.png" },
                    MaxZoomLevel = 17
                }
            },
            {
                "Stamen Toner Light",
                new MapTileLayer
                {
                    SourceName = "Stamen Toner Light",
                    Description = "Map tiles by [Stamen Design](http://stamen.com/), under [CC BY 3.0](http://creativecommons.org/licenses/by/3.0)\nData by [OpenStreetMap](http://openstreetmap.org/), under [ODbL](http://www.openstreetmap.org/copyright)",
                    TileSource = new TileSource { UriFormat = "http://tile.stamen.com/toner-lite/{z}/{x}/{y}.png" },
                    MaxZoomLevel = 18
                }
            },
            {
                "Bing Maps Road",
                new BingMapsTileLayer
                {
                    SourceName = "Bing Maps Road",
                    Description = "© [Microsoft](http://www.bing.com/maps/)",
                    Mode = BingMapsTileLayer.MapMode.Road
                }
            },
            {
                "Bing Maps Aerial",
                new BingMapsTileLayer
                {
                    SourceName = "Bing Maps Aerial",
                    Description = "© [Microsoft](http://www.bing.com/maps/)",
                    Mode = BingMapsTileLayer.MapMode.Aerial,
                    MaxZoomLevel = 21,
                    MapForeground = new SolidColorBrush(Colors.White),
                    MapBackground = new SolidColorBrush(Colors.Black)
                }
            },
            {
                "Bing Maps Aerial with Labels",
                new BingMapsTileLayer
                {
                    SourceName = "Bing Maps Hybrid",
                    Description = "© [Microsoft](http://www.bing.com/maps/)",
                    Mode = BingMapsTileLayer.MapMode.AerialWithLabels,
                    MapForeground = new SolidColorBrush(Colors.White),
                    MapBackground = new SolidColorBrush(Colors.Black)
                }
            },
            {
                "OpenStreetMap WMS",
                new WmsImageLayer
                {
                    Description = "© [terrestris GmbH & Co. KG](http://ows.terrestris.de/)\nData © [OpenStreetMap contributors](http://www.openstreetmap.org/copyright)",
                    ServiceUri = new Uri("http://ows.terrestris.de/osm/service")
                }
            },
            {
                "TopPlusOpen WMS",
                new WmsImageLayer
                {
                    Description = "© [BKG](https://gdz.bkg.bund.de/index.php/default/webdienste/topplus-produkte/wms-topplusopen-mit-layer-fur-normalausgabe-und-druck-wms-topplus-open.html)",
                    ServiceUri = new Uri("https://sgx.geodatenzentrum.de/wms_topplus_open")
                }
            },
            {
                "TopPlusOpen WMTS",
                new WmtsTileLayer
                {
                    Description = "© [BKG](https://gdz.bkg.bund.de/index.php/default/webdienste/topplus-produkte/wmts-topplusopen-wmts-topplus-open.html)",
                    CapabilitiesUri = new Uri("https://sgx.geodatenzentrum.de/wmts_topplus_open/1.0.0/WMTSCapabilities.xml"),
                    SourceName = "TopPlusOpen"
                }
            },
            {
                "SevenCs ChartServer WMS",
                new ChartServerLayer()
            },
            {
                "Seamarks",
                new MapTileLayer
                {
                    SourceName = "OpenSeaMap",
                    TileSource = new TileSource { UriFormat = "http://tiles.openseamap.org/seamark/{z}/{x}/{y}.png" },
                    MinZoomLevel = 9,
                    MaxZoomLevel = 18
                }
            },
        };

        private string currentMapLayerName = "OpenStreetMap";

        public string CurrentMapLayerName
        {
            get { return currentMapLayerName; }
            set
            {
                currentMapLayerName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentMapLayerName)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentMapLayer)));
            }
        }

        public UIElement CurrentMapLayer
        {
            get { return mapLayers[currentMapLayerName]; }
        }

        public UIElement SeamarksLayer
        {
            get { return mapLayers["Seamarks"]; }
        }

        public List<string> MapLayerNames { get; } = new List<string>
        {
            "OpenStreetMap",
            "OpenStreetMap German",
            "Stamen Terrain",
            "Stamen Toner Light",
            "OpenStreetMap WMS",
            "TopPlusOpen WMS",
            "TopPlusOpen WMTS",
            "SevenCs ChartServer WMS",
        };

        public MapLayers()
        {
            //BingMapsTileLayer.ApiKey = "...";

            // Bing Maps TileLayers with tile URLs retrieved from the Imagery Metadata Service
            // (see http://msdn.microsoft.com/en-us/library/ff701716.aspx).
            // A Bing Maps API Key (see http://msdn.microsoft.com/en-us/library/ff428642.aspx) is required
            // for using these layers and must be assigned to the static BingMapsTileLayer.ApiKey property.

            if (!string.IsNullOrEmpty(BingMapsTileLayer.ApiKey))
            {
                MapLayerNames.Add("Bing Maps Road");
                MapLayerNames.Add("Bing Maps Aerial");
                MapLayerNames.Add("Bing Maps Aerial with Labels");
            }
        }
    }

    public class ChartServerLayer : WmsImageLayer
    {
        public ChartServerLayer()
        {
            Description = "© [SevenCs GmbH](http://www.sevencs.com)";
            ServiceUri = new Uri("https://wms.sevencs.com:9090");
            Layers = "ENC";
            MaxBoundingBoxWidth = 360;
        }

        protected override string GetImageUri()
        {
            return base.GetImageUri()
                .Replace("&CRS=AUTO2:97001,", "&CRS=AUTO2:7CS01,")
                .Replace("&CRS=AUTO2:97002,", "&CRS=AUTO2:7CS02,");
        }
    }
}

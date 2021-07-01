using MapControl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
#if WINUI
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
#elif WINDOWS_UWP
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
                    TileSource = new TileSource { UriFormat = "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png" },
                    SourceName = "OpenStreetMap",
                    Description = "© [OpenStreetMap contributors](http://www.openstreetmap.org/copyright)"
                }
            },
            {
                "OpenStreetMap German",
                new MapTileLayer
                {
                    TileSource = new TileSource { UriFormat = "https://{s}.tile.openstreetmap.de/{z}/{x}/{y}.png" },
                    SourceName = "OpenStreetMap German",
                    Description = "© [OpenStreetMap contributors](http://www.openstreetmap.org/copyright)"
                }
            },
            {
                "OpenStreetMap French",
                new MapTileLayer
                {
                    TileSource = new TileSource { UriFormat = "https://{s}.tile.openstreetmap.fr/osmfr/{z}/{x}/{y}.png" },
                    SourceName = "OpenStreetMap French",
                    Description = "© [OpenStreetMap France](https://www.openstreetmap.fr/mentions-legales/) © [OpenStreetMap contributors](http://www.openstreetmap.org/copyright)"
                }
            },
            {
                "OpenTopoMap",
                new MapTileLayer
                {
                    TileSource = new TileSource { UriFormat = "https://{s}.tile.opentopomap.org/{z}/{x}/{y}.png" },
                    SourceName = "OpenTopoMap",
                    Description = "© [OpenTopoMap](https://opentopomap.org/) © [OpenStreetMap contributors](http://www.openstreetmap.org/copyright)",
                    MaxZoomLevel = 17
                }
            },
            {
                "Seamarks",
                new MapTileLayer
                {
                    TileSource = new TileSource { UriFormat = "http://tiles.openseamap.org/seamark/{z}/{x}/{y}.png" },
                    SourceName = "OpenSeaMap",
                    MinZoomLevel = 9,
                    MaxZoomLevel = 18
                }
            },
            {
                "Bing Maps Road",
                new BingMapsTileLayer
                {
                    Mode = BingMapsTileLayer.MapMode.Road,
                    SourceName = "Bing Maps Road",
                    Description = "© [Microsoft](http://www.bing.com/maps/)"
                }
            },
            {
                "Bing Maps Aerial",
                new BingMapsTileLayer
                {
                    Mode = BingMapsTileLayer.MapMode.Aerial,
                    SourceName = "Bing Maps Aerial",
                    Description = "© [Microsoft](http://www.bing.com/maps/)",
                    MapForeground = new SolidColorBrush(Colors.White),
                    MapBackground = new SolidColorBrush(Colors.Black)
                }
            },
            {
                "Bing Maps Aerial with Labels",
                new BingMapsTileLayer
                {
                    Mode = BingMapsTileLayer.MapMode.AerialWithLabels,
                    SourceName = "Bing Maps Hybrid",
                    Description = "© [Microsoft](http://www.bing.com/maps/)",
                    MapForeground = new SolidColorBrush(Colors.White),
                    MapBackground = new SolidColorBrush(Colors.Black)
                }
            },
            {
                "TopPlusOpen WMTS",
                new WmtsTileLayer
                {
                    SourceName = "TopPlusOpen",
                    Description = "© [BKG](https://gdz.bkg.bund.de/index.php/default/webdienste/topplus-produkte/wmts-topplusopen-wmts-topplus-open.html)",
                    CapabilitiesUri = new Uri("https://sgx.geodatenzentrum.de/wmts_topplus_open/1.0.0/WMTSCapabilities.xml")
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
                "OpenStreetMap WMS",
                new WmsImageLayer
                {
                    Description = "© [terrestris GmbH & Co. KG](http://ows.terrestris.de/) © [OpenStreetMap contributors](http://www.openstreetmap.org/copyright)",
                    ServiceUri = new Uri("http://ows.terrestris.de/osm/service")
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
            "OpenStreetMap French",
            "OpenTopoMap",
            "TopPlusOpen WMTS",
            "TopPlusOpen WMS",
            "OpenStreetMap WMS",
        };

        public MapLayers()
        {
            // Add Bing Maps TileLayers with tile URLs retrieved from the Imagery Metadata Service
            // (http://msdn.microsoft.com/en-us/library/ff701716.aspx).
            // A Bing Maps API Key (http://msdn.microsoft.com/en-us/library/ff428642.aspx) is required
            // for using these layers and must be assigned to the static BingMapsTileLayer.ApiKey property.

            if (!string.IsNullOrEmpty(BingMapsTileLayer.ApiKey))
            {
                MapLayerNames.Add("Bing Maps Road");
                MapLayerNames.Add("Bing Maps Aerial");
                MapLayerNames.Add("Bing Maps Aerial with Labels");
            }
        }
    }
}

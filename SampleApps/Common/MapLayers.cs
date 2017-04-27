using MapControl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
#if NETFX_CORE
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
                MapTileLayer.OpenStreetMapTileLayer
            },
            {
                "OpenStreetMap German Style",
                new MapTileLayer
                {
                    SourceName = "OpenStreetMap German",
                    Description = "© [OpenStreetMap Contributors](http://www.openstreetmap.org/copyright)",
                    TileSource = new TileSource { UriFormat = "http://{c}.tile.openstreetmap.de/tiles/osmde/{z}/{x}/{y}.png" }
                }
            },
            {
                "Thunderforest OpenCycleMap",
                new MapTileLayer
                {
                    SourceName = "Thunderforest OpenCycleMap",
                    Description = "Maps © [Thunderforest](http://www.thunderforest.com/), Data © [OpenStreetMap Contributors](http://www.openstreetmap.org/copyright)",
                    TileSource = new TileSource { UriFormat = "http://{c}.tile.thunderforest.com/cycle/{z}/{x}/{y}.png" }
                }
            },
            {
                "Thunderforest Landscape",
                new MapTileLayer
                {
                    SourceName = "Thunderforest Landscape",
                    Description = "Maps © [Thunderforest](http://www.thunderforest.com/), Data © [OpenStreetMap Contributors](http://www.openstreetmap.org/copyright)",
                    TileSource = new TileSource { UriFormat = "http://{c}.tile.thunderforest.com/landscape/{z}/{x}/{y}.png" }
                }
            },
            {
                "Thunderforest Outdoors",
                new MapTileLayer
                {
                    SourceName = "Thunderforest Outdoors",
                    Description = "Maps © [Thunderforest](http://www.thunderforest.com/), Data © [OpenStreetMap Contributors](http://www.openstreetmap.org/copyright)",
                    TileSource = new TileSource { UriFormat = "http://{c}.tile.thunderforest.com/outdoors/{z}/{x}/{y}.png" }
                }
            },
            {
                "Thunderforest Transport",
                new MapTileLayer
                {
                    SourceName = "Thunderforest Transport",
                    Description = "Maps © [Thunderforest](http://www.thunderforest.com/), Data © [OpenStreetMap Contributors](http://www.openstreetmap.org/copyright)",
                    TileSource = new TileSource { UriFormat = "http://{c}.tile.thunderforest.com/transport/{z}/{x}/{y}.png" }
                }
            },
            {
                "Thunderforest Transport Dark",
                new MapTileLayer
                {
                    SourceName = "Thunderforest Transport Dark",
                    Description = "Maps © [Thunderforest](http://www.thunderforest.com/), Data © [OpenStreetMap Contributors](http://www.openstreetmap.org/copyright)",
                    TileSource = new TileSource { UriFormat = "http://{c}.tile.thunderforest.com/transport-dark/{z}/{x}/{y}.png" },
                    MapForeground = new SolidColorBrush(Colors.White),
                    MapBackground = new SolidColorBrush(Colors.Black)
                }
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
            {
                "Bing Maps Road",
                new BingMapsTileLayer
                {
                    SourceName = "Bing Maps Road",
                    Description = "© [Microsoft Corporation](http://www.bing.com/maps/)",
                    Mode = BingMapsTileLayer.MapMode.Road,
                    MaxZoomLevel = 19
                }
            },
            {
                "Bing Maps Aerial",
                new BingMapsTileLayer
                {
                    SourceName = "Bing Maps Aerial",
                    Description = "© [Microsoft Corporation](http://www.bing.com/maps/)",
                    Mode = BingMapsTileLayer.MapMode.Aerial,
                    MaxZoomLevel = 19,
                    MapForeground = new SolidColorBrush(Colors.White),
                    MapBackground = new SolidColorBrush(Colors.Black)
                }
            },
            {
                "Bing Maps Aerial with Labels",
                new BingMapsTileLayer
                {
                    SourceName = "Bing Maps Hybrid",
                    Description = "© [Microsoft Corporation](http://www.bing.com/maps/)",
                    Mode = BingMapsTileLayer.MapMode.AerialWithLabels,
                    MaxZoomLevel = 19,
                    MapForeground = new SolidColorBrush(Colors.White),
                    MapBackground = new SolidColorBrush(Colors.Black)
                }
            },
            {
                "OpenStreetMap WMS",
                new WmsImageLayer
                {
                    Description = "OpenStreetMap WMS",
                    ServerUri = new Uri("http://ows.terrestris.de/osm/service"),
                    Layers = "OSM-WMS",
                    MapBackground = new SolidColorBrush(Colors.LightGray)
                }
            },
            {
                "OpenStreetMap TOPO WMS",
                new WmsImageLayer
                {
                    Description = "OpenStreetMap TOPO WMS",
                    ServerUri = new Uri("http://ows.terrestris.de/osm/service"),
                    Layers = "TOPO-OSM-WMS",
                    MapBackground = new SolidColorBrush(Colors.LightGray)
                }
            }
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
            "OpenStreetMap German Style",
            "Thunderforest OpenCycleMap",
            //"Thunderforest Landscape",
            //"Thunderforest Outdoors",
            //"Thunderforest Transport",
            //"Thunderforest Transport Dark",
            "OpenStreetMap WMS",
            "OpenStreetMap TOPO WMS"
        };

        //public MapLayers()
        //{
        //    BingMapsTileLayer.ApiKey = "...";

        //    // Bing Maps TileLayers with tile URLs retrieved from the Imagery Metadata Service
        //    // (see http://msdn.microsoft.com/en-us/library/ff701716.aspx).
        //    // A Bing Maps API Key (see http://msdn.microsoft.com/en-us/library/ff428642.aspx) is required
        //    // for using these layers and must be assigned to the static BingMapsTileLayer.ApiKey property.

        //    MapLayerNames.Add("Bing Maps Road");
        //    MapLayerNames.Add("Bing Maps Aerial");
        //    MapLayerNames.Add("Bing Maps Aerial with Labels");
        //}
    }
}

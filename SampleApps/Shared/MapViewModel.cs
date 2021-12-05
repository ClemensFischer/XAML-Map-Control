using MapControl;
using System;
using System.Collections.Generic;
#if WINUI
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
#elif UWP
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
#endif

namespace SampleApplication
{
    public class PointItem
    {
        public string Name { get; set; }

        public Location Location { get; set; }
    }

    public class PolylineItem
    {
        public LocationCollection Locations { get; set; }
    }

    public class MapViewModel
    {
        public List<PointItem> Points { get; } = new List<PointItem>();
        public List<PointItem> Pushpins { get; } = new List<PointItem>();
        public List<PolylineItem> Polylines { get; } = new List<PolylineItem>();

        public Dictionary<string, MapProjection> MapProjections { get; } = new Dictionary<string, MapProjection>
        {
            { "Web Mercator", new WebMercatorProjection() },
            { "World Mercator", new WorldMercatorProjection() },
            { "Equirectangular", new EquirectangularProjection() },
            { "Orthographic", new OrthographicProjection() },
            { "Gnomonic", new GnomonicProjection() },
            { "Stereographic", new StereographicProjection() }
        };

        public Dictionary<string, UIElement> MapLayers { get; } = new Dictionary<string, UIElement>
        {
            {
                "OpenStreetMap",
                new MapTileLayer
                {
                    TileSource = new TileSource { UriFormat = "https://tile.openstreetmap.org/{z}/{x}/{y}.png" },
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
                    TileSource = new TileSource { UriFormat = "http://{s}.tile.openstreetmap.fr/osmfr/{z}/{x}/{y}.png" },
                    SourceName = "OpenStreetMap French",
                    Description = "© [OpenStreetMap France](https://www.openstreetmap.fr/mentions-legales/) © [OpenStreetMap contributors](http://www.openstreetmap.org/copyright)"
                }
            },
            {
                "OpenTopoMap",
                new MapTileLayer
                {
                    TileSource = new TileSource { UriFormat = "https://tile.opentopomap.org/{z}/{x}/{y}.png" },
                    SourceName = "OpenTopoMap",
                    Description = "© [OpenTopoMap](https://opentopomap.org/) © [OpenStreetMap contributors](http://www.openstreetmap.org/copyright)",
                    MaxZoomLevel = 17
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
            }
        };

        public Dictionary<string, UIElement> MapOverlays { get; } = new Dictionary<string, UIElement>
        {
            {
                "Sample Image",
                new Image()
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
                "Graticule",
                new MapGraticule
                {
                    Opacity = 0.75
                }
            },
            {
                "Scale",
                new MapScale
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Bottom
                }
            }
        };

        public MapViewModel()
        {
            // Add Bing Maps TileLayers with tile URLs retrieved from the Imagery Metadata Service
            // (http://msdn.microsoft.com/en-us/library/ff701716.aspx).
            // A Bing Maps API Key (http://msdn.microsoft.com/en-us/library/ff428642.aspx) is required
            // for using these layers and must be assigned to the static BingMapsTileLayer.ApiKey property.

            if (!string.IsNullOrEmpty(BingMapsTileLayer.ApiKey))
            {
                MapLayers.Add(
                    "Bing Maps Road",
                    new BingMapsTileLayer
                    {
                        Mode = BingMapsTileLayer.MapMode.Road,
                        SourceName = "Bing Maps Road",
                        Description = "© [Microsoft](http://www.bing.com/maps/)"
                    });

                MapLayers.Add(
                    "Bing Maps Aerial",
                    new BingMapsTileLayer
                    {
                        Mode = BingMapsTileLayer.MapMode.Aerial,
                        SourceName = "Bing Maps Aerial",
                        Description = "© [Microsoft](http://www.bing.com/maps/)",
                        MapForeground = new SolidColorBrush(Colors.White),
                        MapBackground = new SolidColorBrush(Colors.Black)
                    });

                MapLayers.Add(
                    "Bing Maps Aerial with Labels",
                    new BingMapsTileLayer
                    {
                        Mode = BingMapsTileLayer.MapMode.AerialWithLabels,
                        SourceName = "Bing Maps Hybrid",
                        Description = "© [Microsoft](http://www.bing.com/maps/)",
                        MapForeground = new SolidColorBrush(Colors.White),
                        MapBackground = new SolidColorBrush(Colors.Black)
                    });
            }

            var sampleImage = (Image)MapOverlays["Sample Image"];
#if WINUI || UWP
            sampleImage.Source = new BitmapImage(new Uri("ms-appx:///10_535_330.jpg"));
#else
            sampleImage.Source = new BitmapImage(new Uri("pack://siteoforigin:,,,/10_535_330.jpg"));
#endif
            MapPanel.SetBoundingBox(sampleImage, new BoundingBox(53.54031, 8.08594, 53.74871, 8.43750));

            Points.Add(new PointItem
            {
                Name = "Steinbake Leitdamm",
                Location = new Location(53.51217, 8.16603)
            });

            Points.Add(new PointItem
            {
                Name = "Buhne 2",
                Location = new Location(53.50926, 8.15815)
            });

            Points.Add(new PointItem
            {
                Name = "Buhne 4",
                Location = new Location(53.50468, 8.15343)
            });

            Points.Add(new PointItem
            {
                Name = "Buhne 6",
                Location = new Location(53.50092, 8.15267)
            });

            Points.Add(new PointItem
            {
                Name = "Buhne 8",
                Location = new Location(53.49871, 8.15321)
            });

            Points.Add(new PointItem
            {
                Name = "Buhne 10",
                Location = new Location(53.49350, 8.15563)
            });

            Pushpins.Add(new PointItem
            {
                Name = "WHV - Eckwarderhörne",
                Location = new Location(53.5495, 8.1877)
            });

            Pushpins.Add(new PointItem
            {
                Name = "JadeWeserPort",
                Location = new Location(53.5914, 8.14)
            });

            Pushpins.Add(new PointItem
            {
                Name = "Kurhaus Dangast",
                Location = new Location(53.447, 8.1114)
            });

            Pushpins.Add(new PointItem
            {
                Name = "Eckwarderhörne",
                Location = new Location(53.5207, 8.2323)
            });

            Polylines.Add(new PolylineItem
            {
                Locations = LocationCollection.Parse("53.5140,8.1451 53.5123,8.1506 53.5156,8.1623 53.5276,8.1757 53.5491,8.1852 53.5495,8.1877 53.5426,8.1993 53.5184,8.2219 53.5182,8.2386 53.5195,8.2387")
            });

            Polylines.Add(new PolylineItem
            {
                Locations = LocationCollection.Parse("53.5978,8.1212 53.6018,8.1494 53.5859,8.1554 53.5852,8.1531 53.5841,8.1539 53.5802,8.1392 53.5826,8.1309 53.5867,8.1317 53.5978,8.1212")
            });
        }
    }
}

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using MapControl;
using MapControl.UiTools;
using System;
using System.IO;

namespace SampleApplication
{
    public partial class MainWindow : Window
    {
        static MainWindow()
        {
            //TileImageLoader.Cache = new MapControl.Caching.ImageFileCache(TileImageLoader.DefaultCacheFolder);

            var bingMapsApiKeyPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MapControl", "BingMapsApiKey.txt");

            if (File.Exists(bingMapsApiKeyPath))
            {
                BingMapsTileLayer.ApiKey = File.ReadAllText(bingMapsApiKeyPath)?.Trim();
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            if (!string.IsNullOrEmpty(BingMapsTileLayer.ApiKey))
            {
                mapLayersMenuButton.MapLayers.Add(new MapLayerItem
                {
                    Text = "Bing Maps Road",
                    Layer = new BingMapsTileLayer
                    {
                        Mode = BingMapsTileLayer.MapMode.Road,
                        SourceName = "Bing Maps Road",
                        Description = "© [Microsoft](http://www.bing.com/maps/)"
                    }
                });

                mapLayersMenuButton.MapLayers.Add(new MapLayerItem
                {
                    Text = "Bing Maps Aerial",
                    Layer = new BingMapsTileLayer
                    {
                        Mode = BingMapsTileLayer.MapMode.Aerial,
                        SourceName = "Bing Maps Aerial",
                        Description = "© [Microsoft](http://www.bing.com/maps/)",
                        MapForeground = Brushes.White,
                        MapBackground = Brushes.Black
                    }
                });

                mapLayersMenuButton.MapLayers.Add(new MapLayerItem
                {
                    Text = "Bing Maps Aerial with Labels",
                    Layer = new BingMapsTileLayer
                    {
                        Mode = BingMapsTileLayer.MapMode.AerialWithLabels,
                        SourceName = "Bing Maps Hybrid",
                        Description = "© [Microsoft](http://www.bing.com/maps/)",
                        MapForeground = Brushes.White,
                        MapBackground = Brushes.Black
                    }
                });
            }

            AddTestLayers();
        }

        partial void AddTestLayers();

        private void OnMapItemsControlDoubleTapped(object sender, TappedEventArgs e)
        {
            e.Handled = true; // prevent OnMapDoubleTapped
        }

        private void OnMapDoubleTapped(object sender, TappedEventArgs e)
        {
            map.TargetCenter = map.ViewToLocation(e.GetPosition(map));
        }

        private void ResetHeadingButtonClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            map.TargetHeading = 0d;
        }
    }
}

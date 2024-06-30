using System;
using System.IO;
using MapControl;
using MapControl.UiTools;
#if WPF
using System.Windows.Media;
#elif WINUI
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
#elif UWP
using Windows.UI;
using Windows.UI.Xaml.Media;
#elif AVALONIA
using Avalonia.Media;
#endif

namespace SampleApplication
{
#if UWP
    public partial class MainPage
#else
    public partial class MainWindow
#endif
    {
        partial void AddBingMapsLayers()
        {
#if UWP
            var bingMapsApiKeyPath = "BingMapsApiKey.txt";
#else
            var bingMapsApiKeyPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MapControl", "BingMapsApiKey.txt");
#endif
            if (File.Exists(bingMapsApiKeyPath))
            {
                BingMapsTileLayer.ApiKey = File.ReadAllText(bingMapsApiKeyPath)?.Trim();

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
                        MapForeground = new SolidColorBrush(Colors.White),
                        MapBackground = new SolidColorBrush(Colors.Black)
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
                        MapForeground = new SolidColorBrush(Colors.White),
                        MapBackground = new SolidColorBrush(Colors.Black)
                    }
                });
            }
        }
    }
}
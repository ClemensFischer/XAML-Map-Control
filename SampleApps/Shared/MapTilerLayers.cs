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
        partial void AddMapTilerLayers()
        {
#if UWP
            var mapTilerApiKeyPath = "BingMapsApiKey.txt";
#else
            var mapTilerApiKeyPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MapControl", "MapTilerApiKey.txt");
#endif
            if (File.Exists(mapTilerApiKeyPath))
            {
                string apiKey = File.ReadAllText(mapTilerApiKeyPath)?.Trim();

                mapLayersMenuButton.MapLayers.Add(new MapLayerItem
                {
                    Text = "MapTiler Satellite",
                    Layer = new MapTileLayer
                    {
                        TileSource = new TileSource { UriTemplate = "https://api.maptiler.com/maps/satellite/{z}/{x}/{y}.jpg?key=" + apiKey },
                        SourceName = "MapTiler Satellite",
                        Description = "© [MapTiler](https://www.maptiler.com/)"
                    }
                });
            }
        }
    }
}
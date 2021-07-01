using MapControl;
using MapControl.Caching;
using Microsoft.UI.Xaml;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using ViewModel;

namespace WinUiApp
{
    public sealed partial class MainWindow : Window
    {
        private readonly MapViewModel viewModel = new();

        static MainWindow()
        {
            try
            {
                ImageLoader.HttpClient.DefaultRequestHeaders.Add("User-Agent", "XAML Map Control Test Application");

                var appData = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MapControl");

                TileImageLoader.Cache = new ImageFileCache(Path.Combine(appData, "TileCache"));
                BingMapsTileLayer.ApiKey = File.ReadAllText(Path.Combine(appData, "BingMapsApiKey.txt"))?.Trim();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            Title = "XAML Map Control - WinUI Sample Application";

            root.DataContext = viewModel;

            if (TileImageLoader.Cache is ImageFileCache cache)
            {
                Activated += async (s, e) =>
                {
                    await Task.Delay(2000);
                    await cache.Clean();
                };
            }
        }

        private void SeamarksChecked(object sender, RoutedEventArgs e)
        {
            map.Children.Insert(map.Children.IndexOf(graticule), viewModel.MapLayers.SeamarksLayer);
        }

        private void SeamarksUnchecked(object sender, RoutedEventArgs e)
        {
            map.Children.Remove(viewModel.MapLayers.SeamarksLayer);
        }
    }
}

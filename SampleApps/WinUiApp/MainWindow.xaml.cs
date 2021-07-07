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

                var bingMapsApiKeyFile = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MapControl", "BingMapsApiKey.txt");

                BingMapsTileLayer.ApiKey = File.ReadAllText(bingMapsApiKeyFile)?.Trim();

                //TileImageLoader.Cache = new ImageFileCache(TileImageLoader.DefaultCacheFolder);
                //TileImageLoader.Cache = new FileDbCache(TileImageLoader.DefaultCacheFolder);
                //TileImageLoader.Cache = new SQLiteCache(TileImageLoader.DefaultCacheFolder);
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

            if (TileImageLoader.Cache is ImageFileCache)
            {
                Activated += WindowActivated;
            }
        }

        private async void WindowActivated(object sender, WindowActivatedEventArgs e)
        {
            Activated -= WindowActivated;

            await Task.Delay(2000);
            await ((ImageFileCache)TileImageLoader.Cache).Clean();
        }

        private void SeamarksChecked(object sender, RoutedEventArgs e)
        {
            map.Children.Insert(map.Children.IndexOf(graticule), viewModel.MapLayers.SeamarksLayer);
        }

        private void SeamarksUnchecked(object sender, RoutedEventArgs e)
        {
            map.Children.Remove(viewModel.MapLayers.SeamarksLayer);
        }

        private void MapViewportChanged(object sender, ViewportChangedEventArgs e)
        {
            GC.Collect();
        }
    }
}

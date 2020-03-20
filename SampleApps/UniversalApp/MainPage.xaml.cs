using MapControl;
using System;
using ViewModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace UniversalApp
{
    public sealed partial class MainPage : Page
    {
        public MapViewModel ViewModel { get; } = new MapViewModel();

        public MainPage()
        {
            ImageLoader.HttpClient.DefaultRequestHeaders.Add("User-Agent", "XAML Map Control Test Application");
            TileImageLoader.Cache = new MapControl.Caching.ImageFileCache(TileImageLoader.DefaultCacheFolder);
            //TileImageLoader.Cache = new MapControl.Caching.FileDbCache(TileImageLoader.DefaultCacheFolder);
            //TileImageLoader.Cache = new MapControl.Caching.SQLiteCache(TileImageLoader.DefaultCacheFolder);

            InitializeComponent();
            DataContext = ViewModel;

            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var seachartLayer = new WmsImageLayer
            {
                ServiceUri = new Uri("https://wms.sevencs.com:9090")
            };

            var layers = await seachartLayer.GetLayerNamesAsync();

            foreach (var layer in layers)
            {
                System.Diagnostics.Debug.WriteLine(layer);
            }
        }

        private void ImageOpacitySliderValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (mapImage != null)
            {
                mapImage.Opacity = e.NewValue / 100;
            }
        }

        private void SeamarksChecked(object sender, RoutedEventArgs e)
        {
            map.Children.Insert(map.Children.IndexOf(mapGraticule), ViewModel.MapLayers.SeamarksLayer);
        }

        private void SeamarksUnchecked(object sender, RoutedEventArgs e)
        {
            map.Children.Remove(ViewModel.MapLayers.SeamarksLayer);
        }
    }
}

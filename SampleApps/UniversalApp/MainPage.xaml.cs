using System;
using System.Diagnostics;
using System.IO;
using MapControl;
using ViewModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace UniversalApp
{
    public sealed partial class MainPage : Page
    {
        static MainPage()
        {
            try
            {
                ImageLoader.HttpClient.DefaultRequestHeaders.Add("User-Agent", "XAML Map Control Test Application");

                //TileImageLoader.Cache = new MapControl.Caching.ImageFileCache(TileImageLoader.DefaultCacheFolder);
                //TileImageLoader.Cache = new MapControl.Caching.FileDbCache(TileImageLoader.DefaultCacheFolder);
                //TileImageLoader.Cache = new MapControl.Caching.SQLiteCache(TileImageLoader.DefaultCacheFolder);

                BingMapsTileLayer.ApiKey = File.ReadAllText("BingMapsApiKey.txt")?.Trim();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public MapViewModel ViewModel { get; } = new MapViewModel();

        public MainPage()
        {
            InitializeComponent();
            DataContext = ViewModel;
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
            map.Children.Insert(map.Children.IndexOf(graticule), ViewModel.MapLayers.SeamarksLayer);
        }

        private void SeamarksUnchecked(object sender, RoutedEventArgs e)
        {
            map.Children.Remove(ViewModel.MapLayers.SeamarksLayer);
        }
    }
}

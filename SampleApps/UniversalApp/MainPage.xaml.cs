using MapControl;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace SampleApplication
{
    public sealed partial class MainPage : Page
    {
        static MainPage()
        {
            try
            {
                ImageLoader.HttpClient.DefaultRequestHeaders.Add("User-Agent", "XAML Map Control Test Application");

                TileImageLoader.Cache = new MapControl.Caching.ImageFileCache(TileImageLoader.DefaultCacheFolder);
                //TileImageLoader.Cache = new MapControl.Caching.FileDbCache(TileImageLoader.DefaultCacheFolder);
                //TileImageLoader.Cache = new MapControl.Caching.SQLiteCache(TileImageLoader.DefaultCacheFolder);

                BingMapsTileLayer.ApiKey = File.ReadAllText("BingMapsApiKey.txt")?.Trim();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public MainPage()
        {
            InitializeComponent();
        }

        private void ResetHeadingButtonClick(object sender, RoutedEventArgs e)
        {
            map.TargetHeading = 0d;
        }

        private void MapPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var location = map.ViewToLocation(e.GetCurrentPoint(map).Position);
            var latitude = (int)Math.Round(location.Latitude * 60000d);
            var longitude = (int)Math.Round(Location.NormalizeLongitude(location.Longitude) * 60000d);
            var latHemisphere = 'N';
            var lonHemisphere = 'E';

            if (latitude < 0)
            {
                latitude = -latitude;
                latHemisphere = 'S';
            }

            if (longitude < 0)
            {
                longitude = -longitude;
                lonHemisphere = 'W';
            }

            mouseLocation.Text = string.Format(CultureInfo.InvariantCulture,
                "{0}  {1:00} {2:00.000}\n{3} {4:000} {5:00.000}",
                latHemisphere, latitude / 60000, (latitude % 60000) / 1000d,
                lonHemisphere, longitude / 60000, (longitude % 60000) / 1000d);
            mouseLocation.Visibility = Visibility.Visible;
        }

        private void MapPointerExited(object sender, PointerRoutedEventArgs e)
        {
            mouseLocation.Visibility = Visibility.Collapsed;
            mouseLocation.Text = string.Empty;
        }
    }
}

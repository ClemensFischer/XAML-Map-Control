using MapControl;
using MapControl.Caching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace SampleApplication
{
    public sealed partial class MainWindow : Window
    {
        static MainWindow()
        {
            try
            {
                ImageLoader.HttpClient.DefaultRequestHeaders.Add("User-Agent", "XAML Map Control Test Application");

                TileImageLoader.Cache = new ImageFileCache(TileImageLoader.DefaultCacheFolder);
                //TileImageLoader.Cache = new FileDbCache(TileImageLoader.DefaultCacheFolder);
                //TileImageLoader.Cache = new SQLiteCache(TileImageLoader.DefaultCacheFolder);

                var bingMapsApiKeyPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MapControl", "BingMapsApiKey.txt");

                BingMapsTileLayer.ApiKey = File.ReadAllText(bingMapsApiKeyPath)?.Trim();
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

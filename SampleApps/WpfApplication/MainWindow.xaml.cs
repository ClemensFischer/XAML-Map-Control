using MapControl;
using MapControl.Caching;
using MapControl.UiTools;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SampleApplication
{
    public partial class MainWindow : Window
    {
        static MainWindow()
        {
            ImageLoader.HttpClient.DefaultRequestHeaders.Add("User-Agent", "XAML Map Control Test Application");

            TileImageLoader.Cache = new ImageFileCache(TileImageLoader.DefaultCacheFolder);
            //TileImageLoader.Cache = new FileDbCache(TileImageLoader.DefaultCacheFolder);
            //TileImageLoader.Cache = new SQLiteCache(TileImageLoader.DefaultCacheFolder);
            //TileImageLoader.Cache = null;

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
                    Layer = (UIElement)Resources["BingMapsRoad"]
                });

                mapLayersMenuButton.MapLayers.Add(new MapLayerItem
                {
                    Text = "Bing Maps Aerial",
                    Layer = (UIElement)Resources["BingMapsAerial"]
                });

                mapLayersMenuButton.MapLayers.Add(new MapLayerItem
                {
                    Text = "Bing Maps Aerial with Labels",
                    Layer = (UIElement)Resources["BingMapsHybrid"]
                });
            }

            AddTestLayers();

            if (TileImageLoader.Cache is ImageFileCache cache)
            {
                Loaded += async (s, e) =>
                {
                    await Task.Delay(2000);
                    await cache.Clean();
                };
            }
        }

        partial void AddTestLayers();

        private void ResetHeadingButtonClick(object sender, RoutedEventArgs e)
        {
            map.TargetHeading = 0d;
        }

        private async void MapMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                map.TargetCenter = map.ViewToLocation(e.GetPosition(map));
            }
            else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) &&
                map.MapLayer is WmsImageLayer wmsLayer)
            {
                Debug.WriteLine(await wmsLayer.GetFeatureInfoAsync(e.GetPosition(map)));
            }
        }

        private void MapMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var location = map.ViewToLocation(e.GetPosition(map));

            if (location != null)
            {
                measurementLine.Visibility = Visibility.Visible;
                measurementLine.Locations = new LocationCollection(location);
            }
        }

        private void MapMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            measurementLine.Visibility = Visibility.Collapsed;
            measurementLine.Locations = null;
        }

        private void MapMouseMove(object sender, MouseEventArgs e)
        {
            var location = map.ViewToLocation(e.GetPosition(map));

            if (location != null)
            {
                mouseLocation.Visibility = Visibility.Visible;
                mouseLocation.Text = GetLatLonText(location);

                var start = measurementLine.Locations?.FirstOrDefault();

                if (start != null)
                {
                    measurementLine.Locations = LocationCollection.OrthodromeLocations(start, location);
                    mouseLocation.Text += GetDistanceText(location.GetDistance(start));
                }
            }
            else
            {
                mouseLocation.Visibility = Visibility.Collapsed;
                mouseLocation.Text = string.Empty;
            }
        }

        private void MapMouseLeave(object sender, MouseEventArgs e)
        {
            mouseLocation.Visibility = Visibility.Collapsed;
            mouseLocation.Text = string.Empty;
        }

        private void MapManipulationInertiaStarting(object sender, ManipulationInertiaStartingEventArgs e)
        {
            e.TranslationBehavior.DesiredDeceleration = 0.001;
        }

        private void MapItemTouchDown(object sender, TouchEventArgs e)
        {
            var mapItem = (MapItem)sender;
            mapItem.IsSelected = !mapItem.IsSelected;
            e.Handled = true;
        }

        private static string GetLatLonText(Location location)
        {
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

            return string.Format(CultureInfo.InvariantCulture,
                "{0}  {1:00} {2:00.000}\n{3} {4:000} {5:00.000}",
                latHemisphere, latitude / 60000, (latitude % 60000) / 1000d,
                lonHemisphere, longitude / 60000, (longitude % 60000) / 1000d);
        }

        private string GetDistanceText(double distance)
        {
            var unit = "m";

            if (distance >= 1000d)
            {
                distance /= 1000d;
                unit = "km";
            }

            var distanceFormat = distance >= 100d ? "F0" : "F1";

            return string.Format(CultureInfo.InvariantCulture, "\n   {0:" + distanceFormat + "} {1}", distance, unit);
        }
    }
}

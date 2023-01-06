using MapControl;
using MapControl.Caching;
using MapControl.UiTools;
using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.System;

namespace SampleApplication
{
    public sealed partial class MainWindow : Window
    {
        static MainWindow()
        {
            ImageLoader.HttpClient.DefaultRequestHeaders.Add("User-Agent", "XAML Map Control Test Application");

            TileImageLoader.Cache = new ImageFileCache(TileImageLoader.DefaultCacheFolder);
            //TileImageLoader.Cache = new FileDbCache(TileImageLoader.DefaultCacheFolder);
            //TileImageLoader.Cache = new SQLiteCache(TileImageLoader.DefaultCacheFolder);

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

            Title = "XAML Map Control - WinUI Sample Application";

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

            AddTestLayers();

            if (TileImageLoader.Cache is ImageFileCache)
            {
                Activated += WindowActivated;
            }
        }

        partial void AddTestLayers();

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

        private async void MapPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
            {
                var point = e.GetCurrentPoint(map);

                if (point.Properties.IsRightButtonPressed)
                {
                    var location = map.ViewToLocation(point.Position);

                    if (location != null)
                    {
                        measurementLine.Visibility = Visibility.Visible;
                        measurementLine.Locations = new LocationCollection(location);
                    }
                }
                else if (e.KeyModifiers.HasFlag(VirtualKeyModifiers.Control) &&
                    map.MapLayer is WmsImageLayer wmsLayer)
                {
                    Debug.WriteLine(await wmsLayer.GetFeatureInfoAsync(point.Position));
                }
            }
        }

        private void MapPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            measurementLine.Visibility = Visibility.Collapsed;
            measurementLine.Locations = null;
        }

        private void MapPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(map);
            var location = map.ViewToLocation(point.Position);

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

        private void MapPointerExited(object sender, PointerRoutedEventArgs e)
        {
            mouseLocation.Visibility = Visibility.Collapsed;
            mouseLocation.Text = string.Empty;
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

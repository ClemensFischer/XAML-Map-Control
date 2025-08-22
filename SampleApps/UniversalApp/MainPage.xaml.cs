using MapControl;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Windows.Devices.Input;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace SampleApplication
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            //var tileCache = new MapControl.Caching.ImageFileCache(TileImageLoader.DefaultCacheFolder,
            //    LoggerFactory.Create(builder => builder.AddDebug().SetMinimumLevel(LogLevel.Information)));

            //TileImageLoader.Cache = tileCache;
            //Unloaded += (s, e) => tileCache.Dispose();

            ImageLoader.LoggerFactory = LoggerFactory.Create(builder => builder.AddDebug().SetMinimumLevel(LogLevel.Warning));

            InitializeComponent();

            sampleOverlayMenuItem.MapLayerFactory = async () => await GroundOverlay.CreateAsync("etna.kml");

            AddTestLayers();
        }

        partial void AddTestLayers();

        private void MapItemsControlSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("SelectedItems: " + string.Join(", ", ((MapItemsControl)sender).SelectedItems.OfType<PointItem>().Select(item => item.Name)));
        }

        private void MapDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (e.OriginalSource is Map map)
            {
                map.TargetCenter = map.ViewToLocation(e.GetPosition(map));
            }
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

                if (point.Properties.IsRightButtonPressed && map.CapturePointer(e.Pointer))
                {
                    var location = map.ViewToLocation(point.Position);

                    if (location != null)
                    {
                        measurementLine.Visibility = Visibility.Visible;
                        measurementLine.Locations = new LocationCollection(location);
                    }
                }
                else if (e.KeyModifiers.HasFlag(VirtualKeyModifiers.Control) && map.MapLayer is WmsImageLayer wmsLayer)
                {
                    Debug.WriteLine(await wmsLayer.GetFeatureInfoAsync(point.Position));
                }
            }
        }

        private void MapPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
            {
                map.ReleasePointerCapture(e.Pointer);
                measurementLine.Visibility = Visibility.Collapsed;
                measurementLine.Locations = null;
            }
        }

        private void MapPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var location = map.ViewToLocation(e.GetCurrentPoint(map).Position);

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
                mouseLocation.Text = "";
            }
        }

        private void MapPointerExited(object sender, PointerRoutedEventArgs e)
        {
            mouseLocation.Visibility = Visibility.Collapsed;
            mouseLocation.Text = "";
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

        private static string GetDistanceText(double distance)
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

using MapControl;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Windows.System;

namespace SampleApplication
{
    public sealed partial class MainWindow : Window
    {
        static MainWindow()
        {
            //TileImageLoader.Cache = new MapControl.Caching.ImageFileCache(TileImageLoader.DefaultCacheFolder);
        }

        public MainWindow()
        {
            InitializeComponent();

            Title = "XAML Map Control - WinUI Sample Application";

            AddBingMapsLayers();
            AddTestLayers();
        }

        partial void AddBingMapsLayers();
        partial void AddTestLayers();

        private void MapItemsControlSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("SelectedItems: " + string.Join(", ", ((MapItemsControl)sender).SelectedItems.OfType<PointItem>().Select(item => item.Name)));
        }

        private void MapItemsControlDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            e.Handled = true; // prevent MapDoubleTapped
        }

        private void MapDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            map.TargetCenter = map.ViewToLocation(e.GetPosition(map));
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

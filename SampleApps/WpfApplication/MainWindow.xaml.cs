using MapControl;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SampleApplication
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            //TileImageLoader.Cache = new MapControl.Caching.ImageFileCache(TileImageLoader.DefaultCacheFolder);
            TileImageLoader.Cache = new MapControl.Caching.SQLiteCache(TileImageLoader.DefaultCacheFolder);
            //TileImageLoader.Cache = new MapControl.Caching.FileDbCache(TileImageLoader.DefaultCacheFolder);

            Closed += (s, e) => (TileImageLoader.Cache as IDisposable)?.Dispose();

            InitializeComponent();
            AddTestLayers();
        }

        partial void AddTestLayers();

        private void MapItemsControlSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("SelectedItems: " + string.Join(", ", ((MapItemsControl)sender).SelectedItems.OfType<PointItem>().Select(item => item.Name)));
        }

        private void ResetHeadingButtonClick(object sender, RoutedEventArgs e)
        {
            map.TargetHeading = 0d;
        }

        private async void MapMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && e.Source == map)
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

            if (location != null && map.CaptureMouse())
            {
                map.Cursor = Cursors.Cross;
                measurementLine.Visibility = Visibility.Visible;
                measurementLine.Locations = new LocationCollection(location);
            }
        }

        private void MapMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            map.ReleaseMouseCapture();
            map.Cursor = null;
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
                mouseLocation.Text = "";
            }
        }

        private void MapMouseLeave(object sender, MouseEventArgs e)
        {
            mouseLocation.Visibility = Visibility.Collapsed;
            mouseLocation.Text = "";
        }

        private void MapManipulationInertiaStarting(object sender, ManipulationInertiaStartingEventArgs e)
        {
            e.TranslationBehavior.DesiredDeceleration = 0.001;
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

        private void MapItemsControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("PreviewMouseLeftButtonDown");
        }

        private void MapItemsControl_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("PreviewMouseLeftButtonUp");
        }

        private void MapItemsControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("MouseLeftButtonDown");
        }

        private void MapItemsControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("MouseLeftButtonUp");
        }

        private void MapItemsControl_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("PreviewMouseRightButtonDown");
        }

        private void MapItemsControl_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("PreviewMouseRightButtonUp");
        }

        private void MapItemsControl_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("MouseRightButtonDown");
        }

        private void MapItemsControl_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("MouseRightButtonUp");
        }

        private void MapItemsControl_TouchDown(object sender, TouchEventArgs e)
        {
            Debug.WriteLine("TouchDown");
        }

        private void MapItemsControl_TouchUp(object sender, TouchEventArgs e)
        {
            Debug.WriteLine("TouchUp");
        }

        private void MapItemsControl_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            Debug.WriteLine("PreviewTouchDown");
        }

        private void MapItemsControl_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            Debug.WriteLine("PreviewTouchUp");
        }
    }
}

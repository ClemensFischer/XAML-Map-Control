using Avalonia.Controls;
using Avalonia.Input;
using MapControl;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace SampleApplication
{
    public partial class MainWindow : Window
    {
        static MainWindow()
        {
            //TileImageLoader.Cache = new MapControl.Caching.ImageFileCache(TileImageLoader.DefaultCacheFolder);
        }

        public MainWindow()
        {
            InitializeComponent();

            AddBingMapsLayers();
            AddTestLayers();
        }

        partial void AddBingMapsLayers();
        partial void AddTestLayers();

        private void MapItemsControlSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("SelectedItems: " + string.Join(", ", ((MapItemsControl)sender).SelectedItems.OfType<PointItem>().Select(item => item.Name)));
        }

        private void ResetHeadingButtonClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            map.TargetHeading = 0d;
        }

        private void MapItemsControlDoubleTapped(object sender, TappedEventArgs e)
        {
            e.Handled = true; // prevent MapDoubleTapped
        }

        private void MapDoubleTapped(object sender, TappedEventArgs e)
        {
            map.TargetCenter = map.ViewToLocation(e.GetPosition(map));
        }

        private void MapPointerPressed(object sender, PointerPressedEventArgs e)
        {
            if (e.Pointer.Type == PointerType.Mouse)
            {
                var point = e.GetCurrentPoint(map);

                if (point.Properties.IsRightButtonPressed)
                {
                    e.Pointer.Capture(map);
                    var location = map.ViewToLocation(point.Position);

                    if (location != null)
                    {
                        measurementLine.IsVisible = true;
                        measurementLine.Locations = new LocationCollection(location);
                    }
                }
            }
        }

        private void MapPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            if (e.Pointer.Captured == map)
            {
                e.Pointer.Capture(null);
                measurementLine.IsVisible = false;
                measurementLine.Locations = null;
            }
        }

        private void MapPointerMoved(object sender, PointerEventArgs e)
        {
            var location = map.ViewToLocation(e.GetPosition(map));

            if (location != null)
            {
                mouseLocation.IsVisible = true;
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
                mouseLocation.IsVisible = false;
                mouseLocation.Text = "";
            }
        }

        private void MapPointerExited(object sender, PointerEventArgs e)
        {
            mouseLocation.IsVisible = false;
            mouseLocation.Text = "";
        }

        private static string GetLatLonText(MapControl.Location location)
        {
            var latitude = (int)Math.Round(location.Latitude * 60000d);
            var longitude = (int)Math.Round(MapControl.Location.NormalizeLongitude(location.Longitude) * 60000d);
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

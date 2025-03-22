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
        public MainWindow()
        {
            TileImageLoader.Cache = new MapControl.Caching.ImageFileCache(TileImageLoader.DefaultCacheFolder);
            Closed += (s, e) => (TileImageLoader.Cache as IDisposable)?.Dispose();

            InitializeComponent();
            AddTestLayers();
        }

        partial void AddTestLayers();

        private void MapItemsControlSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("SelectedItems: " + string.Join(", ", ((MapItemsControl)sender).SelectedItems.OfType<PointItem>().Select(item => item.Name)));
        }

        private void ResetHeadingButtonClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            map.TargetHeading = 0d;
        }

        private void MapDoubleTapped(object sender, TappedEventArgs e)
        {
            if (e.Source == map)
            {
                map.TargetCenter = map.ViewToLocation(e.GetPosition(map));
            }
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
                        map.Cursor = new Cursor(StandardCursorType.Cross);
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
                map.Cursor = null;
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

        private void MapItemsControl_PointerPressed(object sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            Debug.WriteLine("PointerPressed");
        }

        private void MapItemsControl_PointerReleased(object sender, Avalonia.Input.PointerReleasedEventArgs e)
        {
            Debug.WriteLine("PointerReleased");
        }

        private void MapItemsControl_Tapped(object sender, Avalonia.Input.TappedEventArgs e)
        {
            Debug.WriteLine("Tapped");
        }
    }
}

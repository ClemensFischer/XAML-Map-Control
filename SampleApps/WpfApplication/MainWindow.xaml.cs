using System;
using System.Globalization;
using System.Runtime.Caching;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Caching;
using MapControl;

namespace WpfApplication
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            switch (Properties.Settings.Default.TileCache)
            {
                case "MemoryCache":
                    TileImageLoader.Cache = MemoryCache.Default;
                    break;
                case "FileDbCache":
                    TileImageLoader.Cache = new FileDbCache(TileImageLoader.DefaultCacheName, TileImageLoader.DefaultCacheDirectory);
                    break;
                case "ImageFileCache":
                    TileImageLoader.Cache = new ImageFileCache(TileImageLoader.DefaultCacheName, TileImageLoader.DefaultCacheDirectory);
                    break;
                default:
                    break;
            }

            InitializeComponent();
        }

        private void MapMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                map.ZoomMap(e.GetPosition(map), Math.Floor(map.ZoomLevel + 1.5));
                //map.TargetCenter = map.ViewportPointToLocation(e.GetPosition(map));
            }
        }

        private void MapMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                map.ZoomMap(e.GetPosition(map), Math.Ceiling(map.ZoomLevel - 1.5));
            }
        }

        private void MapMouseLeave(object sender, MouseEventArgs e)
        {
            mouseLocation.Text = string.Empty;
        }

        private void MapMouseMove(object sender, MouseEventArgs e)
        {
            var location = map.ViewportPointToLocation(e.GetPosition(map));
            var longitude = Location.NormalizeLongitude(location.Longitude);
            var latString = location.Latitude < 0 ?
                string.Format(CultureInfo.InvariantCulture, "S  {0:00.00000}", -location.Latitude) :
                string.Format(CultureInfo.InvariantCulture, "N  {0:00.00000}", location.Latitude);
            var lonString = longitude < 0 ?
                string.Format(CultureInfo.InvariantCulture, "W {0:000.00000}", -longitude) :
                string.Format(CultureInfo.InvariantCulture, "E {0:000.00000}", longitude);
            mouseLocation.Text = latString + "\n" + lonString;
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

        private void SeamarksClick(object sender, RoutedEventArgs e)
        {
            var seamarks = (TileLayer)Resources["SeamarksTileLayer"];
            var checkBox = (CheckBox)sender;

            if ((bool)checkBox.IsChecked)
            {
                map.TileLayers.Add(seamarks);
            }
            else
            {
                map.TileLayers.Remove(seamarks);
            }
        }
    }
}

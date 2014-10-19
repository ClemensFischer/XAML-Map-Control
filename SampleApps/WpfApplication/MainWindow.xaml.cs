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
        private TileLayerCollection tileLayers;

        public MainWindow()
        {
            switch (Properties.Settings.Default.TileCache)
            {
                case "MemoryCache":
                    TileImageLoader.Cache = MemoryCache.Default; // this is the default value of the TileImageLoader.Cache property
                    break;
                case "ImageFileCache":
                    TileImageLoader.Cache = new ImageFileCache(TileImageLoader.DefaultCacheName, TileImageLoader.DefaultCacheDirectory);
                    break;
                case "FileDbCache":
                    TileImageLoader.Cache = new FileDbCache(TileImageLoader.DefaultCacheName, TileImageLoader.DefaultCacheDirectory);
                    break;
                case "None":
                    TileImageLoader.Cache = null;
                    break;
                default:
                    break;
            }

            //BingMapsTileLayer.ApiKey = ...

            InitializeComponent();

            tileLayers = (TileLayerCollection)Resources["TileLayers"];
            tileLayerComboBox.SelectedIndex = 0;
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
                latHemisphere, latitude / 60000, (double)(latitude % 60000) / 1000d,
                lonHemisphere, longitude / 60000, (double)(longitude % 60000) / 1000d);
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

        private void TileLayerSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = (ComboBoxItem)tileLayerComboBox.SelectedItem;

            map.TileLayer = tileLayers[(string)selectedItem.Tag]; 

            mapLegend.Inlines.Clear();
            mapLegend.Inlines.AddRange(map.TileLayer.DescriptionInlines);
        }

        private void SeamarksChecked(object sender, RoutedEventArgs e)
        {
            map.TileLayers.Add(tileLayers["Seamarks"]);
        }

        private void SeamarksUnchecked(object sender, RoutedEventArgs e)
        {
            map.TileLayers.Remove(tileLayers["Seamarks"]);
        }
    }
}

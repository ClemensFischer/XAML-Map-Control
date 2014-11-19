using MapControl;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace StoreApplication
{
    public sealed partial class MainPage : Page
    {
        private TileLayerCollection tileLayers;

        public MainPage()
        {
            //TileImageLoader.Cache = new MapControl.Caching.ImageFileCache();
            //TileImageLoader.Cache = new MapControl.Caching.FileDbCache();
            //BingMapsTileLayer.ApiKey = "...";

            this.InitializeComponent();

            tileLayers = (TileLayerCollection)Resources["TileLayers"];
            tileLayerComboBox.SelectedIndex = 0;
        }

        private void ImageOpacitySliderValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (mapImage != null)
            {
                mapImage.Opacity = e.NewValue / 100;
            }
        }

        private void TileLayerSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = (ComboBoxItem)tileLayerComboBox.SelectedItem;

            map.TileLayer = tileLayers[(string)selectedItem.Tag];

            mapLegend.Inlines.Clear();

            foreach (var inline in map.TileLayer.DescriptionInlines)
            {
                mapLegend.Inlines.Add(inline);
            }
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

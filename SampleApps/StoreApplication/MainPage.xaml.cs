using MapControl;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace StoreApplication
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            TileImageLoader.Cache = new ImageFileCache();

            this.InitializeComponent();

            var tileLayers = (TileLayerCollection)Resources["TileLayers"];
            map.TileLayer = tileLayers[0];
        }

        private void ImageOpacitySliderValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (mapImage != null)
            {
                mapImage.Opacity = e.NewValue / 100;
            }
        }

        private void TileLayerComboBoxLoaded(object sender, RoutedEventArgs e)
        {
            ((ComboBox)sender).SelectedIndex = 0;
        }

        private void TileLayerSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedValue = (string)((ComboBox)sender).SelectedValue;
            var tileLayers = (TileLayerCollection)Resources["TileLayers"];
            map.TileLayer = tileLayers[selectedValue];
        }

        private void SeamarksChecked(object sender, RoutedEventArgs e)
        {
            var tileLayers = (TileLayerCollection)Resources["TileLayers"];
            map.TileLayers.Add((TileLayer)tileLayers["Seamarks"]);
        }

        private void SeamarksUnchecked(object sender, RoutedEventArgs e)
        {
            var tileLayers = (TileLayerCollection)Resources["TileLayers"];
            map.TileLayers.Remove((TileLayer)tileLayers["Seamarks"]);
        }
    }
}

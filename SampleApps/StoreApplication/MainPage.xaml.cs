using MapControl;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace StoreApplication
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            tileLayerComboBox.SelectedIndex = 0;
        }

        private void ImageOpacitySliderValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (mapImage != null)
            {
                mapImage.Opacity = e.NewValue / 100;
            }
        }

        private void SeamarksClick(object sender, RoutedEventArgs e)
        {
            var checkBox = (CheckBox)sender;
            var tileLayers = (TileLayerCollection)Resources["TileLayers"];
            var tileLayer = tileLayers["Seamarks"];

            if ((bool)checkBox.IsChecked)
            {
                map.TileLayers.Add(tileLayer);
            }
            else
            {
                map.TileLayers.Remove(tileLayer);
            }
        }

        private void TileLayerSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = (ComboBox)sender;
            var tileLayers = (TileLayerCollection)Resources["TileLayers"];
            map.TileLayer = tileLayers[(string)((ComboBoxItem)comboBox.SelectedItem).Content];
        }
    }
}

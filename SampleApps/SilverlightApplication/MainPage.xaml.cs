using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MapControl;

namespace SilverlightApplication
{
    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();
            tileLayerComboBox.SelectedIndex = 0;
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

        private void TileLayerSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = (ComboBox)sender;
            var tileLayers = (TileLayerCollection)Resources["TileLayers"];
            map.TileLayer = tileLayers[(string)comboBox.SelectedItem];
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
    }
}

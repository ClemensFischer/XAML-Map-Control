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
            //TileImageLoader.Cache = new MapControl.Caching.ImageFileCache();
            //TileImageLoader.Cache = new MapControl.Caching.FileDbCache();
            //BingMapsTileLayer.ApiKey = "...";

            this.InitializeComponent();
        }

        private void ImageOpacitySliderValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (mapImage != null)
            {
                mapImage.Opacity = e.NewValue / 100;
            }
        }

        private void SeamarksChecked(object sender, RoutedEventArgs e)
        {
            map.TileLayers.Add(((TileLayerCollection)Resources["TileLayers"])["Seamarks"]);
        }

        private void SeamarksUnchecked(object sender, RoutedEventArgs e)
        {
            map.TileLayers.Remove(((TileLayerCollection)Resources["TileLayers"])["Seamarks"]);
        }
    }
}

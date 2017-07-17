using ViewModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace UniversalApp
{
    public sealed partial class MainPage : Page
    {
        public MapViewModel ViewModel { get; } = new MapViewModel();

        public MainPage()
        {
            //MapControl.TileImageLoader.Cache = new MapControl.Caching.ImageFileCache();
            //MapControl.TileImageLoader.Cache = new MapControl.Caching.FileDbCache();

            InitializeComponent();
            DataContext = ViewModel;
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
            map.Children.Insert(map.Children.IndexOf(mapGraticule), ViewModel.MapLayers.SeamarksLayer);
        }

        private void SeamarksUnchecked(object sender, RoutedEventArgs e)
        {
            map.Children.Remove(ViewModel.MapLayers.SeamarksLayer);
        }
    }
}

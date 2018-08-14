using MapControl;
using MapControl.Caching;
using ViewModel;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace UniversalApp
{
    public sealed partial class MainPage : Page
    {
        public MapViewModel ViewModel { get; } = new MapViewModel();

        public MainPage()
        {
            TileImageLoader.Cache = new ImageFileCache(TileImageLoader.DefaultCacheFolder);
            //TileImageLoader.Cache = new FileDbCache(TileImageLoader.DefaultCacheFolder);

            InitializeComponent();
            DataContext = ViewModel;

            for (var x = -180d; x < 180d; x += 15d)
            {
                var location = new Location(0d, x);

                var locations = new LocationCollection
                {
                    new Location(0, x - 5),
                    new Location(5, x),
                    new Location(0, x + 5),
                    new Location(-5, x)
                };

                map.Children.Add(new MapPolygon
                {
                    Fill = new SolidColorBrush(Colors.Red) { Opacity = 0.25 },
                    Stroke = new SolidColorBrush(Colors.Red),
                    StrokeThickness = 2,
                    StrokeLineJoin = PenLineJoin.Round,
                    Locations = locations,
                    Location = location,
                });

                map.Children.Add(new Pushpin
                {
                    Content = x,
                    Location = location,
                });
            }
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

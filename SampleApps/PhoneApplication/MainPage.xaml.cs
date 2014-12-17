using System;
using MapControl;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace PhoneApplication
{
    public sealed partial class MainPage : Page
    {
        private bool mapCentered;

        public MainPage()
        {
            //BingMapsTileLayer.ApiKey = "...";

            InitializeComponent();

            DataContext = new ViewModel(Dispatcher);
            NavigationCacheMode = NavigationCacheMode.Required;
        }

        private void MapMenuItemClick(object sender, RoutedEventArgs e)
        {
            var tileLayers = (TileLayerCollection)Resources["TileLayers"];
            map.TileLayer = tileLayers[(string)((FrameworkElement)sender).Tag];
        }

        private void SeamarksChecked(object sender, RoutedEventArgs e)
        {
            var tileLayers = (TileLayerCollection)Resources["TileLayers"];
            map.TileLayers.Add(tileLayers["Seamarks"]);
        }

        private void SeamarksUnchecked(object sender, RoutedEventArgs e)
        {
            var tileLayers = (TileLayerCollection)Resources["TileLayers"];
            map.TileLayers.Remove(tileLayers["Seamarks"]);
        }

        private void CenterButtonClick(object sender, RoutedEventArgs e)
        {
            map.TargetCenter = ((ViewModel)DataContext).Location;
            mapCentered = true;
        }

        private void MapManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            mapCentered = false;
        }

        private void MapManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (mapCentered)
            {
                e.Complete();
            }
            else
            {
                map.TransformMap(e.Position, e.Delta.Translation, e.Delta.Rotation, e.Delta.Scale);
            }
        }
    }

    public class ObjectReferenceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (targetType == typeof(Visibility))
            {
                return value != null ? Visibility.Visible : Visibility.Collapsed;
            }

            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}

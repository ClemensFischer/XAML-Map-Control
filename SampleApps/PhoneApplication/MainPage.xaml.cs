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
        private bool manipulationActive;

        public MainPage()
        {
            InitializeComponent();
            DataContext = new ViewModel(Dispatcher);
            NavigationCacheMode = NavigationCacheMode.Required;
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

        private void MapMenuItemClick(object sender, RoutedEventArgs e)
        {
            var selectedValue = ((MenuFlyoutItem)sender).Text;
            var tileLayers = (TileLayerCollection)Resources["TileLayers"];
            map.TileLayer = tileLayers[selectedValue];
        }

        private void CenterButtonClick(object sender, RoutedEventArgs e)
        {
            manipulationActive = false;            
            map.TargetCenter = ((ViewModel)DataContext).Location;
        }

        private void MapManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            manipulationActive = true;
        }

        private void MapManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            manipulationActive = false;
        }

        private void MapManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (manipulationActive)
            {
                map.TransformMap(e.Position, e.Delta.Translation, e.Delta.Rotation, e.Delta.Scale);
            }
            else
            {
                e.Complete();
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

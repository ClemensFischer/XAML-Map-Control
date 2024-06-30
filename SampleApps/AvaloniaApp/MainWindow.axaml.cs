using Avalonia.Controls;
using Avalonia.Input;
using MapControl;
using System.Diagnostics;
using System.Linq;

namespace SampleApplication
{
    public partial class MainWindow : Window
    {
        static MainWindow()
        {
            //TileImageLoader.Cache = new MapControl.Caching.ImageFileCache(TileImageLoader.DefaultCacheFolder);
        }

        public MainWindow()
        {
            InitializeComponent();

            AddBingMapsLayers();
            AddTestLayers();
        }

        partial void AddBingMapsLayers();
        partial void AddTestLayers();

        private void MapItemsControlSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("SelectedItems: " + string.Join(", ", ((MapItemsControl)sender).SelectedItems.OfType<PointItem>().Select(item => item.Name)));
        }

        private void MapItemsControlDoubleTapped(object sender, TappedEventArgs e)
        {
            e.Handled = true; // prevent MapDoubleTapped
        }

        private void MapDoubleTapped(object sender, TappedEventArgs e)
        {
            map.TargetCenter = map.ViewToLocation(e.GetPosition(map));
        }

        private void ResetHeadingButtonClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            map.TargetHeading = 0d;
        }
    }
}

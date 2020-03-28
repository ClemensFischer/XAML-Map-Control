using MapControl;
using MapControl.Projections;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ProjectionDemo
{
    public partial class MainWindow : Window
    {
        private readonly HttpClient httpClient = new HttpClient();
        private readonly ViewModel viewModel = new ViewModel();

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            viewModel.Projections.Add(new MapControl.Projections.WebMercatorProjection());

            viewModel.Projections.Add(new GeoApiProjection
            {
                WKT = await httpClient.GetStringAsync("https://epsg.io/25832.wkt") // ETRS89 / UTM zone 32N
            });

            viewModel.Layers.Add(
                "OpenStreetMap WMS",
                new WmsImageLayer
                {
                    ServiceUri = new Uri("http://ows.terrestris.de/osm/service"),
                    Layers = "OSM-WMS"
                });

            viewModel.Layers.Add(
                "TopPlusOpen WMS",
                new WmsImageLayer
                {
                    ServiceUri = new Uri("https://sgx.geodatenzentrum.de/wms_topplus_open"),
                    Layers = "web"
                });

            viewModel.Layers.Add(
                "Orthophotos Wiesbaden",
                new WmsImageLayer
                {
                    ServiceUri = new Uri("https://geoportal.wiesbaden.de/cgi-bin/mapserv.fcgi?map=d:/openwimap/umn/map/orthophoto/orthophotos.map"),
                    Layers = "orthophoto2017"
                });

            viewModel.CurrentProjection = viewModel.Projections[0];
            viewModel.CurrentLayer = viewModel.Layers.First().Value;

            DataContext = viewModel;
        }

        private void Map_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var map = (MapBase)sender;
            var pos = e.GetPosition(map);

            viewModel.PushpinLocation = map.ViewToLocation(pos);
        }
    }

    public class ViewModel : INotifyPropertyChanged
    {
        private MapProjection currentProjection;
        private IMapLayer currentLayer;
        private Location pushpinLocation = new Location();

        public event PropertyChangedEventHandler PropertyChanged;

        public List<MapProjection> Projections { get; } = new List<MapProjection>();

        public Dictionary<string, IMapLayer> Layers { get; } = new Dictionary<string, IMapLayer>();

        public MapProjection CurrentProjection
        {
            get => currentProjection;
            set
            {
                currentProjection = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentProjection)));
            }
        }

        public IMapLayer CurrentLayer
        {
            get => currentLayer;
            set
            {
                currentLayer = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentLayer)));
            }
        }

        public Location PushpinLocation
        {
            get => pushpinLocation;
            set
            {
                pushpinLocation = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PushpinLocation)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PushpinText)));
            }
        }

        public string PushpinText
        {
            get
            {
                var latitude = (int)Math.Round(PushpinLocation.Latitude * 36000);
                var longitude = (int)Math.Round(Location.NormalizeLongitude(PushpinLocation.Longitude) * 36000);
                var latHemisphere = 'N';
                var lonHemisphere = 'E';

                if (latitude < 0)
                {
                    latitude = -latitude;
                    latHemisphere = 'S';
                }

                if (longitude < 0)
                {
                    longitude = -longitude;
                    lonHemisphere = 'W';
                }

                return string.Format(CultureInfo.InvariantCulture,
                    "{0}  {1:00} {2:00} {3:00.0}\n{4} {5:000} {6:00} {7:00.0}",
                    latHemisphere, latitude / 36000, (latitude / 600) % 60, (latitude % 600) / 10d,
                    lonHemisphere, longitude / 36000, (longitude / 600) % 60, (longitude % 600) / 10d);
            }
        }
    }
}

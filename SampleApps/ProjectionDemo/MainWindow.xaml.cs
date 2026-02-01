using MapControl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ProjectionDemo
{
    public partial class MainWindow : Window
    {
        private readonly ViewModel viewModel = new();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            viewModel.Projections.Add(new WebMercatorProjection());
            viewModel.Projections.Add(new Etrs89UtmProjection(32));

            viewModel.Layers.Add(
                "OpenStreetMap WMS",
                new WmsImageLayer
                {
                    ServiceUri = new Uri("http://ows.terrestris.de/osm/service"),
                    RequestLayers = "OSM-WMS"
                });

            viewModel.Layers.Add(
                "TopPlusOpen WMS",
                new WmsImageLayer
                {
                    ServiceUri = new Uri("https://sgx.geodatenzentrum.de/wms_topplus_open"),
                    RequestLayers = "web"
                });

            viewModel.Layers.Add(
                "Basemap.de WMS",
                new WmsImageLayer
                {
                    ServiceUri = new Uri("https://sgx.geodatenzentrum.de/wms_basemapde"),
                    RequestLayers = "de_basemapde_web_raster_farbe"
                });

            viewModel.Layers.Add(
                "Orthophotos Wiesbaden",
                new WmsImageLayer
                {
                    ServiceUri = new Uri("https://geoportal.wiesbaden.de/cgi-bin/mapserv.fcgi?map=d:/openwimap/umn/map/orthophoto/orthophotos.map"),
                    RequestLayers = "orthophoto2023"
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
        public event PropertyChangedEventHandler PropertyChanged;

        public List<MapProjection> Projections { get; } = [];

        public Dictionary<string, IMapLayer> Layers { get; } = [];

        public MapProjection CurrentProjection
        {
            get;
            set
            {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentProjection)));
            }
        }

        public IMapLayer CurrentLayer
        {
            get;
            set
            {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentLayer)));
            }
        }

        public Location PushpinLocation
        {
            get;
            set
            {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PushpinLocation)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PushpinText)));
            }
        }

        public string PushpinText
        {
            get
            {
                if (PushpinLocation == null)
                {
                    return null;
                }

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

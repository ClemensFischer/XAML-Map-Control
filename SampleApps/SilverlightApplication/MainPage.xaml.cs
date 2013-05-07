using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using MapControl;

namespace SilverlightApplication
{
    public partial class MainPage : UserControl
    {
        private SamplePoint movingPoint = new SamplePoint
        {
            Name = "Moving",
            Location = new Location(53.5, 8.25)
        };

        public MainPage()
        {
            InitializeComponent();

            var polylines = (ICollection<object>)Resources["Polylines"];
            polylines.Add(
                new SamplePolyline
                {
                    Locations = LocationCollection.Parse("53.5140,8.1451 53.5123,8.1506 53.5156,8.1623 53.5276,8.1757 53.5491,8.1852 53.5495,8.1877 53.5426,8.1993 53.5184,8.2219 53.5182,8.2386 53.5195,8.2387")
                });
            polylines.Add(
                new SamplePolyline
                {
                    Locations = LocationCollection.Parse("53.5978,8.1212 53.6018,8.1494 53.5859,8.1554 53.5852,8.1531 53.5841,8.1539 53.5802,8.1392 53.5826,8.1309 53.5867,8.1317 53.5978,8.1212")
                });

            var points = (ICollection<object>)Resources["Points"];
            points.Add(
                new SamplePoint
                {
                    Name = "Steinbake Leitdamm",
                    Location = new Location(53.51217, 8.16603)
                });
            points.Add(
                new SamplePoint
                {
                    Name = "Buhne 2",
                    Location = new Location(53.50926, 8.15815)
                });
            points.Add(
                new SamplePoint
                {
                    Name = "Buhne 4",
                    Location = new Location(53.50468, 8.15343)
                });
            points.Add(
                new SamplePoint
                {
                    Name = "Buhne 6",
                    Location = new Location(53.50092, 8.15267)
                });
            points.Add(
                new SamplePoint
                {
                    Name = "Buhne 8",
                    Location = new Location(53.49871, 8.15321)
                });
            points.Add(
                new SamplePoint
                {
                    Name = "Buhne 10",
                    Location = new Location(53.49350, 8.15563)
                });
            points.Add(movingPoint);

            var pushpins = (ICollection<object>)Resources["Pushpins"];
            pushpins.Add(
                new SamplePoint
                {
                    Name = "WHV - Eckwarderhörne",
                    Location = new Location(53.5495, 8.1877)
                });
            pushpins.Add(
                new SamplePoint
                {
                    Name = "JadeWeserPort",
                    Location = new Location(53.5914, 8.14)
                });
            pushpins.Add(
                new SamplePoint
                {
                    Name = "Kurhaus Dangast",
                    Location = new Location(53.447, 8.1114)
                });
            pushpins.Add(
                new SamplePoint
                {
                    Name = "Eckwarderhörne",
                    Location = new Location(53.5207, 8.2323)
                });

            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.1) };
            timer.Tick += MovePoint;
            timer.Start();
        }

        private void MovePoint(object sender, EventArgs e)
        {
            movingPoint.Location = new Location(movingPoint.Location.Latitude + 0.001, movingPoint.Location.Longitude + 0.002);

            if (movingPoint.Location.Latitude > 54d)
            {
                movingPoint.Name = "Stopped";
                ((DispatcherTimer)sender).Stop();
            }
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
            if (map != null)
            {
                var comboBox = (ComboBox)sender;
                var tileLayers = (TileLayerCollection)Resources["TileLayers"];
                map.TileLayer = tileLayers[(string)comboBox.SelectedItem];
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
    }
}

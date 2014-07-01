using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
#if WINDOWS_RUNTIME
using Windows.UI.Xaml;
#else
using System.Windows.Threading;
#endif
using MapControl;

namespace ViewModel
{
    public class VmBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            var propertyChanged = PropertyChanged;
            if (propertyChanged != null)
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class VmPoint : VmBase
    {
        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                RaisePropertyChanged("Name");
            }
        }

        private Location location;
        public Location Location
        {
            get { return location; }
            set
            {
                location = value;
                RaisePropertyChanged("Location");
            }
        }
    }

    public class VmPolyline
    {
        public LocationCollection Locations { get; set; }
    }

    public class ViewModel : VmBase
    {
        public ObservableCollection<VmPoint> Points { get; set; }
        public ObservableCollection<VmPoint> Pushpins { get; set; }
        public ObservableCollection<VmPolyline> Polylines { get; set; }

        private Location mapCenter;
        public Location MapCenter
        {
            get { return mapCenter; }
            set
            {
                mapCenter = value;
                RaisePropertyChanged("MapCenter");
            }
        }

        public ViewModel()
        {
            MapCenter = new Location(53.5, 8.2);

            Points = new ObservableCollection<VmPoint>();
            Points.Add(
                new VmPoint
                {
                    Name = "Steinbake Leitdamm",
                    Location = new Location(53.51217, 8.16603)
                });
            Points.Add(
                new VmPoint
                {
                    Name = "Buhne 2",
                    Location = new Location(53.50926, 8.15815)
                });
            Points.Add(
                new VmPoint
                {
                    Name = "Buhne 4",
                    Location = new Location(53.50468, 8.15343)
                });
            Points.Add(
                new VmPoint
                {
                    Name = "Buhne 6",
                    Location = new Location(53.50092, 8.15267)
                });
            Points.Add(
                new VmPoint
                {
                    Name = "Buhne 8",
                    Location = new Location(53.49871, 8.15321)
                });
            Points.Add(
                new VmPoint
                {
                    Name = "Buhne 10",
                    Location = new Location(53.49350, 8.15563)
                });
            Points.Add(
                new VmPoint
                {
                    Name = "Moving",
                    Location = new Location(53.5, 8.25)
                });

            Pushpins = new ObservableCollection<VmPoint>();
            Pushpins.Add(
                new VmPoint
                {
                    Name = "WHV - Eckwarderhörne",
                    Location = new Location(53.5495, 8.1877)
                });
            Pushpins.Add(
                new VmPoint
                {
                    Name = "JadeWeserPort",
                    Location = new Location(53.5914, 8.14)
                });
            Pushpins.Add(
                new VmPoint
                {
                    Name = "Kurhaus Dangast",
                    Location = new Location(53.447, 8.1114)
                });
            Pushpins.Add(
                new VmPoint
                {
                    Name = "Eckwarderhörne",
                    Location = new Location(53.5207, 8.2323)
                });

            //for (double lon = -720; lon <= 720; lon += 15)
            //{
            //    var lat = lon / 10;
            //    Pushpins.Add(
            //        new VmPoint
            //        {
            //            Name = string.Format("{0:00.0}°, {1:000}°", lat, lon),
            //            Location = new Location(lat, lon)
            //        });
            //}

            Polylines = new ObservableCollection<VmPolyline>();
            Polylines.Add(
                new VmPolyline
                {
                    Locations = LocationCollection.Parse("53.5140,8.1451 53.5123,8.1506 53.5156,8.1623 53.5276,8.1757 53.5491,8.1852 53.5495,8.1877 53.5426,8.1993 53.5184,8.2219 53.5182,8.2386 53.5195,8.2387")
                });
            Polylines.Add(
                new VmPolyline
                {
                    Locations = LocationCollection.Parse("53.5978,8.1212 53.6018,8.1494 53.5859,8.1554 53.5852,8.1531 53.5841,8.1539 53.5802,8.1392 53.5826,8.1309 53.5867,8.1317 53.5978,8.1212")
                });

            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(0.1)
            };

            timer.Tick += (s, e) =>
            {
                var p = Points.Last();
                p.Location = new Location(p.Location.Latitude + 0.001, p.Location.Longitude + 0.002);

                if (p.Location.Latitude > 54d)
                {
                    p.Name = "Stopped";
                    ((DispatcherTimer)s).Stop();
                }
            };

            timer.Start();
        }
    }
}

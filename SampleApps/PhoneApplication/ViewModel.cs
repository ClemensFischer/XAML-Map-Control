using System;
using System.ComponentModel;
using MapControl;
using Windows.Devices.Geolocation;
using Windows.UI.Core;

namespace PhoneApplication
{
    public class ViewModel : INotifyPropertyChanged
    {
        private readonly CoreDispatcher dispatcher;
        private readonly Geolocator geoLocator;
        private double accuracy;
        private Location location;

        public event PropertyChangedEventHandler PropertyChanged;

        public ViewModel(CoreDispatcher dispatcher)
        {
            this.dispatcher = dispatcher;

            geoLocator = new Geolocator
            {
                DesiredAccuracy = PositionAccuracy.High,
                MovementThreshold = 1d
            };

            geoLocator.StatusChanged += GeoLocatorStatusChanged;
            geoLocator.PositionChanged += GeoLocatorPositionChanged;
        }

        public double Accuracy
        {
            get { return accuracy; }
            private set
            {
                accuracy = value;
                RaisePropertyChanged("Accuracy");
            }
        }

        public Location Location
        {
            get { return location; }
            private set
            {
                location = value;
                RaisePropertyChanged("Location");
            }
        }

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void GeoLocatorStatusChanged(Geolocator sender, StatusChangedEventArgs args)
        {
            if (args.Status != PositionStatus.Initializing &&
                args.Status != PositionStatus.Ready)
            {
                await dispatcher.RunAsync(CoreDispatcherPriority.Low, () => Location = null);
            }
        }

        private async void GeoLocatorPositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            await dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                Accuracy = args.Position.Coordinate.Accuracy;
                Location = new Location(
                    args.Position.Coordinate.Point.Position.Latitude,
                    args.Position.Coordinate.Point.Position.Longitude);
            });
        }
    }
}

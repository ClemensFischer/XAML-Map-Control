using System.Collections.ObjectModel;
using MapControl;
using System.ComponentModel;

namespace SampleApplication
{
    class SamplePoint : INotifyPropertyChanged
    {
        private string name;
        private Location location;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }

        public Location Location
        {
            get { return location; }
            set
            {
                location = value;
                OnPropertyChanged("Location");
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    class SamplePolyline
    {
        public LocationCollection Locations { get; set; }
    }

    class SampleItemCollection : ObservableCollection<object>
    {
    }
}

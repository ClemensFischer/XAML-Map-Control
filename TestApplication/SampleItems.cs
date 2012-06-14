using System.Collections.ObjectModel;
using MapControl;

namespace MapControlTestApp
{
    class SamplePoint
    {
        public string Name { get; set; }
        public Location Location { get; set; }
    }

    class SamplePolyline
    {
        public LocationCollection Locations { get; set; }
    }

    class SampleItemCollection : ObservableCollection<object>
    {
    }
}

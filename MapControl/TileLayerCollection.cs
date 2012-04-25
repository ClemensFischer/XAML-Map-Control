using System;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace MapControl
{
    public class TileLayerCollection : ObservableCollection<TileLayer>
    {
        private string name;

        public TileLayerCollection()
        {
        }

        public TileLayerCollection(TileLayer tileLayer)
        {
            Add(tileLayer);
        }

        public string Name
        {
            get { return !string.IsNullOrEmpty(name) ? name : (Count > 0 ? this[0].Name : string.Empty); }
            set { name = value; }
        }
    }
}

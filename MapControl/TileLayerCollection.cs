// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2014 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Collections.ObjectModel;
using System.Linq;

namespace MapControl
{
    public class TileLayerCollection : ObservableCollection<TileLayer>
    {
        internal TileLayerCollection(TileLayer tileLayer)
        {
            Add(tileLayer);
        }

        public TileLayerCollection()
        {
        }

        public TileLayer this[string sourceName]
        {
            get { return this.FirstOrDefault(t => t.SourceName == sourceName); }
        }
    }
}

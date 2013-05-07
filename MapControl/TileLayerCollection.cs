// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © Clemens Fischer 2012-2013
// Licensed under the Microsoft Public License (Ms-PL)

using System.Collections.ObjectModel;
using System.Linq;

namespace MapControl
{
    public class TileLayerCollection : ObservableCollection<TileLayer>
    {
        public TileLayer this[string sourceName]
        {
            get { return this.FirstOrDefault(t => t.SourceName == sourceName); }
        }
    }
}

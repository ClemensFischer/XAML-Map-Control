// WPF MapControl - http://wpfmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace MapControl
{
    public class TileLayerCollection : ObservableCollection<TileLayer>
    {
        private string name;

        public string Name
        {
            get { return !string.IsNullOrEmpty(name) ? name : (Count > 0 ? this[0].Name : string.Empty); }
            set { name = value; }
        }
    }
}

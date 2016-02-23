// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2016 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Collections.ObjectModel;
using System.Linq;

namespace MapControl
{
    /// <summary>
    /// A collection of TileLayers with a string indexer that allows
    /// to retrieve individual TileLayers by their SourceName property.
    /// </summary>
    public class TileLayerCollection : Collection<TileLayer>
    {
        public TileLayer this[string sourceName]
        {
            get { return this.FirstOrDefault(t => t.SourceName == sourceName); }
        }
    }
}

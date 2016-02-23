// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2016 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Collections.Generic;

namespace MapControl
{
    public interface ITileImageLoader
    {
        void BeginLoadTiles(TileLayer tileLayer, IEnumerable<Tile> tiles);
        void CancelLoadTiles(TileLayer tileLayer);
    }
}

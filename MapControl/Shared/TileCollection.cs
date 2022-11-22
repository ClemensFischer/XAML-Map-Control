// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Collections.Generic;
using System.Linq;

namespace MapControl
{
    public class TileCollection : List<Tile>
    {
        /// <summary>
        /// Get a matching Tile from a TileCollection or create a new one.
        /// </summary>
        public Tile GetTile(int zoomLevel, int x, int y, int columnCount)
        {
            var tile = this.FirstOrDefault(t => t.ZoomLevel == zoomLevel && t.X == x && t.Y == y);

            if (tile == null)
            {
                tile = new Tile(zoomLevel, x, y, columnCount);

                var equivalentTile = this.FirstOrDefault(
                    t => t.IsLoaded && t.ZoomLevel == tile.ZoomLevel && t.Column == tile.Column && t.Row == tile.Row);

                if (equivalentTile != null)
                {
                    tile.SetImageSource(equivalentTile.Image.Source, false); // no opacity animation
                }
            }

            return tile;
        }
    }
}

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
                    t => t.Image.Source != null && t.ZoomLevel == tile.ZoomLevel && t.Column == tile.Column && t.Row == tile.Row);

                if (equivalentTile != null)
                {
                    tile.IsPending = false;
                    tile.Image.Source = equivalentTile.Image.Source; // no opacity animation
                }
            }

            return tile;
        }
    }
}

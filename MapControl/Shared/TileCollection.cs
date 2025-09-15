﻿using System.Collections.Generic;
using System.Linq;

namespace MapControl
{
    public partial class TileCollection : List<Tile>
    {
        /// <summary>
        /// Adds existing Tiles from the source collection or newly created Tiles to fill the specified tile matrix.
        /// </summary>
        public void FillMatrix(TileCollection source, int zoomLevel, int xMin, int yMin, int xMax, int yMax, int columnCount)
        {
            for (var y = yMin; y <= yMax; y++)
            {
                for (var x = xMin; x <= xMax; x++)
                {
                    var tile = source.FirstOrDefault(t => t.ZoomLevel == zoomLevel && t.X == x && t.Y == y);

                    if (tile == null)
                    {
                        tile = new Tile(zoomLevel, x, y, columnCount);

                        var equivalentTile = source.FirstOrDefault(
                            t => t.Image.Source != null && t.ZoomLevel == tile.ZoomLevel && t.Column == tile.Column && t.Row == tile.Row);

                        if (equivalentTile != null)
                        {
                            tile.IsPending = false;
                            tile.Image.Source = equivalentTile.Image.Source; // no opacity animation
                        }
                    }

                    Add(tile);
                }
            }
        }
    }
}

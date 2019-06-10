// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2019 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace MapControl
{
    public class TileQueue : ConcurrentStack<Tile>
    {
        public void Enqueue(IEnumerable<Tile> tiles)
        {
            tiles = tiles.Where(tile => tile.Pending);

            if (tiles.Any())
            {
                PushRange(tiles.Reverse().ToArray());
            }
        }

        public bool TryDequeue(out Tile tile)
        {
            var success = TryPop(out tile);

            if (success)
            {
                tile.Pending = false;
            }

            return success;
        }
    }
}

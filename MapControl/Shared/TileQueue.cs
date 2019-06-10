// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2019 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MapControl
{
    public class TileQueue : ConcurrentStack<Tile>
    {
        private int taskCount;

        public bool Enqueue(IEnumerable<Tile> tiles)
        {
            tiles = tiles.Where(tile => tile.Pending);

            if (tiles.Any())
            {
                PushRange(tiles.Reverse().ToArray());
                return true;
            }

            return false;
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

        public void RunDequeueTasks(int maxTasks, Func<Tile, Task> tileFunc)
        {
            var newTasks = Math.Min(Count, maxTasks) - taskCount;

            while (--newTasks >= 0)
            {
                Interlocked.Increment(ref taskCount);

                Task.Run(() => DequeueTiles(tileFunc));
            }
        }

        private async Task DequeueTiles(Func<Tile, Task> tileFunc)
        {
            Tile tile;

            while (TryDequeue(out tile))
            {
                try
                {
                    await tileFunc(tile);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("TileQueue: {0}: {1}", tile, ex.Message);
                }
            }

            Interlocked.Decrement(ref taskCount);
        }
    }
}

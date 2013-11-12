// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © Clemens Fischer 2012-2013
// Licensed under the Microsoft Public License (Ms-PL)

using System;

namespace MapControl
{
    public partial class Tile
    {
        public static TimeSpan AnimationDuration = TimeSpan.FromSeconds(0.5);

        public readonly int ZoomLevel;
        public readonly int X;
        public readonly int Y;

        public Tile(int zoomLevel, int x, int y)
        {
            ZoomLevel = zoomLevel;
            X = x;
            Y = y;
        }

        public bool HasImageSource { get; private set; }

        public int XIndex
        {
            get
            {
                var numTiles = 1 << ZoomLevel;
                return ((X % numTiles) + numTiles) % numTiles;
            }
        }
    }
}

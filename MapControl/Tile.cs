// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if NETFX_CORE
using Windows.UI.Xaml.Media.Animation;
#else
using System.Windows.Media.Animation;
#endif

namespace MapControl
{
    public partial class Tile
    {
        public readonly int ZoomLevel;
        public readonly int X;
        public readonly int Y;

        public Tile(int zoomLevel, int x, int y)
        {
            ZoomLevel = zoomLevel;
            X = x;
            Y = y;
        }

        public Uri Uri { get; set; }

        public int XIndex
        {
            get
            {
                var numTiles = 1 << ZoomLevel;
                return ((X % numTiles) + numTiles) % numTiles;
            }
        }

        public DoubleAnimation OpacityAnimation
        {
            get
            {
                return new DoubleAnimation
                {
                    To = 1d,
                    Duration = TimeSpan.FromSeconds(0.5),
                    FillBehavior = FillBehavior.HoldEnd,
                };
            }
        }
    }
}

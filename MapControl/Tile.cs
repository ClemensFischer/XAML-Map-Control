// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2016 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if NETFX_CORE
using Windows.UI.Xaml.Controls;
#else
using System.Windows.Controls;
#endif

namespace MapControl
{
    public partial class Tile
    {
        public static TimeSpan FadeDuration { get; set; }

        static Tile()
        {
            FadeDuration = TimeSpan.FromSeconds(0.2);
        }

        public readonly int ZoomLevel;
        public readonly int X;
        public readonly int Y;
        public readonly Image Image = new Image { Opacity = 0d };

        public Tile(int zoomLevel, int x, int y)
        {
            ZoomLevel = zoomLevel;
            X = x;
            Y = y;
            Pending = true;
        }

        public bool Pending { get; private set; }

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

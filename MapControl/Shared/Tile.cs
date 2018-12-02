// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
#endif

namespace MapControl
{
    public partial class Tile
    {
        public static TimeSpan FadeDuration { get; set; } = TimeSpan.FromSeconds(0.15);

        public readonly int ZoomLevel;
        public readonly int X;
        public readonly int Y;
        public readonly Image Image = new Image { Opacity = 0d, Stretch = Stretch.Fill };

        public Tile(int zoomLevel, int x, int y)
        {
            ZoomLevel = zoomLevel;
            X = x;
            Y = y;
        }

        public bool Pending { get; set; } = true;

        public int XIndex
        {
            get
            {
                var numTiles = 1 << ZoomLevel;
                return ((X % numTiles) + numTiles) % numTiles;
            }
        }

        private void FadeIn()
        {
            Image.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation { From = 0d, To = 1d, Duration = FadeDuration, FillBehavior = FillBehavior.Stop });
            Image.Opacity = 1d;
        }
    }
}

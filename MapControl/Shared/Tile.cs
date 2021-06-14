// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
#elif WINDOWS_UWP
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
        public Tile(int zoomLevel, int x, int y)
        {
            ZoomLevel = zoomLevel;
            X = x;
            Y = y;
        }

        public int ZoomLevel { get; }
        public int X { get; }
        public int Y { get; }

        public int XIndex
        {
            get
            {
                var numTiles = 1 << ZoomLevel;
                return ((X % numTiles) + numTiles) % numTiles;
            }
        }

        public Image Image { get; } = new Image { Opacity = 0d, Stretch = Stretch.Fill };

        public bool Pending { get; set; } = true;

        private void FadeIn()
        {
            Image.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation
            {
                From = 0d,
                To = 1d,
                Duration = MapBase.ImageFadeDuration,
                FillBehavior = FillBehavior.Stop
            });

            Image.Opacity = 1d;
        }
    }
}

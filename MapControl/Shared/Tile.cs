// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2023 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
#elif UWP
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
        public Tile(int zoomLevel, int x, int y, int columnCount)
        {
            ZoomLevel = zoomLevel;
            X = x;
            Y = y;
            Column = ((x % columnCount) + columnCount) % columnCount;
        }

        public int ZoomLevel { get; }
        public int X { get; }
        public int Y { get; }
        public int Column { get; }
        public int Row => Y;

        public Image Image { get; } = new Image
        {
            Stretch = Stretch.Fill,
            IsHitTestVisible = false // avoid touch capture issues
        };

        public bool IsPending { get; set; } = true;

        public void SetImageSource(ImageSource image)
        {
            IsPending = false;

            if (image != null && MapBase.ImageFadeDuration > TimeSpan.Zero)
            {
                AnimateImageOpacity(image);
            }

            Image.Source = image;
        }

        private void BeginOpacityAnimation()
        {
            Image.BeginAnimation(UIElement.OpacityProperty,
                new DoubleAnimation
                {
                    From = 0d,
                    Duration = MapBase.ImageFadeDuration,
                    FillBehavior = FillBehavior.Stop
                });
        }
    }
}

// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if AVALONIA
using Avalonia.Controls;
using Avalonia.Media;
using ImageSource = Avalonia.Media.IImage;
#elif WINUI
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
#elif UWP
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
#else
using System.Windows.Controls;
using System.Windows.Media;
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
            Image.Source = image;

            if (image != null && MapBase.ImageFadeDuration > TimeSpan.Zero)
            {
                AnimateImageOpacity();
            }
        }
    }
}

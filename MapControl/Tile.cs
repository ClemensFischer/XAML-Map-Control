// WPF MapControl - http://wpfmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MapControl
{
    public enum TileLoadState { NotLoaded, Loading, Loaded };

    internal class Tile
    {
        private static readonly DoubleAnimation opacityAnimation = new DoubleAnimation(0d, 1d, TimeSpan.FromSeconds(0.5), FillBehavior.Stop);

        public readonly int ZoomLevel;
        public readonly int X;
        public readonly int Y;
        public readonly Uri Uri;
        public readonly ImageBrush Brush = new ImageBrush();

        public Tile(TileSource tileSource, int zoomLevel, int x, int y)
        {
            ZoomLevel = zoomLevel;
            X = x;
            Y = y;
            Uri = tileSource.GetUri(XIndex, Y, ZoomLevel);
        }

        public TileLoadState LoadState { get; set; }

        public int XIndex
        {
            get
            {
                int numTiles = 1 << ZoomLevel;
                return ((X % numTiles) + numTiles) % numTiles;
            }
        }

        public ImageSource Image
        {
            get { return Brush.ImageSource; }
            set
            {
                if (Brush.ImageSource == null)
                {
                    Brush.BeginAnimation(ImageBrush.OpacityProperty, opacityAnimation);
                }

                Brush.ImageSource = value;
                LoadState = TileLoadState.Loaded;
            }
        }

        public override string ToString()
        {
            return string.Format("{0}.{1}.{2}", ZoomLevel, X, Y);
        }
    }
}

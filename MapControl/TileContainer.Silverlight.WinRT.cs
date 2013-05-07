// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © Clemens Fischer 2012-2013
// Licensed under the Microsoft Public License (Ms-PL)

#if NETFX_CORE
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
#endif

namespace MapControl
{
    internal partial class TileContainer : Panel
    {
        private Matrix GetTransformMatrix(Matrix transform, double scale)
        {
            return transform
                .Scale(scale, scale)
                .Translate(offset.X, offset.Y)
                .RotateAt(rotation, origin.X, origin.Y);
        }

        private Matrix GetTileIndexMatrix(int numTiles)
        {
            var mapToTileScale = (double)numTiles / 360d;
            return ViewportTransform.Matrix
                .Invert() // view to map coordinates
                .Translate(180d, -180d)
                .Scale(mapToTileScale, -mapToTileScale); // map coordinates to tile indices
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            foreach (TileLayer tileLayer in Children)
            {
                tileLayer.Measure(availableSize);
            }

            return new Size();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (TileLayer tileLayer in Children)
            {
                tileLayer.Arrange(new Rect());
            }

            return finalSize;
        }
    }
}

// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © Clemens Fischer 2012-2013
// Licensed under the Microsoft Public License (Ms-PL)

#if NETFX_CORE
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
#else
using System.Windows;
using System.Windows.Controls;
#endif

namespace MapControl
{
    public partial class TileLayer : Panel
    {
        partial void Initialize()
        {
            RenderTransform = transform;
        }

        protected Panel TileContainer
        {
            get { return Parent as Panel; }
        }

        protected void RenderTiles()
        {
            Children.Clear();
            foreach (var tile in tiles)
            {
                Children.Add(tile.Image);
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            foreach (var tile in tiles)
            {
                tile.Image.Measure(availableSize);
            }

            return new Size();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (var tile in tiles)
            {
                var tileSize = (double)(256 << (zoomLevel - tile.ZoomLevel));
                tile.Image.Width = tileSize;
                tile.Image.Height = tileSize;
                tile.Image.Arrange(new Rect(tileSize * tile.X - 256 * grid.X, tileSize * tile.Y - 256 * grid.Y, tileSize, tileSize));
            }

            return finalSize;
        }
    }
}

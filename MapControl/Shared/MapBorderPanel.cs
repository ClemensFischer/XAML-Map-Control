// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2023 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if WINUI
using Microsoft.UI.Xaml;
#elif UWP
using Windows.UI.Xaml;
#else
using System.Windows;
#endif

namespace MapControl
{
    /// <summary>
    /// A MapPanel that adjusts the ViewPosition property of its child elements so that
    /// elements that would be outside the current viewport are arranged on a border area.
    /// Such elements are arranged at a distance of BorderWidth/2 from the edges of the
    /// MapBorderPanel in direction of their original azimuth from the map center.
    /// </summary>
    public class MapBorderPanel : MapPanel
    {
        public static readonly DependencyProperty BorderWidthProperty = DependencyProperty.Register(
            nameof(BorderWidth), typeof(double), typeof(MapBorderPanel), null);

        public static readonly DependencyProperty OnBorderProperty = DependencyProperty.Register(
            "OnBorder", typeof(bool), typeof(MapBorderPanel), null);

        public double BorderWidth
        {
            get => (double)GetValue(BorderWidthProperty);
            set => SetValue(BorderWidthProperty, value);
        }

        public static bool GetOnBorder(FrameworkElement element)
        {
            return (bool)element.GetValue(OnBorderProperty);
        }

        protected override void SetViewPosition(FrameworkElement element, ref Point? position)
        {
            var onBorder = false;
            var w = ParentMap.RenderSize.Width;
            var h = ParentMap.RenderSize.Height;
            var minX = BorderWidth / 2d;
            var minY = BorderWidth / 2d;
            var maxX = w - BorderWidth / 2d;
            var maxY = h - BorderWidth / 2d;

            if (position.HasValue &&
                (position.Value.X < minX || position.Value.X > maxX ||
                 position.Value.Y < minY || position.Value.Y > maxY))
            {
                var dx = position.Value.X - w / 2d;
                var dy = position.Value.Y - h / 2d;
                var cx = (maxX - minX) / 2d;
                var cy = (maxY - minY) / 2d;
                double x, y;

                if (dx < 0d)
                {
                    x = minX;
                    y = minY + cy - cx * dy / dx;
                }
                else
                {
                    x = maxX;
                    y = minY + cy + cx * dy / dx;
                }

                if (y < minY || y > maxY)
                {
                    if (dy < 0d)
                    {
                        x = minX + cx - cy * dx / dy;
                        y = minY;
                    }
                    else
                    {
                        x = minX + cx + cy * dx / dy;
                        y = maxY;
                    }
                }

                position = new Point(x, y);
                onBorder = true;
            }

            element.SetValue(OnBorderProperty, onBorder);

            base.SetViewPosition(element, ref position);
        }
    }
}

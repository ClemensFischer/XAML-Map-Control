#if WPF
using System.Windows;
#elif UWP
using Windows.UI.Xaml;
#elif WINUI
using Microsoft.UI.Xaml;
#elif AVALONIA
using Avalonia;
#endif

namespace MapControl
{
    /// <summary>
    /// A MapPanel that adjusts the ViewPosition property of its child elements so that
    /// elements that would be outside the current viewport are arranged on a border area.
    /// Such elements are arranged at a distance of BorderWidth/2 from the edges of the
    /// MapBorderPanel in direction of their original azimuth from the map center.
    /// </summary>
    public partial class MapBorderPanel : MapPanel
    {
        public static readonly DependencyProperty BorderWidthProperty =
            DependencyPropertyHelper.Register<MapBorderPanel, double>(nameof(BorderWidth));

        public static readonly DependencyProperty OnBorderProperty =
            DependencyPropertyHelper.RegisterAttached<bool>("OnBorder", typeof(MapBorderPanel));

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
            var w = ParentMap.ActualWidth;
            var h = ParentMap.ActualHeight;
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

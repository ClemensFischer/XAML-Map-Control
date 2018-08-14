// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if WINDOWS_UWP
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
#endif

namespace MapControl
{
    /// <summary>
    /// Draws a map scale overlay.
    /// </summary>
    public class MapScale : MapOverlay
    {
        public static readonly DependencyProperty PaddingProperty = DependencyProperty.Register(
            nameof(Padding), typeof(Thickness), typeof(MapScale), new PropertyMetadata(new Thickness(4)));

        private readonly Polyline line = new Polyline();

        private readonly TextBlock label = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            TextAlignment = TextAlignment.Center
        };

        public MapScale()
        {
            IsHitTestVisible = false;
            MinWidth = 100d;

            line.SetBinding(Shape.StrokeProperty, GetBinding(StrokeProperty, nameof(Stroke)));
            line.SetBinding(Shape.StrokeThicknessProperty, GetBinding(StrokeThicknessProperty, nameof(StrokeThickness)));
#if WINDOWS_UWP
            label.SetBinding(TextBlock.ForegroundProperty, GetBinding(ForegroundProperty, nameof(Foreground)));
#endif
            Children.Add(line);
            Children.Add(label);
        }

        public Thickness Padding
        {
            get { return (Thickness)GetValue(PaddingProperty); }
            set { SetValue(PaddingProperty, value); }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var size = new Size();

            if (ParentMap != null && ParentMap.ScaleTransform.ScaleX > 0d)
            {
                var length = MinWidth / ParentMap.ScaleTransform.ScaleX;
                var magnitude = Math.Pow(10d, Math.Floor(Math.Log10(length)));

                if (length / magnitude < 2d)
                {
                    length = 2d * magnitude;
                }
                else if (length / magnitude < 5d)
                {
                    length = 5d * magnitude;
                }
                else
                {
                    length = 10d * magnitude;
                }

                size.Width = length * ParentMap.ScaleTransform.ScaleX + StrokeThickness + Padding.Left + Padding.Right;
                size.Height = 1.25 * FontSize + StrokeThickness + Padding.Top + Padding.Bottom;

                var x1 = Padding.Left + StrokeThickness / 2d;
                var x2 = size.Width - Padding.Right - StrokeThickness / 2d;
                var y1 = size.Height / 2d;
                var y2 = size.Height - Padding.Bottom - StrokeThickness / 2d;

                line.Points = new PointCollection
                {
                    new Point(x1, y1),
                    new Point(x1, y2),
                    new Point(x2, y2),
                    new Point(x2, y1)
                };
                line.Measure(size);

                label.Text = length >= 1000d ? string.Format("{0:0} km", length / 1000d) : string.Format("{0:0} m", length);
                label.Width = size.Width;
                label.Height = size.Height;
                label.Measure(size);
            }

            return size;
        }

        protected override void OnViewportChanged(ViewportChangedEventArgs e)
        {
            InvalidateMeasure();
        }
    }
}

// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if WPF
using System.Windows;
using System.Windows.Media;
#elif UWP
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#elif WINUI
using Windows.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
#endif

namespace MapControl
{
    public partial class PushpinBorder
    {
        public Size ArrowSize
        {
            get => (Size)GetValue(ArrowSizeProperty);
            set => SetValue(ArrowSizeProperty, value);
        }

        public double BorderWidth
        {
            get => (double)GetValue(BorderWidthProperty);
            set => SetValue(BorderWidthProperty, value);
        }

        protected virtual Geometry BuildGeometry()
        {
            var width = Math.Floor(ActualWidth);
            var height = Math.Floor(ActualHeight);
            var x1 = BorderWidth / 2d;
            var y1 = BorderWidth / 2d;
            var x2 = width - x1;
            var y3 = height - y1;
            var y2 = y3 - ArrowSize.Height;
            var aw = ArrowSize.Width;
            var r1 = CornerRadius.TopLeft;
            var r2 = CornerRadius.TopRight;
            var r3 = CornerRadius.BottomRight;
            var r4 = CornerRadius.BottomLeft;

            var figure = new PathFigure
            {
                StartPoint = new Point(x1, y1 + r1),
                IsClosed = true,
                IsFilled = true
            };

            figure.Segments.Add(ArcTo(x1 + r1, y1, r1));
            figure.Segments.Add(LineTo(x2 - r2, y1));
            figure.Segments.Add(ArcTo(x2, y1 + r2, r2));

            if (HorizontalAlignment == HorizontalAlignment.Right)
            {
                figure.Segments.Add(LineTo(x2, y3));
                figure.Segments.Add(LineTo(x2 - aw, y2));
            }
            else
            {
                figure.Segments.Add(LineTo(x2, y2 - r3));
                figure.Segments.Add(ArcTo(x2 - r3, y2, r3));
            }

            if (HorizontalAlignment == HorizontalAlignment.Center)
            {
                var c = width / 2d;
                figure.Segments.Add(LineTo(c + aw / 2d, y2));
                figure.Segments.Add(LineTo(c, y3));
                figure.Segments.Add(LineTo(c - aw / 2d, y2));
            }

            if (HorizontalAlignment == HorizontalAlignment.Left || HorizontalAlignment == HorizontalAlignment.Stretch)
            {
                figure.Segments.Add(LineTo(x1 + aw, y2));
                figure.Segments.Add(LineTo(x1, y3));
            }
            else
            {
                figure.Segments.Add(LineTo(x1 + r4, y2));
                figure.Segments.Add(ArcTo(x1, y2 - r4, r4));
            }

            var geometry = new PathGeometry();
            geometry.Figures.Add(figure);

            return geometry;
        }

        private static LineSegment LineTo(double x, double y)
        {
            return new LineSegment
            {
                Point = new Point(x, y)
            };
        }

        private static ArcSegment ArcTo(double x, double y, double r)
        {
            return new ArcSegment
            {
                Point = new Point(x, y),
                Size = new Size(r, r),
                SweepDirection = SweepDirection.Clockwise
            };
        }
    }
}

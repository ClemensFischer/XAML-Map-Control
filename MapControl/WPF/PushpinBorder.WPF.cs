// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MapControl
{
    public class PushpinBorder : Decorator
    {
        public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(
            nameof(Background), typeof(Brush), typeof(PushpinBorder),
            new FrameworkPropertyMetadata(null,
                FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty BorderBrushProperty = DependencyProperty.Register(
            nameof(BorderBrush), typeof(Brush), typeof(PushpinBorder),
            new FrameworkPropertyMetadata(null,
                FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty BorderThicknessProperty = DependencyProperty.Register(
            nameof(BorderThickness), typeof(double), typeof(PushpinBorder),
            new FrameworkPropertyMetadata(0d,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            nameof(CornerRadius), typeof(CornerRadius), typeof(PushpinBorder),
            new FrameworkPropertyMetadata(new CornerRadius(),
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty PaddingProperty = DependencyProperty.Register(
            nameof(Padding), typeof(Thickness), typeof(PushpinBorder),
            new FrameworkPropertyMetadata(new Thickness(2),
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ArrowSizeProperty = DependencyProperty.Register(
            nameof(ArrowSize), typeof(Size), typeof(PushpinBorder),
            new FrameworkPropertyMetadata(new Size(10d, 20d),
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        public Brush BorderBrush
        {
            get { return (Brush)GetValue(BorderBrushProperty); }
            set { SetValue(BorderBrushProperty, value); }
        }

        public double BorderThickness
        {
            get { return (double)GetValue(BorderThicknessProperty); }
            set { SetValue(BorderThicknessProperty, value); }
        }

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public Thickness Padding
        {
            get { return (Thickness)GetValue(PaddingProperty); }
            set { SetValue(PaddingProperty, value); }
        }

        public Size ArrowSize
        {
            get { return (Size)GetValue(ArrowSizeProperty); }
            set { SetValue(ArrowSizeProperty, value); }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            var width = Padding.Left + Padding.Right;
            var height = Padding.Top + Padding.Bottom;

            if (Child != null)
            {
                Child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                width += Child.DesiredSize.Width;
                height += Child.DesiredSize.Height;
            }

            var minWidth = Math.Max(
                CornerRadius.TopLeft + CornerRadius.TopRight,
                CornerRadius.BottomLeft + CornerRadius.BottomRight + ArrowSize.Width);

            var minHeight = Math.Max(
                CornerRadius.TopLeft + CornerRadius.BottomLeft,
                CornerRadius.TopRight + CornerRadius.BottomRight);

            return new Size(
                Math.Max(width, minWidth) + BorderThickness,
                Math.Max(height, minHeight) + BorderThickness + ArrowSize.Height);
        }

        protected override Size ArrangeOverride(Size size)
        {
            if (Child != null)
            {
                Child.Arrange(new Rect(
                    Padding.Left, Padding.Top, size.Width - Padding.Right, size.Height - Padding.Bottom));
            }

            return size;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var pen = new Pen
            {
                Brush = BorderBrush,
                Thickness = BorderThickness,
                LineJoin = PenLineJoin.Round
            };

            drawingContext.DrawGeometry(Background, pen, BuildGeometry(RenderSize));

            base.OnRender(drawingContext);
        }

        private Geometry BuildGeometry(Size size)
        {
            var x1 = BorderThickness / 2d;
            var y1 = BorderThickness / 2d;
            var x2 = size.Width - x1;
            var y3 = size.Height - y1;
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

            if (HorizontalAlignment != HorizontalAlignment.Left && HorizontalAlignment != HorizontalAlignment.Right)
            {
                var c = size.Width / 2d;
                figure.Segments.Add(LineTo(c + aw / 2d, y2));
                figure.Segments.Add(LineTo(c, y3));
                figure.Segments.Add(LineTo(c - aw / 2d, y2));
            }

            if (HorizontalAlignment == HorizontalAlignment.Left)
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

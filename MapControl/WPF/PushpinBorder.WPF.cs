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

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            nameof(CornerRadius), typeof(CornerRadius), typeof(PushpinBorder),
            new FrameworkPropertyMetadata(new CornerRadius(),
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty PaddingProperty = DependencyProperty.Register(
            nameof(Padding), typeof(Thickness), typeof(PushpinBorder),
            new FrameworkPropertyMetadata(new Thickness(2),
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty BorderThicknessProperty = DependencyProperty.Register(
            nameof(BorderThickness), typeof(double), typeof(PushpinBorder),
            new FrameworkPropertyMetadata(0d,
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
                Math.Max(width, minWidth),
                Math.Max(height, minHeight) + ArrowSize.Height);
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
            var w = RenderSize.Width;
            var aw = ArrowSize.Width;
            var h1 = RenderSize.Height - ArrowSize.Height;
            var h2 = RenderSize.Height;
            var r1 = CornerRadius.TopLeft;
            var r2 = CornerRadius.TopRight;
            var r3 = CornerRadius.BottomRight;
            var r4 = CornerRadius.BottomLeft;

            var pen = new Pen
            {
                Brush = BorderBrush,
                Thickness = BorderThickness,
                LineJoin = PenLineJoin.Round
            };

            var geometry = new StreamGeometry();

            using (var context = geometry.Open())
            {
                context.BeginFigure(new Point(0d, r1), true, true);
                context.ArcTo(new Point(r1, 0d), new Size(r1, r1), 0d, false, SweepDirection.Clockwise, true, true);

                context.LineTo(new Point(w - r2, 0d), true, true);
                context.ArcTo(new Point(w, r2), new Size(r2, r2), 0d, false, SweepDirection.Clockwise, true, true);

                if (HorizontalAlignment == HorizontalAlignment.Right)
                {
                    context.LineTo(new Point(w, h2), true, true);
                    context.LineTo(new Point(w - aw, h1), true, true);
                }
                else
                {
                    context.LineTo(new Point(w, h1 - r3), true, true);
                    context.ArcTo(new Point(w - r3, h1), new Size(r3, r3), 0d, false, SweepDirection.Clockwise, true, true);
                }

                if (HorizontalAlignment != HorizontalAlignment.Left && HorizontalAlignment != HorizontalAlignment.Right)
                {
                    context.LineTo(new Point((w + aw) / 2d, h1), true, true);
                    context.LineTo(new Point(w / 2d, h2), true, true);
                    context.LineTo(new Point((w - aw) / 2d, h1), true, true);
                }

                if (HorizontalAlignment == HorizontalAlignment.Left)
                {
                    context.LineTo(new Point(aw, h1), true, true);
                    context.LineTo(new Point(0d, h2), true, true);
                }
                else
                {
                    context.LineTo(new Point(r4, h1), true, true);
                    context.ArcTo(new Point(0d, h1 - r4), new Size(r4, r4), 0d, false, SweepDirection.Clockwise, true, true);
                }
            }

            drawingContext.DrawGeometry(Background, pen, geometry);

            base.OnRender(drawingContext);
        }
    }
}

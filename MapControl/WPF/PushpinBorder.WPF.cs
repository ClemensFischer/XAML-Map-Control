// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MapControl
{
    public partial class PushpinBorder : Decorator
    {
        public static readonly DependencyProperty ArrowSizeProperty =
            DependencyPropertyHelper.Register<PushpinBorder, Size>(nameof(ArrowSize), new Size(10d, 20d),
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender);

        public static readonly DependencyProperty BorderWidthProperty =
            DependencyPropertyHelper.Register<PushpinBorder, double>(nameof(BorderWidth), 0d,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender);

        public static readonly DependencyProperty BackgroundProperty =
            DependencyPropertyHelper.Register<PushpinBorder, Brush>(nameof(Background), null,
                FrameworkPropertyMetadataOptions.AffectsRender);

        public static readonly DependencyProperty BorderBrushProperty =
            DependencyPropertyHelper.Register<PushpinBorder, Brush>(nameof(BorderBrush), null,
                FrameworkPropertyMetadataOptions.AffectsRender);

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyPropertyHelper.Register<PushpinBorder, CornerRadius>(nameof(CornerRadius), new CornerRadius(),
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender);

        public static readonly DependencyProperty PaddingProperty =
            DependencyPropertyHelper.Register<PushpinBorder, Thickness>(nameof(Padding), new Thickness(2),
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender);

        public Brush Background
        {
            get => (Brush)GetValue(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }

        public Brush BorderBrush
        {
            get => (Brush)GetValue(BorderBrushProperty);
            set => SetValue(BorderBrushProperty, value);
        }

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public Thickness Padding
        {
            get => (Thickness)GetValue(PaddingProperty);
            set => SetValue(PaddingProperty, value);
        }

        protected override Size MeasureOverride(Size constraint)
        {
            var width = 2d * BorderWidth + Padding.Left + Padding.Right;
            var height = 2d * BorderWidth + Padding.Top + Padding.Bottom;

            if (Child != null)
            {
                Child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                width += Child.DesiredSize.Width;
                height += Child.DesiredSize.Height;
            }

            var minWidth = BorderWidth + Math.Max(
                CornerRadius.TopLeft + CornerRadius.TopRight,
                CornerRadius.BottomLeft + CornerRadius.BottomRight + ArrowSize.Width);

            var minHeight = BorderWidth + Math.Max(
                CornerRadius.TopLeft + CornerRadius.BottomLeft,
                CornerRadius.TopRight + CornerRadius.BottomRight);

            return new Size(
                Math.Max(width, minWidth),
                Math.Max(height, minHeight) + ArrowSize.Height);
        }

        protected override Size ArrangeOverride(Size size)
        {
            Child?.Arrange(new Rect(
                BorderWidth + Padding.Left,
                BorderWidth + Padding.Top,
                Child.DesiredSize.Width,
                Child.DesiredSize.Height));

            return DesiredSize;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var pen = new Pen
            {
                Brush = BorderBrush,
                Thickness = BorderWidth,
                LineJoin = PenLineJoin.Round
            };

            drawingContext.DrawGeometry(Background, pen, BuildGeometry());

            base.OnRender(drawingContext);
        }
    }
}

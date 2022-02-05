// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MapControl
{
    public partial class PushpinBorder : Decorator
    {
        public static readonly DependencyProperty ArrowSizeProperty = DependencyProperty.Register(
            nameof(ArrowSize), typeof(Size), typeof(PushpinBorder),
            new FrameworkPropertyMetadata(new Size(10d, 20d),
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty BorderWidthProperty = DependencyProperty.Register(
            nameof(BorderWidth), typeof(double), typeof(PushpinBorder),
            new FrameworkPropertyMetadata(0d,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

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
            if (Child != null)
            {
                Child.Arrange(new Rect(
                    BorderWidth + Padding.Left,
                    BorderWidth + Padding.Top,
                    size.Width - BorderWidth - Padding.Right,
                    size.Height - BorderWidth - Padding.Bottom));
            }

            return size;
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

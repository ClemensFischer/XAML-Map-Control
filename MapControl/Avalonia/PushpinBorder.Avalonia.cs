// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;

namespace MapControl
{
    public partial class PushpinBorder : Decorator
    {
        public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
            DependencyPropertyHelper.Register<PushpinBorder, CornerRadius>(nameof(CornerRadius), new CornerRadius());

        public static readonly StyledProperty<Size> ArrowSizeProperty =
            DependencyPropertyHelper.Register<PushpinBorder, Size>(nameof(ArrowSize), new Size(10d, 20d));

        public static readonly StyledProperty<double> BorderWidthProperty =
            DependencyPropertyHelper.Register<PushpinBorder, double>(nameof(BorderWidth));

        public static readonly StyledProperty<IBrush> BackgroundProperty =
            DependencyPropertyHelper.Register<PushpinBorder, IBrush>(nameof(Background));

        public static readonly StyledProperty<IBrush> BorderBrushProperty =
            DependencyPropertyHelper.Register<PushpinBorder, IBrush>(nameof(BorderBrush));

        static PushpinBorder()
        {
            AffectsMeasure<PushpinBorder>(ArrowSizeProperty, BorderWidthProperty, CornerRadiusProperty);
            AffectsRender<PushpinBorder>(ArrowSizeProperty, BorderWidthProperty, CornerRadiusProperty, BackgroundProperty, BorderBrushProperty);
        }

        private Size RenderSize => Bounds.Size;

        public CornerRadius CornerRadius
        {
            get => GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public IBrush Background
        {
            get => GetValue(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }

        public IBrush BorderBrush
        {
            get => GetValue(BorderBrushProperty);
            set => SetValue(BorderBrushProperty, value);
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

        public override void Render(DrawingContext drawingContext)
        {
            var pen = new Pen
            {
                Brush = BorderBrush,
                Thickness = BorderWidth,
                LineJoin = PenLineJoin.Round
            };

            drawingContext.DrawGeometry(Background, pen, BuildGeometry());

            base.Render(drawingContext);
        }
    }
}

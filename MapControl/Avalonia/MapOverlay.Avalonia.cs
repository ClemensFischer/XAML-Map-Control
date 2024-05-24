// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using Avalonia.Collections;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Media;

namespace MapControl
{
    public partial class MapOverlay
    {
        public static readonly StyledProperty<FontFamily> FontFamilyProperty =
            DependencyPropertyHelper.AddOwner<MapOverlay, FontFamily>(TextElement.FontFamilyProperty);

        public static readonly StyledProperty<double> FontSizeProperty =
            DependencyPropertyHelper.AddOwner<MapOverlay, double>(TextElement.FontSizeProperty);

        public static readonly StyledProperty<FontStyle> FontStyleProperty =
            DependencyPropertyHelper.AddOwner<MapOverlay, FontStyle>(TextElement.FontStyleProperty);

        public static readonly StyledProperty<FontStretch> FontStretchProperty =
            DependencyPropertyHelper.AddOwner<MapOverlay, FontStretch>(TextElement.FontStretchProperty);

        public static readonly StyledProperty<FontWeight> FontWeightProperty =
            DependencyPropertyHelper.AddOwner<MapOverlay, FontWeight>(TextElement.FontWeightProperty);

        public static readonly StyledProperty<IBrush> ForegroundProperty =
            DependencyPropertyHelper.AddOwner<MapOverlay, IBrush>(TextElement.ForegroundProperty);

        public static readonly StyledProperty<IBrush> StrokeProperty =
            DependencyPropertyHelper.AddOwner<MapOverlay, IBrush>(Shape.StrokeProperty);

        public static readonly StyledProperty<double> StrokeThicknessProperty =
            DependencyPropertyHelper.Register<MapOverlay, double>(nameof(StrokeThickness), 1d);

        public static readonly StyledProperty<AvaloniaList<double>> StrokeDashArrayProperty =
            DependencyPropertyHelper.AddOwner<MapOverlay, AvaloniaList<double>>(Shape.StrokeDashArrayProperty);

        public static readonly StyledProperty<double> StrokeDashOffsetProperty =
            DependencyPropertyHelper.AddOwner<MapOverlay, double>(Shape.StrokeDashOffsetProperty);

        public static readonly StyledProperty<PenLineCap> StrokeLineCapProperty =
            DependencyPropertyHelper.AddOwner<MapOverlay, PenLineCap>(Shape.StrokeLineCapProperty);

        public static readonly StyledProperty<PenLineJoin> StrokeLineJoinProperty =
            DependencyPropertyHelper.AddOwner<MapOverlay, PenLineJoin>(Shape.StrokeJoinProperty);

        public static readonly StyledProperty<double> StrokeMiterLimitProperty =
            DependencyPropertyHelper.Register<MapOverlay, double>(nameof(StrokeMiterLimit));

        protected override void OnInitialized()
        {
            base.OnInitialized();

            if (Stroke == null)
            {
                this.SetBinding(StrokeProperty, this.CreateBinding(nameof(Foreground)));
            }
        }

        public Pen CreatePen()
        {
            return new Pen
            {
                Brush = Stroke,
                Thickness = StrokeThickness,
                LineJoin = StrokeLineJoin,
                MiterLimit = StrokeMiterLimit,
                LineCap = StrokeLineCap,
                DashStyle = new DashStyle(StrokeDashArray, StrokeDashOffset)
            };
        }
    }
}

// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MapControl
{
    public partial class MapOverlay
    {
        public static readonly DependencyProperty FontFamilyProperty = TextElement.FontFamilyProperty.AddOwner(
            typeof(MapOverlay), new FrameworkPropertyMetadata { AffectsRender = true, Inherits = true });

        public static readonly DependencyProperty FontSizeProperty = TextElement.FontSizeProperty.AddOwner(
            typeof(MapOverlay), new FrameworkPropertyMetadata { AffectsRender = true, Inherits = true });

        public static readonly DependencyProperty FontStyleProperty = TextElement.FontStyleProperty.AddOwner(
            typeof(MapOverlay), new FrameworkPropertyMetadata { AffectsRender = true, Inherits = true });

        public static readonly DependencyProperty FontStretchProperty = TextElement.FontStretchProperty.AddOwner(
            typeof(MapOverlay), new FrameworkPropertyMetadata { AffectsRender = true, Inherits = true });

        public static readonly DependencyProperty FontWeightProperty = TextElement.FontWeightProperty.AddOwner(
            typeof(MapOverlay), new FrameworkPropertyMetadata { AffectsRender = true, Inherits = true });

        public static readonly DependencyProperty ForegroundProperty = TextElement.ForegroundProperty.AddOwner(
            typeof(MapOverlay), new FrameworkPropertyMetadata { AffectsRender = true, Inherits = true });

        public static readonly DependencyProperty StrokeProperty = Shape.StrokeProperty.AddOwner(
            typeof(MapOverlay), new FrameworkPropertyMetadata { AffectsRender = true });

        public static readonly DependencyProperty StrokeThicknessProperty = Shape.StrokeThicknessProperty.AddOwner(
            typeof(MapOverlay), new FrameworkPropertyMetadata { AffectsRender = true });

        public static readonly DependencyProperty StrokeDashArrayProperty = Shape.StrokeDashArrayProperty.AddOwner(
            typeof(MapOverlay), new FrameworkPropertyMetadata { AffectsRender = true });

        public static readonly DependencyProperty StrokeDashOffsetProperty = Shape.StrokeDashOffsetProperty.AddOwner(
            typeof(MapOverlay), new FrameworkPropertyMetadata { AffectsRender = true });

        public static readonly DependencyProperty StrokeDashCapProperty = Shape.StrokeDashCapProperty.AddOwner(
            typeof(MapOverlay), new FrameworkPropertyMetadata { AffectsRender = true });

        public static readonly DependencyProperty StrokeStartLineCapProperty = Shape.StrokeStartLineCapProperty.AddOwner(
            typeof(MapOverlay), new FrameworkPropertyMetadata { AffectsRender = true });

        public static readonly DependencyProperty StrokeEndLineCapProperty = Shape.StrokeEndLineCapProperty.AddOwner(
            typeof(MapOverlay), new FrameworkPropertyMetadata { AffectsRender = true });

        public static readonly DependencyProperty StrokeLineJoinProperty = Shape.StrokeLineJoinProperty.AddOwner(
            typeof(MapOverlay), new FrameworkPropertyMetadata { AffectsRender = true });

        public static readonly DependencyProperty StrokeMiterLimitProperty = Shape.StrokeMiterLimitProperty.AddOwner(
            typeof(MapOverlay), new FrameworkPropertyMetadata { AffectsRender = true });

        static MapOverlay()
        {
            IsHitTestVisibleProperty.OverrideMetadata(typeof(MapOverlay), new FrameworkPropertyMetadata(false));
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.ValidateProperty(StrokeProperty, this, nameof(Foreground));
        }

        public Pen CreatePen()
        {
            return new Pen
            {
                Brush = Stroke,
                Thickness = StrokeThickness,
                LineJoin = StrokeLineJoin,
                MiterLimit = StrokeMiterLimit,
                StartLineCap = StrokeStartLineCap,
                EndLineCap = StrokeEndLineCap,
                DashCap = StrokeDashCap,
                DashStyle = new DashStyle(StrokeDashArray, StrokeDashOffset)
            };
        }
    }
}

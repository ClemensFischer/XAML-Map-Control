// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2017 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MapControl
{
    public partial class MapOverlay
    {
        public static readonly DependencyProperty FontSizeProperty = Control.FontSizeProperty.AddOwner(typeof(MapOverlay));

        public static readonly DependencyProperty FontFamilyProperty = Control.FontFamilyProperty.AddOwner(typeof(MapOverlay));

        public static readonly DependencyProperty FontStyleProperty = Control.FontStyleProperty.AddOwner(typeof(MapOverlay));

        public static readonly DependencyProperty FontStretchProperty = Control.FontStretchProperty.AddOwner(typeof(MapOverlay));

        public static readonly DependencyProperty FontWeightProperty = Control.FontWeightProperty.AddOwner(typeof(MapOverlay));

        public static readonly DependencyProperty ForegroundProperty = Control.ForegroundProperty.AddOwner(typeof(MapOverlay));

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

        protected Typeface CreateTypeface()
        {
            return new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
        }

        protected Pen CreatePen()
        {
            return new Pen
            {
                Brush = Stroke ?? Foreground,
                Thickness = StrokeThickness,
                DashStyle = new DashStyle(StrokeDashArray, StrokeDashOffset),
                DashCap = StrokeDashCap,
                StartLineCap = StrokeStartLineCap,
                EndLineCap = StrokeEndLineCap,
                LineJoin = StrokeLineJoin,
                MiterLimit = StrokeMiterLimit
            };
        }
    }
}

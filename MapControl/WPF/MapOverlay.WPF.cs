// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
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

        public static readonly DependencyProperty StrokeProperty = Shape.StrokeProperty.AddOwner(typeof(MapOverlay),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty StrokeThicknessProperty = Shape.StrokeThicknessProperty.AddOwner(typeof(MapOverlay),
            new FrameworkPropertyMetadata(1d, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty StrokeDashArrayProperty = Shape.StrokeDashArrayProperty.AddOwner(typeof(MapOverlay),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty StrokeDashOffsetProperty = Shape.StrokeDashOffsetProperty.AddOwner(typeof(MapOverlay),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty StrokeDashCapProperty = Shape.StrokeDashCapProperty.AddOwner(typeof(MapOverlay),
            new FrameworkPropertyMetadata(PenLineCap.Flat, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty StrokeStartLineCapProperty = Shape.StrokeStartLineCapProperty.AddOwner(typeof(MapOverlay),
            new FrameworkPropertyMetadata(PenLineCap.Flat, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty StrokeEndLineCapProperty = Shape.StrokeEndLineCapProperty.AddOwner(typeof(MapOverlay),
            new FrameworkPropertyMetadata(PenLineCap.Flat, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty StrokeLineJoinProperty = Shape.StrokeLineJoinProperty.AddOwner(typeof(MapOverlay),
            new FrameworkPropertyMetadata(PenLineJoin.Miter, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty StrokeMiterLimitProperty = Shape.StrokeMiterLimitProperty.AddOwner(typeof(MapOverlay),
            new FrameworkPropertyMetadata(1d, FrameworkPropertyMetadataOptions.AffectsRender));

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

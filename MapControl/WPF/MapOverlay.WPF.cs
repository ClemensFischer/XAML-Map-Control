// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
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
        public static readonly DependencyProperty FontFamilyProperty =
            DependencyPropertyHelper.AddOwner<MapOverlay>(TextElement.FontFamilyProperty,
                FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits);

        public static readonly DependencyProperty FontSizeProperty =
            DependencyPropertyHelper.AddOwner<MapOverlay>(TextElement.FontSizeProperty,
                FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits);

        public static readonly DependencyProperty FontStyleProperty =
            DependencyPropertyHelper.AddOwner<MapOverlay>(TextElement.FontStyleProperty,
                FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits);

        public static readonly DependencyProperty FontStretchProperty =
            DependencyPropertyHelper.AddOwner<MapOverlay>(TextElement.FontStretchProperty,
                FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits);

        public static readonly DependencyProperty FontWeightProperty =
            DependencyPropertyHelper.AddOwner<MapOverlay>(TextElement.FontWeightProperty,
                FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits);

        public static readonly DependencyProperty ForegroundProperty =
            DependencyPropertyHelper.AddOwner<MapOverlay>(TextElement.ForegroundProperty,
                FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits);

        public static readonly DependencyProperty StrokeProperty =
            DependencyPropertyHelper.AddOwner<MapOverlay>(Shape.StrokeProperty,
                FrameworkPropertyMetadataOptions.AffectsRender);

        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyPropertyHelper.AddOwner<MapOverlay>(Shape.StrokeThicknessProperty,
                FrameworkPropertyMetadataOptions.AffectsRender);

        public static readonly DependencyProperty StrokeDashArrayProperty =
            DependencyPropertyHelper.AddOwner<MapOverlay>(Shape.StrokeDashArrayProperty,
                FrameworkPropertyMetadataOptions.AffectsRender);

        public static readonly DependencyProperty StrokeDashOffsetProperty =
            DependencyPropertyHelper.AddOwner<MapOverlay>(Shape.StrokeDashOffsetProperty,
                FrameworkPropertyMetadataOptions.AffectsRender);

        public static readonly DependencyProperty StrokeDashCapProperty =
            DependencyPropertyHelper.AddOwner<MapOverlay>(Shape.StrokeDashCapProperty,
                FrameworkPropertyMetadataOptions.AffectsRender);

        public static readonly DependencyProperty StrokeStartLineCapProperty =
            DependencyPropertyHelper.AddOwner<MapOverlay>(Shape.StrokeStartLineCapProperty,
                FrameworkPropertyMetadataOptions.AffectsRender);

        public static readonly DependencyProperty StrokeEndLineCapProperty =
            DependencyPropertyHelper.AddOwner<MapOverlay>(Shape.StrokeEndLineCapProperty,
                FrameworkPropertyMetadataOptions.AffectsRender);

        public static readonly DependencyProperty StrokeLineJoinProperty =
            DependencyPropertyHelper.AddOwner<MapOverlay>(Shape.StrokeLineJoinProperty,
                FrameworkPropertyMetadataOptions.AffectsRender);

        public static readonly DependencyProperty StrokeMiterLimitProperty =
            DependencyPropertyHelper.AddOwner<MapOverlay>(Shape.StrokeMiterLimitProperty,
                FrameworkPropertyMetadataOptions.AffectsRender);

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            // If this.Stroke is not explicitly set, bind it to this.Foreground.
            //
            this.SetBindingOnUnsetProperty(StrokeProperty, this, ForegroundProperty, nameof(Foreground));
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

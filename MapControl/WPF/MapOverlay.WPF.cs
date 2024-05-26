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
    public class MapOverlay : MapPanel
    {
        public static readonly DependencyProperty FontFamilyProperty =
            DependencyPropertyHelper.AddOwner<MapOverlay, FontFamily>(TextElement.FontFamilyProperty);

        public static readonly DependencyProperty FontSizeProperty =
            DependencyPropertyHelper.AddOwner<MapOverlay, double>(TextElement.FontSizeProperty);

        public static readonly DependencyProperty FontStyleProperty =
            DependencyPropertyHelper.AddOwner<MapOverlay, FontStyle>(TextElement.FontStyleProperty);

        public static readonly DependencyProperty FontStretchProperty =
            DependencyPropertyHelper.AddOwner<MapOverlay, FontStretch>(TextElement.FontStretchProperty);

        public static readonly DependencyProperty FontWeightProperty =
            DependencyPropertyHelper.AddOwner<MapOverlay, FontWeight>(TextElement.FontWeightProperty);

        public static readonly DependencyProperty ForegroundProperty =
            DependencyPropertyHelper.AddOwner<MapOverlay, Brush>(TextElement.ForegroundProperty);

        public static readonly DependencyProperty StrokeProperty =
            DependencyPropertyHelper.AddOwner<MapOverlay, Brush>(Shape.StrokeProperty);

        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyPropertyHelper.AddOwner<MapOverlay, double>(Shape.StrokeThicknessProperty);

        public static readonly DependencyProperty StrokeDashArrayProperty =
            DependencyPropertyHelper.AddOwner<MapOverlay, DoubleCollection>(Shape.StrokeDashArrayProperty);

        public static readonly DependencyProperty StrokeDashOffsetProperty =
            DependencyPropertyHelper.AddOwner<MapOverlay, double>(Shape.StrokeDashOffsetProperty);

        public static readonly DependencyProperty StrokeLineCapProperty =
            DependencyPropertyHelper.AddOwner<MapOverlay, PenLineCap>(Shape.StrokeDashCapProperty);

        public static readonly DependencyProperty StrokeLineJoinProperty =
            DependencyPropertyHelper.AddOwner<MapOverlay, PenLineJoin>(Shape.StrokeLineJoinProperty);

        public static readonly DependencyProperty StrokeMiterLimitProperty =
            DependencyPropertyHelper.AddOwner<MapOverlay, double>(Shape.StrokeMiterLimitProperty);

        public FontFamily FontFamily
        {
            get => (FontFamily)GetValue(FontFamilyProperty);
            set => SetValue(FontFamilyProperty, value);
        }

        public double FontSize
        {
            get => (double)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        public FontStyle FontStyle
        {
            get => (FontStyle)GetValue(FontStyleProperty);
            set => SetValue(FontStyleProperty, value);
        }

        public FontStretch FontStretch
        {
            get => (FontStretch)GetValue(FontStretchProperty);
            set => SetValue(FontStretchProperty, value);
        }

        public FontWeight FontWeight
        {
            get => (FontWeight)GetValue(FontWeightProperty);
            set => SetValue(FontWeightProperty, value);
        }

        public Brush Foreground
        {
            get => (Brush)GetValue(ForegroundProperty);
            set => SetValue(ForegroundProperty, value);
        }

        public Brush Stroke
        {
            get => (Brush)GetValue(StrokeProperty);
            set => SetValue(StrokeProperty, value);
        }

        public double StrokeThickness
        {
            get => (double)GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }

        public DoubleCollection StrokeDashArray
        {
            get => (DoubleCollection)GetValue(StrokeDashArrayProperty);
            set => SetValue(StrokeDashArrayProperty, value);
        }

        public double StrokeDashOffset
        {
            get => (double)GetValue(StrokeDashOffsetProperty);
            set => SetValue(StrokeDashOffsetProperty, value);
        }

        public PenLineCap StrokeLineCap
        {
            get => (PenLineCap)GetValue(StrokeLineCapProperty);
            set => SetValue(StrokeLineCapProperty, value);
        }

        public PenLineJoin StrokeLineJoin
        {
            get => (PenLineJoin)GetValue(StrokeLineJoinProperty);
            set => SetValue(StrokeLineJoinProperty, value);
        }

        public double StrokeMiterLimit
        {
            get => (double)GetValue(StrokeMiterLimitProperty);
            set => SetValue(StrokeMiterLimitProperty, value);
        }

        public Pen CreatePen()
        {
            return new Pen
            {
                Brush = Stroke,
                Thickness = StrokeThickness,
                LineJoin = StrokeLineJoin,
                MiterLimit = StrokeMiterLimit,
                StartLineCap = StrokeLineCap,
                EndLineCap = StrokeLineCap,
                DashCap = StrokeLineCap,
                DashStyle = new DashStyle(StrokeDashArray, StrokeDashOffset)
            };
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            if (Stroke == null)
            {
                SetBinding(StrokeProperty, this.CreateBinding(nameof(Foreground)));
            }
        }
    }
}

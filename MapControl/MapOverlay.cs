// WPF MapControl - http://wpfmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MapControl
{
    /// <summary>
    /// Base class for map overlays with font, background, foreground and stroke properties.
    /// </summary>
    public class MapOverlay : MapElement
    {
        public static readonly DependencyProperty FontFamilyProperty = Control.FontFamilyProperty.AddOwner(
            typeof(MapOverlay), new FrameworkPropertyMetadata((o, e) => ((MapOverlay)o).typeface = null));

        public static readonly DependencyProperty FontStyleProperty = Control.FontStyleProperty.AddOwner(
            typeof(MapOverlay), new FrameworkPropertyMetadata((o, e) => ((MapOverlay)o).typeface = null));

        public static readonly DependencyProperty FontWeightProperty = Control.FontWeightProperty.AddOwner(
            typeof(MapOverlay), new FrameworkPropertyMetadata((o, e) => ((MapOverlay)o).typeface = null));

        public static readonly DependencyProperty FontStretchProperty = Control.FontStretchProperty.AddOwner(
            typeof(MapOverlay), new FrameworkPropertyMetadata((o, e) => ((MapOverlay)o).typeface = null));

        public static readonly DependencyProperty FontSizeProperty = Control.FontSizeProperty.AddOwner(
            typeof(MapOverlay));

        public static readonly DependencyProperty BackgroundProperty = Control.BackgroundProperty.AddOwner(
            typeof(MapOverlay));

        public static readonly DependencyProperty ForegroundProperty = Control.ForegroundProperty.AddOwner(
            typeof(MapOverlay), new FrameworkPropertyMetadata((o, e) => ((MapOverlay)o).ForegroundChanged((Brush)e.NewValue)));

        public static readonly DependencyProperty StrokeProperty = Shape.StrokeProperty.AddOwner(
            typeof(MapOverlay), new FrameworkPropertyMetadata((o, e) => ((MapOverlay)o).pen.Brush = (Brush)e.NewValue));

        public static readonly DependencyProperty StrokeThicknessProperty = Shape.StrokeThicknessProperty.AddOwner(
            typeof(MapOverlay), new FrameworkPropertyMetadata(0.5, FrameworkPropertyMetadataOptions.AffectsMeasure, (o, e) => ((MapOverlay)o).pen.Thickness = (double)e.NewValue));

        public static readonly DependencyProperty StrokeDashArrayProperty = Shape.StrokeDashArrayProperty.AddOwner(
            typeof(MapOverlay), new FrameworkPropertyMetadata((o, e) => ((MapOverlay)o).pen.DashStyle = new DashStyle((DoubleCollection)e.NewValue, ((MapOverlay)o).StrokeDashOffset)));

        public static readonly DependencyProperty StrokeDashOffsetProperty = Shape.StrokeDashOffsetProperty.AddOwner(
            typeof(MapOverlay), new FrameworkPropertyMetadata((o, e) => ((MapOverlay)o).pen.DashStyle = new DashStyle(((MapOverlay)o).StrokeDashArray, (double)e.NewValue)));

        public static readonly DependencyProperty StrokeDashCapProperty = Shape.StrokeDashCapProperty.AddOwner(
            typeof(MapOverlay), new FrameworkPropertyMetadata((o, e) => ((MapOverlay)o).pen.DashCap = (PenLineCap)e.NewValue));

        public static readonly DependencyProperty StrokeStartLineCapProperty = Shape.StrokeStartLineCapProperty.AddOwner(
            typeof(MapOverlay), new FrameworkPropertyMetadata((o, e) => ((MapOverlay)o).pen.StartLineCap = (PenLineCap)e.NewValue));

        public static readonly DependencyProperty StrokeEndLineCapProperty = Shape.StrokeEndLineCapProperty.AddOwner(
            typeof(MapOverlay), new FrameworkPropertyMetadata((o, e) => ((MapOverlay)o).pen.EndLineCap = (PenLineCap)e.NewValue));

        public static readonly DependencyProperty StrokeLineJoinProperty = Shape.StrokeLineJoinProperty.AddOwner(
            typeof(MapOverlay), new FrameworkPropertyMetadata((o, e) => ((MapOverlay)o).pen.LineJoin = (PenLineJoin)e.NewValue));

        public static readonly DependencyProperty StrokeMiterLimitProperty = Shape.StrokeMiterLimitProperty.AddOwner(
            typeof(MapOverlay), new FrameworkPropertyMetadata((o, e) => ((MapOverlay)o).pen.MiterLimit = (double)e.NewValue));

        private readonly Pen pen;
        private Typeface typeface;

        static MapOverlay()
        {
            UIElement.IsHitTestVisibleProperty.OverrideMetadata(
                typeof(MapOverlay), new FrameworkPropertyMetadata(false));
        }

        public MapOverlay()
        {
            pen = new Pen
            {
                Brush = Stroke,
                Thickness = StrokeThickness,
                DashStyle = new DashStyle(StrokeDashArray, StrokeDashOffset),
                DashCap = StrokeDashCap,
                StartLineCap = StrokeStartLineCap,
                EndLineCap = StrokeEndLineCap,
                LineJoin = StrokeLineJoin,
                MiterLimit = StrokeMiterLimit
            };
        }

        public FontFamily FontFamily
        {
            get { return (FontFamily)GetValue(FontFamilyProperty); }
            set { SetValue(FontFamilyProperty, value); }
        }

        public FontStyle FontStyle
        {
            get { return (FontStyle)GetValue(FontStyleProperty); }
            set { SetValue(FontStyleProperty, value); }
        }

        public FontWeight FontWeight
        {
            get { return (FontWeight)GetValue(FontWeightProperty); }
            set { SetValue(FontWeightProperty, value); }
        }

        public FontStretch FontStretch
        {
            get { return (FontStretch)GetValue(FontStretchProperty); }
            set { SetValue(FontStretchProperty, value); }
        }

        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        public Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        public Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }

        public Brush Stroke
        {
            get { return (Brush)GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }

        public double StrokeThickness
        {
            get { return (double)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }

        public DoubleCollection StrokeDashArray
        {
            get { return (DoubleCollection)GetValue(StrokeDashArrayProperty); }
            set { SetValue(StrokeDashArrayProperty, value); }
        }

        public double StrokeDashOffset
        {
            get { return (double)GetValue(StrokeDashOffsetProperty); }
            set { SetValue(StrokeDashOffsetProperty, value); }
        }

        public PenLineCap StrokeDashCap
        {
            get { return (PenLineCap)GetValue(StrokeDashCapProperty); }
            set { SetValue(StrokeDashCapProperty, value); }
        }

        public PenLineCap StrokeStartLineCap
        {
            get { return (PenLineCap)GetValue(StrokeStartLineCapProperty); }
            set { SetValue(StrokeStartLineCapProperty, value); }
        }

        public PenLineCap StrokeEndLineCap
        {
            get { return (PenLineCap)GetValue(StrokeEndLineCapProperty); }
            set { SetValue(StrokeEndLineCapProperty, value); }
        }

        public PenLineJoin StrokeLineJoin
        {
            get { return (PenLineJoin)GetValue(StrokeLineJoinProperty); }
            set { SetValue(StrokeLineJoinProperty, value); }
        }

        public double StrokeMiterLimit
        {
            get { return (double)GetValue(StrokeMiterLimitProperty); }
            set { SetValue(StrokeMiterLimitProperty, value); }
        }

        protected Pen Pen
        {
            get
            {
                if (pen.Brush == null)
                {
                    pen.Brush = Foreground;
                }

                return pen;
            }
        }

        protected Typeface Typeface
        {
            get
            {
                if (typeface == null)
                {
                    typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
                }

                return typeface;
            }
        }

        protected override void OnViewportChanged()
        {
            InvalidateVisual();
        }

        private void ForegroundChanged(Brush foreground)
        {
            if (Stroke == null)
            {
                pen.Brush = foreground;
            }
        }
    }
}

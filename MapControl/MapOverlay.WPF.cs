// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MapControl
{
    public partial class MapOverlay : FrameworkElement, IMapElement
    {
        public static readonly DependencyProperty BackgroundProperty = Control.BackgroundProperty.AddOwner(
            typeof(MapOverlay));

        public static readonly DependencyProperty FontSizeProperty = Control.FontSizeProperty.AddOwner(
            typeof(MapOverlay));

        public static readonly DependencyProperty FontFamilyProperty = Control.FontFamilyProperty.AddOwner(
            typeof(MapOverlay), new FrameworkPropertyMetadata((o, e) => ((MapOverlay)o).typeface = null));

        public static readonly DependencyProperty FontStyleProperty = Control.FontStyleProperty.AddOwner(
            typeof(MapOverlay), new FrameworkPropertyMetadata((o, e) => ((MapOverlay)o).typeface = null));

        public static readonly DependencyProperty FontStretchProperty = Control.FontStretchProperty.AddOwner(
            typeof(MapOverlay), new FrameworkPropertyMetadata((o, e) => ((MapOverlay)o).typeface = null));

        public static readonly DependencyProperty FontWeightProperty = Control.FontWeightProperty.AddOwner(
            typeof(MapOverlay), new FrameworkPropertyMetadata((o, e) => ((MapOverlay)o).typeface = null));

        public static readonly DependencyProperty ForegroundProperty = Control.ForegroundProperty.AddOwner(
            typeof(MapOverlay), new FrameworkPropertyMetadata((o, e) => ((MapOverlay)o).ForegroundChanged((Brush)e.NewValue)));

        public static readonly DependencyProperty StrokeProperty = Shape.StrokeProperty.AddOwner(
            typeof(MapOverlay), new FrameworkPropertyMetadata((o, e) => ((MapOverlay)o).pen.Brush = (Brush)e.NewValue));

        public static readonly DependencyProperty StrokeThicknessProperty = Shape.StrokeThicknessProperty.AddOwner(
            typeof(MapOverlay), new FrameworkPropertyMetadata(1d, FrameworkPropertyMetadataOptions.AffectsMeasure, (o, e) => ((MapOverlay)o).pen.Thickness = (double)e.NewValue));

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

        private Pen pen;
        private Typeface typeface;

        static MapOverlay()
        {
            UIElement.IsHitTestVisibleProperty.OverrideMetadata(
                typeof(MapOverlay), new FrameworkPropertyMetadata(false));
        }

        partial void Initialize()
        {
            MapPanel.AddParentMapHandlers(this);

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

        public Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
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

        protected virtual void OnViewportChanged()
        {
            InvalidateVisual();
        }

        private void OnViewportChanged(object sender, EventArgs e)
        {
            OnViewportChanged();
        }

        void IMapElement.ParentMapChanged(MapBase oldParentMap, MapBase newParentMap)
        {
            if (oldParentMap != null)
            {
                oldParentMap.ViewportChanged -= OnViewportChanged;
            }

            if (newParentMap != null)
            {
                newParentMap.ViewportChanged += OnViewportChanged;
                OnViewportChanged();
            }
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

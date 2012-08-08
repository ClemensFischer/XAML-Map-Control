// WPF MapControl - http://wpfmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MapControl
{
    /// <summary>
    /// An open map polygon, defined by a collection of geographic locations in the Locations property.
    /// </summary>
    public class MapPolyline : MapElement
    {
        public static readonly DependencyProperty StrokeProperty = Shape.StrokeProperty.AddOwner(
            typeof(MapPolyline), new FrameworkPropertyMetadata(Brushes.Black, (o, e) => ((MapPolyline)o).drawing.Pen.Brush = (Brush)e.NewValue));

        public static readonly DependencyProperty StrokeDashArrayProperty = Shape.StrokeDashArrayProperty.AddOwner(
            typeof(MapPolyline), new FrameworkPropertyMetadata((o, e) => ((MapPolyline)o).drawing.Pen.DashStyle = new DashStyle((DoubleCollection)e.NewValue, ((MapPolyline)o).StrokeDashOffset)));

        public static readonly DependencyProperty StrokeDashOffsetProperty = Shape.StrokeDashOffsetProperty.AddOwner(
            typeof(MapPolyline), new FrameworkPropertyMetadata((o, e) => ((MapPolyline)o).drawing.Pen.DashStyle = new DashStyle(((MapPolyline)o).StrokeDashArray, (double)e.NewValue)));

        public static readonly DependencyProperty StrokeDashCapProperty = Shape.StrokeDashCapProperty.AddOwner(
            typeof(MapPolyline), new FrameworkPropertyMetadata((o, e) => ((MapPolyline)o).drawing.Pen.DashCap = (PenLineCap)e.NewValue));

        public static readonly DependencyProperty StrokeStartLineCapProperty = Shape.StrokeStartLineCapProperty.AddOwner(
            typeof(MapPolyline), new FrameworkPropertyMetadata((o, e) => ((MapPolyline)o).drawing.Pen.StartLineCap = (PenLineCap)e.NewValue));

        public static readonly DependencyProperty StrokeEndLineCapProperty = Shape.StrokeEndLineCapProperty.AddOwner(
            typeof(MapPolyline), new FrameworkPropertyMetadata((o, e) => ((MapPolyline)o).drawing.Pen.EndLineCap = (PenLineCap)e.NewValue));

        public static readonly DependencyProperty StrokeLineJoinProperty = Shape.StrokeLineJoinProperty.AddOwner(
            typeof(MapPolyline), new FrameworkPropertyMetadata((o, e) => ((MapPolyline)o).drawing.Pen.LineJoin = (PenLineJoin)e.NewValue));

        public static readonly DependencyProperty StrokeMiterLimitProperty = Shape.StrokeMiterLimitProperty.AddOwner(
            typeof(MapPolyline), new FrameworkPropertyMetadata((o, e) => ((MapPolyline)o).drawing.Pen.MiterLimit = (double)e.NewValue));

        public static readonly DependencyProperty StrokeThicknessProperty = Shape.StrokeThicknessProperty.AddOwner(
            typeof(MapPolyline), new FrameworkPropertyMetadata((o, e) => ((MapPolyline)o).UpdatePenThickness()));

        public static readonly DependencyProperty TransformStrokeProperty = DependencyProperty.Register(
            "TransformStroke", typeof(bool), typeof(MapPolyline), new FrameworkPropertyMetadata((o, e) => ((MapPolyline)o).UpdatePenThickness()));

        public static readonly DependencyProperty LocationsProperty = DependencyProperty.Register(
            "Locations", typeof(LocationCollection), typeof(MapPolyline), new FrameworkPropertyMetadata((o, e) => ((MapPolyline)o).UpdateGeometry()));

        protected readonly DrawingVisual visual = new DrawingVisual();
        protected readonly GeometryDrawing drawing = new GeometryDrawing();

        public MapPolyline()
        {
            drawing.Pen = new Pen(Stroke, StrokeThickness);

            using (DrawingContext drawingContext = visual.RenderOpen())
            {
                drawingContext.DrawDrawing(drawing);
            }

            Loaded += (o, e) => UpdateGeometry();
        }

        public Brush Stroke
        {
            get { return (Brush)GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
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

        public double StrokeThickness
        {
            get { return (double)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }

        public bool TransformStroke
        {
            get { return (bool)GetValue(TransformStrokeProperty); }
            set { SetValue(TransformStrokeProperty, value); }
        }

        public LocationCollection Locations
        {
            get { return (LocationCollection)GetValue(LocationsProperty); }
            set { SetValue(LocationsProperty, value); }
        }

        public double TransformedStrokeThickness
        {
            get { return drawing.Pen.Thickness; }
        }

        public PathGeometry TransformedGeometry
        {
            get { return drawing.Geometry as PathGeometry; }
        }

        protected override int VisualChildrenCount
        {
            get { return 1; }
        }

        protected override Visual GetVisualChild(int index)
        {
            return visual;
        }

        protected override void OnInitialized(EventArgs eventArgs)
        {
            base.OnInitialized(eventArgs);

            AddVisualChild(visual);
        }

        protected override void OnViewportChanged()
        {
            double scale = 1d;

            if (TransformStroke)
            {
                scale = ParentMap.CenterScale * Map.MeterPerDegree;
            }

            drawing.Pen.Thickness = scale * StrokeThickness;
        }

        protected virtual void UpdateGeometry()
        {
            UpdateGeometry(false);
        }

        protected void UpdateGeometry(bool closed)
        {
            if (ParentMap != null && Locations != null && Locations.Count > 0)
            {
                drawing.Geometry = CreateGeometry(Locations, closed);
                OnViewportChanged();
            }
            else
            {
                drawing.Geometry = null;
            }
        }

        private void UpdatePenThickness()
        {
            if (ParentMap != null)
            {
                OnViewportChanged();
            }
        }

        private Geometry CreateGeometry(LocationCollection locations, bool closed)
        {
            StreamGeometry geometry = new StreamGeometry
            {
                Transform = ParentMap.ViewportTransform
            };

            using (StreamGeometryContext sgc = geometry.Open())
            {
                sgc.BeginFigure(ParentMap.MapTransform.Transform(locations.First()), closed, closed);

                if (Locations.Count > 1)
                {
                    sgc.PolyLineTo(ParentMap.MapTransform.Transform(locations.Skip(1)), true, true);
                }
            }

            return geometry;
        }
    }
}

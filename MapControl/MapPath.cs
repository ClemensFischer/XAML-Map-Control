using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MapControl
{
    public class MapPath : MapElement
    {
        public static readonly DependencyProperty DataProperty = Path.DataProperty.AddOwner(
            typeof(MapPath), new FrameworkPropertyMetadata((o, e) => ((MapPath)o).UpdateGeometry()));

        public static readonly DependencyProperty FillProperty = Shape.FillProperty.AddOwner(
            typeof(MapPath), new FrameworkPropertyMetadata((o, e) => ((MapPath)o).drawing.Brush = (Brush)e.NewValue));

        public static readonly DependencyProperty StrokeProperty = Shape.StrokeProperty.AddOwner(
            typeof(MapPath), new FrameworkPropertyMetadata(Brushes.Black, (o, e) => ((MapPath)o).drawing.Pen.Brush = (Brush)e.NewValue));

        public static readonly DependencyProperty StrokeDashArrayProperty = Shape.StrokeDashArrayProperty.AddOwner(
            typeof(MapPath), new FrameworkPropertyMetadata((o, e) => ((MapPath)o).drawing.Pen.DashStyle = new DashStyle((DoubleCollection)e.NewValue, ((MapPath)o).StrokeDashOffset)));

        public static readonly DependencyProperty StrokeDashOffsetProperty = Shape.StrokeDashOffsetProperty.AddOwner(
            typeof(MapPath), new FrameworkPropertyMetadata((o, e) => ((MapPath)o).drawing.Pen.DashStyle = new DashStyle(((MapPath)o).StrokeDashArray, (double)e.NewValue)));

        public static readonly DependencyProperty StrokeDashCapProperty = Shape.StrokeDashCapProperty.AddOwner(
            typeof(MapPath), new FrameworkPropertyMetadata((o, e) => ((MapPath)o).drawing.Pen.DashCap = (PenLineCap)e.NewValue));

        public static readonly DependencyProperty StrokeStartLineCapProperty = Shape.StrokeStartLineCapProperty.AddOwner(
            typeof(MapPath), new FrameworkPropertyMetadata((o, e) => ((MapPath)o).drawing.Pen.StartLineCap = (PenLineCap)e.NewValue));

        public static readonly DependencyProperty StrokeEndLineCapProperty = Shape.StrokeEndLineCapProperty.AddOwner(
            typeof(MapPath), new FrameworkPropertyMetadata((o, e) => ((MapPath)o).drawing.Pen.EndLineCap = (PenLineCap)e.NewValue));

        public static readonly DependencyProperty StrokeLineJoinProperty = Shape.StrokeLineJoinProperty.AddOwner(
            typeof(MapPath), new FrameworkPropertyMetadata((o, e) => ((MapPath)o).drawing.Pen.LineJoin = (PenLineJoin)e.NewValue));

        public static readonly DependencyProperty StrokeMiterLimitProperty = Shape.StrokeMiterLimitProperty.AddOwner(
            typeof(MapPath), new FrameworkPropertyMetadata((o, e) => ((MapPath)o).drawing.Pen.MiterLimit = (double)e.NewValue));

        public static readonly DependencyProperty StrokeThicknessProperty = Shape.StrokeThicknessProperty.AddOwner(
            typeof(MapPath), new FrameworkPropertyMetadata((o, e) => ((MapPath)o).UpdatePenThickness()));

        public static readonly DependencyProperty TransformStrokeProperty = DependencyProperty.Register(
            "TransformStroke", typeof(bool), typeof(MapPath), new FrameworkPropertyMetadata((o, e) => ((MapPath)o).UpdatePenThickness()));

        private readonly DrawingVisual visual = new DrawingVisual();
        private readonly GeometryDrawing drawing = new GeometryDrawing();

        public MapPath()
        {
            drawing.Brush = Fill;
            drawing.Pen = new Pen(Stroke, StrokeThickness);

            using (DrawingContext drawingContext = visual.RenderOpen())
            {
                drawingContext.DrawDrawing(drawing);
            }

            Loaded += (o, e) => UpdateGeometry();
        }

        public Geometry Data
        {
            get { return (Geometry)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        public Brush Fill
        {
            get { return (Brush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
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

        protected override void OnViewTransformChanged(Map parentMap)
        {
            double scale = 1d;

            if (TransformStroke && Data != null)
            {
                Point center = Data.Bounds.Location + (Vector)Data.Bounds.Size / 2d;
                scale = parentMap.GetMapScale(center) * Map.MeterPerDegree;
            }

            drawing.Pen.Thickness = scale * StrokeThickness;
        }

        private void UpdateGeometry()
        {
            Map parentMap = MapPanel.GetParentMap(this);

            if (parentMap != null && Data != null)
            {
                drawing.Geometry = parentMap.MapTransform.Transform(Data);
                drawing.Geometry.Transform = parentMap.ViewTransform;
                OnViewTransformChanged(parentMap);
            }
            else
            {
                drawing.Geometry = null;
            }
        }

        private void UpdatePenThickness()
        {
            Map parentMap = MapPanel.GetParentMap(this);

            if (parentMap != null)
            {
                OnViewTransformChanged(parentMap);
            }
        }
    }
}

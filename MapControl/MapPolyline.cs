// WPF MapControl - http://wpfmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MapControl
{
    /// <summary>
    /// An open map polygon, defined by a collection of geographic locations in the Locations property.
    /// </summary>
    public class MapPolyline : FrameworkElement
    {
        public static readonly DependencyProperty StrokeProperty = Shape.StrokeProperty.AddOwner(
            typeof(MapPolyline), new FrameworkPropertyMetadata((o, e) => ((MapPolyline)o).Drawing.Pen.Brush = (Brush)e.NewValue));

        public static readonly DependencyProperty StrokeThicknessProperty = Shape.StrokeThicknessProperty.AddOwner(
            typeof(MapPolyline));

        public static readonly DependencyProperty StrokeDashArrayProperty = Shape.StrokeDashArrayProperty.AddOwner(
            typeof(MapPolyline), new FrameworkPropertyMetadata((o, e) => ((MapPolyline)o).Drawing.Pen.DashStyle = new DashStyle((DoubleCollection)e.NewValue, ((MapPolyline)o).StrokeDashOffset)));

        public static readonly DependencyProperty StrokeDashOffsetProperty = Shape.StrokeDashOffsetProperty.AddOwner(
            typeof(MapPolyline), new FrameworkPropertyMetadata((o, e) => ((MapPolyline)o).Drawing.Pen.DashStyle = new DashStyle(((MapPolyline)o).StrokeDashArray, (double)e.NewValue)));

        public static readonly DependencyProperty StrokeDashCapProperty = Shape.StrokeDashCapProperty.AddOwner(
            typeof(MapPolyline), new FrameworkPropertyMetadata((o, e) => ((MapPolyline)o).Drawing.Pen.DashCap = (PenLineCap)e.NewValue));

        public static readonly DependencyProperty StrokeStartLineCapProperty = Shape.StrokeStartLineCapProperty.AddOwner(
            typeof(MapPolyline), new FrameworkPropertyMetadata((o, e) => ((MapPolyline)o).Drawing.Pen.StartLineCap = (PenLineCap)e.NewValue));

        public static readonly DependencyProperty StrokeEndLineCapProperty = Shape.StrokeEndLineCapProperty.AddOwner(
            typeof(MapPolyline), new FrameworkPropertyMetadata((o, e) => ((MapPolyline)o).Drawing.Pen.EndLineCap = (PenLineCap)e.NewValue));

        public static readonly DependencyProperty StrokeLineJoinProperty = Shape.StrokeLineJoinProperty.AddOwner(
            typeof(MapPolyline), new FrameworkPropertyMetadata((o, e) => ((MapPolyline)o).Drawing.Pen.LineJoin = (PenLineJoin)e.NewValue));

        public static readonly DependencyProperty StrokeMiterLimitProperty = Shape.StrokeMiterLimitProperty.AddOwner(
            typeof(MapPolyline), new FrameworkPropertyMetadata((o, e) => ((MapPolyline)o).Drawing.Pen.MiterLimit = (double)e.NewValue));

        public static readonly DependencyProperty TransformStrokeProperty = DependencyProperty.Register(
            "TransformStroke", typeof(bool), typeof(MapPolyline), new FrameworkPropertyMetadata((o, e) => ((MapPolyline)o).SetPenThicknessBinding()));

        public static readonly DependencyProperty LocationsProperty = DependencyProperty.Register(
            "Locations", typeof(LocationCollection), typeof(MapPolyline), new FrameworkPropertyMetadata((o, e) => ((MapPolyline)o).UpdateGeometry()));

        protected readonly GeometryDrawing Drawing = new GeometryDrawing();

        static MapPolyline()
        {
            MapPanel.ParentMapPropertyKey.OverrideMetadata(
                typeof(MapPolyline),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, ParentMapPropertyChanged));
        }

        public MapPolyline()
        {
            Drawing.Pen = new Pen
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

        public MapBase ParentMap
        {
            get { return MapPanel.GetParentMap(this); }
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

        public PathGeometry TransformedGeometry
        {
            get { return Drawing.Geometry as PathGeometry; }
        }

        public double TransformedStrokeThickness
        {
            get { return Drawing.Pen.Thickness; }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawDrawing(Drawing);
        }

        protected virtual void UpdateGeometry()
        {
            UpdateGeometry(false);
        }

        protected void UpdateGeometry(bool closed)
        {
            if (ParentMap != null && Locations != null && Locations.Count > 0)
            {
                Drawing.Geometry = CreateGeometry(Locations, closed);
            }
            else
            {
                Drawing.Geometry = null;
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

        private void SetPenThicknessBinding()
        {
            BindingBase binding = new Binding { Source = this, Path = new PropertyPath(MapPolyline.StrokeThicknessProperty)};

            if (TransformStroke && ParentMap != null)
            {
                MultiBinding multiBinding = new MultiBinding { Converter = new PenThicknessConverter() };
                multiBinding.Bindings.Add(binding);
                multiBinding.Bindings.Add(new Binding { Source = ParentMap, Path = new PropertyPath(MapBase.CenterScaleProperty)});
                binding = multiBinding;
            }

            BindingOperations.SetBinding(Drawing.Pen, Pen.ThicknessProperty, binding);
        }

        private static void ParentMapPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            MapPolyline polyline = obj as MapPolyline;

            if (polyline != null)
            {
                polyline.UpdateGeometry();
                polyline.SetPenThicknessBinding();
            }
        }

        private class PenThicknessConverter : IMultiValueConverter
        {
            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            {
                return (double)values[0] * (double)values[1] * MapBase.MeterPerDegree;
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
    }
}

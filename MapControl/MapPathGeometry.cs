using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace MapControl
{
    public static class MapPathGeometry
    {
        public static PathGeometry Transform(this GeneralTransform transform, Geometry geometry)
        {
            PathGeometry pathGeometry = geometry as PathGeometry;

            if (pathGeometry == null)
            {
                pathGeometry = PathGeometry.CreateFromGeometry(geometry);
            }

            if (geometry.Transform != null && geometry.Transform != System.Windows.Media.Transform.Identity)
            {
                GeneralTransformGroup transformGroup = new GeneralTransformGroup();
                transformGroup.Children.Add(geometry.Transform);
                transformGroup.Children.Add(transform);
                transform = transformGroup;
            }

            return new PathGeometry(Transform(transform, pathGeometry.Figures),
                                    pathGeometry.FillRule, System.Windows.Media.Transform.Identity);
        }

        public static PathFigureCollection Transform(this GeneralTransform transform, PathFigureCollection figures)
        {
            PathFigureCollection transformedFigures = new PathFigureCollection();

            foreach (PathFigure figure in figures)
            {
                transformedFigures.Add(Transform(transform, figure));
            }

            transformedFigures.Freeze();

            return transformedFigures;
        }

        public static PathFigure Transform(this GeneralTransform transform, PathFigure figure)
        {
            PathSegmentCollection transformedSegments = new PathSegmentCollection(figure.Segments.Count);

            foreach (PathSegment segment in figure.Segments)
            {
                PathSegment transformedSegment = null;

                if (segment is LineSegment)
                {
                    LineSegment lineSegment = (LineSegment)segment;

                    transformedSegment = new LineSegment(
                        transform.Transform(lineSegment.Point),
                        lineSegment.IsStroked);
                }
                else if (segment is PolyLineSegment)
                {
                    PolyLineSegment polyLineSegment = (PolyLineSegment)segment;

                    transformedSegment = new PolyLineSegment(
                        polyLineSegment.Points.Select(transform.Transform),
                        polyLineSegment.IsStroked);
                }
                else if (segment is ArcSegment)
                {
                    ArcSegment arcSegment = (ArcSegment)segment;
                    Size size = arcSegment.Size;
                    MapTransform mapTransform = transform as MapTransform;

                    if (mapTransform != null)
                    {
                        double yScale = mapTransform.RelativeScale(arcSegment.Point);

                        if (arcSegment.RotationAngle == 0d)
                        {
                            size.Height *= yScale;
                        }
                        else
                        {
                            double sinR = Math.Sin(arcSegment.RotationAngle * Math.PI / 180d);
                            double cosR = Math.Cos(arcSegment.RotationAngle * Math.PI / 180d);

                            size.Width *= Math.Sqrt(yScale * yScale * sinR * sinR + cosR * cosR);
                            size.Height *= Math.Sqrt(yScale * yScale * cosR * cosR + sinR * sinR);
                        }
                    }

                    transformedSegment = new ArcSegment(
                        transform.Transform(arcSegment.Point),
                        size,
                        arcSegment.RotationAngle,
                        arcSegment.IsLargeArc,
                        arcSegment.SweepDirection,
                        arcSegment.IsStroked);
                }
                else if (segment is BezierSegment)
                {
                    BezierSegment bezierSegment = (BezierSegment)segment;

                    transformedSegment = new BezierSegment(
                        transform.Transform(bezierSegment.Point1),
                        transform.Transform(bezierSegment.Point2),
                        transform.Transform(bezierSegment.Point3),
                        bezierSegment.IsStroked);
                }
                else if (segment is PolyBezierSegment)
                {
                    PolyBezierSegment polyBezierSegment = (PolyBezierSegment)segment;

                    transformedSegment = new PolyBezierSegment(
                        polyBezierSegment.Points.Select(transform.Transform),
                        polyBezierSegment.IsStroked);
                }
                else if (segment is QuadraticBezierSegment)
                {
                    QuadraticBezierSegment quadraticBezierSegment = (QuadraticBezierSegment)segment;

                    transformedSegment = new QuadraticBezierSegment(
                        transform.Transform(quadraticBezierSegment.Point1),
                        transform.Transform(quadraticBezierSegment.Point2),
                        quadraticBezierSegment.IsStroked);
                }
                else if (segment is PolyQuadraticBezierSegment)
                {
                    PolyQuadraticBezierSegment polyQuadraticBezierSegment = (PolyQuadraticBezierSegment)segment;

                    transformedSegment = new PolyQuadraticBezierSegment(
                        polyQuadraticBezierSegment.Points.Select(transform.Transform),
                        polyQuadraticBezierSegment.IsStroked);
                }

                if (transformedSegment != null)
                {
                    transformedSegment.IsSmoothJoin = segment.IsSmoothJoin;
                    transformedSegments.Add(transformedSegment);
                }
            }

            PathFigure transformedFigure = new PathFigure(
                transform.Transform(figure.StartPoint),
                transformedSegments,
                figure.IsClosed);

            transformedFigure.IsFilled = figure.IsFilled;
            transformedFigure.Freeze();

            return transformedFigure;
        }
    }
}

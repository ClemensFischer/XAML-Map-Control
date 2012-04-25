using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace MapControl
{
    public static class MapStreamGeometry
    {
        public static MapStreamGeometryContext Open(this StreamGeometry mapGeometry, MapTransform transform)
        {
            return new MapStreamGeometryContext(mapGeometry.Open(), transform);
        }
    }

    public class MapStreamGeometryContext : IDisposable
    {
        StreamGeometryContext context;
        MapTransform transform;

        public MapStreamGeometryContext(StreamGeometryContext context, MapTransform transform)
        {
            this.context = context;
            this.transform = transform;
        }

        void IDisposable.Dispose()
        {
            context.Close();
        }

        public void Close()
        {
            context.Close();
        }

        public void BeginFigure(Point startPoint, bool isFilled, bool isClosed)
        {
            context.BeginFigure(transform.Transform(startPoint), isFilled, isClosed);
        }

        public void ArcTo(Point point, Size size, double rotationAngle, bool isLargeArc, SweepDirection sweepDirection, bool isStroked, bool isSmoothJoin)
        {
            double yScale = transform.RelativeScale(point);

            if (rotationAngle == 0d)
            {
                size.Height *= yScale;
            }
            else
            {
                double sinR = Math.Sin(rotationAngle * Math.PI / 180d);
                double cosR = Math.Cos(rotationAngle * Math.PI / 180d);

                size.Width *= Math.Sqrt(yScale * yScale * sinR * sinR + cosR * cosR);
                size.Height *= Math.Sqrt(yScale * yScale * cosR * cosR + sinR * sinR);
            }

            context.ArcTo(transform.Transform(point), size, rotationAngle, isLargeArc, sweepDirection, isStroked, isSmoothJoin);
        }

        public void LineTo(Point point, bool isStroked, bool isSmoothJoin)
        {
            context.LineTo(transform.Transform(point), isStroked, isSmoothJoin);
        }

        public void QuadraticBezierTo(Point point1, Point point2, bool isStroked, bool isSmoothJoin)
        {
            context.QuadraticBezierTo(transform.Transform(point1), transform.Transform(point2), isStroked, isSmoothJoin);
        }

        public void BezierTo(Point point1, Point point2, Point point3, bool isStroked, bool isSmoothJoin)
        {
            context.BezierTo(transform.Transform(point1), transform.Transform(point2), transform.Transform(point3), isStroked, isSmoothJoin);
        }

        public void PolyLineTo(IList<Point> points, bool isStroked, bool isSmoothJoin)
        {
            context.PolyLineTo(TransformPoints(points), isStroked, isSmoothJoin);
        }

        public void PolyQuadraticBezierTo(IList<Point> points, bool isStroked, bool isSmoothJoin)
        {
            context.PolyQuadraticBezierTo(TransformPoints(points), isStroked, isSmoothJoin);
        }

        public void PolyBezierTo(IList<Point> points, bool isStroked, bool isSmoothJoin)
        {
            context.PolyBezierTo(TransformPoints(points), isStroked, isSmoothJoin);
        }

        private IList<Point> TransformPoints(IList<Point> points)
        {
            Point[] transformedPoints = new Point[points.Count];

            for (int i = 0; i < transformedPoints.Length; i++)
            {
                transformedPoints[i] = transform.Transform(points[i]);
            }

            return transformedPoints;
        }
    }
}

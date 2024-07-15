// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
#if UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
#endif

namespace MapControl
{
    /// <summary>
    /// Base class of MapPolyline and MapPolygon.
    /// </summary>
    public class MapPolypoint : MapPath
    {
        public static readonly DependencyProperty FillRuleProperty =
            DependencyPropertyHelper.Register<MapPolygon, FillRule>(nameof(FillRule), FillRule.EvenOdd,
                (polypoint, oldValue, newValue) => ((PathGeometry)polypoint.Data).FillRule = newValue);

        public FillRule FillRule
        {
            get => (FillRule)GetValue(FillRuleProperty);
            set => SetValue(FillRuleProperty, value);
        }

        protected MapPolypoint()
        {
            Data = new PathGeometry();
        }

        protected void DataCollectionPropertyChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            if (oldValue is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= DataCollectionChanged;
            }

            if (newValue is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += DataCollectionChanged;
            }

            UpdateData();
        }

        protected void DataCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateData();
        }

        protected void UpdateData(IEnumerable<Location> locations, bool closed)
        {
            var pathFigures = ((PathGeometry)Data).Figures;
            pathFigures.Clear();

            if (ParentMap != null && locations?.Count() >= 2)
            {
                var longitudeOffset = GetLongitudeOffset(Location ?? locations.First());

                AddPolylinePoints(pathFigures, locations, longitudeOffset, closed);
            }
        }

        private void AddPolylinePoints(PathFigureCollection pathFigures, IEnumerable<Location> locations, double longitudeOffset, bool closed)
        {
            var points = locations
                .Select(location => LocationToView(location, longitudeOffset))
                .Where(point => point.HasValue)
                .Select(point => point.Value);

            if (closed)
            {
                var figure = new PathFigure
                {
                    StartPoint = points.First(),
                    IsClosed = true,
                    IsFilled = true
                };

                var polyline = new PolyLineSegment();

                foreach (var point in points.Skip(1))
                {
                    polyline.Points.Add(point);
                }

                figure.Segments.Add(polyline);
                pathFigures.Add(figure);
            }
            else
            {
                PathFigure figure = null;
                PolyLineSegment polyline = null;
                var viewport = new Rect(0, 0, ParentMap.RenderSize.Width, ParentMap.RenderSize.Height);
                var pointList = points.ToList();

                for (int i = 1; i < pointList.Count; i++)
                {
                    var p1 = pointList[i - 1];
                    var p2 = pointList[i];
                    var inside = GetIntersections(ref p1, ref p2, viewport);

                    if (inside)
                    {
                        if (figure == null)
                        {
                            figure = new PathFigure
                            {
                                StartPoint = p1,
                                IsClosed = false,
                                IsFilled = true
                            };

                            polyline = new PolyLineSegment();
                            figure.Segments.Add(polyline);
                            pathFigures.Add(figure);
                        }

                        polyline.Points.Add(p2);
                    }

                    if (!inside || p2 != pointList[i])
                    {
                        figure = null;
                    }
                }
            }
        }

        /// <summary>
        /// Calculates the potential intersections of a line segment given by (p1,p2) with a rectangle.
        /// Updates either p1, p2, or both with any found intersection and returns a value that indicates
        /// whether the segment intersects or lies inside the rectangle.
        /// </summary>
        private static bool GetIntersections(ref Point p1, ref Point p2, Rect rect)
        {
            if (rect.Contains(p1) && rect.Contains(p2))
            {
                return true;
            }

            var topLeft = new Point(rect.X, rect.Y);
            var topRight = new Point(rect.X + rect.Width, rect.Y);
            var bottomLeft = new Point(rect.X, rect.Y + rect.Height);
            var bottomRight = new Point(rect.X + rect.Width, rect.Y + rect.Height);
            var numIntersections = 0;

            if (GetIntersection(ref p1, ref p2, topLeft, bottomLeft, p => p.X <= rect.X)) // left edge
            {
                numIntersections++;
            }

            if (GetIntersection(ref p1, ref p2, topLeft, topRight, p => p.Y <= rect.Y)) // top edge
            {
                numIntersections++;
            }

            if (numIntersections < 2 &&
                GetIntersection(ref p1, ref p2, topRight, bottomRight, p => p.X >= rect.X + rect.Width)) // right edge
            {
                numIntersections++;
            }

            if (numIntersections < 2 &&
                GetIntersection(ref p1, ref p2, bottomLeft, bottomRight, p => p.Y >= rect.Y + rect.Height)) // bottom edge
            {
                numIntersections++;
            }

            return numIntersections > 0;
        }

        private static bool GetIntersection(ref Point p1, ref Point p2, Point p3, Point p4, Predicate<Point> predicate)
        {
            var intersection = GetIntersection(p1, p2, p3, p4);

            if (!intersection.HasValue)
            {
                return false;
            }

            if (predicate(p1))
            {
                p1 = intersection.Value;
            }
            else
            {
                p2 = intersection.Value;
            }

            return true;
        }

        /// <summary>
        /// Returns the intersection point of two line segments given by (p1,p2) and (p3,p4),
        /// or null if no intersection exists. See https://stackoverflow.com/a/1968345.
        /// </summary>
        private static Point? GetIntersection(Point p1, Point p2, Point p3, Point p4)
        {
            var x12 = p2.X - p1.X;
            var y12 = p2.Y - p1.Y;
            var x34 = p4.X - p3.X;
            var y34 = p4.Y - p3.Y;
            var x13 = p3.X - p1.X;
            var y13 = p3.Y - p1.Y;

            var d = x12 * y34 - x34 * y12;
            var s = (x13 * y12 - y13 * x12) / d;
            var t = (x13 * y34 - y13 * x34) / d;

            if (s >= 0d && s <= 1d && t >= 0d && t <= 1d)
            {
                return new Point(p1.X + t * x12, p1.Y + t * y12);
            }

            return null;
        }
    }
}

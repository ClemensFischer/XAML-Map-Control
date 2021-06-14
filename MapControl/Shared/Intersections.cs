// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if WINUI || WINDOWS_UWP
using Windows.Foundation;
#else
using System.Windows;
#endif

namespace MapControl
{
    public static class Intersections
    {
        /// <summary>
        /// Returns the intersection point of two line segments given by (p1,p2) and (p3,p4),
        /// or null if no intersection exists. See https://stackoverflow.com/a/1968345.
        /// </summary>
        public static Point? GetIntersection(Point p1, Point p2, Point p3, Point p4)
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

        /// <summary>
        /// Calculates the potential intersections of a line segment given by (p1,p2) with a rectangle.
        /// Updates either p1, p2, or both with any found intersection and returns a value that indicates
        /// whether the segment intersects or lies inside the rectangle.
        /// </summary>
        public static bool GetIntersections(ref Point p1, ref Point p2, Rect rect)
        {
            if (rect.Contains(p1) && rect.Contains(p2))
            {
                return true;
            }

            var topLeft = new Point(rect.Left, rect.Top);
            var topRight = new Point(rect.Right, rect.Top);
            var bottomLeft = new Point(rect.Left, rect.Bottom);
            var bottomRight = new Point(rect.Right, rect.Bottom);
            var numIntersections = 0;

            if (GetIntersection(ref p1, ref p2, topLeft, bottomLeft, p => p.X <= rect.Left)) // left edge
            {
                numIntersections++;
            }

            if (GetIntersection(ref p1, ref p2, topLeft, topRight, p => p.Y <= rect.Top)) // top edge
            {
                numIntersections++;
            }

            if (numIntersections < 2 &&
                GetIntersection(ref p1, ref p2, topRight, bottomRight, p => p.X >= rect.Right)) // right edge
            {
                numIntersections++;
            }

            if (numIntersections < 2 &&
                GetIntersection(ref p1, ref p2, bottomLeft, bottomRight, p => p.Y >= rect.Bottom)) // bottom edge
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
    }
}

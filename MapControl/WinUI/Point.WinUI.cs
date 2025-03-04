﻿namespace MapControl
{
    /// <summary>
    /// Replaces Windows.Foundation.Point for double floating point precision.
    /// </summary>
    public readonly struct Point
    {
        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double X { get; }
        public double Y { get; }

        public static implicit operator Windows.Foundation.Point(Point p)
        {
            return new Windows.Foundation.Point(p.X, p.Y);
        }

        public static implicit operator Point(Windows.Foundation.Point p)
        {
            return new Point(p.X, p.Y);
        }

        public static bool operator ==(Point p1, Point p2)
        {
            return p1.X == p2.X && p1.Y == p2.Y;
        }

        public static bool operator !=(Point p1, Point p2)
        {
            return !(p1 == p2);
        }

        public override bool Equals(object obj)
        {
            return obj is Point p && this == p;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }
    }
}

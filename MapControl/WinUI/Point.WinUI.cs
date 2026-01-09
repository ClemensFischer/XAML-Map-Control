using System;

namespace MapControl
{
    /// <summary>
    /// Replaces Windows.Foundation.Point for double floating point precision.
    /// </summary>
    public readonly struct Point(double x, double y) : IEquatable<Point>
    {
        public double X => x;
        public double Y => y;

        public static implicit operator Windows.Foundation.Point(Point p) => new(p.X, p.Y);

        public static implicit operator Point(Windows.Foundation.Point p) => new(p.X, p.Y);

        public static bool operator ==(Point p1, Point p2) => p1.Equals(p2);

        public static bool operator !=(Point p1, Point p2) => !p1.Equals(p2);

        public bool Equals(Point p) => X == p.X && Y == p.Y;

        public override bool Equals(object obj) => obj is Point p && Equals(p);

        public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode();
    }
}

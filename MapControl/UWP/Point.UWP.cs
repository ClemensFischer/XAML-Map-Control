// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

namespace MapControl
{
    /// <summary>
    /// Replaces Windows.Foundation.Point to achieve necessary floating point precision.
    /// </summary>
    public struct Point
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        public static implicit operator Windows.Foundation.Point(Point p)
        {
            return new Windows.Foundation.Point(p.X, p.Y);
        }

        public static implicit operator Point(Windows.Foundation.Point p)
        {
            return new Point(p.X, p.Y);
        }

        public static explicit operator Point(Vector v)
        {
            return new Point(v.X, v.Y);
        }

        public static Point operator -(Point p)
        {
            return new Point(-p.X, -p.Y);
        }

        public static Point operator +(Point p, Vector v)
        {
            return new Point(p.X + v.X, p.Y + v.Y);
        }

        public static Point operator -(Point p, Vector v)
        {
            return new Point(p.X - v.X, p.Y - v.Y);
        }

        public static Vector operator -(Point p1, Point p2)
        {
            return new Vector(p1.X - p2.X, p1.Y - p2.Y);
        }

        public static bool operator ==(Point p1, Point p2)
        {
            return p1.X == p2.X && p1.Y == p2.Y;
        }

        public static bool operator !=(Point p1, Point p2)
        {
            return !(p1 == p2);
        }

        public override bool Equals(object o)
        {
            return o is Point && this == (Point)o;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }
    }
}

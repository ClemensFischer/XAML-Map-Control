// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

namespace MapControl
{
    public struct Vector
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Vector(double x, double y)
        {
            X = x;
            Y = y;
        }

        public static implicit operator Windows.Foundation.Point(Vector v)
        {
            return new Windows.Foundation.Point(v.X, v.Y);
        }

        public static implicit operator Vector(Windows.Foundation.Point v)
        {
            return new Vector(v.X, v.Y);
        }

        public static explicit operator Vector(Point p)
        {
            return new Vector(p.X, p.Y);
        }

        public static Vector operator -(Vector v)
        {
            return new Vector(-v.X, -v.Y);
        }

        public static Point operator +(Vector v, Point p)
        {
            return new Point(v.X + p.X, v.Y + p.Y);
        }

        public static Vector operator +(Vector v1, Vector v2)
        {
            return new Vector(v1.X + v2.X, v1.Y + v2.Y);
        }

        public static Vector operator -(Vector v1, Vector v2)
        {
            return new Vector(v1.X - v2.X, v1.Y - v2.Y);
        }

        public static Vector operator *(double f, Vector v)
        {
            return new Vector(f * v.X, f * v.Y);
        }

        public static Vector operator *(Vector v, double f)
        {
            return new Vector(f * v.X, f * v.Y);
        }

        public static bool operator ==(Vector v1, Vector v2)
        {
            return v1.X == v2.X && v1.Y == v2.Y;
        }

        public static bool operator !=(Vector v1, Vector v2)
        {
            return !(v1 == v2);
        }

        public override bool Equals(object o)
        {
            return o is Vector && this == (Vector)o;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }
    }
}

using System;

namespace MapControl
{
    /// <summary>
    /// Replaces Windows.Foundation.Rect for double floating point precision.
    /// </summary>
    public readonly struct Rect : IEquatable<Rect>
    {
        public Rect(double x, double y, double width, double height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public Rect(Point p1, Point p2)
        {
            X = Math.Min(p1.X, p2.X);
            Y = Math.Min(p1.Y, p2.Y);
            Width = Math.Max(p1.X, p2.X) - X;
            Height = Math.Max(p1.Y, p2.Y) - Y;
        }

        public double X { get; }
        public double Y { get; }
        public double Width { get; }
        public double Height { get; }

        public static implicit operator Windows.Foundation.Rect(Rect r) => new(r.X, r.Y, r.Width, r.Height);

        public static implicit operator Rect(Windows.Foundation.Rect r) => new(r.X, r.Y, r.Width, r.Height);

        public static bool operator ==(Rect r1, Rect r2) => r1.Equals(r2);

        public static bool operator !=(Rect r1, Rect r2) => !r1.Equals(r2);

        public bool Equals(Rect r) => X == r.X && Y == r.Y && Width == r.Width && Height == r.Height;

        public override bool Equals(object obj) => obj is Rect r && Equals(r);

        public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode() ^ Width.GetHashCode() ^ Height.GetHashCode();

        public bool Contains(Point p) => p.X >= X && p.X <= X + Width && p.Y >= Y && p.Y <= Y + Height;
    }
}

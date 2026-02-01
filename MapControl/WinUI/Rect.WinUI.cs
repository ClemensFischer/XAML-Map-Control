using System;

namespace MapControl
{
    /// <summary>
    /// Replaces Windows.Foundation.Rect for double floating point precision.
    /// </summary>
    public readonly struct Rect(double x, double y, double width, double height) : IEquatable<Rect>
    {
        public double X => x;
        public double Y => y;
        public double Width => width;
        public double Height => height;

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

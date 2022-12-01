// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;

namespace MapControl
{
    /// <summary>
    /// Replaces Windows.Foundation.Rect for double floating point precision.
    /// </summary>
    public struct Rect
    {
        public Rect(double x, double y, double width, double height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public Rect(Point p, Windows.Foundation.Size s)
        {
            X = p.X;
            Y = p.Y;
            Width = s.Width;
            Height = s.Height;
        }

        public Rect(Point p1, Point p2)
        {
            X = Math.Min(p1.X, p2.X);
            Y = Math.Min(p1.Y, p2.Y);
            Width = Math.Max(p1.X, p2.X) - X;
            Height = Math.Max(p1.Y, p2.Y) - Y;
        }

        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public bool Contains(Point p)
        {
            return p.X >= X && p.X <= X + Width
                && p.Y >= Y && p.Y <= Y + Height;
        }

        public static implicit operator Windows.Foundation.Rect(Rect r)
        {
            return new Windows.Foundation.Rect(r.X, r.Y, r.Width, r.Height);
        }

        public static implicit operator Rect(Windows.Foundation.Rect r)
        {
            return new Rect(r.X, r.Y, r.Width, r.Height);
        }

        public static bool operator ==(Rect r1, Rect r2)
        {
            return r1.X == r2.X && r1.Y == r2.Y
                && r1.Width == r2.Width && r1.Height == r2.Height;
        }

        public static bool operator !=(Rect r1, Rect r2)
        {
            return !(r1 == r2);
        }

        public override bool Equals(object obj)
        {
            return obj is Rect r && this == r;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Width.GetHashCode() ^ Height.GetHashCode();
        }
    }
}
